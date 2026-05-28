namespace NetworkPlugin.Network.Server;

/// <summary>
/// 服务端模块常量定义，集中管理所有服务器侧配置项的默认值。
/// </summary>
/// <remarks>
/// 这些常量是服务器行为的“物理约束”：端口、容量、超时阈值。
/// 需要在运行时覆盖的参数（如端口、密钥）应通过 <see cref="RelayServerConfig"/> 传入。
/// </remarks>
public static class ServerConstants
{
    /// <summary>会话清理检查间隔（毫秒），后台线程按此周期扫描过期断线会话。</summary>
    public const int SessionCleanupIntervalMs = 2000;

    /// <summary>后台线程休眠间隔（毫秒），控制主循环 CPU 占用。</summary>
    public const int BgThreadSleepMs = 15;

    /// <summary>最大房间数上限。</summary>
    public const int MaxRoomCount = 100;

    /// <summary>默认游戏服务器端口（直连房主模式）。</summary>
    public const int DefaultPort = 7777;

    /// <summary>默认中继服务器端口（公网撮合模式）。</summary>
    public const int DefaultRelayPort = 8888;

    /// <summary>默认最大同时连接数。</summary>
    public const int DefaultMaxConnections = 1000;

    /// <summary>心跳超时（秒），超过此时间未收到心跳即视为断连。</summary>
    public const int HeartbeatTimeoutSeconds = 30;

    /// <summary>每间房默认最大玩家数。</summary>
    public const int DefaultMaxPlayersPerRoom = 4;

    /// <summary>断开连接超时（秒），LiteNetLib 底层断连判定阈值。</summary>
    public const int DisconnectTimeoutSeconds = 30;
}
