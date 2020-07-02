using com.bitscopic.hilleman.core.dao.iface;
using com.bitscopic.hilleman.core.utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

namespace com.bitscopic.hilleman.core.dao.sql
{
    public abstract class MyConfigurationSQLDao : IMyConfigurationDao
    {
        ISqlConnection _cxn;

        public MyConfigurationSQLDao(ISqlConnection cxn)
        {
            _cxn = cxn;
        }

        public Dictionary<string, string> loadAllConfigs()
        {
            using (IDbCommand cmd = buildLoadAllConfigsCmd())
            {
                using (IDataReader rdr = cmd.ExecuteReader())
                {
                    return toAllConfigs(rdr);
                }
            }
        }

        internal Dictionary<string, string> toAllConfigs(IDataReader rdr)
        {
            Dictionary<String, String> result = new Dictionary<string, string>();

            while (rdr.Read())
            {
                String key = SqlUtils.safeGet(rdr, "CONFIG_KEY");
                String value = SqlUtils.safeGet(rdr, "CONFIG_VALUE");

                if (!result.ContainsKey(key))
                {
                    result.Add(key, value);
                }
            }

            return result;
        }

        internal abstract IDbCommand buildLoadAllConfigsCmd();

        public void updateConfig(string key, string value, bool validateUpdate = false)
        {
            IDbTransaction tx = _cxn.startTransaction();

            try
            {
                IDbCommand deleteCmd = buildRemoveConfigCmd(configKey: key);
                deleteCmd.Transaction = tx;
                if (1 != deleteCmd.ExecuteNonQuery() && validateUpdate)
                {
                    throw new ArgumentException("No configs updated - invalid config key?");
                }

                IDbCommand insertCmd = buildInsertConfigCmd(key, value);
                insertCmd.Transaction = tx;
                if (1 != insertCmd.ExecuteNonQuery())
                {
                    throw new ArgumentException("No configs updated - invalid config?");
                }

                tx.Commit();
            }
            catch (Exception)
            {
                tx.Rollback();
                throw;
            }
            finally
            {

            }
        }

        internal abstract IDbCommand buildRemoveConfigCmd(String configKey);
        internal abstract IDbCommand buildInsertConfigCmd(String configKey, String configValue);
    }

    public class SQLiteMyConfigurationSQLDao : MyConfigurationSQLDao
    {
        SqliteConnection _cxn;

        public SQLiteMyConfigurationSQLDao(ISqlConnection cxn) : base(cxn)
        {
            _cxn = (SqliteConnection)cxn;
        }

        internal override IDbCommand buildLoadAllConfigsCmd()
        {
            IDbCommand cmd = _cxn.buildCommand();
            cmd.CommandText = "SELECT * FROM CONFIGS WHERE ACTIVE=1";
            return cmd;
        }

        internal override IDbCommand buildInsertConfigCmd(string configKey, string configValue)
        {
            IDbCommand cmd = _cxn.buildCommand();
            cmd.CommandText = "INSERT INTO CONFIGS (CONFIG_KEY, CONFIG_VALUE) VALUES(@confKey, @confVal)";

            SQLiteParameter p1 = new SQLiteParameter("confKey");
            p1.Value = configKey;
            cmd.Parameters.Add(p1);

            SQLiteParameter p2 = new SQLiteParameter("confVal");
            p2.Value = configValue;
            cmd.Parameters.Add(p2);

            return cmd;
        }

        internal override IDbCommand buildRemoveConfigCmd(string configKey)
        {
            IDbCommand cmd = _cxn.buildCommand();
            cmd.CommandText = "DELETE FROM CONFIGS WHERE CONFIG_KEY=@confKey";

            SQLiteParameter p1 = new SQLiteParameter("confKey");
            p1.Value = configKey;
            cmd.Parameters.Add(p1);

            return cmd;
        }
    }
}
