using System;

namespace com.bitscopic.hilleman.core.dao.iface.praedigene
{
    public interface IUtilityDao
    {
        System.Data.IDataReader executeSql(String sql);
    }
}
