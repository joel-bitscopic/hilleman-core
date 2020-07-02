using System;
using com.bitscopic.hilleman.core.domain.resource;
using com.bitscopic.hilleman.core.domain;
using System.Data;

namespace com.bitscopic.hilleman.core.dao.sql
{
    public class OracleConnection : ISqlConnection, IConnection
    {
        Oracle.ManagedDataAccess.Client.OracleConnection _cxn = null;
        SourceSystem _source = null;
        String _provider;
        bool _isPooled;

        public OracleConnection(SourceSystem source)
        {
            _source = source;
            _provider = "Oracle"; 
            _cxn = new Oracle.ManagedDataAccess.Client.OracleConnection(source.connectionString);
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
                if (_isPooled) // return connection if pooled
                {
                    this.disconnect();
                }
            }
        }

        /// <summary>
        /// Insert a new record and return the new record ID specified by the request's output parameter. MUST BE NAMED: "out_row_id"
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public String insertReturningRowId(IDbCommand request, bool usingTransaction = false)
        {
            try
            {
                request.ExecuteNonQuery();
                return Convert.ToString(Convert.ToDecimal(((Oracle.ManagedDataAccess.Client.OracleParameter)request.Parameters[":out_row_id"]).Value));
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
            _cxn.Open();
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
                return new Oracle.ManagedDataAccess.Client.OracleParameter() { DbType = DbType.String };
            }
            else if (value is DateTime)
            {
                return new Oracle.ManagedDataAccess.Client.OracleParameter() { DbType = DbType.DateTime };
            }
            else if (value is Int16)
            {
                return new Oracle.ManagedDataAccess.Client.OracleParameter() { DbType = DbType.Int16 };
            }
            else if (value is Int32)
            {
                return new Oracle.ManagedDataAccess.Client.OracleParameter() { DbType = DbType.Int32 };
            }
            else if (value is Int64)
            {
                return new Oracle.ManagedDataAccess.Client.OracleParameter() { DbType = DbType.Int64 };
            }
            else if (value is byte[])
            {
                return new Oracle.ManagedDataAccess.Client.OracleParameter() { DbType = DbType.Binary };
            }

            throw new NotImplementedException("These is no parameter type for " + value.GetType().FullName);
        }
    }
}