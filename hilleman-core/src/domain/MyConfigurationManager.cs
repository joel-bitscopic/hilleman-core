using com.bitscopic.hilleman.core.dao.factory;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace com.bitscopic.hilleman.core.domain
{
    public static class MyConfigurationManager
    {
        // case insensitive config dict
        internal static Dictionary<String, Object> configsByKey = new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase);

        /// <summary>
        /// Clear the current configs and reload
        /// </summary>
        internal static void reloadConfigs()
        {
            configsByKey = new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase);
            loadConfigs();
        }

        internal static void reloadConfigsFromSQLiteDB(String dbFilePath)
        {
            configsByKey = new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase);
            Dictionary<String, String> configsFromConfigDao = ConfigDaoFactory.getMyConfigurationDao(new SourceSystem() { connectionString = "DataSource=" + dbFilePath + ";Version=3;Pooling=True;Max Pool Size=8;", type = SourceSystemType.SQLITE }).loadAllConfigs();
            foreach (String key in configsFromConfigDao.Keys)
            {
                if (!configsByKey.ContainsKey(key))
                {
                    configsByKey.TryAdd(key, configsFromConfigDao[key]);
                }
            }
        }


        internal static void loadConfigs()
        {
            IConfiguration testSettingsConfig = new ConfigurationBuilder()
                .AddJsonFile("testsettings.json", optional: true, reloadOnChange: true)
                .Build();

            IConfiguration appSettingsConfig = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            IEnumerable<KeyValuePair<String, String>> testKeysAndVals = testSettingsConfig.AsEnumerable();
            IEnumerable<KeyValuePair<String, String>> appKeysAndVals = appSettingsConfig.AsEnumerable();

            IEnumerator<KeyValuePair<String, String>> testEnum = testKeysAndVals.GetEnumerator();
            while (testEnum.MoveNext())
            {
                configsByKey.TryAdd(testEnum.Current.Key, testEnum.Current.Value);
            }

            IEnumerator<KeyValuePair<String, String>> appEnum = appKeysAndVals.GetEnumerator();
            while (appEnum.MoveNext())
            {
                configsByKey.TryAdd(appEnum.Current.Key, appEnum.Current.Value);
            }

            if (configsByKey.ContainsKey("MyConfigDatabaseConnectionString") && configsByKey.ContainsKey("MyConfigDatabaseProvider"))
            {
                Dictionary<String, String> configsFromConfigDao = ConfigDaoFactory.getMyConfigurationDao().loadAllConfigs();
                foreach (String key in configsFromConfigDao.Keys)
                {
                    if (!configsByKey.ContainsKey(key))
                    {
                        configsByKey.TryAdd(key, configsFromConfigDao[key]);
                    }
                }
            }
        }

        public static String getValue(String configKey)
        {
            if (configsByKey.Count == 0)
            {
                loadConfigs();
            }

            if (configsByKey.ContainsKey(configKey))
            {
                return (String)configsByKey[configKey];
            }
            else
            {
                return String.Empty;
            }
        }

        public static void setValue(string key, string value)
        {
            if (configsByKey == null || configsByKey.Count == 0)
            {
                reloadConfigs();
            }

            if (configsByKey.ContainsKey(key))
            {
                configsByKey[key] = value;
            }
            else
            {
                configsByKey.Add(key, value);
            }
        }
    }
}
