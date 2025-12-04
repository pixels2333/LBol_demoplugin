

//TODO:这个地方用了日志系统和依赖注入,如果使用了分离服务器,需要修改日志系统和依赖注入
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using BepInEx.Logging;
using LiteNetLib;
using LiteNetLib.Utils;

namespace NetworkPlugin.Network.Server;

/// <summary>
/// LiteNetLib网络服务器类
/// 负责处理客户端连接、消息转发、玩家会话管理和游戏同步
/// </summary>
public class NetworkServer
{
    #region 私有字段

    /// <summary>
    /// LiteNetLib事件监听器
    /// </summary>
    private EventBasedNetListener _listener;

    /// <summary>
    /// LiteNetLib网络管理器
    /// </summary>
    private NetManager _netManager;

    /// <summary>
    /// 服务器监听端口
    /// </summary>
    private int _port;

    /// <summary>
    /// 最大连接数
    /// </summary>
    private int _maxConnections;

    /// <summary>
    /// 连接密钥
    /// </summary>
    private string _connectionKey;

    /// <summary>
    /// BepInEx日志源
    /// </summary>
    private readonly ManualLogSource _logger;

    /// <summary>
    /// 管理玩家会话的字典，键为Peer ID
    /// </summary>
    private readonly Dictionary<int, PlayerSession> _playerSessions = [];

    #endregion

    #region 公共事件

    /// <summary>
    /// 游戏事件处理委托
    /// </summary>
    /// <param name="eventType">事件类型</param>
    /// <param name="eventData">事件数据</param>
    /// <param name="sender">发送者会话</param>
    public delegate void GameEventHandler(string eventType, object eventData, PlayerSession sender);

    /// <summary>
    /// 游戏事件接收事件
    /// 当收到客户端的游戏事件时触发
    /// </summary>
    public event GameEventHandler OnGameEventReceived;

    #endregion

    #region 构造函数

    /// <summary>
    /// 初始化网络服务器
    /// </summary>
    /// <param name="port">监听端口</param>
    /// <param name="maxConnections">最大连接数</param>
    /// <param name="connectionKey">连接密钥</param>
    /// <param name="logger">日志记录器</param>
    public NetworkServer(int port, int maxConnections, string connectionKey, ManualLogSource logger)
    {
        _port = port;
        _maxConnections = maxConnections;
        _connectionKey = connectionKey;
        _listener = new EventBasedNetListener();
        _logger = logger;
        _netManager = new NetManager(_listener);

        // 注册网络事件处理器
        RegisterEvents();
    }

    #endregion

    #region 事件注册

