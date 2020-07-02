using com.bitscopic.hilleman.core.domain;
using com.bitscopic.hilleman.core.dao.sql;
using com.bitscopic.hilleman.core.utils;
using com.bitscopic.hilleman.core.dao.iface;
using System;

namespace com.bitscopic.hilleman.core.dao.factory
{
    public static class ConfigDaoFactory
    {
        internal static String getDataProviderFromConfig()
        {
            return MyConfigurationManager.getValue("MyConfigDatabaseProvider");
        }

        internal static String getConnectionStringFromConfig()
        {
            return MyConfigurationManager.getValue("MyConfigDatabaseConnectionString");
        }

        internal static SqlConnectionPoolSource getPoolingConfig()
        {
            String poolingEnabledStr = MyConfigurationManager.getValue("MyConfigDatabaseConnectionPoolingEnabled");
            if (String.IsNullOrEmpty(poolingEnabledStr) || !StringUtils.parseBool(poolingEnabledStr))
            {
                return null;
            }
            else
            {
                return new SqlConnectionPoolSource() { poolingEnabled = true };
            }
        }

        internal static SourceSystem getSourceSystemFromConfig()
        {
            SourceSystemType providerType = SourceSystemType.SQLITE;
            String provider = getDataProviderFromConfig();
            if (String.Equals("SQLite", provider, StringComparison.CurrentCultureIgnoreCase))
            {
                providerType = SourceSystemType.SQLITE;
            }
            else if (String.Equals("Oracle", provider, StringComparison.CurrentCultureIgnoreCase))
            {
                providerType = SourceSystemType.ORACLE;
            }
            else
            {
                throw new NotImplementedException("Unable to determine data provider type");
            }

            return new SourceSystem()
            {
                connectionString = getConnectionStringFromConfig(),
                type = providerType,
                poolingConfig = getPoolingConfig()
            };
        }

        public static IMyConfigurationDao getMyConfigurationDao()
        {
            return getMyConfigurationDao(getSourceSystemFromConfig());
        }

        public static IMyConfigurationDao getMyConfigurationDao(SourceSystem source)
        {
            if (source.type == SourceSystemType.SQLITE)
            {
                ISqlConnection sqliteCxn = SqlConnectionFactory.getConnectionBySource(source);
                return new com.bitscopic.hilleman.core.dao.sql.SQLiteMyConfigurationSQLDao(sqliteCxn);
            }
            else if (source.type == SourceSystemType.ORACLE)
            {
                throw new NotImplementedException("An Oracle configuration manager has not yet been implemented");
            }

            throw new NotImplementedException("Unable to determine which config DAO to build!!");
        }



    }
}