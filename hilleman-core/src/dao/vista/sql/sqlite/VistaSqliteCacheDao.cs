using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;
using System.Configuration;
using com.bitscopic.hilleman.core.utils;
using com.bitscopic.hilleman.core.domain;

namespace com.bitscopic.hilleman.core.dao.sql.sqlite
{
    public class VistaSqliteCacheDao : ICrrudDao, IDisposable
    {
        String _cxnString = "";

        public VistaSqliteCacheDao(String connectionString)
        {
            _cxnString = connectionString;
        }

        public DataTable executeSql(String sql)
        {
            using (SQLiteConnection cxn = new SQLiteConnection(_cxnString))
            {
                cxn.Open();

                using (SQLiteCommand cmd = cxn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    System.Console.WriteLine(sql);
                    using (SQLiteDataReader rdr = cmd.ExecuteReader())
                    {
                        DataTable result = new DataTable();
                        result.Load(rdr);
                        return result;
                    }
                }
            }
        }

        public void Dispose()
        {
            // any cleanup? all SqliteDao APIs are currently relying on 'using' statements to ensure resources are cleaned up
        }

        public ReadRangeResponse readRange(ReadRangeRequest request)
        {
            return SqlUtils.convertDataTableToReadRangeResponse(request,
                this.executeSql(
                    new SqlVistaFieldTranslator(MyConfigurationManager.getValue("SqlVistaMapFiles")).getSqlQueryFromReadRange(request)));
        }

        public ReadResponse read(ReadRequest request)
        {
            SqlVistaFieldTranslator tx = new SqlVistaFieldTranslator(MyConfigurationManager.getValue("SqlVistaMapFiles"));
            return SqlUtils.convertDataTableToReadResponse(request, this.executeSql(tx.getSqlQueryFromRead(request)), tx);
        }

        public CreateResponse create(CreateRequest request)
        {
            throw new NotImplementedException();
        }

        public UpdateResponse update(UpdateRequest request)
        {
            throw new NotImplementedException();
        }

        public DeleteResponse delete(DeleteRequest request)
        {
            throw new NotImplementedException();
        }

        public domain.SourceSystem getSource()
        {
            return new domain.SourceSystem() { connectionString = _cxnString };
        }
    }
}