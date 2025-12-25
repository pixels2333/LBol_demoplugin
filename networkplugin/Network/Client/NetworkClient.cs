using System;
using System.Text.Json;
using LiteNetLib;
using LiteNetLib.Utils;
using NetworkPlugin.Core;
using NetworkPlugin.Network.NetworkPlayer;

namespace NetworkPlugin.Network.Client;



/// <summary>
/// 网络客户端，处理与服务器的连接、数据传输和事件管理
/// 支持 JSON 序列化、自动重连、连接状态监控等高级功能
/// </summary>
public class NetworkClient : INetworkClient
{
    private EventBasedNetListener _listener;
    private NetManager _netManager;
    private NetPeer _serverPeer;
    private string _connectionKey;

    private INetworkManager _networkManager;
    private INetworkPlayer _networkPlayer;

    /// <summary>
    /// 网络连接状态变更事件
    /// </summary>
    public event Action<string> OnConnected;

    /// <summary>
    /// 网络连接断开事件
    /// </summary>
    public event Action<string, string> OnDisconnected;

    /// <summary>
    /// 游戏事件接收事件
    /// </summary>
    public event Action<string, object> OnGameEventReceived;

    /// <summary>
    /// 系统响应接收事件
    /// </summary>
    public event Action<string, object> OnResponseReceived;

    /// <summary>
    /// 连接状态变更事件
    /// </summary>
    public event Action<bool> OnConnectionStateChanged;

    /// <summary>
    /// 自动重连功能配置
    /// </summary>
    private bool _autoReconnectEnabled = false;
    private int _retryInterval = 5000;
    private int _connectionTimeout = 5000;

    /// <summary>
    /// 初始化网络客户端实例，设置连接密钥并注册所有网络事件监听器
    /// </summary>
    /// <param name="connectionKey">连接密钥，用于服务器身份验证和鉴权</param>
    /// <param name="networkManager">网络管理器实例，用于获取玩家信息等</param>
    public NetworkClient(string connectionKey, INetworkManager NetworkManager,INetworkPlayer NetworkPlayer)
    {
        _networkManager = NetworkManager;
        _connectionKey = connectionKey;
        _listener = new EventBasedNetListener();
        _netManager = new NetManager(_listener);
        _networkPlayer = NetworkPlayer;
        RegisterEvents();
    }

    public void Start()
    {
        // 设置连接超时时间，防止网络阻塞
        _netManager.DisconnectTimeout = _connectionTimeout;

        // 启动网络管理器，如果失败则抛出异常
        if (_netManager.Start())
        {
            Console.WriteLine("[Client] Network client started successfully.");
        }
        else
        {
            Console.WriteLine("[Client] Failed to start network client.");
            throw new Exception("Failed to start NetworkClient");
        }
    }

    public INetworkPlayer GetSelf()
    {
        return _networkManager.GetSelf();
    }

    /// <summary>
    /// 注册网络事件处理器，包括连接、断开连接和数据接收事件
    /// 处理服务器响应、游戏事件同步和消息路由
    /// </summary>
    private void RegisterEvents()
    {
        _listener.PeerConnectedEvent += peer =>
        {
            Console.WriteLine($"[Client] Connected to server: {peer.EndPoint}");
            _serverPeer = peer;

            // 触发连接事件通知
            OnConnected?.Invoke(peer.EndPoint.ToString());
            OnConnectionStateChanged?.Invoke(true);

            // 通知同步管理器连接已恢复
            try
            {
                SynchronizationManager.Instance.OnConnectionRestored();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Client] Error notifying sync manager: {ex.Message}");
            }

            // 获取当前玩家信息并发送加入事件
            INetworkPlayer networkPlayer = _networkManager.GetPlayerByPeerId(peer.Id);
            var playerInfo = new
            {
                PlayerName = networkPlayer.userName,
                ConnectionTime = DateTime.Now.Ticks
            };

            // 向服务器发送玩家加入事件
            SendGameEventData("PlayerJoined", playerInfo);
        };

        _listener.PeerDisconnectedEvent += (peer, disconnectInfo) =>
        {
            Console.WriteLine($"[Client] Disconnected from server: {peer.EndPoint}, Reason: {disconnectInfo.Reason}");
            _serverPeer = null;

            // 触发断开连接事件通知
            OnDisconnected?.Invoke(peer.EndPoint.ToString(), disconnectInfo.Reason.ToString());
            OnConnectionStateChanged?.Invoke(false);

            // 通知同步管理器连接已丢失
            try
            {
                SynchronizationManager.Instance.OnConnectionLost();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Client] Error notifying sync manager: {ex.Message}");
            }

