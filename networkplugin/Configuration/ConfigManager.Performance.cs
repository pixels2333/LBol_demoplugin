using BepInEx;
using BepInEx.Configuration;
using System;

namespace NetworkPlugin.Configuration;

public partial class ConfigManager
{
    /// <summary>
    /// 性能参数配置区域 - 调整队列大小和缓存策略
    /// </summary>
    #region 性能参数

    /// <summary>
    /// 事件队列最大容量
    /// 网络不可用时事件队列的最大条目数量
    /// 超过此容量的新事件会被丢弃
    /// </summary>
    public ConfigEntry<int> MaxQueueSize { get; private set; }

    /// <summary>
    /// 状态缓存存活时间（分钟）
    /// 本地状态缓存的存活时间，超过此时间的缓存会被清理
    /// 默认为5分钟，可以根据需要调整
    /// </summary>
    public ConfigEntry<float> StateCacheExpiryMinutes { get; private set; }

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
        // 在Performance区域下绑定性能相关配置
        MaxQueueSize = configFile.Bind(
            "Performance",
            "MaxQueueSize",
            100,
            "网络不可用时事件队列的最大条目数量，超过此容量的新事件会被丢弃"
        );

        StateCacheExpiryMinutes = configFile.Bind(
            "Performance",
            "StateCacheExpiryMinutes",
            5.0f,
            "本地状态缓存的存活时间（分钟），超过此时间的缓存会被清理"
        );

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
