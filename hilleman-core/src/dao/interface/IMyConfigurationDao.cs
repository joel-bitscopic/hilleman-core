using System.Collections.Generic;

namespace com.bitscopic.hilleman.core.dao.iface
{
    public interface IMyConfigurationDao
    {
        Dictionary<string, string> loadAllConfigs();

        void updateConfig(string configKey, string configValue, bool validateUpdate = false);
    }
}
