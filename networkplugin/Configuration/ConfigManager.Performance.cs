using BepInEx.Configuration;

namespace NetworkPlugin.Configuration;

public partial class ConfigManager
{
        #region 性能参数

        public ConfigEntry<int> NetworkTimeoutSeconds { get; private set; }

        public ConfigEntry<int> MaxReconnectAttempts { get; private set; }

    #endregion

        private void BindPerformanceSettings(ConfigFile configFile)
    {

        NetworkTimeoutSeconds = configFile.Bind(
            "Performance",
            "NetworkTimeoutSeconds",
            30,
            "网络操作的超时时间（秒），超过此时间未响应则判定为超时"
        );

        MaxReconnectAttempts = configFile.Bind(
            "Performance",
            "MaxReconnectAttempts",
            5,
            "网络断开后的最大重连尝试次数，超过此次数后停止自动重连"
        );
    }
}
