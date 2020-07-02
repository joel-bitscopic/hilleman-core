using com.bitscopic.hilleman.core.dao.iface.praedigene;
using System;
using System.Data.SQLite;
using System.Data;

namespace com.bitscopic.hilleman.core.dao.sql
{
    public class SqlUtilityDao : IUtilityDao
    {
        ISqlConnection _cxn;

        public SqlUtilityDao(ISqlConnection cxn)
        {
            _cxn = cxn;
        }

        public IDataReader executeSql(String sql)
        {
            using (SQLiteCommand cmd = (SQLiteCommand)_cxn.buildCommand())
            {
                cmd.CommandText = sql;
                return _cxn.select(cmd);
            }
        }

    }
}
