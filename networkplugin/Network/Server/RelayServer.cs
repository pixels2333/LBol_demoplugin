// NOTE: RelayServer（中继服务器）
// - 提供房间（Room）管理能力：创建/加入/离开、成员列表广播、房主权限校验等。
// - 提供中继转发能力：房间广播（RoomMessage）/点对点转发（DirectMessage）/游戏事件转发等。
// - 底层网络收发、鉴权与队列调度由 ServerCore 统一实现（见 BaseGameServer/Core）。
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.Extensions.Logging;
using NetworkPlugin.Configuration;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Network.Room;
using NetworkPlugin.Utils;
using NetworkPlugin.Network.Server.Core;
using NetworkPlugin.Network.Utils;

namespace NetworkPlugin.Network.Server;

/// <summary>
/// Relay 中继服务器：负责房间管理与消息转发（不同于 Host/直连模式的 <see cref="NetworkServer"/>）。
/// </summary>
/// <remarks>
/// 关键设计点：
/// - 会话（<see cref="PlayerSession"/>）的生命周期由 <see cref="BaseGameServer"/> 统一管理；
///   本类通过若干只读属性别名访问基础字典（便于阅读/保持旧代码结构）。
/// - 房间状态由 <see cref="NetworkRoom"/> 管理，本类负责把玩家会话映射到房间并做权限校验。
/// - 消息类型由 <see cref="NetworkMessage.Type"/> 驱动：系统消息走 switch 路由，游戏事件走前缀规则。
/// </remarks>
public class RelayServer : BaseGameServer
{
    #region 私有字段

    /// <summary>
    /// 结构化日志输出。
    /// </summary>
    private readonly ILogger<RelayServer> _logger;

    /// <summary>
    /// 预留：后续如需按需解析服务（如房间工厂/鉴权器等）可通过 DI 获取。
    /// 当前版本仅保存引用，不在本类内直接使用。
    /// </summary>
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// 配置管理器：提供监听端口、连接密钥、最大连接数等服务端参数。
    /// </summary>
    private readonly ConfigManager _configManager;

    /// <summary>
    /// 统一同步锁对象：直接复用基类的 <see cref="BaseGameServer.SyncRoot"/>，避免多把锁导致状态不同步。
    /// </summary>
    private object _lock => SyncRoot;

    /// <summary>
    /// 房间字典：Key=RoomId，Value=房间实例。
    /// </summary>
    private readonly Dictionary<string, NetworkRoom> _rooms = [];

    /// <summary>
    /// 会话索引：Key=NetPeer，Value=玩家会话（来自基类统一维护）。
    /// </summary>
    private Dictionary<NetPeer, PlayerSession> _sessionsByPeer => SessionsByPeer;

    /// <summary>
    /// 会话索引：Key=PlayerId，Value=玩家会话（来自基类统一维护）。
    /// </summary>
    private Dictionary<string, PlayerSession> _sessionsByPlayerId => SessionsByPlayerId;

    /// <summary>
    /// 当前连接包装：用于向指定玩家发送消息/追踪房间归属。
    /// 之所以单独维护，是为了与房间层（<see cref="NetworkRoom"/>）解耦。
    /// </summary>
    private readonly Dictionary<string, NetworkConnection> _connectionsByPlayerId = [];

    /// <summary>
    /// 断线时间记录：用于重连窗口判定（来自基类统一维护）。
    /// </summary>
    private Dictionary<string, DateTime> _disconnectedAtByPlayerId => DisconnectedAtByPlayerId;

    /// <summary>
    /// 允许重连的宽限期（秒）。在此窗口内可携带 token 重新绑定到原 PlayerId。
    /// </summary>
    private readonly TimeSpan _reconnectGracePeriod = TimeSpan.FromSeconds(60);

    /// <summary>
    /// 底层 ServerCore（LiteNetLib 封装）：统一处理网络事件与消息队列。
    /// </summary>
    private IServerCore _core => Core;

    #endregion

    #region 构造函数

    /// <summary>
    /// 创建中继服务器。
    /// </summary>
    /// <param name="logger">日志记录器。</param>
    /// <param name="serviceProvider">依赖注入服务提供器（预留）。</param>
    /// <param name="configManager">配置管理器（读取端口/连接密钥等）。</param>

