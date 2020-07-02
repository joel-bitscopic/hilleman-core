using com.bitscopic.hilleman.core.dao.sql.sqlite;
using System;

namespace com.bitscopic.hilleman.core.dao.sql
{
    public class SqlConnectionFactory
    {
        public static com.bitscopic.hilleman.core.dao.sql.ISqlConnection getConnectionBySource(com.bitscopic.hilleman.core.domain.SourceSystem ss)
        {
            if (ss.type == domain.SourceSystemType.SQLITE)
            {
                return new com.bitscopic.hilleman.core.dao.sql.SqliteConnection(ss);
            }
            else if (ss.type == domain.SourceSystemType.ORACLE)
            {
                return new com.bitscopic.hilleman.core.dao.sql.OracleConnection(ss);
            }
            else
            {
                throw new ArgumentException(Enum.GetName(typeof(domain.SourceSystemType), ss.type) + " is not currently a valid SQL source system type");
            }
        }
    }
}