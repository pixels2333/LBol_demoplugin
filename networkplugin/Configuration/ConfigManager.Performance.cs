using BepInEx.Configuration;

namespace NetworkPlugin.Configuration;

public partial class ConfigManager
{
    /// <summary>
    /// 性能参数配置区域 - 调整队列大小和缓存策略
    /// </summary>
    #region 性能参数

    // MaxQueueSize / StateCacheExpiryMinutes 的声明和绑定统一在 Configuration/ConfigManager.Sync.cs 的 Sync.Performance 区域。
    // 此处不再重复 Bind()，避免覆盖覆盖 Sync.Performance 区域的配置值。

    /// <summary>
    /// 网络超时时间（秒）
    /// 网络操作的超时时间，超过此时间未响应则判定为超时
    /// 默认为30秒
    /// </summary>
    public ConfigEntry<int> NetworkTimeoutSeconds { get; private set; }

    /// <summary>
    /// 重连尝试次数
    /// 网络断开后的最大重连尝试次数
    /// 超过此次数后停止自动重连
    /// </summary>
    public ConfigEntry<int> MaxReconnectAttempts { get; private set; }

    #endregion

    /// <summary>
    /// 绑定性能参数配置
    /// </summary>
    private void BindPerformanceSettings(ConfigFile configFile)
    {
        // MaxQueueSize / StateCacheExpiryMinutes 已在 ConfigManager.Sync.cs 的 Sync.Performance 区域绑定，此处不重复。

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
