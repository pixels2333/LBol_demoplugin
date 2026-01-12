

//TODO:这个地方用了日志系统和依赖注入,如果使用了分离服务器,需要修改日志系统和依赖注入
// 直连房主服务器：用于房主/客机直连联机，管理会话与广播游戏事件。
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using BepInEx.Logging;
using LiteNetLib;
using LiteNetLib.Utils;
using NetworkPlugin.Network.Server.Core;

namespace NetworkPlugin.Network.Server;

/// <summary>
/// LiteNetLib网络服务器类
/// 负责处理客户端连接、消息转发、玩家会话管理和游戏同步
/// </summary>
public class NetworkServer : BaseGameServer
{
    #region 私有字段

    /// <summary>
    /// LiteNetLib事件监听器
    /// </summary>
    private IServerCore _core => Core;

    // 兼容：保留旧实现中对 `_listener/_netManager` 的引用，实际由 ServerCore 托管。
    private EventBasedNetListener _listener => _core.Listener;
    private NetManager _netManager => _core.NetManager;

    /// <summary>
    /// LiteNetLib网络管理器
    /// </summary>
    // private NetManager _netManager; // 由 ServerCore 托管

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
    private Dictionary<string, PlayerSession> _sessionsByPlayerId => SessionsByPlayerId;
    private readonly Dictionary<int, string> _playerIdByPeerId = new();
    private Dictionary<string, DateTime> _disconnectedAtByPlayerId => DisconnectedAtByPlayerId;
    private readonly TimeSpan _reconnectGracePeriod = TimeSpan.FromSeconds(60);
    private readonly Dictionary<int, PlayerSession> _playerSessions = new();

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
        : base(CreateCore(port, maxConnections, connectionKey, logger))
    {
        _port = port;
        _maxConnections = maxConnections;
        _connectionKey = connectionKey;
        // _listener = new EventBasedNetListener(); // 由 ServerCore 托管
        _logger = logger;

        // 事件注册由 BaseGameServer 托管（避免双通道注册导致行为不一致）
    }

    #endregion

    #region 事件注册

