namespace NetworkPlugin.Network.Server;

/// <summary>
/// 中继服务器配置
/// </summary>
public class RelayServerConfig
{
    /// <summary>中继服务器监听端口</summary>
    public int Port { get; set; } = ServerConstants.DefaultRelayPort;
    /// <summary>最大连接数</summary>
    public int MaxConnections { get; set; } = ServerConstants.DefaultMaxConnections;
    /// <summary>最大房间数</summary>
    public int MaxRooms { get; set; } = ServerConstants.MaxRoomCount;
    /// <summary>每间房最大玩家数</summary>
    public int MaxPlayersPerRoom { get; set; } = ServerConstants.DefaultMaxPlayersPerRoom;
    /// <summary>断开连接超时时间（秒）</summary>
    public int DisconnectTimeoutSeconds { get; set; } = ServerConstants.DisconnectTimeoutSeconds;
    /// <summary>服务器名称：显示给客户端的服务器标识</summary>
    public string ServerName { get; set; } = "LBoL Relay Server";
    /// <summary>连接密钥：客户端连接时需要验证的安全密钥</summary>
    public string ConnectionKey { get; set; } = "LBoL_Network_Plugin";
    /// <summary>启用NAT穿透：是否开启NAT穿透功能</summary>
    public bool EnableNatPunchthrough { get; set; } = true;
}
