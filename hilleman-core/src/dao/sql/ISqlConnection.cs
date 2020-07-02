using com.bitscopic.hilleman.core.domain;
using System;
using System.Data;

namespace com.bitscopic.hilleman.core.dao.sql
{
    public interface ISqlConnection : IDisposable
    {
        IDbCommand buildCommand();

        IDataReader select(IDbCommand request);

        DataTable selectToTable(IDbCommand request);

        Int32 insertUpdateDelete(IDbCommand request);

        Int32 insertUpdateDelete(IDbCommand request, IDbTransaction tx);

        object executeScalar(IDbCommand request);

        IDbTransaction startTransaction();

        void commitTransaction(IDbTransaction tx);

        void rollbackTransaction(IDbTransaction tx);

        String getProvider();

        SourceSystem getSource();

        String insertReturningRowId(IDbCommand request, bool usingTransaction = false);

        /// <summary>
        /// Build a IDbDataParameter with the appropriate Parameter Type for an object (e.g. for SQLite, a DateTime value -> System.Data.SQLite.SQLiteParameter())
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        IDbDataParameter getParameterForObject(object value);
    }
}