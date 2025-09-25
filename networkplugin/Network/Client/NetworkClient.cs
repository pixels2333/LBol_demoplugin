using System;
using LiteNetLib;
using LiteNetLib.Utils;

namespace NetworkPlugin.Network.Client;

public class NetworkClient : INetworkClient
{
    private EventBasedNetListener _listener;
    private NetManager _netManager;
    private NetPeer _serverPeer;
    private string _connectionKey;

    /// <summary>
    /// 初始化 NetworkClient 实例并注册事件监听。
    /// </summary>
    /// <param name="connectionKey">连接密钥，用于服务器鉴权。</param>
    public NetworkClient(string connectionKey)
    {
        _connectionKey = connectionKey; // 保存连接密钥
        _listener = new EventBasedNetListener(); // 创建事件监听器
        _netManager = new NetManager(_listener); // 初始化网络管理器
        RegisterEvents(); // 注册网络事件
    }

    /// <summary>
    /// 注册网络事件，包括连接、断开和数据接收。
    /// </summary>
    private void RegisterEvents()
    {
        _listener.PeerConnectedEvent += peer =>
        {
            Console.WriteLine($"[Client] Connected to server: {peer.EndPoint}");
            _serverPeer = peer; // 保存服务器端点
            // 可选：在这里触发一个事件，通知其他组件连接成功
        };

        _listener.PeerDisconnectedEvent += (peer, disconnectInfo) =>
        {
            Console.WriteLine($"[Client] Disconnected from server: {peer.EndPoint}, Reason: {disconnectInfo.Reason}");
            _serverPeer = null; // 清空服务器端点
            // 可选：在这里触发一个事件，通知其他组件连接断开
        };

        _listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
        {
            try
            {
                string responseType = dataReader.GetString(); // 读取响应类型
                Console.WriteLine($"[Client] Received response from {fromPeer.EndPoint}: Type = '{responseType}'");

                if (responseType.EndsWith("GetSelf_RESPONSE"))
                {
                    // 回退一格，重新读取响应类型
                    // dataReader.Position -= responseType.Length + sizeof(int); // 这里根据实际协议调整
                    HandleRequestResponse(fromPeer, dataReader);
                }
                else
                {
                    Console.WriteLine($"[Client] Unknown response type: {responseType} from {fromPeer.EndPoint}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Client] Error processing data from {fromPeer.EndPoint}: {ex.Message}");
            }
            finally
            {
                dataReader.Recycle(); // 回收数据读取器
            }
        };
    }

    /// <summary>
    /// 启动客户端服务。
    /// </summary>
    public void Start()
    {
        _netManager.Start(); // 启动网络管理器
        Console.WriteLine("[Client] Client services started.");
    }

    /// <summary>
    /// 连接到指定服务器。
    /// </summary>
    /// <param name="host">服务器主机地址。</param>
    /// <param name="port">服务器端口。</param>
    public void ConnectToServer(string host, int port)
    {
        Console.WriteLine($"[Client] Attempting to connect to {host}:{port} with key '{_connectionKey}'...");
        NetDataWriter connectData = new(); // 创建连接数据
        connectData.Put(_connectionKey); // 写入连接密钥
        _netManager.Connect(host, port, connectData); // 发起连接请求
    }

    /// <summary>
    /// 轮询网络事件，处理接收到的数据。
    /// </summary>
    public void PollEvents()
    {
        _netManager.PollEvents(); // 轮询事件
    }

    /// <summary>
    /// 停止客户端服务。
    /// </summary>
    public void Stop()
    {
        _netManager.Stop(); // 停止网络管理器
        Console.WriteLine("[Client] Client services stopped.");
    }

    /// <summary>
    /// 获取客户端是否已连接到服务器。
    /// </summary>
    public bool IsConnected => _serverPeer != null && _serverPeer.ConnectionState == ConnectionState.Connected; // 判断连接状态



    public void SendRequest<T>(string requestHeader, T requestdata)
    {
        if (IsConnected)
        {
            NetDataWriter writer = new();
            writer.Put(requestHeader); // 写入请求类型
            
            switch (requestdata)
            {

                case float f: writer.Put(f); break;
                case double d: writer.Put(d); break;
                case long l: writer.Put(l); break;
                case int i: writer.Put(i); break;
                case string s: writer.Put(s); break;
                case bool b: writer.Put(b); break;
                default: throw new NotSupportedException($"Type {typeof(T)} is not supported by NetDataWriter.Put");
            }

            _serverPeer.Send(writer, DeliveryMethod.ReliableOrdered); // 发送请求到服务器
            Console.WriteLine($"[Client] Request sent: {requestHeader} with data {requestdata}");
        }
        else
        {
            Console.WriteLine("[Client] Not connected to server. Cannot send request.");
        }
    }

    /// <summary>
    /// 处理 SendRequest 响应（通用响应处理）。
    /// </summary>
    private void HandleRequestResponse(NetPeer fromPeer, NetDataReader dataReader)
    {
        object result = null;
        try
        {
            string responseHeader = dataReader.GetString(); // 读取响应类型
            Console.WriteLine($"[Client] Received response: Type = '{responseHeader}' from {fromPeer.EndPoint}");

            

            string responsedata = dataReader.GetString(); // 读取消息内容
            Console.WriteLine($"[Client] Message: {responsedata}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Client] Error handling response: {ex.Message}");
        }
        
    }



}
