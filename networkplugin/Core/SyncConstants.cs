using System;

namespace NetworkPlugin.Core;

/// <summary>
/// 同步模块常量定义
/// </summary>
public static class SyncConstants
{
    /// <summary>事件队列默认最大容量</summary>
    public const int DefaultMaxQueueSize = 100;

    /// <summary>状态缓存默认过期时间（分钟）</summary>
    public const double DefaultCacheExpiryMinutes = 5.0;

    /// <summary>远程事件缓冲区超时（秒）</summary>
    public const double EventBufferTimeoutSeconds = 30.0;

    /// <summary>FullSync 请求节流间隔（秒）</summary>
    public const double FullSyncThrottleIntervalSeconds = 2.0;

    /// <summary>时间戳允许的未来偏差（秒）</summary>
    public const int TimestampMaxFutureSeconds = 5;

    /// <summary>时间戳允许的历史偏差（小时）</summary>
    public const int TimestampMaxPastHours = 1;

    /// <summary>事件队列默认容量</summary>
    public const int DefaultQueueCapacity = 100;

    /// <summary>EventBufferManager Key 冲突处理窗口（tick）</summary>
    public const long KeyWindowTicks = TimeSpan.TicksPerSecond * 5;
}
