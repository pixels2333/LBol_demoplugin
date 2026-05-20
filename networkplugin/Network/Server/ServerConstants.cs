namespace NetworkPlugin.Network.Server;

/// <summary>
/// 服务端模块常量定义
/// </summary>
public static class ServerConstants
{
    /// <summary>会话清理检查间隔（毫秒）</summary>
    public const int SessionCleanupIntervalMs = 2000;

    /// <summary>后台线程休眠间隔（毫秒）</summary>
    public const int BgThreadSleepMs = 15;

    /// <summary>最大房间数</summary>
    public const int MaxRoomCount = 100;

    /// <summary>默认服务器端口</summary>
    public const int DefaultPort = 7777;

    /// <summary>默认中继服务器端口</summary>
    public const int DefaultRelayPort = 8888;

    /// <summary>默认最大连接数</summary>
    public const int DefaultMaxConnections = 1000;

    /// <summary>心跳超时（秒）</summary>
    public const int HeartbeatTimeoutSeconds = 30;

    /// <summary>每间房默认最大玩家数</summary>
    public const int DefaultMaxPlayersPerRoom = 4;

    /// <summary>断开连接超时（秒）</summary>
    public const int DisconnectTimeoutSeconds = 30;
}
