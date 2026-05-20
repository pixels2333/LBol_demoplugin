namespace NetworkPlugin.Network.Server;

/// <summary>
/// 中继服务器配置
/// </summary>
public class RelayServerConfig
{
    public int Port { get; set; } = ServerConstants.DefaultRelayPort;
    public int MaxConnections { get; set; } = ServerConstants.DefaultMaxConnections;
    public int MaxRooms { get; set; } = ServerConstants.MaxRoomCount;
    public int MaxPlayersPerRoom { get; set; } = ServerConstants.DefaultMaxPlayersPerRoom;
    public int DisconnectTimeoutSeconds { get; set; } = ServerConstants.DisconnectTimeoutSeconds;
    public string ServerName { get; set; } = "LBoL Relay Server"; // 服务器名称：显示给客户端的服务器标识
    public string ConnectionKey { get; set; } = "LBoL_Network_Plugin"; // 连接密钥：客户端连接时需要验证的安全密钥
    public bool EnableNatPunchthrough { get; set; } = true; // 启用NAT穿透：是否开启NAT穿透功能
}
