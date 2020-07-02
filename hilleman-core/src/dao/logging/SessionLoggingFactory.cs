using com.bitscopic.hilleman.core.domain;
using System;
using System.Configuration;

namespace com.bitscopic.hilleman.core.dao.logging
{
    public class SessionLoggingFactory
    {
        public static ISessionDao getSessionDao()
        {
            if (String.Equals("SQLite", MyConfigurationManager.getValue("LoggingProvider"), StringComparison.CurrentCultureIgnoreCase))
            {
                return new SqliteSessionDao(MyConfigurationManager.getValue("LoggingDbConnectionString"));
            }
            else
            {
                throw new NotImplementedException("SQLite session logging is the only DB currently enabled");
            }
        }
    }
}