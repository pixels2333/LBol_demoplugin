using System;

namespace NetworkPlugin.Network.Client;

public interface INetworkClient
{
    void Start();
    void ConnectToServer(string host, int port);
    void PollEvents();
    void Stop();
    bool IsConnected { get; }

    void SendRequest<T>(string requestHeader, T requestdata);

    // TODO:如果客户端需要处理服务器的响应并更新UI或模型，可以在这里添加事件或回调
    // 例如：
    // event Action<int, string> ProcessNumberResponseReceived;
}
