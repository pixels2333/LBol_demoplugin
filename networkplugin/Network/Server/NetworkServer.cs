// NOTE: 这里使用了日志系统和依赖注入；如果后续引入分离服务器，需要相应调整日志系统与依赖注入。
// 直连房主服务器：用于房主/客机直连联机，管理会话与广播游戏事件。
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using BepInEx.Logging;
using LiteNetLib;
using LiteNetLib.Utils;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Network.Server.Core;
using NetworkPlugin.Utils;

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
    private readonly HashSet<string> _routeProbeOnceKeys = new(StringComparer.Ordinal);

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

    #region 消息处理

    /// <summary>
    /// 检查消息类型是否为游戏事件消息
    /// </summary>
    /// <param name="messageType">消息类型字符串</param>
    /// <returns>如果是游戏事件消息返回true，否则返回false</returns>
    private bool IsGameEvent(string messageType)
    {
        return NetworkMessageTypes.IsGameEvent(messageType, NetworkMessageTypes.GameEventRoute.HostServer);
    }

    /// <summary>
    /// 处理游戏同步事件
    /// 接收客户端发送的游戏事件，更新会话状态，并广播给其他玩家
    /// </summary>
    private void LogRouteProbeOnce(string routeKey, string details)
    {
        if (string.IsNullOrWhiteSpace(routeKey))
        {
            return;
        }

        try
        {
            bool shouldLog;
            lock (_routeProbeOnceKeys)
            {
                shouldLog = _routeProbeOnceKeys.Add(routeKey);
            }

            if (!shouldLog)
            {
                return;
            }

            _logger?.LogInfo($"[RouteProbe] {routeKey}: {details}");
        }
        catch (Exception ex) { _logger?.LogWarning($"[RouteProbe] LogRouteProbeOnce error: {ex.Message}"); }
    }

    private PlayerSession GetConnectedHostSession()
    {
        return SessionsByPeer.Values.FirstOrDefault(session => session.IsHost && session.IsConnected);
    }

    private bool TryGetConnectedTargetSession(string targetPlayerId, out PlayerSession targetSession)
    {
        targetSession = null;
        return !string.IsNullOrWhiteSpace(targetPlayerId) &&
               TryGetSession(targetPlayerId, out targetSession) &&
               targetSession.IsConnected;
    }

    private static string TryGetJsonStringProperty(JsonElement root, params string[] propertyNames)
    {
        for (int index = 0; index < propertyNames.Length; index++)
        {
            string propertyName = propertyNames[index];
            if (!root.TryGetProperty(propertyName, out JsonElement propertyValue) || propertyValue.ValueKind != JsonValueKind.String)
            {
                continue;
            }

            string value = propertyValue.GetString();
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }

    private static string GetNestedPayloadJson(JsonElement root)
    {
        return root.TryGetProperty("Payload", out JsonElement payloadElement)
            ? payloadElement.GetRawText()
            : "{}";
    }

    private bool TryRouteControlledMessage(PlayerSession senderSession, string messageType, string jsonPayload)
    {
        switch (messageType)
        {
            case NetworkMessageTypes.RoomStateRequest:
                RouteRoomStateRequest(senderSession, jsonPayload);
                return true;
            case NetworkMessageTypes.RoomStateUpload:
                RouteRoomStateUpload(senderSession, jsonPayload);
                return true;
            case NetworkMessageTypes.RoomStateResponse:
                RouteRoomStateResponse(senderSession, jsonPayload);
                return true;
            case NetworkMessageTypes.FullStateSyncRequest:
                RouteFullStateSyncRequest(senderSession, jsonPayload);
                return true;
            case NetworkMessageTypes.FullStateSyncResponse:
                RouteFullStateSyncResponse(senderSession, jsonPayload);
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// Host/直连模式下的 RoomStateRequest 路由：请求定向转发给房主（主机作为房间状态中枢）。
    /// </summary>
    private void RouteRoomStateRequest(PlayerSession senderSession, string jsonPayload)
    {
        try
        {
            PlayerSession hostSession = GetConnectedHostSession();
            if (hostSession == null)
            {
                return;
            }

            LogRouteProbeOnce(NetworkMessageTypes.RoomStateRequest,
                $"sender={senderSession?.PlayerId}, host={hostSession.PlayerId}");

            // 定向转发给房主：由房主侧 RoomStateManager 生成并回发 RoomStateResponse。
            SendRawJsonToPeer(hostSession.Peer, NetworkMessageTypes.RoomStateRequest, jsonPayload);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"[服务器] 路由 RoomStateRequest 异常: {ex.Message}");
        }
    }

    /// <summary>
    /// Host/直连模式下的 RoomStateUpload 路由：上传定向转发给房主。
    /// </summary>
    private void RouteRoomStateUpload(PlayerSession senderSession, string jsonPayload)
    {
        try
        {
            PlayerSession hostSession = GetConnectedHostSession();
            if (hostSession == null)
            {
                return;
            }

            LogRouteProbeOnce(NetworkMessageTypes.RoomStateUpload,
                $"sender={senderSession?.PlayerId}, host={hostSession.PlayerId}");

            SendRawJsonToPeer(hostSession.Peer, NetworkMessageTypes.RoomStateUpload, jsonPayload);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"[服务器] 路由 RoomStateUpload 异常: {ex.Message}");
        }
    }

    /// <summary>
    /// Host/直连模式下的 RoomStateResponse 路由：仅单播给请求方（payload.TargetPlayerId 或 payload.RequesterId）。
    /// </summary>
    private void RouteRoomStateResponse(PlayerSession senderSession, string jsonPayload)
    {
        try
        {
            if (!senderSession.IsHost)
            {
                return;
            }

            JsonElement root = JsonSerializer.Deserialize<JsonElement>(jsonPayload);
            string targetPlayerId = TryGetJsonStringProperty(root, "TargetPlayerId", "RequesterId");

            if (string.IsNullOrWhiteSpace(targetPlayerId))
            {
                return;
            }

            if (!TryGetConnectedTargetSession(targetPlayerId, out PlayerSession targetSession))
            {
                return;
            }

            LogRouteProbeOnce(NetworkMessageTypes.RoomStateResponse,
                $"host={senderSession?.PlayerId}, target={targetPlayerId}");

            SendRawJsonToPeer(targetSession.Peer, NetworkMessageTypes.RoomStateResponse, jsonPayload);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"[服务器] 路由 RoomStateResponse 异常: {ex.Message}");
        }
    }

    /// <summary>
    /// Host/直连模式下的 FullStateSyncRequest 路由：将请求转发给房主（由房主侧 MidGameJoinManager 生成快照并回发响应）。
    /// </summary>
    /// <param name="senderSession">请求方会话。</param>
    /// <param name="jsonPayload">请求负载 JSON。</param>
    private void RouteFullStateSyncRequest(PlayerSession senderSession, string jsonPayload)
    {
        try
        {
            // 查找房主会话（直连模式的权威端）。
            PlayerSession hostSession = GetConnectedHostSession();
            if (hostSession == null)
            {
                return;
            }

            LogRouteProbeOnce(NetworkMessageTypes.FullStateSyncRequest,
                $"sender={senderSession?.PlayerId}, host={hostSession.PlayerId}");

            // 请求必须以自己为 TargetPlayerId，避免代替他人拉取快照。
            JsonElement root = JsonSerializer.Deserialize<JsonElement>(jsonPayload);
            string targetPlayerId = TryGetJsonStringProperty(root, "TargetPlayerId");
            if (!string.IsNullOrWhiteSpace(targetPlayerId) && !string.Equals(targetPlayerId, senderSession.PlayerId, StringComparison.Ordinal))
            {
                return;
            }

            // 定向转发给房主：由房主侧进行 JoinToken 校验与快照生成。
            SendRawJsonToPeer(hostSession.Peer, NetworkMessageTypes.FullStateSyncRequest, jsonPayload);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"[服务器] 路由 FullStateSyncRequest 异常: {ex.Message}");
        }
    }

    /// <summary>
    /// Host/直连模式下的 FullStateSyncResponse 路由：仅单播给请求方（payload.TargetPlayerId）。
    /// </summary>
    /// <param name="senderSession">响应发送方（应为房主）。</param>
    /// <param name="jsonPayload">响应负载 JSON。</param>
    private void RouteFullStateSyncResponse(PlayerSession senderSession, string jsonPayload)
    {
        try
        {
            if (!senderSession.IsHost)
            {
                return;
            }

            JsonElement root = JsonSerializer.Deserialize<JsonElement>(jsonPayload);
            string targetPlayerId = TryGetJsonStringProperty(root, "TargetPlayerId");
            if (string.IsNullOrWhiteSpace(targetPlayerId))
            {
                return;
            }

            if (!TryGetConnectedTargetSession(targetPlayerId, out PlayerSession targetSession))
            {
                return;
            }

            LogRouteProbeOnce(NetworkMessageTypes.FullStateSyncResponse,
                $"host={senderSession?.PlayerId}, target={targetPlayerId}");

            SendRawJsonToPeer(targetSession.Peer, NetworkMessageTypes.FullStateSyncResponse, jsonPayload);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"[服务器] 路由 FullStateSyncResponse 异常: {ex.Message}");
        }
    }

    /// <summary>
    /// 向指定 peer 发送一条“类型 + JSON payload”的原始消息（避免二次序列化改变字段结构）。
    /// </summary>
    /// <param name="peer">目标 peer。</param>
    /// <param name="messageType">消息类型。</param>
    /// <param name="jsonPayload">JSON payload 字符串。</param>
    private void SendRawJsonToPeer(NetPeer peer, string messageType, string jsonPayload)
    {
        try
        {
            NetDataWriter writer = new();
            writer.Put(messageType);
            writer.Put(jsonPayload ?? string.Empty);
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"[服务器] 发送原始消息异常: type={messageType}, err={ex.Message}");
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

            SendMessage(fromPeer, NetworkMessageTypes.HeartbeatResponse, new
            {
                Timestamp = DateTime.UtcNow.Ticks,
                Ping = session.Ping
            });
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
        string json = JsonCompat.Serialize(eventData);

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
                    Plugin.Logger?.LogError($"[服务器] 广播游戏事件异常: to={session.PlayerId}, err={ex.Message}");
                    _logger?.LogError($"[服务器] 广播游戏事件异常: to={session.PlayerId}, err={ex.Message}");
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
            string json = JsonCompat.Serialize(data);
            NetDataWriter writer = new NetDataWriter();
            writer.Put(messageType);
            writer.Put(json);
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[服务器] 发送消息异常: to={peer.EndPoint}, err={ex.Message}");
            _logger?.LogError($"[服务器] 发送消息异常: to={peer.EndPoint}, err={ex.Message}");
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

        SendMessage(peer, NetworkMessageTypes.Welcome, welcomeData);
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

        BroadcastMessage(NetworkMessageTypes.PlayerListUpdate, new { Players = playerList });
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
                BroadcastMessage(NetworkMessageTypes.HostChanged, new { NewHostId = newHost.PlayerId });
                Plugin.Logger?.LogInfo($"[服务器] 房主已变更为 {newHost.PlayerId}");
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

    private void HandleSystemMessageCore(PlayerSession senderSession, string messageType, string jsonPayload)
    {
        try
        {
            if (TryRouteControlledMessage(senderSession, messageType, jsonPayload))
            {
                return;
            }

            switch (messageType)
            {
                case NetworkMessageTypes.PlayerJoined:
                    HandlePlayerJoined(senderSession.Peer, jsonPayload);
                    return;
                case NetworkMessageTypes.Heartbeat:
                    HandleHeartbeat(senderSession.Peer);
                    return;
                case NetworkMessageTypes.GetSelf_REQUEST:
                    HandleGetSelfRequest(senderSession.Peer);
                    return;
                case NetworkMessageTypes.UpdatePlayerLocation:
                    HandleUpdatePlayerLocation(senderSession.Peer, jsonPayload);
                    return;
                case NetworkMessageTypes.Reconnect_REQUEST:
                    HandleReconnectRequest(senderSession.Peer, jsonPayload);
                    return;
                case NetworkMessageTypes.DirectMessage:
                    HandleDirectMessage(senderSession, jsonPayload);
                    return;
                default:
                    Plugin.Logger?.LogInfo($"[服务器] 未知系统消息类型: {messageType}, 来自 {senderSession.Peer.EndPoint}");
                    _logger?.LogWarning($"[服务器] 未知系统消息类型: {messageType}, 来自 {senderSession.Peer.EndPoint}");
                    return;
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[服务器] 处理系统消息异常: type={messageType}, from={senderSession.Peer.EndPoint}, err={ex.Message}");
            _logger?.LogError($"[服务器] 处理系统消息异常: type={messageType}, from={senderSession.Peer.EndPoint}, err={ex.Message}");
        }
    }

    /// <summary>
    /// 处理点对点消息中继（DirectMessage）。
    /// </summary>
    /// <param name="fromPeer">发送者 peer。</param>
    /// <param name="jsonPayload">外层 DirectMessage payload（包含 TargetPlayerId/Type/Payload）。</param>
    private void HandleDirectMessage(PlayerSession senderSession, string jsonPayload)
    {
        try
        {
            JsonElement root = JsonSerializer.Deserialize<JsonElement>(jsonPayload);
            if (root.ValueKind != JsonValueKind.Object)
            {
                return;
            }

            string targetPlayerId = TryGetJsonStringProperty(root, "TargetPlayerId");

            if (!TryGetConnectedTargetSession(targetPlayerId, out PlayerSession targetSession))
            {
                return;
            }

            string innerType = TryGetJsonStringProperty(root, "Type") ?? NetworkMessageTypes.DirectMessage;

            LogRouteProbeOnce($"DirectMessage/{innerType}",
                $"sender={senderSession.PlayerId}, target={targetPlayerId}");

            string innerJson = GetNestedPayloadJson(root);
            if (TryRouteControlledMessage(senderSession, innerType, innerJson))
            {
                return;
            }

            // 其他 DirectMessage：透传内层类型与 payload。
            SendRawJsonToPeer(targetSession.Peer, innerType, innerJson);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"[服务器] 处理 DirectMessage 异常: {ex.Message}");
        }
    }

    private void HandleGameEventCore(PlayerSession session, string eventType, string jsonPayload)
    {
        try
        {
            object eventData = JsonSerializer.Deserialize<object>(jsonPayload);
            string summary = NetLogHelper.BuildSummary(eventType, jsonPayload);
            Plugin.Logger?.LogInfo($"[服务器] 收到游戏事件: type={eventType}, from={session.PlayerId} ({summary})");

            session.UpdateMessageTime();

            OnGameEventReceived?.Invoke(eventType, eventData, session);

            if (TryRouteControlledMessage(session, eventType, jsonPayload))
            {
                return;
            }

            BroadcastGameEvent(eventType, eventData, session.Peer.Id);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[服务器] 处理游戏事件异常: {ex.Message}");
            _logger?.LogError($"[服务器] 处理游戏事件异常: {ex.Message}");
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

            Plugin.Logger?.LogInfo($"[服务器] 玩家加入: {session.PlayerName} ({session.PlayerId})");

            BroadcastMessage(NetworkMessageTypes.PlayerJoined, new
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
            Plugin.Logger?.LogError($"[服务器] 处理 PlayerJoined 异常: {ex.Message}");
            _logger?.LogError($"[服务器] 处理 PlayerJoined 异常: {ex.Message}");
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

            SendMessage(fromPeer, NetworkMessageTypes.GetSelf_RESPONSE, responseData);
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
                SendMessage(fromPeer, NetworkMessageTypes.Reconnect_RESPONSE, new { Success = false, Error = "Invalid request" });
                return;
            }

            if (!_sessionsByPlayerId.TryGetValue(request.PlayerId, out var targetSession))
            {
                SendMessage(fromPeer, NetworkMessageTypes.Reconnect_RESPONSE, new { Success = false, Error = "Unknown playerId" });
                return;
            }

            if (targetSession.IsConnected)
            {
                SendMessage(fromPeer, NetworkMessageTypes.Reconnect_RESPONSE, new { Success = false, Error = "Already connected" });
                return;
            }

            string expectedToken = TryGetMetadataString(targetSession.Metadata, "ReconnectToken");
            if (!string.Equals(expectedToken, request.ReconnectToken, StringComparison.Ordinal))
            {
                SendMessage(fromPeer, NetworkMessageTypes.Reconnect_RESPONSE, new { Success = false, Error = "Invalid token" });
                return;
            }

            if (_disconnectedAtByPlayerId.TryGetValue(request.PlayerId, out var disconnectedAt))
            {
                if (DateTime.UtcNow - disconnectedAt > _reconnectGracePeriod)
                {
                    SendMessage(fromPeer, NetworkMessageTypes.Reconnect_RESPONSE, new { Success = false, Error = "Reconnect window expired" });
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

            SendMessage(fromPeer, NetworkMessageTypes.Reconnect_RESPONSE, new
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
            _logger?.LogError($"[服务器] 处理 {NetworkMessageTypes.Reconnect_REQUEST} 异常: {ex.Message}");
            SendMessage(fromPeer, NetworkMessageTypes.Reconnect_RESPONSE, new { Success = false, Error = "Server error" });
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
                Plugin.Logger?.LogError($"[服务器] UpdatePlayerLocation payload 无效: from={session.PlayerId}");
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
            Plugin.Logger?.LogError($"[服务器] 处理 UpdatePlayerLocation 异常: {ex.Message}");
            _logger?.LogError($"[服务器] 处理 UpdatePlayerLocation 异常: {ex.Message}");
        }
    }

    /// <summary>
    /// 启动网络服务器
    /// 开始监听指定端口，接受客户端连接
    /// </summary>
    public override void Start()
    {
        _core.Start();
        Plugin.Logger?.LogInfo($"[服务器] 已启动，监听端口 {_port}。");
        _logger?.LogInfo($"[服务器] 已启动，监听端口 {_port}。");
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
        Plugin.Logger?.LogInfo("[服务器] 已停止。");
        _logger?.LogInfo("[服务器] 已停止。");
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
        HandleGameEventCore(session, eventType, jsonPayload);
    }

    protected override void HandleSystemMessage(PlayerSession session, string messageType, string jsonPayload, DeliveryMethod deliveryMethod)
    {
        HandleSystemMessageCore(session, messageType, jsonPayload);
    }

    protected override void OnSessionConnected(PlayerSession session)
    {
        _playerIdByPeerId[session.Peer.Id] = session.PlayerId;
        _playerSessions[session.Peer.Id] = session;

        Plugin.Logger?.LogInfo($"[服务器] 客户端已连接: {session.Peer.EndPoint}");
        _logger?.LogInfo($"[服务器] 客户端已连接: {session.Peer.EndPoint}");

        BroadcastPlayerList();
        SendWelcomeMessage(session.Peer, session);
    }

    protected override void OnSessionDisconnected(PlayerSession session, DisconnectInfo disconnectInfo)
    {
        Plugin.Logger?.LogInfo($"[服务器] 客户端已断开: {session.Peer.EndPoint}, 原因: {disconnectInfo.Reason}");
        _logger?.LogInfo($"[服务器] 客户端已断开: {session.Peer.EndPoint}, 原因: {disconnectInfo.Reason}");

        _playerIdByPeerId.Remove(session.Peer.Id);
        _playerSessions.Remove(session.Peer.Id);

        BroadcastPlayerList();
    }

    #endregion


}