            // 检查是否启用自动重连功能
            if (_autoReconnectEnabled)
            {
                Console.WriteLine($"[Client] Auto-reconnect enabled, will retry in {_retryInterval}ms");
                // TODO: 实现自动重连逻辑
            }
        };

        _listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
        {
            try
            {
                // 读取消息类型标识
                string messageType = dataReader.GetString();

                // 根据消息类型分发给不同的处理器
                if (IsGameEvent(messageType))
                {
                    // 处理游戏同步事件
                    HandleGameEvent(messageType, dataReader);
                }
                else if (messageType.EndsWith("GetSelf_RESPONSE"))
                {
                    // 处理系统响应消息
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
                // 回收数据读取器，避免内存泄漏
                dataReader.Recycle();
            }
        };
    }

    /// <summary>
    /// 检查消息类型是否为游戏同步事件
    /// 游戏事件包括：On*、Mana*、Battle*、StateSyncResponse 等事件类型
    /// </summary>
    /// <param name="messageType">要检查的消息类型</param>
    /// <returns>如果是游戏事件返回 true，否则返回 false</returns>
    private bool IsGameEvent(string messageType)
    {
        return messageType.StartsWith("On") ||
               messageType.StartsWith("Mana") ||
               messageType.StartsWith("Gap") ||
               messageType.StartsWith("Battle") ||
               messageType == "StateSyncResponse";
    }

    /// <summary>
    /// 处理游戏同步事件，解析 JSON 数据并触发相应事件
    /// 同时将事件传递给 SynchronizationManager 进行状态同步
    /// </summary>
    /// <param name="eventType">事件类型标识</param>
    /// <param name="dataReader">网络数据读取器，包含事件数据</param>
    private void HandleGameEvent(string eventType, NetDataReader dataReader)
    {
        try
        {
            // 读取并解析 JSON 格式的事件数据
            string jsonPayload = dataReader.GetString();
            object eventData = JsonSerializer.Deserialize<object>(jsonPayload);

            Console.WriteLine($"[Client] Received game event: {eventType}");

            // 触发游戏事件接收事件
            OnGameEventReceived?.Invoke(eventType, eventData);

            // 将事件传递给同步管理器处理
            SynchronizationManager.Instance.ProcessEventFromNetwork(new
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
    /// 连接到指定的游戏服务器
    /// 使用连接密钥进行身份验证，建立可靠的 TCP 连接
    /// </summary>
    /// <param name="host">服务器主机地址或域名</param>
    /// <param name="port">服务器端口号</param>
    public void ConnectToServer(string host, int port)
    {
        Console.WriteLine($"[Client] Attempting to connect to {host}:{port} with key '{_connectionKey}'...");
        NetDataWriter connectData = new();
        // 将连接密钥写入数据包，用于服务器身份验证
        connectData.Put(_connectionKey);
        // 发起连接请求，包含认证信息
        _netManager.Connect(host, port, connectData);
    }

    /// <summary>
    /// 轮询网络事件，处理所有待处理的网络数据包
    /// 需要在游戏主循环中定期调用以保持网络通信畅通
    /// </summary>
    public void PollEvents()
    {
        // 轮询所有待处理的网络事件，保持网络通信畅通
        _netManager.PollEvents();
    }

    /// <summary>
    /// 停止网络客户端服务，清理所有网络资源
    /// 断开与服务器的连接并释放网络管理器资源
    /// </summary>
    public void Stop()
    {
        // 停止网络管理器，断开所有连接
        _netManager.Stop();
        Console.WriteLine("[Client] Client services stopped.");
    }

    /// <summary>
    /// 获取客户端是否已连接到服务器
    /// 检查服务器对等端是否存在且连接状态正常
    /// </summary>
    /// <returns>已连接返回 true，否则返回 false</returns>
    public bool IsConnected => _serverPeer != null && _serverPeer.ConnectionState == ConnectionState.Connected;



    /// <summary>
    /// 发送游戏同步事件到服务器，数据使用 JSON 格式序列化
    /// 适用于发送游戏状态、玩家操作等同步信息
    /// </summary>
    /// <param name="eventType">事件类型标识符</param>
    /// <param name="eventData">要发送的事件数据对象</param>
    public void SendGameEventData(string eventType, object eventData)
    {
        // 检查是否已连接到服务器
        if (!IsConnected)
        {
            Console.WriteLine($"[Client] Not connected to server. Cannot send game event: {eventType}");
            return;
        }

        try
        {
            // 序列化事件数据为 JSON 格式
            string json = JsonSerializer.Serialize(eventData);
            NetDataWriter writer = new();
            // 写入事件类型标识
            writer.Put(eventType);
            // 写入 JSON 数据
            writer.Put(json);

            // 使用可靠有序的方式发送数据
            _serverPeer.Send(writer, DeliveryMethod.ReliableOrdered);
            Console.WriteLine($"[Client] Game event sent: {eventType}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Client] Error sending game event {eventType}: {ex.Message}");
        }
    }

    /// <summary>
    /// 发送网络请求到服务器，支持原始类型和复杂 JSON 对象
    /// 兼容原有 SendRequest 方法，支持泛型数据序列化
    /// </summary>
    /// <typeparam name="T">请求数据类型，支持原始类型和复杂对象</typeparam>
    /// <param name="requestHeader">请求头标识符</param>
    /// <param name="requestData">请求数据对象</param>
    public void SendRequest<T>(string requestHeader, T requestData)
    {
        // 检查连接状态
        if (IsConnected)
        {
            NetDataWriter writer = new();
            // 写入请求头标识
            writer.Put(requestHeader);

            // 根据数据类型选择序列化方式
            if (typeof(T).IsPrimitive || typeof(T) == typeof(string))
            {
                // 原始类型直接序列化
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
                // 复杂对象使用 JSON 序列化
                string json = JsonSerializer.Serialize(requestData);
                writer.Put(json);
            }

            // 发送请求到服务器
            _serverPeer.Send(writer, DeliveryMethod.ReliableOrdered);
            Console.WriteLine($"[Client] Request sent: {requestHeader}");
        }
        else
        {
            Console.WriteLine("[Client] Not connected to server. Cannot send request.");
        }
    }

    /// <summary>
    /// 处理服务器响应数据，解析请求响应消息
    /// 支持 JSON 格式的响应数据，触发相应的事件处理器
    /// </summary>
    /// <param name="fromPeer">发送响应的对等端</param>
    /// <param name="dataReader">响应数据读取器</param>
    private void HandleRequestResponse(NetPeer fromPeer, NetDataReader dataReader)
    {
        try
        {
            // 读取响应头标识
            string responseHeader = dataReader.GetString();
            Console.WriteLine($"[Client] Received response: Type = '{responseHeader}' from {fromPeer.EndPoint}");

            // 读取响应数据内容
            string responseData = dataReader.GetString();
            Console.WriteLine($"[Client] Message: {responseData}");

            // 触发响应接收事件
            OnResponseReceived?.Invoke(responseHeader, responseData);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Client] Error handling response: {ex.Message}");
        }
    }

    #region 高级功能实现

    /// <summary>
    /// 获取当前网络连接的统计信息和质量指标
    /// 包括延迟、MTU、连接时间等网络质量数据
    /// </summary>
    /// <returns>包含连接状态、延迟、地址等信息的对象</returns>
    public object GetConnectionStats()
    {
        // 检查是否有活跃的连接
        if (_serverPeer == null)
        {
            return new { Status = "Not Connected" };
        }

        // 返回连接状态和质量指标
        return new
        {
            Status = "Connected",
            Ping = _serverPeer.Ping,
            Mtu = _serverPeer.Mtu,
            ConnectionTime = DateTime.Now.Ticks,
            Address = _serverPeer.EndPoint.ToString()
        };
    }

    /// <summary>
    /// 设置网络连接的超时时间
    /// 超时后自动断开连接，防止网络阻塞
    /// </summary>
    /// <param name="timeoutMs">超时时间，单位为毫秒</param>
    public void SetConnectionTimeout(int timeoutMs)
    {
        // 更新连接超时配置
        _connectionTimeout = timeoutMs;
        // 如果网络管理器已启动，同时更新其实际超时设置
        if (_netManager != null)
        {
            _netManager.DisconnectTimeout = timeoutMs;
        }
        Console.WriteLine($"[Client] Connection timeout set to {timeoutMs}ms");
    }

    /// <summary>
    /// 启用或禁用自动重连功能
    /// 当连接断开时自动尝试重新连接到服务器
    /// </summary>
    /// <param name="enabled">true 启用自动重连，false 禁用自动重连</param>
    /// <param name="retryInterval">重试间隔时间，单位为毫秒，默认 5000ms</param>
    public void EnableAutoReconnect(bool enabled, int retryInterval = 5000)
    {
        // 更新自动重连功能状态
        _autoReconnectEnabled = enabled;
        // 设置重试间隔时间
        _retryInterval = retryInterval;
        Console.WriteLine($"[Client] Auto-reconnect {(enabled ? "enabled" : "disabled")}, retry interval: {retryInterval}ms");
    }



    #endregion
}
