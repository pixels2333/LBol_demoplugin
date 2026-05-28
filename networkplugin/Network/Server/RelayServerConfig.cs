namespace NetworkPlugin.Network.Server;

/// <summary>
/// 中继服务器运行时配置，控制端口、容量、超时及安全策略。
/// </summary>
/// <remarks>
/// 默认值与 <see cref="ServerConstants"/> 保持一致；房主在创建房间时可通过 UI 修改部分参数。
/// </remarks>
public class RelayServerConfig
{
    /// <summary>中继服务器监听端口，默认 <see cref="ServerConstants.DefaultRelayPort"/> (8888)。</summary>
    public int Port { get; set; } = ServerConstants.DefaultRelayPort;

    /// <summary>最大同时连接数，默认 <see cref="ServerConstants.DefaultMaxConnections"/> (1000)。</summary>
    public int MaxConnections { get; set; } = ServerConstants.DefaultMaxConnections;

    /// <summary>最大房间数，默认 <see cref="ServerConstants.MaxRoomCount"/> (100)。</summary>
    public int MaxRooms { get; set; } = ServerConstants.MaxRoomCount;

    /// <summary>每间房最大玩家数，默认 <see cref="ServerConstants.DefaultMaxPlayersPerRoom"/> (4)。</summary>
    public int MaxPlayersPerRoom { get; set; } = ServerConstants.DefaultMaxPlayersPerRoom;

    /// <summary>断开连接超时（秒），默认 <see cref="ServerConstants.DisconnectTimeoutSeconds"/> (30)。</summary>
    public int DisconnectTimeoutSeconds { get; set; } = ServerConstants.DisconnectTimeoutSeconds;

    /// <summary>服务器名称，显示在客户端房间列表中。</summary>
    public string ServerName { get; set; } = "LBoL Relay Server";

    /// <summary>连接密钥，客户端必须在连接数据包中携带以通过鉴权。</summary>
    public string ConnectionKey { get; set; } = "LBoL_Network_Plugin";

    /// <summary>是否启用 NAT 穿透（NAT Punchthrough），用于公网直连场景。</summary>
    public bool EnableNatPunchthrough { get; set; } = true;
}
