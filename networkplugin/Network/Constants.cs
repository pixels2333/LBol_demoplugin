namespace NetworkPlugin.Network;

/// <summary>
/// 网络模块常量定义
/// </summary>
public static class NetworkConstants
{
    /// <summary>服务器默认端口</summary>
    public const int DefaultPort = 7777;

    /// <summary>中继服务器默认端口</summary>
    public const int RelayPort = 8888;

    /// <summary>最大连接数</summary>
    public const int MaxConnections = 1000;

    /// <summary>心跳间隔（毫秒）</summary>
    public const int HeartbeatIntervalMs = 5000;

    /// <summary>连接超时时间（毫秒）</summary>
    public const int ConnectTimeoutMs = 300;

    /// <summary>NAT 探测 STUN 服务器端口</summary>
    public const int NatStunPort = 3478;

    /// <summary>NAT 探测 TTL 值</summary>
    public const int NatTtl = 300;

    /// <summary>中继服务器后台线程休眠间隔（毫秒）</summary>
    public const int BgThreadSleepMs = 15;

    /// <summary>最大房间数</summary>
    public const int MaxRoomCount = 100;

    /// <summary>会话清理检查间隔（毫秒）</summary>
    public const int SessionCleanupIntervalMs = 2000;

    /// <summary>重连尝试间隔（毫秒）</summary>
    public const int ReconnectIntervalMs = 5000;

    /// <summary>重连优雅期（秒）</summary>
    public const int ReconnectGracePeriodSeconds = 60;

    /// <summary>默认重连最大尝试次数</summary>
    public const int DefaultMaxReconnectAttempts = 5;

    /// <summary>默认网络超时（秒）</summary>
    public const int DefaultNetworkTimeoutSeconds = 30;
}
