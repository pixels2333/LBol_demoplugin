using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using BepInEx.Logging;
using LiteNetLib;
using LiteNetLib.Utils;
//TODO:这个地方用了日志系统和依赖注入,如果使用了分离服务器,需要修改日志系统和依赖注入
namespace NetworkPlugin.Network.Server;

public class NetworkServer
{
    private EventBasedNetListener _listener;
    private NetManager _netManager;
    private int _port;
    private int _maxConnections;
    private string _connectionKey;

    private readonly ManualLogSource _logger;

    // 管理玩家会话
    private readonly Dictionary<int, PlayerSession> _playerSessions = new Dictionary<int, PlayerSession>();

    // 游戏事件处理委托
    public delegate void GameEventHandler(string eventType, object eventData, PlayerSession sender);
    public event GameEventHandler OnGameEventReceived;

    public NetworkServer(int port, int maxConnections, string connectionKey, ManualLogSource logger)
    {
        _port = port;
        _maxConnections = maxConnections;
        _connectionKey = connectionKey;
        _listener = new EventBasedNetListener();
        _logger = logger;
        _netManager = new NetManager(_listener);
        RegisterEvents();
    }

    private void RegisterEvents()
    {
        _listener.ConnectionRequestEvent += request =>
        {
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

        _listener.PeerConnectedEvent += peer =>
        {
            Console.WriteLine($"[Server] Client connected: {peer.EndPoint}");
            _logger?.LogInfo($"[Server] Client connected: {peer.EndPoint}");

            // 创建玩家会话
            var session = new PlayerSession
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

    /// <summary>
    /// 检查是否为游戏事件消息
    /// </summary>
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
    /// </summary>
    private void HandleGameEvent(NetPeer fromPeer, string eventType, NetDataReader dataReader)
    {
        try
        {
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

            // 触发游戏事件处理器
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
    /// 处理系统消息
    /// </summary>
    private void HandleSystemMessage(NetPeer fromPeer, string messageType, NetDataReader dataReader)
    {
        try
        {
            switch (messageType)
            {
                case "PlayerJoined":
                    HandlePlayerJoined(fromPeer, dataReader);
                    break;
                case "Heartbeat":
                    HandleHeartbeat(fromPeer);
                    break;
                case "GetSelf_REQUEST":
                    HandleGetSelfRequest(fromPeer, dataReader);
                    break;
                default:
                    Console.WriteLine($"[Server] Unknown system message: {messageType}");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Server] Error handling system message {messageType}: {ex.Message}");
        }
    }

    /// <summary>
    /// 处理玩家加入
    /// </summary>
    private void HandlePlayerJoined(NetPeer fromPeer, NetDataReader dataReader)
    {
        try
        {
            string jsonPayload = dataReader.GetString();
            var playerInfo = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonPayload);

            if (_playerSessions.TryGetValue(fromPeer.Id, out var session))
            {
                if (playerInfo.TryGetValue("PlayerName", out var nameObj) && nameObj != null)
                {
                    session.PlayerName = nameObj.ToString();
                }

                Console.WriteLine($"[Server] Player {session.PlayerName} ({session.PlayerId}) joined the game");

                // 广播玩家加入消息
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
        }
    }

    /// <summary>
    /// 处理心跳包
    /// </summary>
    private void HandleHeartbeat(NetPeer fromPeer)
    {
        if (_playerSessions.TryGetValue(fromPeer.Id, out var session))
        {
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
    /// 处理GetSelf请求
    /// </summary>
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

    /// <summary>
    /// 广播游戏事件给所有玩家（除了指定的发送者）
    /// </summary>
    private void BroadcastGameEvent(string eventType, object eventData, int excludePeerId)
    {
        var json = JsonSerializer.Serialize(eventData);

        foreach (var kvp in _playerSessions)
        {
            if (kvp.Key != excludePeerId && kvp.Value.IsConnected)
            {
                try
                {
                    var writer = new NetDataWriter();
                    writer.Put(eventType);
                    writer.Put(json);
                    kvp.Value.Peer.Send(writer, DeliveryMethod.ReliableOrdered);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Server] Error broadcasting game event to {kvp.Value.PlayerId}: {ex.Message}");
                }
            }
        }
    }

    /// <summary>
    /// 广播系统消息给所有玩家
    /// </summary>
    private void BroadcastMessage(string messageType, object data, int? excludePeerId = null)
    {
        var json = JsonSerializer.Serialize(data);

        foreach (var kvp in _playerSessions)
        {
            if (excludePeerId.HasValue && kvp.Key == excludePeerId.Value)
                continue;

            if (kvp.Value.IsConnected)
            {
                SendMessage(kvp.Value.Peer, messageType, data);
            }
        }
    }

    /// <summary>
    /// 发送消息给特定玩家
    /// </summary>
    private void SendMessage(NetPeer peer, string messageType, object data)
    {
        try
        {
            var json = JsonSerializer.Serialize(data);
            var writer = new NetDataWriter();
            writer.Put(messageType);
            writer.Put(json);
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Server] Error sending message to {peer.EndPoint}: {ex.Message}");
        }
    }

    /// <summary>
    /// 发送欢迎消息给新玩家
    /// </summary>
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
    /// 广播玩家列表
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

    /// <summary>
    /// 获取所有玩家会话
    /// </summary>
    public IReadOnlyCollection<PlayerSession> GetPlayerSessions()
    {
        return _playerSessions.Values;
    }

    /// <summary>
    /// 获取玩家会话
    /// </summary>
    public PlayerSession GetPlayerSession(int peerId)
    {
        _playerSessions.TryGetValue(peerId, out var session);
        return session;
    }

    /// <summary>
    /// 获取当前玩家数量
    /// </summary>
    public int PlayerCount => _playerSessions.Count;

    public void Start()
    {
        _netManager.Start(_port);
        Console.WriteLine($"[Server] Server started on port {_port}.");
    }

    public void PollEvents()
    {
        _netManager.PollEvents();
    }

    public void Stop()
    {
        _netManager.Stop();
        Console.WriteLine("[Server] Server stopped.");
    }

   
}

