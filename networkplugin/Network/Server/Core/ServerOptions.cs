// 服务器公共内核配置：统一端口、连接数、鉴权与超时等参数，供 Host/Relay 两种模式复用。
namespace NetworkPlugin.Network.Server.Core;

public sealed class ServerOptions
{
    public int Port { get; set; }
    public int MaxConnections { get; set; } = 32;
    public string ConnectionKey { get; set; } = string.Empty;

    public int DisconnectTimeoutMs { get; set; } = 30_000;
    public int PingIntervalMs { get; set; } = 1_000;

    public int MaxQueueSize { get; set; } = 2_000;
    public int MaxMessagesPerTick { get; set; } = 512;

    public bool UseBackgroundThread { get; set; } = false;
    public int BackgroundThreadSleepMs { get; set; } = 15;
}

