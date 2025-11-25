using System;
using System.Text.Json;
using LiteNetLib;
using LiteNetLib.Utils;
using NetworkPlugin.Network.NetworkPlayer;
using NetworkPlugin.Core;
using NetworkPlugin.Events;

namespace NetworkPlugin.Network.Client;

public class NetworkClient : INetworkClient
{
    private EventBasedNetListener _listener;
    private NetManager _netManager;
    private NetPeer _serverPeer;
    private string _connectionKey;

    private INetworkManager _networkManager;

    /// <summary>
    /// 初始化 NetworkClient 实例并注册事件监听。
    /// </summary>
    /// <param name="connectionKey">连接密钥，用于服务器鉴权。</param>
    public NetworkClient(string connectionKey, INetworkManager NetworkManager)
    {
        _networkManager = NetworkManager;
        _connectionKey = connectionKey;
        _listener = new EventBasedNetListener();
        _netManager = new NetManager(_listener);
        RegisterEvents();
    }

    /// <summary>
    /// 注册网络事件，包括连接、断开和数据接收。
    /// </summary>
    private void RegisterEvents()
    {
        _listener.PeerConnectedEvent += peer =>
        {
            Console.WriteLine($"[Client] Connected to server: {peer.EndPoint}");
            _serverPeer = peer;

            // 通知SynchronizationManager连接已建立
            try
            {
                SynchronizationManager.Instance.OnConnectionRestored();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Client] Error notifying sync manager: {ex.Message}");
            }

            // 客户端在连接时向服务器上传自己的数据
            INetworkPlayer networkPlayer = _networkManager.GetPlayer("username");

            // 发送玩家信息
            var playerInfo = new
            {
                PlayerId = networkPlayer.Id,
                PlayerName = networkPlayer.Name,
                ConnectionTime = DateTime.Now.Ticks
            };

            SendGameEvent("PlayerJoined", playerInfo);
        };

        _listener.PeerDisconnectedEvent += (peer, disconnectInfo) =>
        {
            Console.WriteLine($"[Client] Disconnected from server: {peer.EndPoint}, Reason: {disconnectInfo.Reason}");
            _serverPeer = null;

            // TODO: 可以在这里触发重连逻辑
        };

        _listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
        {
            try
            {
                string messageType = dataReader.GetString();

                // 处理游戏同步事件
                if (IsGameEvent(messageType))
                {
                    HandleGameEvent(messageType, dataReader);
                }
                // 处理系统响应
                else if (messageType.EndsWith("GetSelf_RESPONSE"))
                {
                    HandleRequestResponse(fromPeer, dataReader);
                }
                else
                {
                    Console.WriteLine($"[Client] Unknown message type: {messageType} from {fromPeer.EndPoint}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Client] Error processing data from {fromPeer.EndPoint}: {ex.Message}");
            }
            finally
            {
                dataReader.Recycle();
            }
        };
    }

    /// <summary>
    /// 检查是否为游戏事件消息
    /// </summary>
    private bool IsGameEvent(string messageType)
    {
        return messageType.StartsWith("On") ||
               messageType.StartsWith("Mana") ||
               messageType.StartsWith("Gap") ||
               messageType.StartsWith("Battle") ||
               messageType == "StateSyncResponse";
    }

    /// <summary>
    /// 处理游戏同步事件
    /// </summary>
    private void HandleGameEvent(string eventType, NetDataReader dataReader)
    {
        try
        {
            string jsonPayload = dataReader.GetString();
            var eventData = JsonSerializer.Deserialize<object>(jsonPayload);

            Console.WriteLine($"[Client] Received game event: {eventType}");

            // 交给SynchronizationManager处理
            SynchronizationManager.Instance.ProcessNetworkEvent(new
            {
                EventType = eventType,
                Payload = eventData,
                Timestamp = DateTime.Now.Ticks
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Client] Error handling game event: {ex.Message}");
        }
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



    /// <summary>
    /// 发送游戏同步事件（JSON格式）
    /// </summary>
    /// <param name="eventType">事件类型</param>
    /// <param name="eventData">事件数据</param>
    public void SendGameEvent(string eventType, object eventData)
    {
        if (!IsConnected)
        {
            Console.WriteLine($"[Client] Not connected to server. Cannot send game event: {eventType}");
            return;
        }

        try
        {
            var json = JsonSerializer.Serialize(eventData);
            NetDataWriter writer = new NetDataWriter();
            writer.Put(eventType);
            writer.Put(json);

            _serverPeer.Send(writer, DeliveryMethod.ReliableOrdered);
            Console.WriteLine($"[Client] Game event sent: {eventType}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Client] Error sending game event {eventType}: {ex.Message}");
        }
    }

    /// <summary>
    /// 兼容原有的SendRequest方法，但现在支持JSON对象
    /// </summary>
    public void SendRequest<T>(string requestHeader, T requestData)
    {
        if (IsConnected)
        {
            NetDataWriter writer = new NetDataWriter();
            writer.Put(requestHeader);

            // 支持JSON序列化复杂对象
            if (typeof(T).IsPrimitive || typeof(T) == typeof(string))
            {
                switch (requestData)
                {
                    case float f: writer.Put(f); break;
                    case double d: writer.Put(d); break;
                    case long l: writer.Put(l); break;
                    case int i: writer.Put(i); break;
                    case string s: writer.Put(s); break;
                    case bool b: writer.Put(b); break;
                    default: throw new NotSupportedException($"Type {typeof(T)} is not supported by NetDataWriter.Put");
                }
            }
            else
            {
                // 复杂对象使用JSON序列化
                var json = JsonSerializer.Serialize(requestData);
                writer.Put(json);
            }

            _serverPeer.Send(writer, DeliveryMethod.ReliableOrdered);
            Console.WriteLine($"[Client] Request sent: {requestHeader}");
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
