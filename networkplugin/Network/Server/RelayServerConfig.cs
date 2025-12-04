namespace NetworkPlugin.Network.Server;

/// <summary>
/// 中继服务器配置
/// </summary>
public class RelayServerConfig
{
    public int Port { get; set; } = 8888; // 服务器监听端口：默认8888端口
    public int MaxConnections { get; set; } = 1000; // 最大连接数：服务器最多支持的客户端连接数
    public int MaxRooms { get; set; } = 100; // 最大房间数：服务器最多支持的游戏房间数量
    public int MaxPlayersPerRoom { get; set; } = 4; // 每间房最大玩家数：单个房间支持的最大玩家数量
    public int DisconnectTimeoutSeconds { get; set; } = 30; // 断开连接超时：连接断开检测的超时时间
    public string ServerName { get; set; } = "LBoL Relay Server"; // 服务器名称：显示给客户端的服务器标识
    public string ConnectionKey { get; set; } = "LBoL_Network_Plugin"; // 连接密钥：客户端连接时需要验证的安全密钥
    public bool EnableNatPunchthrough { get; set; } = true; // 启用NAT穿透：是否开启NAT穿透功能
}