    /// <summary>
    /// 注册LiteNetLib网络事件处理器
    /// 包括连接请求、连接建立、连接断开和消息接收事件
    /// </summary>
    /// <summary>
    /// 注册基于 ServerCore 的事件处理器（Host/直连模式）。
    /// </summary>
    private void RegisterCoreEvents()
    {
        _core.PeerConnected += peer =>
        {
            Console.WriteLine($"[Server] Client connected: {peer.EndPoint}");
            _logger?.LogInfo($"[Server] Client connected: {peer.EndPoint}");

            string playerId = $"Player_{peer.Id}";
            bool isHost = _sessionsByPlayerId.Values.All(s => !s.IsHost);

            PlayerSession session = new PlayerSession
            {
                Peer = peer,
                PlayerId = playerId,
                ConnectedAt = DateTime.UtcNow,
                LastHeartbeat = DateTime.UtcNow,
                LastMessageAt = DateTime.UtcNow,
                IsConnected = true,
                IsHost = isHost
            };

            session.Metadata["ReconnectToken"] = GenerateReconnectToken();

            _sessionsByPlayerId[playerId] = session;
            _playerIdByPeerId[peer.Id] = playerId;
            _disconnectedAtByPlayerId.Remove(playerId);
            _playerSessions[peer.Id] = session;

            BroadcastPlayerList();
            SendWelcomeMessage(peer, session);
        };

        _core.PeerDisconnected += (peer, disconnectInfo) =>
        {
            Console.WriteLine($"[Server] Client disconnected: {peer.EndPoint}, Reason: {disconnectInfo.Reason}");
            _logger?.LogInfo($"[Server] Client disconnected: {peer.EndPoint}, Reason: {disconnectInfo.Reason}");

            if (!_playerIdByPeerId.TryGetValue(peer.Id, out string playerId))
            {
                return;
            }

            _playerIdByPeerId.Remove(peer.Id);
            _playerSessions.Remove(peer.Id);

            if (_sessionsByPlayerId.TryGetValue(playerId, out var session))
            {
                session.IsConnected = false;
                _disconnectedAtByPlayerId[playerId] = DateTime.UtcNow;
            }

            BroadcastPlayerList();
        };

        _core.PeerLatencyUpdated += (peer, latency) =>
        {
            if (_playerIdByPeerId.TryGetValue(peer.Id, out var playerId) &&
                _sessionsByPlayerId.TryGetValue(playerId, out var session))
            {
                session.Ping = latency;
            }
        };

        _core.MessageReceived += message =>
        {
            if (IsGameEvent(message.Type))
            {
                HandleGameEvent(message.FromPeer, message.Type, message.JsonPayload);
                return;
            }

            HandleSystemMessage(message.FromPeer, message.Type, message.JsonPayload);
        };
    }

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
        try
        {
            switch (messageType)
            {
                case "PlayerJoined":
                    HandlePlayerJoined(fromPeer, dataReader);
                    return;
                case "Heartbeat":
                    HandleHeartbeat(fromPeer);
                    return;
                case "GetSelf_REQUEST":
                    HandleGetSelfRequest(fromPeer, dataReader);
                    return;
                case "UpdatePlayerLocation":
                    HandleUpdatePlayerLocation(fromPeer, dataReader);
                    return;
                default:
                    Console.WriteLine($"[Server] Unknown system message type: {messageType} from {fromPeer.EndPoint}");
                    _logger?.LogWarning($"[Server] Unknown system message type: {messageType} from {fromPeer.EndPoint}");
                    return;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Server] Error handling system message {messageType} from {fromPeer.EndPoint}: {ex.Message}");
            _logger?.LogError($"[Server] Error handling system message {messageType} from {fromPeer.EndPoint}: {ex.Message}");
        }

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
               messageType == "StateSyncRequest" ||
               messageType == "FullStateSyncRequest" ||
               messageType == "FullStateSyncResponse";
    }

    // TODO: 实现 FullStateSyncRequest 的服务端处理：
    // - 由 Host 生成快照/追赶事件并向请求方回复 FullStateSyncResponse（而非简单广播）。
    // - 将 ReconnectionManager 的快照/事件历史与该请求的响应对齐。

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
            if (!SessionsByPeer.TryGetValue(fromPeer, out var session))
            {
                Console.WriteLine($"[Server] Received game event from unknown peer: {fromPeer.EndPoint}");
                return;
            }

            string jsonPayload = dataReader.GetString();
            object eventData = JsonSerializer.Deserialize<object>(jsonPayload);

            Console.WriteLine($"[Server] Received game event: {eventType} from {session.PlayerId}");

            session.UpdateMessageTime();

            OnGameEventReceived?.Invoke(eventType, eventData, session);

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

            if (SessionsByPeer.TryGetValue(fromPeer, out var session))
            {
                if (playerInfo.TryGetValue("PlayerName", out object nameObj) && nameObj != null)
                {
                    session.PlayerName = nameObj.ToString();
                }

                if (playerInfo.TryGetValue("CharacterId", out object charObj) && charObj != null)
                {
                    session.Metadata["CharacterId"] = charObj.ToString();
                }

                Console.WriteLine($"[Server] Player {session.PlayerName} ({session.PlayerId}) joined the game");

                BroadcastMessage("PlayerJoined", new
                {
                    PlayerId = session.PlayerId,
                    PlayerName = session.PlayerName,
                    IsHost = session.IsHost,
                    CharacterId = session.Metadata.TryGetValue("CharacterId", out var cid) ? cid?.ToString() : null
                }, excludePeerId: fromPeer.Id);

                BroadcastPlayerList();
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
        if (SessionsByPeer.TryGetValue(fromPeer, out var session))
        {
            session.UpdateHeartbeat();

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
        if (SessionsByPeer.TryGetValue(fromPeer, out var session))
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
    /// 处理客户端上报的玩家位置更新
    /// </summary>
    /// <param name="fromPeer">发送请求的客户端</param>
    /// <param name="dataReader">消息数据（JSON 字符串）</param>
    private void HandleUpdatePlayerLocation(NetPeer fromPeer, NetDataReader dataReader)
    {
        try
        {
            if (!SessionsByPeer.TryGetValue(fromPeer, out var session))
            {
                return;
            }

            string jsonPayload = dataReader.GetString();
            JsonElement root;
            try
            {
                root = JsonSerializer.Deserialize<JsonElement>(jsonPayload);
            }
            catch
            {
                Console.WriteLine($"[Server] Invalid UpdatePlayerLocation payload from {session.PlayerId}");
                return;
            }

            if (root.ValueKind != JsonValueKind.Object)
            {
                return;
            }

            if (root.TryGetProperty("LocationX", out JsonElement xElem) && xElem.TryGetInt32(out int x))
            {
                session.Metadata["LocationX"] = x;
            }
            if (root.TryGetProperty("LocationY", out JsonElement yElem) && yElem.TryGetInt32(out int y))
            {
                session.Metadata["LocationY"] = y;
            }
            if (root.TryGetProperty("Stage", out JsonElement stageElem) && stageElem.TryGetInt32(out int stage))
            {
                session.Metadata["Stage"] = stage;
            }
            if (root.TryGetProperty("LocationName", out JsonElement nameElem) && nameElem.ValueKind == JsonValueKind.String)
            {
                session.Metadata["LocationName"] = nameElem.GetString();
            }

            BroadcastPlayerList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Server] Error handling UpdatePlayerLocation: {ex.Message}");
            _logger?.LogError($"[Server] Error handling UpdatePlayerLocation: {ex.Message}");
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
        string json = JsonSerializer.Serialize(eventData);

        foreach (var session in SessionsByPeer.Values)
        {
            if (session.Peer.Id != excludePeerId && session.IsConnected)
            {
                try
                {
                    NetDataWriter writer = new NetDataWriter();
                    writer.Put(eventType);
                    writer.Put(json);
                    session.Peer.Send(writer, DeliveryMethod.ReliableOrdered);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Server] Error broadcasting game event to {session.PlayerId}: {ex.Message}");
                    _logger?.LogError($"[Server] Error broadcasting game event to {session.PlayerId}: {ex.Message}");
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
        foreach (var session in SessionsByPeer.Values)
        {
            if (excludePeerId.HasValue && session.Peer.Id == excludePeerId.Value)
            {
                continue;
            }

            if (session.IsConnected)
            {
                SendMessage(session.Peer, messageType, data);
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
            string json = JsonSerializer.Serialize(data);
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
    private static int? TryGetMetadataInt(Dictionary<string, object> metadata, string key)
    {
        if (metadata == null || !metadata.TryGetValue(key, out object value) || value == null)
        {
            return null;
        }

        return value switch
        {
            int i => i,
            long l => (int)l,
            float f => (int)f,
            double d => (int)d,
            string s when int.TryParse(s, out int i) => i,
            JsonElement je when je.ValueKind == JsonValueKind.Number && je.TryGetInt32(out int i) => i,
            JsonElement je when je.ValueKind == JsonValueKind.String && int.TryParse(je.GetString(), out int i) => i,
            _ => null
        };
    }

    private static string TryGetMetadataString(Dictionary<string, object> metadata, string key)
    {
        if (metadata == null || !metadata.TryGetValue(key, out object value) || value == null)
        {
            return null;
        }

        return value switch
        {
            string s => s,
            JsonElement je when je.ValueKind == JsonValueKind.String => je.GetString(),
            _ => value.ToString()
        };
    }

    private static new string GenerateReconnectToken()
    {
        return Guid.NewGuid().ToString("N");
    }

    private void SendWelcomeMessage(NetPeer peer, PlayerSession session)  
    {
        var welcomeData = new
        {
            Message = "Welcome to the server!",
            PlayerId = session.PlayerId,
            IsHost = session.IsHost,
            ReconnectToken = TryGetMetadataString(session.Metadata, "ReconnectToken"),
            PlayerList = _sessionsByPlayerId.Values.Select(s => new
            {
                PlayerId = s.PlayerId,
                PlayerName = s.PlayerName,
                IsHost = s.IsHost,
                IsConnected = s.IsConnected,
                CharacterId = TryGetMetadataString(s.Metadata, "CharacterId"),
                LocationX = TryGetMetadataInt(s.Metadata, "LocationX") ?? -1,
                LocationY = TryGetMetadataInt(s.Metadata, "LocationY") ?? -1,
                Stage = TryGetMetadataInt(s.Metadata, "Stage") ?? -1,
                LocationName = TryGetMetadataString(s.Metadata, "LocationName")
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
        var playerList = _sessionsByPlayerId.Values.Select(s => new
        {
            PlayerId = s.PlayerId,
            PlayerName = s.PlayerName,
            IsHost = s.IsHost,
            IsConnected = s.IsConnected,
            CharacterId = TryGetMetadataString(s.Metadata, "CharacterId"),
            LocationX = TryGetMetadataInt(s.Metadata, "LocationX") ?? -1,
            LocationY = TryGetMetadataInt(s.Metadata, "LocationY") ?? -1,
            Stage = TryGetMetadataInt(s.Metadata, "Stage") ?? -1,
            LocationName = TryGetMetadataString(s.Metadata, "LocationName")
        }).ToList();

        BroadcastMessage("PlayerListUpdate", new { Players = playerList });
    }

    private void CleanupDisconnectedSessions()
    {
        if (_disconnectedAtByPlayerId.Count == 0)
        {
            return;
        }

        DateTime now = DateTime.UtcNow;
        List<string> expired = null;

        foreach (var kvp in _disconnectedAtByPlayerId)
        {
            if (now - kvp.Value > _reconnectGracePeriod)
            {
                expired ??= new List<string>();
                expired.Add(kvp.Key);
            }
        }

        if (expired == null)
        {
            return;
        }

        bool hostRemoved = false;
        foreach (string playerId in expired)
        {
            if (_sessionsByPlayerId.TryGetValue(playerId, out var session))
            {
                hostRemoved |= session.IsHost;
                _sessionsByPlayerId.Remove(playerId);
            }

            _disconnectedAtByPlayerId.Remove(playerId);
        }

        if (hostRemoved)
        {
            foreach (var s in _sessionsByPlayerId.Values)
            {
                s.IsHost = false;
            }

            var newHost = SessionsByPeer.Values.FirstOrDefault(s => s.IsConnected);
            if (newHost != null)
            {
                newHost.IsHost = true;
                BroadcastMessage("HostChanged", new { NewHostId = newHost.PlayerId });
                Console.WriteLine($"[Server] Host changed to {newHost.PlayerId}");
            }
        }

        BroadcastPlayerList();
    }

    #endregion

    #region 公共方法

    /// <summary>
    /// 获取所有玩家会话的只读集合
    /// </summary>
    /// <returns>玩家会话集合</returns>
    public IReadOnlyCollection<PlayerSession> GetPlayerSessions()
    {
        return SessionsByPeer.Values;
    }

    /// <summary>
    /// 根据Peer ID获取特定玩家会话
    /// </summary>
    /// <param name="peerId">Peer ID</param>
    /// <returns>玩家会话，如果不存在则返回null</returns>
    public PlayerSession GetPlayerSession(int peerId)
    {
        return SessionsByPeer.Values.FirstOrDefault(s => s.Peer.Id == peerId);
    }

    /// <summary>
    /// 获取当前连接的玩家数量
    /// </summary>
    public int PlayerCount => SessionsByPeer.Count;

    private void HandleSystemMessage(NetPeer fromPeer, string messageType, string jsonPayload)
    {
        try
        {
            switch (messageType)
            {
                case "PlayerJoined":
                    HandlePlayerJoined(fromPeer, jsonPayload);
                    return;
                case "Heartbeat":
                    HandleHeartbeat(fromPeer);
                    return;
                case "GetSelf_REQUEST":
                    HandleGetSelfRequest(fromPeer);
                    return;
                case "UpdatePlayerLocation":
                    HandleUpdatePlayerLocation(fromPeer, jsonPayload);    
                    return;
                case "Reconnect_REQUEST":
                    HandleReconnectRequest(fromPeer, jsonPayload);
                    return;
                default:
                    Console.WriteLine($"[Server] Unknown system message type: {messageType} from {fromPeer.EndPoint}");
                    _logger?.LogWarning($"[Server] Unknown system message type: {messageType} from {fromPeer.EndPoint}");
                    return;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Server] Error handling system message {messageType} from {fromPeer.EndPoint}: {ex.Message}");
            _logger?.LogError($"[Server] Error handling system message {messageType} from {fromPeer.EndPoint}: {ex.Message}");
        }
    }

    private void HandleGameEvent(NetPeer fromPeer, string eventType, string jsonPayload)
    {
        try
        {
            if (!SessionsByPeer.TryGetValue(fromPeer, out var session))
            {
                Console.WriteLine($"[Server] Received game event from unknown peer: {fromPeer.EndPoint}");
                return;
            }

            object eventData = JsonSerializer.Deserialize<object>(jsonPayload);
            Console.WriteLine($"[Server] Received game event: {eventType} from {session.PlayerId}");

            session.UpdateMessageTime();

            OnGameEventReceived?.Invoke(eventType, eventData, session);
            BroadcastGameEvent(eventType, eventData, fromPeer.Id);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Server] Error handling game event: {ex.Message}");
            _logger?.LogError($"[Server] Error handling game event: {ex.Message}");
        }
    }

    private void HandlePlayerJoined(NetPeer fromPeer, string jsonPayload)
    {
        try
        {
            var playerInfo = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonPayload);
            if (!SessionsByPeer.TryGetValue(fromPeer, out var session))
            {
                return;
            }

            if (playerInfo != null)
            {
                if (playerInfo.TryGetValue("PlayerName", out object nameObj) && nameObj != null)
                {
                    session.PlayerName = nameObj.ToString();
                }

                if (playerInfo.TryGetValue("CharacterId", out object charObj) && charObj != null)
                {
                    session.Metadata["CharacterId"] = charObj.ToString();
                }
            }

            Console.WriteLine($"[Server] Player {session.PlayerName} ({session.PlayerId}) joined the game");

            BroadcastMessage("PlayerJoined", new
            {
                PlayerId = session.PlayerId,
                PlayerName = session.PlayerName,
                IsHost = session.IsHost,
                CharacterId = session.Metadata.TryGetValue("CharacterId", out var cid) ? cid?.ToString() : null
            }, excludePeerId: fromPeer.Id);

            BroadcastPlayerList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Server] Error handling PlayerJoined: {ex.Message}");
            _logger?.LogError($"[Server] Error handling PlayerJoined: {ex.Message}");
        }
    }

    private void HandleGetSelfRequest(NetPeer fromPeer)
    {
        if (SessionsByPeer.TryGetValue(fromPeer, out var session))
        {
            var responseData = new
            {
                PlayerId = session.PlayerId,
                PlayerName = session.PlayerName,
                IsHost = session.IsHost,
                ConnectedAt = session.ConnectedAt.Ticks,
                ReconnectToken = TryGetMetadataString(session.Metadata, "ReconnectToken"),
            };

            SendMessage(fromPeer, "GetSelf_RESPONSE", responseData);
        }
    }

    private sealed class ReconnectRequest
    {
        public string PlayerId { get; set; } = string.Empty;
        public string ReconnectToken { get; set; } = string.Empty;
    }

    private void HandleReconnectRequest(NetPeer fromPeer, string jsonPayload)
    {
        try
        {
            ReconnectRequest request = JsonSerializer.Deserialize<ReconnectRequest>(jsonPayload);
            if (request == null || string.IsNullOrWhiteSpace(request.PlayerId) || string.IsNullOrWhiteSpace(request.ReconnectToken))
            {
                SendMessage(fromPeer, "Reconnect_RESPONSE", new { Success = false, Error = "Invalid request" });
                return;
            }

            if (!_sessionsByPlayerId.TryGetValue(request.PlayerId, out var targetSession))
            {
                SendMessage(fromPeer, "Reconnect_RESPONSE", new { Success = false, Error = "Unknown playerId" });
                return;
            }

            if (targetSession.IsConnected)
            {
                SendMessage(fromPeer, "Reconnect_RESPONSE", new { Success = false, Error = "Already connected" });
                return;
            }

            string expectedToken = TryGetMetadataString(targetSession.Metadata, "ReconnectToken");
            if (!string.Equals(expectedToken, request.ReconnectToken, StringComparison.Ordinal))
            {
                SendMessage(fromPeer, "Reconnect_RESPONSE", new { Success = false, Error = "Invalid token" });
                return;
            }

            if (_disconnectedAtByPlayerId.TryGetValue(request.PlayerId, out var disconnectedAt))
            {
                if (DateTime.UtcNow - disconnectedAt > _reconnectGracePeriod)
                {
                    SendMessage(fromPeer, "Reconnect_RESPONSE", new { Success = false, Error = "Reconnect window expired" });
                    return;
                }
            }

            // 移除当前 peer 可能已经被分配的临时会话（避免同一个 peerId 对应多个 PlayerId）
            if (_playerIdByPeerId.TryGetValue(fromPeer.Id, out var currentPlayerId) &&
                !string.Equals(currentPlayerId, request.PlayerId, StringComparison.Ordinal))
            {
                _playerIdByPeerId.Remove(fromPeer.Id);
                _playerSessions.Remove(fromPeer.Id);

                _sessionsByPlayerId.Remove(currentPlayerId);
                _disconnectedAtByPlayerId.Remove(currentPlayerId);
                SessionsByPeer.Remove(fromPeer);
            }

            targetSession.Peer = fromPeer;
            targetSession.IsConnected = true;
            targetSession.UpdateHeartbeat();
            targetSession.UpdateMessageTime();

            _playerIdByPeerId[fromPeer.Id] = targetSession.PlayerId;
            _playerSessions[fromPeer.Id] = targetSession;
            _disconnectedAtByPlayerId.Remove(targetSession.PlayerId);

            // 关键：更新 BaseGameServer 的 peer->session 映射，否则后续收包无法找到 session
            SessionsByPeer[fromPeer] = targetSession;

            SendMessage(fromPeer, "Reconnect_RESPONSE", new
            {
                Success = true,
                PlayerId = targetSession.PlayerId,
                IsHost = targetSession.IsHost,
                ConnectedAt = targetSession.ConnectedAt.Ticks
            });

            BroadcastPlayerList();
        }
        catch (Exception ex)
        {
            _logger?.LogError($"[Server] Error handling Reconnect_REQUEST: {ex.Message}");
            SendMessage(fromPeer, "Reconnect_RESPONSE", new { Success = false, Error = "Server error" });
        }
    }

    private void HandleUpdatePlayerLocation(NetPeer fromPeer, string jsonPayload)
    {
        try
        {
            if (!SessionsByPeer.TryGetValue(fromPeer, out var session))
            {
                return;
            }

            JsonElement root;
            try
            {
                root = JsonSerializer.Deserialize<JsonElement>(jsonPayload);
            }
            catch
            {
                Console.WriteLine($"[Server] Invalid UpdatePlayerLocation payload from {session.PlayerId}");
                return;
            }

            if (root.ValueKind != JsonValueKind.Object)
            {
                return;
            }

            if (root.TryGetProperty("LocationX", out JsonElement xElem) && xElem.TryGetInt32(out int x))
            {
                session.Metadata["LocationX"] = x;
            }
            if (root.TryGetProperty("LocationY", out JsonElement yElem) && yElem.TryGetInt32(out int y))
            {
                session.Metadata["LocationY"] = y;
            }
            if (root.TryGetProperty("Stage", out JsonElement stageElem) && stageElem.TryGetInt32(out int stage))
            {
                session.Metadata["Stage"] = stage;
            }
            if (root.TryGetProperty("LocationName", out JsonElement nameElem) && nameElem.ValueKind == JsonValueKind.String)
            {
                session.Metadata["LocationName"] = nameElem.GetString();
            }

            BroadcastPlayerList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Server] Error handling UpdatePlayerLocation: {ex.Message}");
            _logger?.LogError($"[Server] Error handling UpdatePlayerLocation: {ex.Message}");
        }
    }

    /// <summary>
    /// 启动网络服务器
    /// 开始监听指定端口，接受客户端连接
    /// </summary>
    public override void Start()
    {
        _core.Start();
        Console.WriteLine($"[Server] Server started on port {_port}.");
        _logger?.LogInfo($"[Server] Server started on port {_port}.");
    }

    /// <summary>
    /// 轮询网络事件
    /// 应在主游戏循环中定期调用以处理网络消息
    /// </summary>
    public override void PollEvents()
    {
        _core.PollEvents();
        CleanupDisconnectedSessions();
    }

    /// <summary>
    /// 停止网络服务器
    /// 断开所有客户端连接并停止监听
    /// </summary>
    public override void Stop()
    {
        _core.Stop();
        _playerSessions.Clear();
        _playerIdByPeerId.Clear();
        _sessionsByPlayerId.Clear();
        _disconnectedAtByPlayerId.Clear();
        Console.WriteLine("[Server] Server stopped.");
        _logger?.LogInfo("[Server] Server stopped.");
    }

    private static IServerCore CreateCore(int port, int maxConnections, string connectionKey, ManualLogSource logger)
    {
        return new ServerCore(
            new ServerOptions
            {
                Port = port,
                MaxConnections = maxConnections,
                ConnectionKey = connectionKey,
                DisconnectTimeoutMs = 30_000,
                PingIntervalMs = 1_000,
                UseBackgroundThread = false,
            },
            new BepInExServerLogger(logger));
    }

    protected override TimeSpan ReconnectGracePeriod => _reconnectGracePeriod;

    protected override string CreatePlayerId(NetPeer peer) => $"Player_{peer.Id}";

    protected override PlayerSession CreateSession(NetPeer peer, string playerId)
    {
        bool isHost = _sessionsByPlayerId.Values.All(s => !s.IsHost);
        return new PlayerSession
        {
            Peer = peer,
            PlayerId = playerId,
            ConnectedAt = DateTime.UtcNow,
            LastHeartbeat = DateTime.UtcNow,
            LastMessageAt = DateTime.UtcNow,
            IsConnected = true,
            IsHost = isHost
        };
    }

    protected override bool IsGameEventType(string messageType) => IsGameEvent(messageType);

    protected override void HandleGameEvent(PlayerSession session, string eventType, string jsonPayload, DeliveryMethod deliveryMethod)
    {
        HandleGameEvent(session.Peer, eventType, jsonPayload);
    }

    protected override void HandleSystemMessage(PlayerSession session, string messageType, string jsonPayload, DeliveryMethod deliveryMethod)
    {
        HandleSystemMessage(session.Peer, messageType, jsonPayload);
    }

    protected override void OnSessionConnected(PlayerSession session)
    {
        _playerIdByPeerId[session.Peer.Id] = session.PlayerId;
        _playerSessions[session.Peer.Id] = session;

        Console.WriteLine($"[Server] Client connected: {session.Peer.EndPoint}");
        _logger?.LogInfo($"[Server] Client connected: {session.Peer.EndPoint}");

        BroadcastPlayerList();
        SendWelcomeMessage(session.Peer, session);
    }

    protected override void OnSessionDisconnected(PlayerSession session, DisconnectInfo disconnectInfo)
    {
        Console.WriteLine($"[Server] Client disconnected: {session.Peer.EndPoint}, Reason: {disconnectInfo.Reason}");
        _logger?.LogInfo($"[Server] Client disconnected: {session.Peer.EndPoint}, Reason: {disconnectInfo.Reason}");

        _playerIdByPeerId.Remove(session.Peer.Id);
        _playerSessions.Remove(session.Peer.Id);

        BroadcastPlayerList();
    }

    #endregion


}

