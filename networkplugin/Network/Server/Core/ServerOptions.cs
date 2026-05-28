// 服务器公共内核配置：统一端口、连接数、鉴权与超时等参数，供 Host/Relay 两种模式复用。
namespace NetworkPlugin.Network.Server.Core;

/// <summary>
/// 服务器公共内核配置
/// 统一端口、连接数、鉴权与超时等参数，供 Host/Relay 两种模式复用
/// </summary>
public sealed class ServerOptions
{
    /// <summary>服务器监听端口</summary>
    public int Port { get; set; }
    /// <summary>最大连接数</summary>
    public int MaxConnections { get; set; } = 32;
    /// <summary>连接密钥，用于客户端鉴权</summary>
    public string ConnectionKey { get; set; } = string.Empty;

    /// <summary>断开连接超时时间（毫秒）</summary>
    public int DisconnectTimeoutMs { get; set; } = 30_000;
    /// <summary>Ping 探测间隔（毫秒）</summary>
    public int PingIntervalMs { get; set; } = 1_000;

    /// <summary>入站消息队列最大容量</summary>
    public int MaxQueueSize { get; set; } = 2_000;
    /// <summary>每次轮询处理的最大消息数</summary>
    public int MaxMessagesPerTick { get; set; } = 512;

    /// <summary>是否使用后台线程运行主循环</summary>
    public bool UseBackgroundThread { get; set; } = false;
    /// <summary>后台线程休眠间隔（毫秒）</summary>
    public int BackgroundThreadSleepMs { get; set; } = 15;
}
