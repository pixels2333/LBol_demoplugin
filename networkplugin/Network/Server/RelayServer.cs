using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.Extensions.Logging;

namespace NetworkPlugin.Network.Server;

/// <summary>
/// 基于LiteNetLib的中继服务器 - 为NAT穿透和P2P连接提供支持
/// 参考杀戮尖塔Together in Spire的P2P + Steam Relay架构
/// 重要性: ⭐⭐⭐⭐⭐ (网络架构基础)
/// TODO: 需要实现完整的房间创建、加入、消息转发逻辑
/// </summary>
public class RelayServer
{
    private readonly ILogger<RelayServer> _logger;
    private readonly IServiceProvider _serviceProvider;
    private EventBasedNetListener _listener;
    private NetManager _netManager;

    /// <summary>
    /// 服务器配置
    /// </summary>
    private readonly RelayServerConfig _config;

    /// <summary>
    /// 房间列表 - 房间ID到房间对象的映射
    /// </summary>
    private readonly Dictionary<string, NetworkRoom> _rooms;

    /// <summary>
    /// 玩家会话 - NetPeer到玩家会话的映射
    /// </summary>
    private readonly Dictionary<NetPeer, PlayerSession> _playerSessions;

    /// <summary>
    /// 服务器运行状态
    /// </summary>
    private bool _isRunning;

    /// <summary>
    /// 服务器线程
    /// </summary>
    private Thread? _serverThread;

    /// <summary>
    /// 取消令牌源
    /// </summary>
    private CancellationTokenSource? _cancellationTokenSource;

    /// <summary>
    /// 用于线程安全的锁对象
    /// </summary>
    private readonly object _lock = new();

    /// <summary>
    /// 实例化中继服务器
    /// </summary>
    public RelayServer(RelayServerConfig config, ILogger<RelayServer> logger, IServiceProvider serviceProvider)
    {
        _config = config;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _rooms = [];
        _playerSessions = [];
        _isRunning = false;

        InitializeNetManager();
    } 

    /// <summary>
    /// 初始化LiteNetLib网络管理器
    /// </summary>
    private void InitializeNetManager()
    {
        _listener = new EventBasedNetListener();
        _netManager = new NetManager(_listener)
        {
            IPv6Enabled = IPv6Mode.Disabled,
            UnsyncedDeliveryEvent = true,
            UnsyncedEvents = true,
            AutoRecycle = true,
            DisconnectTimeout = _config.DisconnectTimeoutSeconds * 1000,
            // PingInterval = 1000, // 每1秒发送一次ping
            // ConnectionRequestTime = 5000 // 连接请求超时时间
        };

        RegisterEvents();
    } // 初始化LiteNetLib网络管理器：配置网络参数、设置连接超时并注册事件处理器

