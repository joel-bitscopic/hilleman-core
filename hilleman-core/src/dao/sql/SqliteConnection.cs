using System;
using com.bitscopic.hilleman.core.domain.resource;
using com.bitscopic.hilleman.core.domain;
using System.Data;
using com.bitscopic.hilleman.core.dao.sql;

namespace com.bitscopic.hilleman.core.dao.sql
{
    public class SqliteConnection : ISqlConnection, IConnection
    {
        System.Data.SQLite.SQLiteConnection _cxn = null;
        SourceSystem _source = null;
        String _provider;
        bool _isPooled;

        public SqliteConnection(SourceSystem source)
        {
            _provider = "SQLite";
            _source = source;
            _cxn = new System.Data.SQLite.SQLiteConnection(source.connectionString);
            if (source.poolingConfig != null && source.poolingConfig is SqlConnectionPoolSource && ((SqlConnectionPoolSource)source.poolingConfig).poolingEnabled)
            {
                _isPooled = true;
            }
        }

        public void Dispose()
        {
            _cxn.Dispose();
        }

        public String getProvider() { return _provider; }

        public IDbTransaction startTransaction()
        {
            if (_cxn.State != ConnectionState.Open)
            {
                this.connect();
            }
            return _cxn.BeginTransaction();
        }

        public void commitTransaction(IDbTransaction tx)
        {
            try
            {
                tx.Commit();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (_isPooled)
                {
                    this.disconnect();
                }
            }
        }

        public void rollbackTransaction(IDbTransaction tx)
        {
            try
            {
                tx.Rollback();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (_isPooled)
                {
                    this.disconnect();
                }
            }
        }

        public IDbCommand buildCommand()
        {
            if (_cxn.State != ConnectionState.Open)
            {
                this.connect();
            }
            return _cxn.CreateCommand();
        }

        public IDataReader select(IDbCommand request)
        {
            if (_isPooled)
            {
                try
                {
                    IDataReader rdr = request.ExecuteReader();
                    // return a MockDataReader so we can dispose of the rdr here and not worry about the closed resource
                    MockDataReader mdr = new MockDataReader();
                    DataTable table = new DataTable();
                    table.Load(rdr);
                    mdr.Table = table;
                    return mdr;
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    this.disconnect();
                }
            }
            else
            {
                return request.ExecuteReader();
            }
        }

        public String insertReturningRowId(IDbCommand request, bool usingTransaction = false)
        {
            try
            {
                request.ExecuteNonQuery();
                using (IDbCommand cmd = this.buildCommand())
                {
                    cmd.CommandText = "SELECT last_insert_rowid()";
                    return Convert.ToString(cmd.ExecuteScalar());
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (_isPooled && !usingTransaction)
                {
                    this.disconnect();
                }
            }

        }

        public Int32 insertUpdateDelete(IDbCommand request)
        {
            try
            {
                return request.ExecuteNonQuery();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (_isPooled)
                {
                    this.disconnect();
                }
            }
        }

        /// <summary>
        /// Use this API to explicitly note the request is part of a transaction and connection should not be hijacked/closed
        /// </summary>
        /// <param name="request"></param>
        /// <param name="tx"></param>
        /// <returns></returns>
        public Int32 insertUpdateDelete(IDbCommand request, IDbTransaction tx)
        {
            if (request.Transaction == null)
            {
                request.Transaction = tx;
            }
            return request.ExecuteNonQuery();
        }

        public object executeScalar(IDbCommand request)
        {
            try
            {
                return request.ExecuteScalar();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (_isPooled)
                {
                    this.disconnect();
                }
            }
        }

        public void connect()
        {
            try
            {
                _cxn.Open();
            }
            catch (System.Data.SQLite.SQLiteException sqliteExc)
            {
                if (sqliteExc.Message.Contains("unable to open"))
                {
                    if (_source != null)
                    {
                        throw new System.Data.SQLite.SQLiteException("unable to open datbase file at " + _source.connectionString);
                    }
                    else
                    {
                        throw new System.Data.SQLite.SQLiteException("Unable to open databse file - no source defined");
                    }
                }
                throw;
            }
        }

        public void disconnect()
        {
            _cxn.Close();
        }

        public DataTable selectToTable(IDbCommand request)
        {
            try
            {
                using (IDataReader rdr = request.ExecuteReader())
                {
                    DataTable table = new DataTable();
                    table.Load(rdr);
                    return table;
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (_isPooled)
                {
                    this.disconnect();
                }
            }
        }

        public SourceSystem getSource()
        {
            return _source;
        }

        public IDbDataParameter getParameterForObject(object value)
        {
            if (value == null)
            {
                return null;
            }
            else if (value is String)
            {
                return new System.Data.SQLite.SQLiteParameter(DbType.String);
            }
            else if (value is DateTime)
            {
                return new System.Data.SQLite.SQLiteParameter(DbType.DateTime);
            }
            else if (value is Int16)
            {
                return new System.Data.SQLite.SQLiteParameter(DbType.Int16);
            }
            else if (value is Int32)
            {
                return new System.Data.SQLite.SQLiteParameter(DbType.Int32);
            }
            else if (value is Int64)
            {
                return new System.Data.SQLite.SQLiteParameter(DbType.Int64);
            }
            else if (value is byte[])
            {
                return new System.Data.SQLite.SQLiteParameter(DbType.Binary);
            }

            throw new NotImplementedException("These is no parameter type for " + value.GetType().FullName);
        }
    }
}