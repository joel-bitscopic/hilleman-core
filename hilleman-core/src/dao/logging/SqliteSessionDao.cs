using System;
using System.Data.SQLite;
using System.Data;
using com.bitscopic.hilleman.core.domain.session;
using com.bitscopic.hilleman.core.utils;
using System.Text;
using System.Threading.Tasks;
using com.bitscopic.hilleman.core.dao.sql;

namespace com.bitscopic.hilleman.core.dao.logging
{
    public class SqliteSessionDao : ISessionDao, IDisposable
    {
        ISqlConnection _cxn;

        public SqliteSessionDao(String connectionString)
        {
            _cxn = new SqliteConnection(new domain.SourceSystem() { connectionString = connectionString });
        }

        public void saveSessionAsync(HillemanSession session)
        {
            Task t = new Task(() => saveSession(session));
            t.ContinueWith(rslt => saveSessionExceptionSink(t), TaskContinuationOptions.OnlyOnFaulted);
            t.Start();
        }

        private Task saveSessionExceptionSink(Task t)
        {
            if (t.IsFaulted || t.Exception != null)
            {
                LogUtils.LOG(t.Exception.ToString());
                // log? don't care right now - just observe to prevent unhandled exceptions
            }
            return t;
        }

        public void saveSession(domain.session.HillemanSession session)
        {
            using (IDbTransaction tx = _cxn.startTransaction()) 
            {
                using (IDbCommand saveSess = buildSaveSessionCommand(session))
                {
                    _cxn.insertUpdateDelete(saveSess, tx);
                }
                using (IDbCommand saveRequests = buildSaveSessionRequestsCommand(session))
                {
                    _cxn.insertUpdateDelete(saveRequests, tx); 
                }
                _cxn.commitTransaction(tx);
            }
        }

        internal IDbCommand buildSaveSessionCommand(HillemanSession session)
        {
            IDbCommand cmd = _cxn.buildCommand(); 
            cmd.CommandText = "INSERT INTO sessions (session_token, start, end, source_system_id, user_id) values (@token, @start, @end, @srcSysId, @uid)";

            SQLiteParameter tokenParam = new SQLiteParameter("token");
            tokenParam.Value = session.sessionToken;
            cmd.Parameters.Add(tokenParam);

            SQLiteParameter startParam = new SQLiteParameter("start");
            startParam.Value = DateUtils.toIsoString(session.sessionStart);
            cmd.Parameters.Add(startParam);

            SQLiteParameter endParam = new SQLiteParameter("end");
            endParam.Value = DateUtils.toIsoString(session.sessionEnd);
            cmd.Parameters.Add(endParam);

            SQLiteParameter srcParam = new SQLiteParameter("srcSysId");
            srcParam.Value = session.getBaseConnection().getSource().id;
            cmd.Parameters.Add(srcParam);

            SQLiteParameter uidParam = new SQLiteParameter("uid");
            uidParam.Value = session.endUser.id;
            cmd.Parameters.Add(uidParam);

            return cmd;
        }

        internal IDbCommand buildSaveSessionRequestsCommand(HillemanSession session)
        {
            IDbCommand cmd = _cxn.buildCommand(); //_cxn.CreateCommand();
            cmd.CommandText = "INSERT INTO session_requests (session_token, request_timestamp, response_timestamp, request_name, request_args) values ";

            StringBuilder sb = new StringBuilder();
            foreach (HillemanRequest request in session.requests)
            {
                sb.Append("('" + session.sessionToken + "', "); 
                sb.Append("'" + DateUtils.toIsoString(request.requestTimestamp) + "', "); 
                sb.Append("'" + DateUtils.toIsoString(request.responseTimestamp) + "', "); 
                sb.Append("'" + request.requestName + "', ");
                sb.Append("'" + SqlUtils.adjustStringForSql(request.getArgsString()) + "'"); // don't forget to escape args string
                sb.Append("),");
            }
            sb.Remove(sb.Length - 1, 1); // remove last comma

            cmd.CommandText += sb.ToString();
            return cmd;
        }

        #region IDisposable
        public void Dispose()
        {
            _cxn.Dispose();
        }

        #endregion

    }
}