    /// <summary>
    /// 注册LiteNetLib网络事件处理器
    /// 包括连接请求、连接建立、连接断开和消息接收事件
    /// </summary>
    private void RegisterEvents()
    {
        // 处理连接请求事件
        _listener.ConnectionRequestEvent += request =>
        {
            // 检查是否达到最大连接数
            if (_netManager.PeersCount < _maxConnections)
            {
                // 注意：在实际应用中，检查密钥应该更安全，这里只是简单比较
                if (request.Data.GetString(_connectionKey.Length) == _connectionKey)
                {
                    request.AcceptIfKey(_connectionKey);
                    Console.WriteLine($"[Server] Accepted connection from {request.RemoteEndPoint}");
                    _logger?.LogInfo($"[Server] Accepted connection from {request.RemoteEndPoint}");
                }
                else
                {
                    request.Reject();
                    Console.WriteLine($"[Server] Rejected connection from {request.RemoteEndPoint} due to invalid key.");
                    _logger?.LogWarning($"[Server] Rejected connection from {request.RemoteEndPoint} due to invalid key.");
                }
            }
            else
            {
                request.Reject();
                Console.WriteLine($"[Server] Rejected connection from {request.RemoteEndPoint}: Max connections reached.");
                _logger?.LogWarning($"[Server] Rejected connection from {request.RemoteEndPoint}: Max connections reached.");
            }
        };

        // 处理客户端连接成功事件
        _listener.PeerConnectedEvent += peer =>
        {
            Console.WriteLine($"[Server] Client connected: {peer.EndPoint}");
            _logger?.LogInfo($"[Server] Client connected: {peer.EndPoint}");

            // 创建玩家会话
            PlayerSession session = new PlayerSession
            {
                Peer = peer,
                PlayerId = $"Player_{peer.Id}",
                ConnectedAt = DateTime.UtcNow,
                LastHeartbeat = DateTime.UtcNow,
                LastMessageAt = DateTime.UtcNow,
                IsConnected = true,
                IsHost = _playerSessions.Count == 0 // 第一个连接的玩家成为房主
            };

            _playerSessions[peer.Id] = session;

            // 通知其他玩家有新玩家加入
            BroadcastPlayerList();

            // 发送欢迎消息给新玩家
            SendWelcomeMessage(peer, session);
        };

        // 处理客户端断开连接事件
        _listener.PeerDisconnectedEvent += (peer, disconnectInfo) =>
        {
            Console.WriteLine($"[Server] Client disconnected: {peer.EndPoint}, Reason: {disconnectInfo.Reason}");
            _logger?.LogInfo($"[Server] Client disconnected: {peer.EndPoint}, Reason: {disconnectInfo.Reason}");

            if (_playerSessions.TryGetValue(peer.Id, out var session))
            {
                _playerSessions.Remove(peer.Id);

                // 如果房主离开，指定新房主
                if (session.IsHost && _playerSessions.Count > 0)
                {
                    var newHost = _playerSessions.Values.First();
                    newHost.IsHost = true;

                    BroadcastMessage("HostChanged", new { NewHostId = newHost.PlayerId });
                    Console.WriteLine($"[Server] Host changed to {newHost.PlayerId}");
                }

                // 通知其他玩家有人离开
                BroadcastPlayerList();
            }
        };

        // 处理网络消息接收事件
        _listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
        {
            try
            {
                string messageType = dataReader.GetString();

                // 处理游戏同步事件
                if (IsGameEvent(messageType))
                {
                    HandleGameEvent(fromPeer, messageType, dataReader);
                }
                // 处理其他系统消息
                else
                {
                    HandleSystemMessage(fromPeer, messageType, dataReader);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Server] Error processing data from {fromPeer.EndPoint}: {ex.Message}");
                _logger?.LogError($"[Server] Error processing data from {fromPeer.EndPoint}: {ex.Message}");
            }
            finally
            {
                dataReader.Recycle();
            }
        };
    }

    private void HandleSystemMessage(NetPeer fromPeer, string messageType, NetPacketReader dataReader)
    {
        //TODO:未实现
        throw new NotImplementedException();

    }


    #endregion

    #region 消息处理

    /// <summary>
    /// 检查消息类型是否为游戏事件消息
    /// </summary>
    /// <param name="messageType">消息类型字符串</param>
    /// <returns>如果是游戏事件消息返回true，否则返回false</returns>
    private bool IsGameEvent(string messageType)
    {
        return messageType.StartsWith("On") ||
               messageType.StartsWith("Mana") ||
               messageType.StartsWith("Gap") ||
               messageType.StartsWith("Battle") ||
               messageType == "StateSyncRequest";
    }

