using BepInEx.Configuration;

namespace NetworkPlugin.Configuration;

public partial class ConfigManager
{
        public ConfigManager(ConfigFile configFile)
    {

        BindFeatureToggles(configFile);

        BindUiSettings(configFile);

        BindPerformanceSettings(configFile);

        BindNetworkSettings(configFile);

        BindSyncConfiguration(configFile);

        BindGameBalanceSettings(configFile);
    }
}