    public RelayServer(ILogger<RelayServer> logger, IServiceProvider serviceProvider, ConfigManager configManager)
        : base(CreateCore(configManager, logger))
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configManager = configManager;
    }

    #endregion

    #region 生命周期

    /// <summary>
    /// 启动中继服务器（开始监听端口并处理网络收发）。
    /// </summary>
    public override void Start()
    {
        // 启动底层网络与后台收发线程：ServerCore 内部维护 NetManager、队列、线程等。
        _core.Start();
        _logger.LogInformation($"[RelayServer] Server started on port {_configManager.RelayServerPort.Value}");
    }

    /// <summary>
    /// 停止中继服务器并清理内存状态。
    /// </summary>
    public override void Stop()
    {
        _logger.LogInformation("[RelayServer] Stopping server...");

        // 先停止底层网络：阻断后续回调/收包，避免清理过程中仍有并发写入。
        _core.Stop();

        lock (_lock)
        {
            // 清理所有内存索引，避免下次 Start 时残留旧连接/旧房间造成“幽灵玩家”。
            _connectionsByPlayerId.Clear();
            _sessionsByPlayerId.Clear();
            _sessionsByPeer.Clear();
            _disconnectedAtByPlayerId.Clear();
            _rooms.Clear();
        }

        _logger.LogInformation("[RelayServer] Server stopped");
    }

    #endregion

    #region Core 事件注册（兼容旧实现）

    private void RegisterCoreEvents()
    {
        // 说明：本方法为“直接订阅 _core 事件”的旧实现入口。
        // 当前架构下，BaseGameServer 会统一托管连接/断线/消息分发，并回调本类底部的 override 方法。
        // 之所以保留：便于回溯/对照逻辑；以及必要时可快速切回直接订阅模式排查问题。
        _core.PeerConnected += peer =>
        {
            lock (_lock)
            {
                // 新连接进入：为其分配 PlayerId，并初始化会话（含 token，用于断线重连校验）。
                string playerId = GeneratePlayerId();
                PlayerSession session = new()
                {
                    Peer = peer,
                    PlayerId = playerId,
                    PlayerName = $"Player_{playerId[..6]}",
                    ConnectedAt = DateTime.UtcNow,
                    LastHeartbeat = DateTime.UtcNow,
                    LastMessageAt = DateTime.UtcNow,
                    IsConnected = true,
                    IsHost = false,
                    CurrentRoomId = string.Empty
                };

                // 每个会话生成一个重连 token：客户端后续发 Reconnect_REQUEST 时必须带回该 token。
                session.Metadata["ReconnectToken"] = GenerateReconnectToken();

                // 建立多索引：Peer -> Session / PlayerId -> Session / PlayerId -> Connection。
                _sessionsByPeer[peer] = session;
                _sessionsByPlayerId[playerId] = session;
                _connectionsByPlayerId[playerId] = new NetworkConnection(peer, playerId);

                // 握手：把 PlayerId + token 下发给客户端，客户端应保存以便重连/身份标识。
                SendMessageToPeer(peer, new NetworkMessage
                {
                    Type = "Welcome",
                    Payload = new
                    {
                        PlayerId = playerId,
                        ReconnectToken = TryGetMetadataString(session.Metadata, "ReconnectToken"),
                        ServerTime = DateTime.UtcNow.Ticks
                    },
                    SenderPlayerId = "SERVER"
                }, DeliveryMethod.ReliableOrdered);

                _logger.LogInformation($"[RelayServer] Client connected: {peer.EndPoint}, PlayerId={playerId}");
            }
        };

        _core.PeerDisconnected += (peer, disconnectInfo) =>
        {
            lock (_lock)
            {
                // peer 断线：如果没有找到会话，说明它并未完成握手或已被清理。
                if (!_sessionsByPeer.TryGetValue(peer, out var session))
                {
                    return;
                }

                _logger.LogInformation($"[RelayServer] Client disconnected: {peer.EndPoint}, Reason: {disconnectInfo.Reason}, PlayerId={session.PlayerId}");    

                session.IsConnected = false;
                // 记录断线时间：用于重连窗口判定（超过窗口则拒绝重连并清理状态）。
                _disconnectedAtByPlayerId[session.PlayerId] = DateTime.UtcNow;

                if (!string.IsNullOrEmpty(session.CurrentRoomId) && _rooms.TryGetValue(session.CurrentRoomId, out var room))
                {
                    // 从房间移除该玩家，并广播最新成员列表。
                    room.RemovePlayer(session.PlayerId);
                    BroadcastPlayerList(room);

                    if (room.PlayerCount == 0)
                    {
                        // 房间空了就销毁：避免房间字典无限增长。
                        _rooms.Remove(room.RoomId);
                        _logger.LogInformation($"[RelayServer] Room {room.RoomId} destroyed (empty)");
                    }
                }

                // 移除“连接态”索引；会话仍保留在 PlayerId 索引中以支持重连。
                _connectionsByPlayerId.Remove(session.PlayerId);
                _sessionsByPeer.Remove(peer);
            }
        };

        _core.PeerLatencyUpdated += (peer, latency) =>
        {
            lock (_lock)
            {
                // 更新 ping：心跳响应会把 ping 回传给客户端，用于 UI 显示/诊断。
                if (_sessionsByPeer.TryGetValue(peer, out var session))
                {
                    session.Ping = latency;
                }
            }
        };

        _core.MessageReceived += inbound =>
        {
            try
            {
                // 统一把底层 inbound 数据包装为 NetworkMessage，便于复用下方消息路由逻辑。
                NetworkMessage message = new()
                {
                    Type = inbound.Type,
                    Payload = inbound.JsonPayload,
                    SenderPlayerId = GetPlayerId(inbound.FromPeer)
                };

                // 路由分发：根据 message.Type 做系统消息处理/房间转发/游戏事件广播。
                ProcessMessage(inbound.FromPeer, message, inbound.DeliveryMethod);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[RelayServer] Error processing message from {inbound.FromPeer?.EndPoint}");
            }
        };
    }

    #endregion

    #region 消息路由与房间协议（Relay）

    /// <summary>
    /// 处理来自指定 <see cref="NetPeer"/> 的消息，并按协议类型路由到对应处理函数。
    /// </summary>
    /// <param name="fromPeer">消息来源 peer。</param>
    /// <param name="message">协议消息（type + payload）。</param>
    /// <param name="deliveryMethod">LiteNetLib 投递方式（可靠/有序/不可靠等）。</param>
    private void ProcessMessage(NetPeer fromPeer, NetworkMessage message, DeliveryMethod deliveryMethod)
    {
        lock (_lock)
        {
            // Relay 服务端的权限/房间判断都依赖会话；如果 peer 没有会话，直接忽略该消息。
            if (!_sessionsByPeer.TryGetValue(fromPeer, out var session))
            {
                return;
            }

            // 更新会话最后消息时间：用于超时检测、统计、以及“活跃玩家”判断。
            session.UpdateMessageTime();

            switch (message.Type)
            {
                case "CreateRoom":
                    // 创建房间：创建后当前玩家成为房主，并立即加入房间。
                    HandleCreateRoom(session, message);
                    return;
                case "JoinRoom":
                    // 加入房间：加入成功后广播成员列表给房间所有玩家。
                    HandleJoinRoom(session, message);
                    return;
                case "LeaveRoom":
                    // 离开房间：离开后可能销毁空房间。
                    HandleLeaveRoom(session);
                    return;
                case "RoomMessage":
                    // 房间广播消息：把 message.Payload 内层数据广播给同房间的其他玩家。
                    HandleRoomMessage(session, message, deliveryMethod);
                    return;
                case "DirectMessage":
                    // 点对点消息：由服务端作为中继，转发到目标玩家（不做业务解析）。
                    HandleDirectMessage(session, message, deliveryMethod);
                    return;
                case "Heartbeat":
                    // 心跳：刷新心跳时间并回传 timestamp/ping。
                    HandleHeartbeat(session, fromPeer);
                    return;
                case "GetRoomList":
                    // 大厅请求：返回房间列表状态（不包含敏感信息/不包含实际连接对象）。
                    HandleGetRoomList(fromPeer);
                    return;
                case "KickPlayer":
                    // 踢人：仅房主可踢同房间玩家。
                    HandleKickPlayer(session, message);
                    return;
                case "Reconnect_REQUEST":
                    // 断线重连：携带 playerId + reconnectToken 在窗口期内重新绑定 peer。
                    HandleReconnectRequest(fromPeer, message);
                    return;

                case NetworkMessageTypes.NatInfoReport:
                    // NAT 信息上报：玩家把自身 NAT 信息上报给服务端缓存，供其他玩家查询。
                    HandleNatInfoReport(session, message);
                    return;
                case NetworkMessageTypes.NatInfoRequest:
                    // NAT 信息请求：向服务端查询目标玩家的 NAT 信息（便于双方打洞）。
                    HandleNatInfoRequest(session, message);
                    return;

                // 兼容 Host 模式依赖的系统消息（按房间作用域生效）
                case "PlayerJoined":
                    // 玩家加入后补充信息：昵称、角色等；服务端再转发给同房间其他玩家。
                    HandlePlayerJoined(session, message);
                    return;
                case "GetSelf_REQUEST":
                    // 客户端询问“自己是谁”：返回 PlayerId/昵称/是否房主等。
                    HandleGetSelfRequest(session, fromPeer);
                    return;
                case "UpdatePlayerLocation":
                    // 更新位置信息：用于房间 UI 显示（地图坐标/关卡/地点名）。
                    HandleUpdatePlayerLocation(session, message);
                    return;

                case NetworkMessageTypes.FullStateSyncRequest:
                    // 完整状态快照请求：按 roomId 作用域定向转发给房主（避免广播泄漏 JoinToken）。
                    HandleFullStateSyncRequest(session, message, deliveryMethod);
                    return;

                case NetworkMessageTypes.FullStateSyncResponse:
                    // 完整状态快照响应：仅单播给请求方（避免房间内广播导致敏感信息扩散/无谓负载）。
                    HandleFullStateSyncResponse(session, message, deliveryMethod);
                    return;

                case NetworkMessageTypes.RoomStateRequest:
                    HandleRoomStateRequest(session, message, deliveryMethod);
                    return;

                case NetworkMessageTypes.RoomStateUpload:
                    HandleRoomStateUpload(session, message, deliveryMethod);
                    return;

                case NetworkMessageTypes.RoomStateResponse:
                    HandleRoomStateResponse(session, message, deliveryMethod);
                    return;
            }

            // 不属于系统消息：尝试按“游戏事件”规则转发给同房间其他玩家（同步/战斗/聊天等）。
            if (IsGameEvent(message.Type))
            {
                ForwardGameEventToRoom(session, message.Type, message.Payload, deliveryMethod);
                return;
            }

            // 未识别消息：打印告警，便于排查客户端协议差异/版本不一致。
            _logger.LogWarning($"[RelayServer] Unknown message type: {message.Type}");
        }
    }

    #endregion

    #region 心跳与大厅

    /// <summary>
    /// 处理客户端心跳：刷新会话心跳时间，并返回服务端时间戳与当前 ping。
    /// </summary>
    /// <param name="session">来源会话。</param>
    /// <param name="peer">来源 peer。</param>
    private void HandleHeartbeat(PlayerSession session, NetPeer peer)
    {
        // 刷新会话心跳时间：后续断线判定/超时清理会依赖该字段。
        session.UpdateHeartbeat();

        // 心跳响应：客户端可用它校准时间/显示延迟（ping）。
        SendMessageToPeer(peer, new NetworkMessage
        {
            Type = "HeartbeatResponse",
            Payload = new
            {
                Timestamp = DateTime.UtcNow.Ticks,
                Ping = session.Ping
            },
            SenderPlayerId = "SERVER"
        }, DeliveryMethod.ReliableOrdered);
    }

    /// <summary>
    /// 返回当前房间列表（用于大厅 UI）。
    /// </summary>
    /// <param name="peer">请求方 peer。</param>
    private void HandleGetRoomList(NetPeer peer)
    {
        // 把每个房间的“可公开状态”序列化给客户端（不包含连接对象/不包含敏感字段）。
        var rooms = _rooms.Values.Select(r => r.GetStatus()).ToList();
        SendMessageToPeer(peer, new NetworkMessage
        {
            Type = "RoomList",
            Payload = new { Rooms = rooms },
            SenderPlayerId = "SERVER"
        }, DeliveryMethod.ReliableOrdered);
    }

    #endregion

    #region 房间管理

    /// <summary>
    /// 创建房间并让创建者加入房间。
    /// </summary>
    /// <param name="session">创建者会话。</param>
    /// <param name="message">创建房间消息（包含 <see cref="RoomConfig"/>）。</param>
    private void HandleCreateRoom(PlayerSession session, NetworkMessage message)
    {
        // 约束：一个玩家同时只能在一个房间里；创建新房间前先离开旧房间（如果存在）。
        if (!string.IsNullOrEmpty(session.CurrentRoomId))
        {
            HandleLeaveRoom(session);
        }

        // 从 payload 解析房间配置（最大人数、模式等）。
        RoomConfig roomConfig = message.GetRoomConfigPayload();

        // 生成短房间号：便于客户端显示/手动输入加入。
        string roomId = GenerateRoomId();

        // 创建房间对象并注册到字典：后续 Join/广播等都依赖该索引。
        NetworkRoom room = new(roomId, roomConfig, _logger);
        _rooms[roomId] = room;

        // 为该玩家准备 NetworkConnection：房间层通过它向玩家发消息/追踪房间归属。
        if (!_connectionsByPlayerId.TryGetValue(session.PlayerId, out var connection))
        {
            connection = new NetworkConnection(session.Peer, session.PlayerId);
            _connectionsByPlayerId[session.PlayerId] = connection;
        }

        // 将创建者加入房间（房间内部会处理 Host 归属、容量等规则）。
        var joinResult = room.AddPlayer(session.PlayerId, connection);
        if (!joinResult.IsSuccess)
        {
            SendErrorMessage(session.Peer, "CreateRoomFailed", joinResult.ErrorMessage ?? "Unknown error");
            return;
        }

        // 写回“当前房间”：后续 RoomMessage/游戏事件转发都以这个字段作为默认 roomId。
        session.CurrentRoomId = roomId;
        connection.CurrentRoomId = roomId;

        // 回包：告知客户端房间已创建，并返回 roomId/配置/创建者 id。
        SendMessageToPeer(session.Peer, new NetworkMessage
        {
            Type = "RoomCreated",
            Payload = new
            {
                RoomId = roomId,
                RoomConfig = roomConfig,
                PlayerId = session.PlayerId
            },
            SenderPlayerId = "SERVER"
        }, DeliveryMethod.ReliableOrdered);

        // 广播房间成员列表：让客户端 UI 立刻刷新（包括房主标记/成员在线状态）。
        BroadcastPlayerList(room);
        _logger.LogInformation($"[RelayServer] Room created: {roomId} by player {session.PlayerId}");
    }

    /// <summary>
    /// 将玩家加入指定房间（或加入其当前房间）。
    /// </summary>
    /// <param name="session">加入者会话。</param>
    /// <param name="message">加入房间消息（payload 中包含 RoomId）。</param>
    private void HandleJoinRoom(PlayerSession session, NetworkMessage message)
    {
        // 兼容：如果 payload 未显式提供 RoomId，则允许沿用会话中的 CurrentRoomId（用于“重试加入”场景）。
        string roomId = TryGetStringProperty(message.Payload, "RoomId") ?? session.CurrentRoomId;
        if (string.IsNullOrEmpty(roomId))
        {
            SendErrorMessage(session.Peer, "JoinRoomFailed", "Missing RoomId");
            return;
        }

        // 校验：房间必须存在。
        if (!_rooms.TryGetValue(roomId, out var room))
        {
            SendErrorMessage(session.Peer, "JoinRoomFailed", $"Room not found: {roomId}");
            return;
        }

        // 校验：房间容量（具体策略由 NetworkRoom/RoomConfig 定义）。
        if (room.IsFull)
        {
            SendErrorMessage(session.Peer, "JoinRoomFailed", "Room is full");
            return;
        }

        // 约束：一个玩家同一时刻只在一个房间；加入新房间前先离开旧房间。
        if (!string.IsNullOrEmpty(session.CurrentRoomId))
        {
            HandleLeaveRoom(session);
        }

        // 准备连接包装（供房间层广播/转发使用）。
        if (!_connectionsByPlayerId.TryGetValue(session.PlayerId, out var connection))
        {
            connection = new NetworkConnection(session.Peer, session.PlayerId);
            _connectionsByPlayerId[session.PlayerId] = connection;
        }

        var joinResult = room.AddPlayer(session.PlayerId, connection);
        if (!joinResult.IsSuccess)
        {
            SendErrorMessage(session.Peer, "JoinRoomFailed", joinResult.ErrorMessage ?? "Unknown error");
            return;
        }

        // 加入成功：更新会话/连接的当前房间字段，用于后续默认路由。
        session.CurrentRoomId = roomId;
        connection.CurrentRoomId = roomId;

        // 回包：通知加入成功。
        SendMessageToPeer(session.Peer, new NetworkMessage
        {
            Type = "RoomJoined",
            Payload = new { RoomId = roomId, PlayerId = session.PlayerId },
            SenderPlayerId = "SERVER"
        }, DeliveryMethod.ReliableOrdered);

        // 广播成员列表：让房间内所有客户端立刻刷新 UI（含在线状态/房主标记）。
        BroadcastPlayerList(room);
    }

    /// <summary>
    /// 将玩家从其当前房间移除，并在必要时销毁空房间。
    /// </summary>
    /// <param name="session">要离开的会话。</param>
    private void HandleLeaveRoom(PlayerSession session)
    {
        // 没有房间可离开：直接返回。
        if (string.IsNullOrEmpty(session.CurrentRoomId))
        {
            return;
        }

        // 先从房间中移除玩家。
        if (_rooms.TryGetValue(session.CurrentRoomId, out var room))
        {
            room.RemovePlayer(session.PlayerId);

            // 移除后广播成员列表：其余玩家需要感知该玩家已离开/掉线。
            BroadcastPlayerList(room);

            if (room.PlayerCount == 0)
            {
                // 房间空了：销毁，释放内存并避免列表中出现“空房间”。
                _rooms.Remove(room.RoomId);
                _logger.LogInformation($"[RelayServer] Room {room.RoomId} destroyed (empty)");
            }
        }

        // 断开会话与连接的房间绑定（否则后续消息可能被错误路由到旧房间）。
        if (_connectionsByPlayerId.TryGetValue(session.PlayerId, out var connection))
        {
            connection.CurrentRoomId = string.Empty;
        }

        session.CurrentRoomId = string.Empty;
    }

    /// <summary>
    /// 房间广播消息：把内层消息转发给同房间其他玩家。
    /// </summary>
    /// <param name="session">发送者会话。</param>
    /// <param name="message">外层 RoomMessage（payload 内包含 RoomId/Type/Payload）。</param>
    /// <param name="deliveryMethod">投递方式（可靠/不可靠）。</param>
    private void HandleRoomMessage(PlayerSession session, NetworkMessage message, DeliveryMethod deliveryMethod)
    {
        // RoomId 允许显式指定；未指定时使用 session.CurrentRoomId（更省 payload）。
        string roomId = TryGetStringProperty(message.Payload, "RoomId") ?? session.CurrentRoomId;
        if (string.IsNullOrEmpty(roomId) || !_rooms.TryGetValue(roomId, out var room))
        {
            // 不在房间里：拒绝广播，避免跨房间污染。
            SendErrorMessage(session.Peer, "RoomMessageFailed", "Not in room");
            return;
        }

        // 内层消息类型：用于客户端区分具体业务（聊天/准备状态/自定义房间事件等）。
        string innerType = TryGetStringProperty(message.Payload, "Type") ?? "RoomMessage";

        // 内层负载：优先取 payload.Payload；否则回退为整个 payload（兼容旧客户端格式）。
        object innerPayload = TryGetObjectProperty(message.Payload, "Payload") ?? message.Payload;

        // 广播给房间内其他玩家（排除发送者自己，避免本地“重复回显”）。
        room.BroadcastMessage(new NetworkMessage
        {
            Type = innerType,
            Payload = innerPayload,
            SenderPlayerId = session.PlayerId
        }, excludePlayerId: session.PlayerId);
    }

    /// <summary>
    /// 点对点转发：将消息转发给目标玩家（由服务端作为中继）。
    /// </summary>
    /// <param name="session">发送者会话。</param>
    /// <param name="message">外层 DirectMessage（payload 内包含 TargetPlayerId/Type/Payload）。</param>
    /// <param name="deliveryMethod">投递方式（由客户端选择，服务端仅透传）。</param>
    private void HandleDirectMessage(PlayerSession session, NetworkMessage message, DeliveryMethod deliveryMethod)
    {
        // 解析目标玩家 ID。
        string targetPlayerId = TryGetStringProperty(message.Payload, "TargetPlayerId");

        // 解析内层类型与负载（同 RoomMessage 的结构）。
        string innerType = TryGetStringProperty(message.Payload, "Type") ?? "DirectMessage";
        object innerPayload = TryGetObjectProperty(message.Payload, "Payload") ?? message.Payload;

        // FullSync 控制消息：强制走“按房间作用域 + 房主/请求方定向”逻辑，避免滥用 DirectMessage 绕过房间隔离。
        if (string.Equals(innerType, NetworkMessageTypes.FullStateSyncRequest, StringComparison.Ordinal))
        {
            HandleFullStateSyncRequest(session, new NetworkMessage
            {
                Type = innerType,
                Payload = innerPayload,
                SenderPlayerId = session.PlayerId
            }, deliveryMethod);
            return;
        }

        if (string.Equals(innerType, NetworkMessageTypes.FullStateSyncResponse, StringComparison.Ordinal))
        {
            HandleFullStateSyncResponse(session, new NetworkMessage
            {
                Type = innerType,
                Payload = innerPayload,
                SenderPlayerId = session.PlayerId
            }, deliveryMethod);
            return;
        }

        if (string.IsNullOrEmpty(targetPlayerId) || !_connectionsByPlayerId.TryGetValue(targetPlayerId, out var connection))
        {
            // 目标不在线/不存在：拒绝发送。
            SendErrorMessage(session.Peer, "DirectMessageFailed", "Target not found");
            return;
        }

        // 通过 NetworkConnection 向目标 peer 发送消息。
        connection.SendMessage(new NetworkMessage
        {
            Type = innerType,
            Payload = innerPayload,
            SenderPlayerId = session.PlayerId
        }, deliveryMethod);
    }

    /// <summary>
    /// 房主踢人：把目标玩家从房间移除并广播成员列表。
    /// </summary>
    /// <param name="session">操作者会话（必须是房主）。</param>
    /// <param name="message">踢人消息（payload 内包含 TargetPlayerId）。</param>
    private void HandleKickPlayer(PlayerSession session, NetworkMessage message)
    {
        // 必须在房间内才能踢人。
        if (string.IsNullOrEmpty(session.CurrentRoomId) || !_rooms.TryGetValue(session.CurrentRoomId, out var room))
        {
            SendErrorMessage(session.Peer, "KickPlayerFailed", "Not in room");
            return;
        }

        // 权限校验：只有房主可以踢人（避免普通成员恶意踢人）。
        if (!string.Equals(room.HostPlayerId, session.PlayerId, StringComparison.Ordinal))
        {
            SendErrorMessage(session.Peer, "KickPlayerFailed", "Only host can kick");
            return;
        }

        // 校验目标是否在房间内。
        string targetPlayerId = TryGetStringProperty(message.Payload, "TargetPlayerId");
        if (string.IsNullOrEmpty(targetPlayerId) || !room.ContainsPlayer(targetPlayerId))
        {
            SendErrorMessage(session.Peer, "KickPlayerFailed", "Target not in room");
            return;
        }

        // 从房间移除目标，并广播成员列表（目标客户端需自行处理被踢后的 UI/状态）。
        room.RemovePlayer(targetPlayerId);
        BroadcastPlayerList(room);
    }

    #endregion

    #region 重连与 NAT 穿透

    /// <summary>
    /// 重连请求结构：客户端需携带 <c>PlayerId</c> 与 <c>ReconnectToken</c> 以证明身份。
    /// </summary>
    private sealed class ReconnectRequest
    {
        public string PlayerId { get; set; } = string.Empty;
        public string ReconnectToken { get; set; } = string.Empty;
    }

    /// <summary>
    /// 处理断线重连请求：在允许窗口内，将新的 peer 重新绑定到旧的 PlayerId 会话。
    /// </summary>
    /// <param name="fromPeer">发起重连请求的 peer（新连接）。</param>
    /// <param name="message">重连请求消息（payload 应为 JSON 字符串或可反序列化对象）。</param>
    private void HandleReconnectRequest(NetPeer fromPeer, NetworkMessage message)
    {
        try
        {
            // 这里期望 payload 为 JSON（字符串或 JsonElement）。
            // 若客户端传的是匿名对象，message.Payload.ToString() 可能不是 JSON，
            // 进而导致反序列化失败——失败时会走 catch 并返回 Server error。
            ReconnectRequest request = JsonSerializer.Deserialize<ReconnectRequest>(message.Payload?.ToString() ?? string.Empty);
            if (request == null || string.IsNullOrWhiteSpace(request.PlayerId) || string.IsNullOrWhiteSpace(request.ReconnectToken))
            {
                // 请求格式不合法：直接拒绝（避免后续空引用或绕过校验）。
                SendMessageToPeer(fromPeer, new NetworkMessage { Type = "Reconnect_RESPONSE", Payload = new { Success = false, Error = "Invalid request" }, SenderPlayerId = "SERVER" }, DeliveryMethod.ReliableOrdered);
                return;
            }

            lock (_lock)
            {
                // 1) 查找旧会话：只有“曾经握手成功并被服务端记录”的玩家才能重连。
                if (!_sessionsByPlayerId.TryGetValue(request.PlayerId, out var session))
                {
                    SendMessageToPeer(fromPeer, new NetworkMessage { Type = "Reconnect_RESPONSE", Payload = new { Success = false, Error = "Unknown playerId" }, SenderPlayerId = "SERVER" }, DeliveryMethod.ReliableOrdered);
                    return;
                }

                // 2) 避免重复连接：如果该 PlayerId 仍处于 connected，说明没有真正断线或状态未同步。
                if (session.IsConnected)
                {
                    SendMessageToPeer(fromPeer, new NetworkMessage { Type = "Reconnect_RESPONSE", Payload = new { Success = false, Error = "Already connected" }, SenderPlayerId = "SERVER" }, DeliveryMethod.ReliableOrdered);
                    return;
                }

                // 3) token 校验：防止“猜 playerId”后伪造重连占用他人会话。
                string expectedToken = TryGetMetadataString(session.Metadata, "ReconnectToken");
                if (!string.Equals(expectedToken, request.ReconnectToken, StringComparison.Ordinal))
                {
                    SendMessageToPeer(fromPeer, new NetworkMessage { Type = "Reconnect_RESPONSE", Payload = new { Success = false, Error = "Invalid token" }, SenderPlayerId = "SERVER" }, DeliveryMethod.ReliableOrdered);
                    return;
                }

                // 4) 重连窗口校验：超过窗口视为“彻底离线”，避免服务端长期保留会话占用资源。
                if (_disconnectedAtByPlayerId.TryGetValue(request.PlayerId, out var disconnectedAt) &&
                    DateTime.UtcNow - disconnectedAt > _reconnectGracePeriod)
                {
                    SendMessageToPeer(fromPeer, new NetworkMessage { Type = "Reconnect_RESPONSE", Payload = new { Success = false, Error = "Reconnect window expired" }, SenderPlayerId = "SERVER" }, DeliveryMethod.ReliableOrdered);
                    return;
                }

                // 5) 重绑定 peer：后续所有发往该 PlayerId 的消息都应走新的 peer。
                session.Peer = fromPeer;
                session.IsConnected = true;
                session.UpdateHeartbeat();
                session.UpdateMessageTime();

                // 6) 维护 peer->session 索引：老 peer 在断线时已移除，这里写入新 peer。
                _sessionsByPeer[fromPeer] = session;

                // 7) 重建连接包装：确保房间层能继续向该玩家发消息（并保持当前房间信息）。
                _connectionsByPlayerId[session.PlayerId] = new NetworkConnection(fromPeer, session.PlayerId, session.CurrentRoomId);

                // 8) 清除断线时间：表示已在窗口内成功重连。
                _disconnectedAtByPlayerId.Remove(session.PlayerId);

                // 9) 回包：告知客户端重连成功，并返回服务端视角的基础状态（房间/昵称等）。
                SendMessageToPeer(fromPeer, new NetworkMessage
                {
                    Type = "Reconnect_RESPONSE",
                    Payload = new
                    {
                        Success = true,
                        PlayerId = session.PlayerId,
                        PlayerName = session.PlayerName,
                        CurrentRoomId = session.CurrentRoomId
                    },
                    SenderPlayerId = "SERVER"
                }, DeliveryMethod.ReliableOrdered);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[RelayServer] Error handling Reconnect_REQUEST");
            SendMessageToPeer(fromPeer, new NetworkMessage { Type = "Reconnect_RESPONSE", Payload = new { Success = false, Error = "Server error" }, SenderPlayerId = "SERVER" }, DeliveryMethod.ReliableOrdered);
        }
    }

    /// <summary>
    /// 客户端上报 NAT 信息：服务端缓存后供其他玩家查询（用于打洞/直连协商）。
    /// </summary>
    /// <param name="session">上报者会话。</param>
    /// <param name="message">上报消息（payload 应包含 NatInfo）。</param>
    private void HandleNatInfoReport(PlayerSession session, NetworkMessage message)
    {
        try
        {
            // 把 payload 统一转成 JsonElement，便于做字段校验与反序列化。
            JsonElement root = GetJsonElement(message.Payload);
            if (root.ValueKind != JsonValueKind.Object)
            {
                // payload 不合法：返回错误，客户端可提示“网络信息上报失败”。
                SendMessageToPeer(session.Peer, new NetworkMessage { Type = NetworkMessageTypes.NatError, Payload = new { Error = "Invalid payload" }, SenderPlayerId = "SERVER" }, DeliveryMethod.ReliableOrdered);
                return;
            }

            // 将 NAT 信息反序列化为 NatTraversal.NatInfo；具体字段由 NatTraversal 定义。
            NatTraversal.NatInfo natInfo = JsonSerializer.Deserialize<NatTraversal.NatInfo>(root.GetRawText());
            if (natInfo == null)
            {
                SendMessageToPeer(session.Peer, new NetworkMessage { Type = NetworkMessageTypes.NatError, Payload = new { Error = "Invalid natInfo" }, SenderPlayerId = "SERVER" }, DeliveryMethod.ReliableOrdered);
                return;
            }

            // 注册到 NAT 信息表：Key=PlayerId。
            NatTraversal.RegisterPeerNatInfo(session.PlayerId, natInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[RelayServer] Error handling NatInfoReport");
        }
    }

    /// <summary>
    /// 查询目标玩家 NAT 信息：返回给请求方（用于 P2P 打洞）。
    /// </summary>
    /// <param name="session">请求方会话。</param>
    /// <param name="message">请求消息（payload 内包含 TargetPlayerId）。</param>
    private void HandleNatInfoRequest(PlayerSession session, NetworkMessage message)
    {
        try
        {
            // 目标玩家 ID 必须提供，否则无法定位 NAT 信息。
            string targetPlayerId = TryGetStringProperty(message.Payload, "TargetPlayerId");
            if (string.IsNullOrWhiteSpace(targetPlayerId))
            {
                SendMessageToPeer(session.Peer, new NetworkMessage { Type = NetworkMessageTypes.NatError, Payload = new { Error = "Missing TargetPlayerId" }, SenderPlayerId = "SERVER" }, DeliveryMethod.ReliableOrdered);
                return;
            }

            // 从 NAT 信息表中读取目标 NAT 信息。
            NatTraversal.NatInfo natInfo = NatTraversal.GetPeerNatInfo(targetPlayerId);
            if (natInfo == null)
            {
                SendMessageToPeer(session.Peer, new NetworkMessage { Type = NetworkMessageTypes.NatError, Payload = new { Error = "Nat info not found" }, SenderPlayerId = "SERVER" }, DeliveryMethod.ReliableOrdered);
                return;
            }

            // 返回 NAT 信息：请求方拿到后可尝试与目标建立直连。
            SendMessageToPeer(session.Peer, new NetworkMessage
            {
                Type = NetworkMessageTypes.NatInfoResponse,
                Payload = new { TargetPlayerId = targetPlayerId, NatInfo = natInfo },
                SenderPlayerId = "SERVER"
            }, DeliveryMethod.ReliableOrdered);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[RelayServer] Error handling NatInfoRequest");
            SendMessageToPeer(session.Peer, new NetworkMessage { Type = NetworkMessageTypes.NatError, Payload = new { Error = "Server error" }, SenderPlayerId = "SERVER" }, DeliveryMethod.ReliableOrdered);
        }
    }

    #endregion

    #region 玩家信息与状态（兼容消息）

    /// <summary>
    /// 兼容 Host 模式的“玩家加入”消息：用于上报玩家昵称/角色等信息，并在房间内广播。
    /// </summary>
    /// <param name="session">上报者会话。</param>
    /// <param name="message">PlayerJoined 消息（payload 内可能包含 PlayerName/CharacterId）。</param>
    private void HandlePlayerJoined(PlayerSession session, NetworkMessage message)
    {
        try
        {
            // 统一解析 payload 为 JsonElement，便于读取字段（兼容 object/string/json）。
            JsonElement root = GetJsonElement(message.Payload);
            if (root.ValueKind == JsonValueKind.Object)
            {
                // 玩家昵称：用于房间列表显示。
                if (root.TryGetProperty("PlayerName", out var nameElem) && nameElem.ValueKind == JsonValueKind.String)
                {
                    session.PlayerName = nameElem.GetString();
                }

                // 角色信息：用于房间内 UI 展示（例如角色头像/名称）。
                if (root.TryGetProperty("CharacterId", out var charElem) && charElem.ValueKind == JsonValueKind.String)
                {
                    session.Metadata["CharacterId"] = charElem.GetString();
                }
            }

            if (!string.IsNullOrEmpty(session.CurrentRoomId) && _rooms.TryGetValue(session.CurrentRoomId, out var room))
            {
                // 广播给房间内其他玩家：告知“某玩家已加入并携带昵称/角色信息”。
                room.BroadcastMessage(new NetworkMessage
                {
                    Type = "PlayerJoined",
                    Payload = new
                    {
                        PlayerId = session.PlayerId,
                        PlayerName = session.PlayerName,
                        IsHost = string.Equals(room.HostPlayerId, session.PlayerId, StringComparison.Ordinal),
                        CharacterId = session.Metadata.TryGetValue("CharacterId", out var cid) ? cid?.ToString() : null
                    },
                    SenderPlayerId = "SERVER"
                }, excludePlayerId: session.PlayerId);

                // 同步成员列表：确保所有人看到一致的成员状态（房主/在线/昵称/位置等）。
                BroadcastPlayerList(room);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[RelayServer] Error handling PlayerJoined");
        }
    }

    /// <summary>
    /// 返回服务端视角的“自己信息”（用于客户端校验身份/显示）。
    /// </summary>
    /// <param name="session">请求者会话。</param>
    /// <param name="peer">请求者 peer。</param>
    private void HandleGetSelfRequest(PlayerSession session, NetPeer peer)
    {
        // 直接回包当前会话信息：PlayerId/昵称/是否房主/连接时间等。
        SendMessageToPeer(peer, new NetworkMessage
        {
            Type = "GetSelf_RESPONSE",
            Payload = new
            {
                PlayerId = session.PlayerId,
                PlayerName = session.PlayerName,
                IsHost = IsRoomHost(session),
                ConnectedAt = session.ConnectedAt.Ticks
            },
            SenderPlayerId = "SERVER"
        }, DeliveryMethod.ReliableOrdered);
    }

    /// <summary>
    /// 更新玩家地图位置/关卡等信息（用于房间 UI 同步展示）。
    /// </summary>
    /// <param name="session">上报者会话。</param>
    /// <param name="message">位置更新消息（payload 内包含 LocationX/LocationY/Stage/LocationName）。</param>
    private void HandleUpdatePlayerLocation(PlayerSession session, NetworkMessage message)
    {
        try
        {
            // payload 必须为对象才能读字段；否则忽略。
            JsonElement root = GetJsonElement(message.Payload);
            if (root.ValueKind != JsonValueKind.Object)
            {
                return;
            }

            // 逐字段读取并写入 session.Metadata：避免引入新的强类型字段导致会话结构变化过大。
            if (root.TryGetProperty("LocationX", out var xElem) && xElem.TryGetInt32(out int x))
            {
                session.Metadata["LocationX"] = x;
            }
            if (root.TryGetProperty("LocationY", out var yElem) && yElem.TryGetInt32(out int y))
            {
                session.Metadata["LocationY"] = y;
            }
            if (root.TryGetProperty("Stage", out var stageElem) && stageElem.TryGetInt32(out int stage))
            {
                session.Metadata["Stage"] = stage;
            }
            if (root.TryGetProperty("LocationName", out var nameElem) && nameElem.ValueKind == JsonValueKind.String)
            {
                session.Metadata["LocationName"] = nameElem.GetString();
            }

            if (!string.IsNullOrEmpty(session.CurrentRoomId) && _rooms.TryGetValue(session.CurrentRoomId, out var room))
            {
                // 位置变化后刷新成员列表：房间内其他玩家可以看到该玩家当前位置/关卡变化。
                BroadcastPlayerList(room);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[RelayServer] Error handling UpdatePlayerLocation");
        }
    }

    #endregion

    #region 游戏事件转发与成员列表广播

    /// <summary>
    /// 将游戏事件转发给同房间的其他玩家（中继广播）。
    /// </summary>
    /// <param name="session">发送者会话。</param>
    /// <param name="eventType">事件类型（通常为 On*/Battle*/Mana* 等）。</param>
    /// <param name="payload">事件负载（通常为 JSON 或可序列化对象）。</param>
    /// <param name="deliveryMethod">投递方式（可靠/不可靠）。</param>
    private void ForwardGameEventToRoom(PlayerSession session, string eventType, object payload, DeliveryMethod deliveryMethod)
    {
        // 没有房间：不转发（避免跨房间污染/避免单机状态误广播）。
        if (string.IsNullOrEmpty(session.CurrentRoomId) || !_rooms.TryGetValue(session.CurrentRoomId, out var room))
        {
            return;
        }

        // 广播给房间内其他玩家（排除发送者自己）。
        room.BroadcastMessage(new NetworkMessage
        {
            Type = eventType,
            Payload = payload,
            SenderPlayerId = session.PlayerId
        }, excludePlayerId: session.PlayerId);
    }

    /// <summary>
    /// 广播房间成员列表（用于大厅/房间 UI 显示）。
    /// </summary>
    /// <param name="room">房间实例。</param>
    private void BroadcastPlayerList(NetworkRoom room)
    {
        // 将 room 中维护的 PlayerId 列表映射到会话对象，构建可序列化的“成员快照”。
        // 说明：这里不直接暴露 PlayerSession/NetworkConnection，避免传输敏感/不可序列化对象。
        var players = room.GetAllPlayerIds()
            .Select(pid => _sessionsByPlayerId.TryGetValue(pid, out var s) ? s : null)
            .Where(s => s != null)
            .Select(s => new
            {
                PlayerId = s.PlayerId,
                PlayerName = s.PlayerName,
                IsHost = string.Equals(room.HostPlayerId, s.PlayerId, StringComparison.Ordinal),
                IsConnected = s.IsConnected,
                CharacterId = s.Metadata.TryGetValue("CharacterId", out var cid) ? cid?.ToString() : null,
                LocationX = TryGetMetadataInt(s.Metadata, "LocationX") ?? -1,
                LocationY = TryGetMetadataInt(s.Metadata, "LocationY") ?? -1,
                Stage = TryGetMetadataInt(s.Metadata, "Stage") ?? -1,
                LocationName = TryGetMetadataString(s.Metadata, "LocationName"),
            })
            .ToList();

        // 广播成员列表给房间内所有玩家。
        room.BroadcastMessage(new NetworkMessage
        {
            Type = "PlayerListUpdate",
            Payload = new { Players = players },
            SenderPlayerId = "SERVER"
        });
    }

    /// <summary>
    /// 判断会话是否为其当前房间的房主。
    /// </summary>
    /// <param name="session">会话。</param>
    /// <returns>是房主返回 true，否则 false。</returns>
    private bool IsRoomHost(PlayerSession session)
    {
        return !string.IsNullOrEmpty(session.CurrentRoomId)
               && _rooms.TryGetValue(session.CurrentRoomId, out var room)
               && string.Equals(room.HostPlayerId, session.PlayerId, StringComparison.Ordinal);
    }

    #endregion

    #region 消息发送与协议辅助

    /// <summary>
    /// 向指定 peer 发送一条 <see cref="NetworkMessage"/>。
    /// </summary>
    /// <param name="peer">目标 peer。</param>
    /// <param name="message">消息对象。</param>
    /// <param name="deliveryMethod">投递方式。</param>
    private void SendMessageToPeer(NetPeer peer, NetworkMessage message, DeliveryMethod deliveryMethod)
    {
        try
        {
            NetDataWriter writer = new();

            // 协议格式（LiteNetLib 自定义）：先写入 Type，再写入序列化后的 Payload（JSON 字符串）。
            // 注意：如果 payload 本身就是 JSON 字符串，这里再次 Serialize 会把它当作普通字符串加引号。
            // 目前客户端侧一般用 JsonSerializer 再解析一次，因此保持“总是 JSON 字符串”更一致。
            writer.Put(message.Type);
            writer.Put(JsonCompat.Serialize(message.Payload));

            // 发送：具体是否可靠/有序由 deliveryMethod 决定。
            peer.Send(writer, deliveryMethod);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[RelayServer] Failed to send message to peer");
        }
    }

    /// <summary>
    /// 发送统一错误消息（Type=Error，payload 包含 ErrorType 与 Message）。
    /// </summary>
    /// <param name="peer">目标 peer。</param>
    /// <param name="errorType">错误类型标识（便于客户端区分来源）。</param>
    /// <param name="errorMessage">错误详情。</param>
    private void SendErrorMessage(NetPeer peer, string errorType, string errorMessage)
    {
        // 错误消息一律用可靠有序投递，确保客户端一定能收到并提示。
        SendMessageToPeer(peer, new NetworkMessage
        {
            Type = "Error",
            Payload = new { ErrorType = errorType, Message = errorMessage },
            SenderPlayerId = "SERVER"
        }, DeliveryMethod.ReliableOrdered);
    }

    /// <summary>
    /// 根据 peer 反查 PlayerId（用于日志或 SenderPlayerId 填充）。
    /// </summary>
    private string GetPlayerId(NetPeer peer)
    {
        return _sessionsByPeer.TryGetValue(peer, out var session) ? session.PlayerId : "unknown";
    }

    /// <summary>
    /// 判断一个消息类型是否属于“游戏事件”范畴（需要转发到房间）。
    /// </summary>
    /// <param name="messageType">消息类型。</param>
    /// <returns>是游戏事件返回 true，否则 false。</returns>
    private static bool IsGameEvent(string messageType)
    {
        // 约定：多数同步事件以固定前缀命名（On*/Battle*/Mana*/Gap*）。
        // 特殊类型：聊天与状态同步请求/响应也视作游戏事件，走房间广播/转发通道。
        return messageType.StartsWith("On", StringComparison.Ordinal) ||
               messageType.StartsWith("Mana", StringComparison.Ordinal) ||
               messageType.StartsWith("Gap", StringComparison.Ordinal) ||
               messageType.StartsWith("Battle", StringComparison.Ordinal) ||
               messageType == NetworkMessageTypes.ChatMessage ||
               messageType == "StateSyncRequest" ||
               messageType == NetworkMessageTypes.RoomStateBroadcast;
    }

    /// <summary>
    /// Relay 模式下的房间状态请求：按 roomId 作用域定向转发给房间房主。
    /// </summary>
    private void HandleRoomStateRequest(PlayerSession session, NetworkMessage message, DeliveryMethod deliveryMethod)
    {
        string roomId = TryGetStringProperty(message.Payload, "RoomId") ?? session.CurrentRoomId;
        if (string.IsNullOrWhiteSpace(roomId) || !_rooms.TryGetValue(roomId, out var room))
        {
            SendErrorMessage(session.Peer, "RoomStateRequestFailed", "Room not found");
            return;
        }

        if (!string.Equals(session.CurrentRoomId, roomId, StringComparison.Ordinal) || !room.ContainsPlayer(session.PlayerId))
        {
            SendErrorMessage(session.Peer, "RoomStateRequestFailed", "Not in room");
            return;
        }

        string hostPlayerId = room.HostPlayerId;
        if (string.IsNullOrWhiteSpace(hostPlayerId) || !_connectionsByPlayerId.TryGetValue(hostPlayerId, out var hostConnection))
        {
            SendErrorMessage(session.Peer, "RoomStateRequestFailed", "Host not available");
            return;
        }

        // 确保 payload 带 RoomId，便于房主侧校验/日志。
        object payloadToHost = EnsureRoomIdInPayload(message.Payload, roomId);

        hostConnection.SendMessage(new NetworkMessage
        {
            Type = NetworkMessageTypes.RoomStateRequest,
            Payload = payloadToHost,
            SenderPlayerId = session.PlayerId
        }, deliveryMethod);
    }

    /// <summary>
    /// Relay 模式下的房间状态上传：上传定向转发给房间房主。
    /// </summary>
    private void HandleRoomStateUpload(PlayerSession session, NetworkMessage message, DeliveryMethod deliveryMethod)
    {
        string roomId = TryGetStringProperty(message.Payload, "RoomId") ?? session.CurrentRoomId;
        if (string.IsNullOrWhiteSpace(roomId) || !_rooms.TryGetValue(roomId, out var room))
        {
            SendErrorMessage(session.Peer, "RoomStateUploadFailed", "Room not found");
            return;
        }

        if (!string.Equals(session.CurrentRoomId, roomId, StringComparison.Ordinal) || !room.ContainsPlayer(session.PlayerId))
        {
            SendErrorMessage(session.Peer, "RoomStateUploadFailed", "Not in room");
            return;
        }

        string hostPlayerId = room.HostPlayerId;
        if (string.IsNullOrWhiteSpace(hostPlayerId) || !_connectionsByPlayerId.TryGetValue(hostPlayerId, out var hostConnection))
        {
            SendErrorMessage(session.Peer, "RoomStateUploadFailed", "Host not available");
            return;
        }

        object payloadToHost = EnsureRoomIdInPayload(message.Payload, roomId);

        hostConnection.SendMessage(new NetworkMessage
        {
            Type = NetworkMessageTypes.RoomStateUpload,
            Payload = payloadToHost,
            SenderPlayerId = session.PlayerId
        }, deliveryMethod);
    }

    /// <summary>
    /// Relay 模式下的房间状态响应：仅单播给请求方（payload.TargetPlayerId 或 payload.RequesterId）。
    /// </summary>
    private void HandleRoomStateResponse(PlayerSession session, NetworkMessage message, DeliveryMethod deliveryMethod)
    {
        string roomId = TryGetStringProperty(message.Payload, "RoomId") ?? session.CurrentRoomId;
        if (string.IsNullOrWhiteSpace(roomId) || !_rooms.TryGetValue(roomId, out var room))
        {
            SendErrorMessage(session.Peer, "RoomStateResponseFailed", "Room not found");
            return;
        }

        // 仅房主可发送 Response，避免伪造。
        if (!string.Equals(room.HostPlayerId, session.PlayerId, StringComparison.Ordinal))
        {
            SendErrorMessage(session.Peer, "RoomStateResponseFailed", "Not host");
            return;
        }

        string targetPlayerId = TryGetStringProperty(message.Payload, "TargetPlayerId") ?? TryGetStringProperty(message.Payload, "RequesterId");
        if (string.IsNullOrWhiteSpace(targetPlayerId) || !_connectionsByPlayerId.TryGetValue(targetPlayerId, out var targetConnection))
        {
            SendErrorMessage(session.Peer, "RoomStateResponseFailed", "Target not available");
            return;
        }

        object payloadToTarget = EnsureRoomIdInPayload(message.Payload, roomId);

        targetConnection.SendMessage(new NetworkMessage
        {
            Type = NetworkMessageTypes.RoomStateResponse,
            Payload = payloadToTarget,
            SenderPlayerId = session.PlayerId
        }, deliveryMethod);
    }

    /// <summary>
    /// Relay 模式下的完整状态快照请求路由：按 roomId 作用域转发给房间房主。
    /// </summary>
    /// <param name="session">发起请求的会话（通常是中途加入者）。</param>
    /// <param name="message">请求消息（payload 内应包含 RequestId/RoomId/TargetPlayerId/LastKnownEventIndex/JoinToken）。</param>
    /// <param name="deliveryMethod">投递方式（通常为可靠有序）。</param>
    private void HandleFullStateSyncRequest(PlayerSession session, NetworkMessage message, DeliveryMethod deliveryMethod)
    {
        // 作用域：优先取 payload.RoomId；否则回退 session.CurrentRoomId。
        string roomId = TryGetStringProperty(message.Payload, "RoomId") ?? session.CurrentRoomId;
        if (string.IsNullOrWhiteSpace(roomId) || !_rooms.TryGetValue(roomId, out var room))
        {
            SendErrorMessage(session.Peer, "FullStateSyncRequestFailed", "Room not found");
            return;
        }

        // 发送者必须属于该房间，避免跨房间请求窥探。
        if (!string.Equals(session.CurrentRoomId, roomId, StringComparison.Ordinal) || !room.ContainsPlayer(session.PlayerId))
        {
            SendErrorMessage(session.Peer, "FullStateSyncRequestFailed", "Not in room");
            return;
        }

        // 请求必须以自己为 TargetPlayerId（防止请求方代替他人拉取快照）。
        string targetPlayerId = TryGetStringProperty(message.Payload, "TargetPlayerId");
        if (!string.IsNullOrWhiteSpace(targetPlayerId) && !string.Equals(targetPlayerId, session.PlayerId, StringComparison.Ordinal))
        {
            SendErrorMessage(session.Peer, "FullStateSyncRequestFailed", "Target mismatch");
            return;
        }

        // 将请求定向转发给房主；房主侧由 MidGameJoinManager 生成快照并回发 FullStateSyncResponse。
        string hostPlayerId = room.HostPlayerId;
        if (string.IsNullOrWhiteSpace(hostPlayerId) || !_connectionsByPlayerId.TryGetValue(hostPlayerId, out var hostConnection))
        {
            SendErrorMessage(session.Peer, "FullStateSyncRequestFailed", "Host not available");
            return;
        }

        // 确保 payload 带 RoomId，便于房主侧做一致性校验/日志。
        object payloadToHost = EnsureRoomIdInPayload(message.Payload, roomId);

        hostConnection.SendMessage(new NetworkMessage
        {
            Type = NetworkMessageTypes.FullStateSyncRequest,
            Payload = payloadToHost,
            SenderPlayerId = session.PlayerId
        }, deliveryMethod);
    }

    /// <summary>
    /// Relay 模式下的完整状态快照响应路由：仅单播给请求方（payload.TargetPlayerId）。
    /// </summary>
    /// <param name="session">发送响应的会话（必须是房主）。</param>
    /// <param name="message">响应消息（payload 内应包含 RequestId/TargetPlayerId/FullSnapshot/MissedEvents 等）。</param>
    /// <param name="deliveryMethod">投递方式（通常为可靠有序）。</param>
    private void HandleFullStateSyncResponse(PlayerSession session, NetworkMessage message, DeliveryMethod deliveryMethod)
    {
        // 房间作用域：优先取 payload.RoomId；否则回退 session.CurrentRoomId。
        string roomId = TryGetStringProperty(message.Payload, "RoomId") ?? session.CurrentRoomId;
        if (string.IsNullOrWhiteSpace(roomId) || !_rooms.TryGetValue(roomId, out var room))
        {
            SendErrorMessage(session.Peer, "FullStateSyncResponseFailed", "Room not found");
            return;
        }

        // 只有房主可发送 FullSync 响应，避免成员伪造快照。
        if (!string.Equals(room.HostPlayerId, session.PlayerId, StringComparison.Ordinal))
        {
            SendErrorMessage(session.Peer, "FullStateSyncResponseFailed", "Only host can respond");
            return;
        }

        string targetPlayerId = TryGetStringProperty(message.Payload, "TargetPlayerId");
        if (string.IsNullOrWhiteSpace(targetPlayerId))
        {
            SendErrorMessage(session.Peer, "FullStateSyncResponseFailed", "Missing TargetPlayerId");
            return;
        }

        // 目标必须仍在房间内，避免把快照发给无关会话。
        if (!room.ContainsPlayer(targetPlayerId) || !_connectionsByPlayerId.TryGetValue(targetPlayerId, out var targetConnection))
        {
            SendErrorMessage(session.Peer, "FullStateSyncResponseFailed", "Target not in room");
            return;
        }

        targetConnection.SendMessage(new NetworkMessage
        {
            Type = NetworkMessageTypes.FullStateSyncResponse,
            Payload = message.Payload,
            SenderPlayerId = session.PlayerId
        }, deliveryMethod);
    }

    /// <summary>
    /// 为 FullSync 请求确保 payload 包含 RoomId 字段（保持原字段结构平铺，避免嵌套改变协议形状）。
    /// </summary>
    /// <param name="payload">原始 payload（可能是 JSON 字符串/JsonElement/匿名对象）。</param>
    /// <param name="roomId">要补充的 RoomId。</param>
    /// <returns>包含 RoomId 的 payload（失败时回退为原 payload）。</returns>
    private static object EnsureRoomIdInPayload(object payload, string roomId)
    {
        if (string.IsNullOrWhiteSpace(roomId))
        {
            return payload;
        }

        try
        {
            // 已存在 RoomId：无需修改。
            string existingRoomId = TryGetStringProperty(payload, "RoomId");
            if (!string.IsNullOrWhiteSpace(existingRoomId))
            {
                return payload;
            }

            // 合并字段：使用字典承载，避免匿名对象反射复杂度。
            string json = payload is string s ? s : JsonCompat.Serialize(payload);
            var dict = JsonCompat.Deserialize<Dictionary<string, object>>(json) ?? new Dictionary<string, object>();
            dict["RoomId"] = roomId;
            return dict;
        }
        catch
        {
            // 保守回退：不因为补字段失败而阻断请求。
            return payload;
        }
    }

    #endregion

    #region JSON/Metadata 工具

    /// <summary>
    /// 从会话 metadata 中读取 int 值（兼容 int/long/double/string/JsonElement 等多种来源）。
    /// </summary>
    /// <param name="metadata">会话 metadata。</param>
    /// <param name="key">键。</param>
    /// <returns>解析成功返回 int，否则返回 null。</returns>
    private static int? TryGetMetadataInt(Dictionary<string, object> metadata, string key)
    {
        if (metadata == null || !metadata.TryGetValue(key, out object value) || value == null)
        {
            return null;
        }

        // 说明：这里做宽松解析，避免因不同模块写入的类型差异导致 UI/广播构建失败。
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

    /// <summary>
    /// 从会话 metadata 中读取 string 值（兼容 string/JsonElement/其它类型 ToString）。
    /// </summary>
    /// <param name="metadata">会话 metadata。</param>
    /// <param name="key">键。</param>
    /// <returns>读取成功返回字符串，否则返回 null。</returns>
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

    /// <summary>
    /// 从 payload（object/string/json）中读取指定字段的 string 值。
    /// </summary>
    private static string TryGetStringProperty(object payload, string propertyName)
    {
        JsonElement root = GetJsonElement(payload);
        if (root.ValueKind == JsonValueKind.Object &&
            root.TryGetProperty(propertyName, out var value) &&
            value.ValueKind == JsonValueKind.String)
        {
            return value.GetString();
        }

        return null;
    }

    /// <summary>
    /// 从 payload 中读取指定字段的 object 值（返回 JsonElement 或基础类型）。
    /// </summary>
    private static object TryGetObjectProperty(object payload, string propertyName)
    {
        JsonElement root = GetJsonElement(payload);
        if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty(propertyName, out var value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.Object => value,
            JsonValueKind.Array => value,
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number => value.TryGetInt64(out long l) ? l : null,
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => null
        };
    }

    /// <summary>
    /// 将任意 payload 归一化为 <see cref="JsonElement"/>（用于字段读取/反序列化）。
    /// </summary>
    private static JsonElement GetJsonElement(object payload)
    {
        try
        {
            // 常见来源：
            // - JsonElement：直接使用
            // - string：视为 JSON 文本
            // - 其它对象：先 Serialize 再 Deserialize 成 JsonElement
            return JsonCompat.ToJsonElement(payload);
        }
        catch
        {
            // 解析失败：返回 default（ValueKind=Undefined），调用方需做 ValueKind 校验。
            return default;
        }
    }

    #endregion

    #region ServerCore 创建

    /// <summary>
    /// 根据配置创建底层 <see cref="IServerCore"/>（统一承载 LiteNetLib/线程/队列）。
    /// </summary>
    private static IServerCore CreateCore(ConfigManager configManager, ILogger<RelayServer> logger)
    {
        // 把配置映射为 ServerOptions：端口、最大连接数、连接密钥、超时等。
        return new ServerCore(
            new ServerOptions
            {
                Port = configManager.RelayServerPort.Value,
                MaxConnections = configManager.RelayServerMaxConnections.Value,
                ConnectionKey = configManager.RelayServerConnectionKey.Value,
                DisconnectTimeoutMs = configManager.NetworkTimeoutSeconds.Value * 1000,
                PingIntervalMs = 1000,
                MaxQueueSize = 2000,
                UseBackgroundThread = true,
                BackgroundThreadSleepMs = 15,
            },
            new MicrosoftServerLogger(logger));
    }

    #endregion

    #region BaseGameServer 覆写（核心行为）

    /// <summary>
    /// 重连宽限期：基类用它判断断线会话是否仍允许重连。
    /// </summary>
    protected override TimeSpan ReconnectGracePeriod => _reconnectGracePeriod;

    /// <summary>
    /// 生成玩家 ID（在 Relay 模式下通常是随机 GUID）。
    /// </summary>
    /// <param name="peer">当前连接 peer（此实现不使用该参数）。</param>
    /// <returns>玩家 ID 字符串。</returns>
    protected override string CreatePlayerId(NetPeer peer) => GeneratePlayerId();

    /// <summary>
    /// 创建一个新的会话对象（用于新连接握手完成后的初始化）。
    /// </summary>
    /// <param name="peer">连接 peer。</param>
    /// <param name="playerId">为该连接分配的 playerId。</param>
    /// <returns>初始化后的会话。</returns>
    protected override PlayerSession CreateSession(NetPeer peer, string playerId)
    {
        // 初始化的字段尽量保持“可用默认值”：
        // - PlayerName：默认使用 playerId 前缀，后续可通过 PlayerJoined 更新昵称。
        // - IsHost：由 NetworkRoom 决定；会话初始不是房主。
        // - CurrentRoomId：空字符串表示尚未进入房间。
        return new PlayerSession
        {
            Peer = peer,
            PlayerId = playerId,
            PlayerName = $"Player_{playerId[..6]}",
            ConnectedAt = DateTime.UtcNow,
            LastHeartbeat = DateTime.UtcNow,
            LastMessageAt = DateTime.UtcNow,
            IsConnected = true,
            IsHost = false,
            CurrentRoomId = string.Empty
        };
    }

    /// <summary>
    /// 判断某消息类型是否应视为“游戏事件”（由基类决定是否进入 <see cref="HandleGameEvent"/>）。
    /// </summary>
    protected override bool IsGameEventType(string messageType) => IsGameEvent(messageType);

    /// <summary>
    /// 基类判定为“游戏事件”时的回调：这里复用本类的统一路由（最终会转发到房间）。
    /// </summary>
    protected override void HandleGameEvent(PlayerSession session, string eventType, string jsonPayload, DeliveryMethod deliveryMethod)
    {
        // 注意：基类传入的 jsonPayload 为 string，这里作为 Payload 透传给 ProcessMessage。
        ProcessMessage(session.Peer, new NetworkMessage
        {
            Type = eventType,
            Payload = jsonPayload,
            SenderPlayerId = session.PlayerId
        }, deliveryMethod);
    }
 
    /// <summary>
    /// 基类判定为“系统消息”时的回调：这里同样复用统一路由。
    /// </summary>
    protected override void HandleSystemMessage(PlayerSession session, string messageType, string jsonPayload, DeliveryMethod deliveryMethod)
    {
        ProcessMessage(session.Peer, new NetworkMessage
        {
            Type = messageType,
            Payload = jsonPayload,
            SenderPlayerId = session.PlayerId
        }, deliveryMethod);
    }

    /// <summary>
    /// 会话建立后回调（由基类触发）：这里负责建立连接包装并下发 Welcome（playerId/token）。
    /// </summary>
    /// <param name="session">已初始化的会话。</param>
    protected override void OnSessionConnected(PlayerSession session)
    {
        lock (_lock)
        {
            // 更新/建立连接包装：后续房间广播与点对点转发都依赖该字典。
            _connectionsByPlayerId[session.PlayerId] = new NetworkConnection(session.Peer, session.PlayerId);
        }

        // 下发 Welcome：客户端需要保存 playerId 与 reconnectToken（用于 Reconnect_REQUEST）。
        SendMessageToPeer(session.Peer, new NetworkMessage
        {
            Type = "Welcome",
            Payload = new
            {
                PlayerId = session.PlayerId,
                ReconnectToken = TryGetMetadataString(session.Metadata, "ReconnectToken"),
                ServerTime = DateTime.UtcNow.Ticks
            },
            SenderPlayerId = "SERVER"
        }, DeliveryMethod.ReliableOrdered);

        _logger.LogInformation($"[RelayServer] Client connected: {session.Peer.EndPoint}, PlayerId={session.PlayerId}");
    }

    /// <summary>
    /// 会话断开回调（由基类触发）：移出房间并清理连接包装。
    /// </summary>
    /// <param name="session">断开的会话。</param>
    /// <param name="disconnectInfo">断线信息（原因等）。</param>
    protected override void OnSessionDisconnected(PlayerSession session, DisconnectInfo disconnectInfo)
    {
        lock (_lock)
        {
            _logger.LogInformation($"[RelayServer] Client disconnected: {session.Peer.EndPoint}, Reason: {disconnectInfo.Reason}, PlayerId={session.PlayerId}");

            if (!string.IsNullOrEmpty(session.CurrentRoomId) && _rooms.TryGetValue(session.CurrentRoomId, out var room))
            {
                // 从房间移除并广播列表：让其他成员知道该玩家已离开/掉线。
                room.RemovePlayer(session.PlayerId);
                BroadcastPlayerList(room);

                if (room.PlayerCount == 0)
                {
                    // 最后一人离开：销毁房间。
                    _rooms.Remove(room.RoomId);
                    _logger.LogInformation($"[RelayServer] Room {room.RoomId} destroyed (empty)");
                }
            }

            // 清理连接包装：防止后续 DirectMessage 仍能找到该玩家并向旧 peer 发送。
            _connectionsByPlayerId.Remove(session.PlayerId);
        }
    }

    #endregion

    #region ID/Token 生成

    /// <summary>
    /// 生成玩家 ID（GUID，无分隔符）。
    /// </summary>
    private static string GeneratePlayerId()
    {
        return Guid.NewGuid().ToString("N");
    }

    /// <summary>
    /// 生成重连 token（GUID，无分隔符）。
    /// </summary>
    private static new string GenerateReconnectToken()
    {
        return Guid.NewGuid().ToString("N");
    }

    /// <summary>
    /// 生成短房间号（6 位大写字母/数字）。
    /// </summary>
    private static string GenerateRoomId()
    {
        Random random = new();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, 6).Select(s => s[random.Next(s.Length)]).ToArray());
    }

    #endregion
}