    /// <summary>
    /// 注册LiteNetLib事件监听器
    /// </summary>
    private void RegisterEvents()
    {
        // 连接请求事件
        _listener.ConnectionRequestEvent += request =>
        {
            lock (_lock)
            {
                try
                {
                    _logger.LogInformation($"[RelayServer] Connection request from {request.RemoteEndPoint}");

                    // 检查最大连接数
                    if (_netManager.PeersCount >= _config.MaxConnections)
                    {
                        _logger.LogWarning($"[RelayServer] Rejecting connection: max connections reached");
                        request.Reject();
                        return;
                    }

                    // 读取连接密钥
                    var dataReader = request.Data;
                    string connectionKey = dataReader.GetString();

                    // 验证密钥
                    if (connectionKey != _config.ConnectionKey)
                    {
                        _logger.LogWarning($"[RelayServer] Rejecting connection: invalid key");
                        request.Reject();
                        return;
                    }

                    // 接受连接
                    var peer = request.Accept();
                    _logger.LogInformation($"[RelayServer] Accepted connection from {request.RemoteEndPoint}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[RelayServer] Error handling connection request: {ex.Message}");
                    request.Reject();
                }
            }
        };

        // 对等节点连接事件
        _listener.PeerConnectedEvent += peer =>
        {
            lock (_lock)
            {
                try
                {
                    _logger.LogInformation($"[RelayServer] Client connected: {peer.EndPoint}");

                    // 创建玩家会话
                    PlayerSession session = new PlayerSession
                    {
                        Peer = peer,
                        PlayerId = GeneratePlayerId(),
                        PlayerName = $"Player_{GeneratePlayerId().Substring(0, 6)}",
                        ConnectedAt = DateTime.UtcNow,
                        LastHeartbeat = DateTime.UtcNow,
                        IsConnected = true
                    };

                    _playerSessions[peer] = session;

                    // 发送欢迎消息
                    var welcomeMessage = new NetworkMessage
                    {
                        Type = "Welcome",
                        Payload = new
                        {
                            PlayerId = session.PlayerId,
                            ServerTime = DateTime.UtcNow.Ticks
                        },
                        SenderPlayerId = "SERVER"
                    };

                    SendMessageToPeer(peer, welcomeMessage, DeliveryMethod.ReliableOrdered);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[RelayServer] Error handling peer connected event: {ex.Message}");
                }
            }
        };

        // 对等节点断开事件
        _listener.PeerDisconnectedEvent += (peer, disconnectInfo) =>
        {
            lock (_lock)
            {
                try
                {
                    _logger.LogInformation($"[RelayServer] Client disconnected: {peer.EndPoint}, Reason: {disconnectInfo.Reason}");

                    if (_playerSessions.TryGetValue(peer, out var session))
                    {
                        session.IsConnected = false;

                        // 如果玩家在房间中，从房间移除
                        if (!string.IsNullOrEmpty(session.CurrentRoomId))
                        {
                            if (_rooms.TryGetValue(session.CurrentRoomId, out var room))
                            {
                                room.RemovePlayer(session.PlayerId);

                                // 如果房间为空，销毁房间
                                if (room.PlayerCount == 0)
                                {
                                    _rooms.Remove(session.CurrentRoomId);
                                    _logger.LogInformation($"[RelayServer] Room {session.CurrentRoomId} destroyed (empty)");
                                }
                            }
                        }

                        _playerSessions.Remove(peer);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[RelayServer] Error handling peer disconnected event: {ex.Message}");
                }
            }
        };

        // 网络接收事件
        _listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
        {
            try
            {
                // 读取消息类型
                var messageType = dataReader.GetString();

                // 读取消息负载（JSON字符串）
                var jsonPayload = dataReader.GetString();

                // 反序列化消息
                var message = new NetworkMessage
                {
                    Type = messageType,
                    Payload = jsonPayload,
                    SenderPlayerId = GetPlayerId(fromPeer)
                };

                // 处理消息
                ProcessMessage(fromPeer, message, deliveryMethod);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[RelayServer] Error processing message from {fromPeer?.EndPoint}: {ex.Message}");
            }
            finally
            {
                dataReader.Recycle();
            }
        };

        // 网络错误事件
        _listener.NetworkErrorEvent += (endPoint, socketError) =>
        {
            _logger.LogError($"[RelayServer] Network error from {endPoint}: {socketError}");
        };

        // 网络延迟更新事件（用于ping统计）
        _listener.NetworkLatencyUpdateEvent += (peer, latency) =>
        {
            if (_playerSessions.TryGetValue(peer, out var session))
            {
                session.Ping = latency;
                _logger.LogDebug($"[RelayServer] Player {session.PlayerId} ping updated: {latency}ms");
            }
        };
    } // 注册LiteNetLib事件监听器：监听连接、断开、消息接收等网络事件并处理

    /// <summary>
    /// 处理收到的消息
    /// </summary>
    private void ProcessMessage(NetPeer fromPeer, NetworkMessage message, DeliveryMethod deliveryMethod)
    {
        lock (_lock)
        {
            _logger.LogDebug($"[RelayServer] Processing message from {fromPeer.EndPoint}: {message.Type}");

            switch (message.Type)
            {
                case "CreateRoom":
                    HandleCreateRoom(fromPeer, message);
                    break;
                case "JoinRoom":
                    HandleJoinRoom(fromPeer, message);
                    break;
                case "LeaveRoom":
                    HandleLeaveRoom(fromPeer, message);
                    break;
                case "RoomMessage":
                    HandleRoomMessage(fromPeer, message, deliveryMethod);
                    break;
                case "DirectMessage":
                    HandleDirectMessage(fromPeer, message, deliveryMethod);
                    break;
                case "Heartbeat":
                    HandleHeartbeat(fromPeer, message);
                    break;
                case "GetRoomList":
                    HandleGetRoomList(fromPeer, message);
                    break;
                case "KickPlayer":
                    HandleKickPlayer(fromPeer, message);
                    break;
                default:
                    _logger.LogWarning($"[RelayServer] Unknown message type: {message.Type}");
                    break;
            }
        }
    } // 处理收到的消息：根据消息类型路由到相应的处理方法，支持房间创建、加入、消息等功能

    /// <summary>
    /// 启动中继服务器
    /// </summary>
    public void Start()
    {
        if (_isRunning)
        {
            _logger.LogWarning("[RelayServer] Server is already running");
            return;
        }

        try
        {
            _cancellationTokenSource = new CancellationTokenSource();

            // 启动LiteNetLib网络管理器
            _netManager.Start(_config.Port);
            _logger.LogInformation($"[RelayServer] Server started on port {_config.Port}");

            _isRunning = true;

            // 启动服务器主循环
            _serverThread = new Thread(ServerLoop)
            {
                IsBackground = true,
                Name = "RelayServer Main Loop"
            };
            _serverThread.Start();

            _logger.LogInformation($"[RelayServer] Server started successfully with max connections: {_config.MaxConnections}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"[RelayServer] Failed to start server: {ex.Message}");
            Stop();
            throw;
        }
    } // 启动中继服务器：启动网络管理器，开始监听连接并启动服务器主循环

    /// <summary>
    /// 停止中继服务器
    /// </summary>
    public void Stop()
    {
        if (!_isRunning)
        {
            return;
        }

        _logger.LogInformation("[RelayServer] Stopping server...");

        _isRunning = false;
        _cancellationTokenSource?.Cancel();

        // 通知所有连接断开
        lock (_lock)
        {
            foreach (var session in _playerSessions.Values)
            {
                if (session.IsConnected)
                {
                    session.Peer.Disconnect();
                }
            }
        }

        // 停止LiteNetLib网络管理器
        _netManager.Stop();

        // 清理状态
        lock (_lock)
        {
            _playerSessions.Clear();
            _rooms.Clear();
        }

        _logger.LogInformation("[RelayServer] Server stopped");
    } // 停止中继服务器：断开所有连接，停止网络管理器并清理资源

    /// <summary>
    /// 服务器主循环
    /// </summary>
    private void ServerLoop()
    {
        var token = _cancellationTokenSource!.Token;

        while (_isRunning && !token.IsCancellationRequested)
        {
            try
            {
                // 处理LiteNetLib事件
                _netManager.PollEvents();

                // 定期清理超时连接
                CleanupTimeoutConnections();

                // 定期清理空房间
                CleanupEmptyRooms();

                Thread.Sleep(15); // 避免CPU占用过高
            }
            catch (Exception ex)
            {
                _logger.LogError($"[RelayServer] Error in server loop: {ex.Message}");
            }
        }
    } // 服务器主循环：处理网络事件、清理超时连接和空房间，维持服务器运行状态

    /// <summary>
    /// 处理创建房间请求
    /// </summary>
    private void HandleCreateRoom(NetPeer fromPeer, NetworkMessage message)
    {
        try
        {
            var session = _playerSessions[fromPeer];

            // 如果玩家已在房间中，先离开
            if (!string.IsNullOrEmpty(session.CurrentRoomId))
            {
                HandleLeaveRoom(fromPeer, message);
            }

            // 获取房间配置
            var roomConfig = message.GetPayload<RoomConfig>() ?? RoomConfig.Default();

            // 生成唯一房间ID
            var roomId = GenerateRoomId();

            // 创建房间
            NetworkRoom room = new NetworkRoom(roomId, roomConfig, _logger);

            lock (_lock)
            {
                _rooms[roomId] = room;
            }

            // 让创建者加入房间
            var joinResult = room.AddPlayer(session.PlayerId, session);

            if (joinResult.Success)
            {
                session.CurrentRoomId = roomId;

                // 发送房间创建成功消息
                SendMessageToPeer(fromPeer, new NetworkMessage
                {
                    Type = "RoomCreated",
                    Payload = new
                    {
                        RoomId = roomId,
                        RoomConfig = room.Config,
                        PlayerId = session.PlayerId
                    },
                    SenderPlayerId = "SERVER"
                }, DeliveryMethod.ReliableOrdered);

                _logger.LogInformation($"[RelayServer] Room created: {roomId} by player {session.PlayerId}");
            }
            else
            {
                SendErrorMessage(fromPeer, "CreateRoomFailed", joinResult.ErrorMessage ?? "Unknown error");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"[RelayServer] Error handling CreateRoom: {ex.Message}");
            SendErrorMessage(fromPeer, "CreateRoomError", ex.Message);
        }
    } // 处理创建房间请求：为新玩家创建游戏房间，分配唯一ID并设置房主权限

    // TODO: 继续实现其他消息处理方法

    /// <summary>
    /// 生成玩家ID
    /// </summary>
    private string GeneratePlayerId()
    {
        return Guid.NewGuid().ToString("N");
    } // 生成玩家ID：为每个连接的玩家生成唯一标识符

    /// <summary>
    /// 生成房间ID
    /// </summary>
    private string GenerateRoomId()
    {
        // 简单的6位随机ID
        Random random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, 6)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    } // 生成房间ID：为游戏房间生成6位随机字母数字组合的标识符

    /// <summary>
    /// 向对等节点发送消息
    /// </summary>
    private void SendMessageToPeer(NetPeer peer, NetworkMessage message, DeliveryMethod deliveryMethod)
    {
        try
        {
            NetDataWriter writer = new NetDataWriter();
            writer.Put(message.Type);
            writer.Put(JsonSerializer.Serialize(message.Payload));
            peer.Send(writer, deliveryMethod);
        }
        catch (Exception ex)
        {
            _logger.LogError($"[RelayServer] Failed to send message to peer: {ex.Message}");
        }
    } // 向对等节点发送消息：序列化消息并通过LiteNetLib发送给指定客户端

    /// <summary>
    /// 发送错误消息
    /// </summary>
    private void SendErrorMessage(NetPeer peer, string errorType, string errorMessage)
    {
        var errorMessageObj = new NetworkMessage
        {
            Type = "Error",
            Payload = new
            {
                ErrorType = errorType,
                Message = errorMessage
            },
            SenderPlayerId = "SERVER"
        };
        SendMessageToPeer(peer, errorMessageObj, DeliveryMethod.ReliableOrdered);
    } // 发送错误消息：向客户端发送错误类型和错误信息的网络消息

    /// <summary>
    /// 清理超时连接
    /// </summary>
    private void CleanupTimeoutConnections()
    {
        // TODO: 实现连接超时检测逻辑
    } // 清理超时连接：检测并断开超过指定时间没有心跳的客户端连接

    /// <summary>
    /// 获取玩家ID
    /// </summary>
    private string GetPlayerId(NetPeer peer)
    {
        return _playerSessions.TryGetValue(peer, out var session) ? session.PlayerId : "unknown";
    } // 获取玩家ID：根据网络对等节点获取对应的玩家会话ID

    // TODO: 实现其他消息处理方法
    // HandleJoinRoom, HandleLeaveRoom, HandleRoomMessage, HandleDirectMessage, HandleHeartbeat, HandleGetRoomList, HandleKickPlayer
} // 基于LiteNetLib的中继服务器：为NAT穿透和P2P连接提供支持，管理房间创建、玩家连接和消息转发

/// <summary>
/// 中继服务器配置
/// </summary>
public class RelayServerConfig
{
    public int Port { get; set; } = 8888; // 服务器监听端口：默认8888端口
    public int MaxConnections { get; set; } = 1000; // 最大连接数：服务器最多支持的客户端连接数
    public int MaxRooms { get; set; } = 100; // 最大房间数：服务器最多支持的游戏房间数量
    public int MaxPlayersPerRoom { get; set; } = 4; // 每间房最大玩家数：单个房间支持的最大玩家数量
    public int DisconnectTimeoutSeconds { get; set; } = 30; // 断开连接超时：连接断开检测的超时时间
    public string ServerName { get; set; } = "LBoL Relay Server"; // 服务器名称：显示给客户端的服务器标识
    public string ConnectionKey { get; set; } = "LBoL_Network_Plugin"; // 连接密钥：客户端连接时需要验证的安全密钥
    public bool EnableNatPunchthrough { get; set; } = true; // 启用NAT穿透：是否开启NAT穿透功能
}