    /// <summary>
    /// 处理游戏同步事件
    /// 接收客户端发送的游戏事件，更新会话状态，并广播给其他玩家
    /// </summary>
    /// <param name="fromPeer">发送事件的网络对等体</param>
    /// <param name="eventType">事件类型</param>
    /// <param name="dataReader">数据读取器</param>
    private void HandleGameEvent(NetPeer fromPeer, string eventType, NetDataReader dataReader)
    {
        try
        {
            // 验证发送者是否为已知玩家
            if (!_playerSessions.TryGetValue(fromPeer.Id, out var session))
            {
                Console.WriteLine($"[Server] Received game event from unknown peer: {fromPeer.EndPoint}");
                return;
            }

            string jsonPayload = dataReader.GetString();
            var eventData = JsonSerializer.Deserialize<object>(jsonPayload);

            Console.WriteLine($"[Server] Received game event: {eventType} from {session.PlayerId}");

            // 更新会话活动时间
            session.UpdateMessageTime();

            // 触发游戏事件处理器，通知上层应用
            OnGameEventReceived?.Invoke(eventType, eventData, session);

            // 广播游戏事件给其他玩家（除了发送者）
            BroadcastGameEvent(eventType, eventData, fromPeer.Id);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Server] Error handling game event: {ex.Message}");
            _logger?.LogError($"[Server] Error handling game event: {ex.Message}");
        }
    }

    /// <summary>
    /// 处理玩家加入消息
    /// 更新玩家信息并广播给其他玩家
    /// </summary>
    /// <param name="fromPeer">发送消息的网络对等体</param>
    /// <param name="dataReader">数据读取器</param>
    private void HandlePlayerJoined(NetPeer fromPeer, NetDataReader dataReader)
    {
        try
        {
            string jsonPayload = dataReader.GetString();
            var playerInfo = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonPayload);

            if (_playerSessions.TryGetValue(fromPeer.Id, out var session))
            {
                // 提取玩家名称
                if (playerInfo.TryGetValue("PlayerName", out var nameObj) && nameObj != null)
                {
                    session.PlayerName = nameObj.ToString();
                }

                Console.WriteLine($"[Server] Player {session.PlayerName} ({session.PlayerId}) joined the game");

                // 广播玩家加入消息给其他玩家
                BroadcastMessage("PlayerJoined", new
                {
                    PlayerId = session.PlayerId,
                    PlayerName = session.PlayerName,
                    IsHost = session.IsHost
                }, excludePeerId: fromPeer.Id);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Server] Error handling PlayerJoined: {ex.Message}");
            _logger?.LogError($"[Server] Error handling PlayerJoined: {ex.Message}");
        }
    }

    /// <summary>
    /// 处理心跳包
    /// 更新玩家会话的心跳时间并发送响应
    /// </summary>
    /// <param name="fromPeer">发送心跳的网络对等体</param>
    private void HandleHeartbeat(NetPeer fromPeer)
    {
        if (_playerSessions.TryGetValue(fromPeer.Id, out var session))
        {
            // 更新心跳时间
            session.UpdateHeartbeat();

            // 发送心跳响应
            SendMessage(fromPeer, "HeartbeatResponse", new
            {
                Timestamp = DateTime.UtcNow.Ticks,
                Ping = session.Ping
            });
        }
    }

    /// <summary>
    /// 处理获取自身信息请求
    /// 向客户端发送其会话信息
    /// </summary>
    /// <param name="fromPeer">发送请求的网络对等体</param>
    /// <param name="dataReader">数据读取器（未使用）</param>
    private void HandleGetSelfRequest(NetPeer fromPeer, NetDataReader dataReader)
    {
        if (_playerSessions.TryGetValue(fromPeer.Id, out var session))
        {
            var responseData = new
            {
                PlayerId = session.PlayerId,
                PlayerName = session.PlayerName,
                IsHost = session.IsHost,
                ConnectedAt = session.ConnectedAt.Ticks
            };

            SendMessage(fromPeer, "GetSelf_RESPONSE", responseData);
        }
    }

    #endregion

    #region 消息发送与广播

    /// <summary>
    /// 广播游戏事件给所有玩家（除了指定的发送者）
    /// </summary>
    /// <param name="eventType">事件类型</param>
    /// <param name="eventData">事件数据</param>
    /// <param name="excludePeerId">要排除的Peer ID</param>
    private void BroadcastGameEvent(string eventType, object eventData, int excludePeerId)
    {
        var json = JsonSerializer.Serialize(eventData);

        foreach (var kvp in _playerSessions)
        {
            // 排除发送者，只转发给其他玩家
            if (kvp.Key != excludePeerId && kvp.Value.IsConnected)
            {
                try
                {
                    NetDataWriter writer = new NetDataWriter();
                    writer.Put(eventType);
                    writer.Put(json);
                    kvp.Value.Peer.Send(writer, DeliveryMethod.ReliableOrdered);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Server] Error broadcasting game event to {kvp.Value.PlayerId}: {ex.Message}");
                    _logger?.LogError($"[Server] Error broadcasting game event to {kvp.Value.PlayerId}: {ex.Message}");
                }
            }
        }
    }

    /// <summary>
    /// 广播系统消息给所有玩家
    /// </summary>
    /// <param name="messageType">消息类型</param>
    /// <param name="data">消息数据</param>
    /// <param name="excludePeerId">可选的要排除的Peer ID</param>
    private void BroadcastMessage(string messageType, object data, int? excludePeerId = null)
    {
        var json = JsonSerializer.Serialize(data);

        foreach (var kvp in _playerSessions)
        {
            // 如果指定了要排除的Peer ID，则跳过
            if (excludePeerId.HasValue && kvp.Key == excludePeerId.Value)
            {
                continue;
            }

            // 只发送给已连接的玩家
            if (kvp.Value.IsConnected)
            {
                SendMessage(kvp.Value.Peer, messageType, data);
            }
        }
    }

    /// <summary>
    /// 发送消息给特定玩家
    /// </summary>
    /// <param name="peer">目标网络对等体</param>
    /// <param name="messageType">消息类型</param>
    /// <param name="data">消息数据</param>
    private void SendMessage(NetPeer peer, string messageType, object data)
    {
        try
        {
            var json = JsonSerializer.Serialize(data);
            NetDataWriter writer = new NetDataWriter();
            writer.Put(messageType);
            writer.Put(json);
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Server] Error sending message to {peer.EndPoint}: {ex.Message}");
            _logger?.LogError($"[Server] Error sending message to {peer.EndPoint}: {ex.Message}");
        }
    }

    /// <summary>
    /// 发送欢迎消息给新玩家
    /// 包含玩家ID、房主状态和当前玩家列表
    /// </summary>
    /// <param name="peer">新连接的网络对等体</param>
    /// <param name="session">玩家会话</param>
    private void SendWelcomeMessage(NetPeer peer, PlayerSession session)
    {
        var welcomeData = new
        {
            Message = "Welcome to the server!",
            PlayerId = session.PlayerId,
            IsHost = session.IsHost,
            PlayerList = _playerSessions.Values.Select(s => new
            {
                PlayerId = s.PlayerId,
                PlayerName = s.PlayerName,
                IsHost = s.IsHost
            }).ToList()
        };

        SendMessage(peer, "Welcome", welcomeData);
    }

    /// <summary>
    /// 广播玩家列表更新
    /// 通知所有玩家当前连接的玩家状态
    /// </summary>
    private void BroadcastPlayerList()
    {
        var playerList = _playerSessions.Values.Select(s => new
        {
            PlayerId = s.PlayerId,
            PlayerName = s.PlayerName,
            IsHost = s.IsHost,
            IsConnected = s.IsConnected
        }).ToList();

        BroadcastMessage("PlayerListUpdate", new { Players = playerList });
    }

    #endregion

    #region 公共方法

    /// <summary>
    /// 获取所有玩家会话的只读集合
    /// </summary>
    /// <returns>玩家会话集合</returns>
    public IReadOnlyCollection<PlayerSession> GetPlayerSessions()
    {
        return _playerSessions.Values;
    }

    /// <summary>
    /// 根据Peer ID获取特定玩家会话
    /// </summary>
    /// <param name="peerId">Peer ID</param>
    /// <returns>玩家会话，如果不存在则返回null</returns>
    public PlayerSession GetPlayerSession(int peerId)
    {
        _playerSessions.TryGetValue(peerId, out var session);
        return session;
    }

    /// <summary>
    /// 获取当前连接的玩家数量
    /// </summary>
    public int PlayerCount => _playerSessions.Count;

    /// <summary>
    /// 启动网络服务器
    /// 开始监听指定端口，接受客户端连接
    /// </summary>
    public void Start()
    {
        _netManager.Start(_port);
        Console.WriteLine($"[Server] Server started on port {_port}.");
        _logger?.LogInfo($"[Server] Server started on port {_port}.");
    }

    /// <summary>
    /// 轮询网络事件
    /// 应在主游戏循环中定期调用以处理网络消息
    /// </summary>
    public void PollEvents()
    {
        _netManager.PollEvents();
    }

    /// <summary>
    /// 停止网络服务器
    /// 断开所有客户端连接并停止监听
    /// </summary>
    public void Stop()
    {
        _netManager.Stop();
        Console.WriteLine("[Server] Server stopped.");
        _logger?.LogInfo("[Server] Server stopped.");
    }

    #endregion


}

