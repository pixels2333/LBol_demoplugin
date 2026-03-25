using System;

namespace NetworkPlugin.Network.Client;

/// <summary>
/// 网络客户端接口，定义客户端连接、事件轮询、数据发送等核心操作。
/// </summary>
public interface INetworkClient
{
    /// <summary>启动客户端网络服务。</summary>
    void Start();
    /// <summary>
    /// 连接到指定服务器。
    /// </summary>
    /// <param name="host">服务器主机地址。</param>
    /// <param name="port">服务器端口号。</param>
    void ConnectToServer(string host, int port);
    /// <summary>轮询并处理挂起的网络事件。</summary>
    void PollEvents();
    /// <summary>停止客户端网络服务并断开连接。</summary>
    void Stop();
    /// <summary>客户端是否已连接到服务器。</summary>
    bool IsConnected { get; }

    /// <summary>
    /// 向服务器发送指定类型的请求数据。
    /// </summary>
    /// <typeparam name="T">请求数据类型。</typeparam>
    /// <param name="requestHeader">请求头标识字符串。</param>
    /// <param name="requestdata">请求数据内容。</param>
    void SendRequest<T>(string requestHeader, T requestdata);

    // TODO:如果客户端需要处理服务器的响应并更新UI或模型，可以在这里添加事件或回调
    // 例如：
    // event Action<int, string> ProcessNumberResponseReceived;
}
