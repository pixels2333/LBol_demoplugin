using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace NetworkPlugin.Network.Server;

/// <summary>
/// 网络房间 - 管理房间内的玩家和消息广播
/// </summary>
public class NetworkRoom(string roomId, RoomConfig config, ILogger logger)
{
    private readonly ILogger _logger = logger;
    private readonly object _lock = new();

    /// <summary>
    /// 房间ID
    /// </summary>
    public string RoomId { get; } = roomId;

    /// <summary>
    /// 房间配置
    /// </summary>
    public RoomConfig Config { get; } = config;

    /// <summary>
    /// 房间内的玩家连接
    /// </summary>
    private readonly Dictionary<string, NetworkConnection> _players = [];

    /// <summary>
    /// 房间创建时间
    /// </summary>
    public DateTime CreatedAt { get; } = DateTime.UtcNow;

    /// <summary>
    /// 房主ID
    /// </summary>
    public string HostPlayerId { get; private set; } = string.Empty;

    /// <summary>
    /// 房间是否满员
    /// </summary>
    public bool IsFull => _players.Count >= Config.MaxPlayers;

    /// <summary>
    /// 当前玩家数量
    /// </summary>
    public int PlayerCount => _players.Count;

    /// <summary>
    /// 最大玩家数
    /// </summary>
    public int MaxPlayers => Config.MaxPlayers;

    /// <summary>
    /// 房间是否在游戏中
    /// </summary>
    public bool IsInGame { get; private set; }

    /// <summary>
    /// 添加玩家到房间
    /// </summary>
    public JoinResult AddPlayer(string playerId, NetworkConnection connection)
    {
        lock (_lock)
        {
            if (_players.ContainsKey(playerId))
            {
                return JoinResult.Failed($"Player {playerId} already in room");
            }

            if (IsFull)
            {
                return JoinResult.Failed("Room is full");
            }

            _players[playerId] = connection;
            connection.CurrentRoomId = RoomId;

            // 第一个加入的玩家成为房主
            if (string.IsNullOrEmpty(HostPlayerId))
            {
                HostPlayerId = playerId;
                _logger.LogInformation($"[NetworkRoom] Player {playerId} became host of room {RoomId}");
            }

            _logger.LogInformation($"[NetworkRoom] Player {playerId} joined room {RoomId}, current count: {PlayerCount}");

            return JoinResult.Success();
        }
    }

    /// <summary>
    /// 从房间移除玩家
    /// </summary>
    public void RemovePlayer(string playerId)
    {
        lock (_lock)
        {
            if (!_players.ContainsKey(playerId))
            {
                return;
            }

            _players.Remove(playerId);

            // 如果房主离开，指定新房主
            if (HostPlayerId == playerId && _players.Count > 0)
            {
                HostPlayerId = _players.Keys.First();
                BroadcastMessage(new NetworkMessage
                {
                    Type = "HostChanged",
                    Payload = new { NewHostId = HostPlayerId }
                });
                _logger.LogInformation($"[NetworkRoom] Host changed to {HostPlayerId} in room {RoomId}");
            }

            _logger.LogInformation($"[NetworkRoom] Player {playerId} left room {RoomId}, current count: {PlayerCount}");
        }
    }

    /// <summary>
    /// 向房间所有玩家广播消息
    /// </summary>
    public void BroadcastMessage(NetworkMessage message, string? excludePlayerId = null)
    {
        lock (_lock)
        {
            foreach (var kvp in _players)
            {
                if (kvp.Key == excludePlayerId)
                {
                    continue;
                }

                try
                {
                    kvp.Value.SendMessage(message);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[NetworkRoom] Failed to send message to {kvp.Key}: {ex.Message}");
                }
            }
        }
    }

    /// <summary>
    /// 向特定玩家发送消息
    /// </summary>
    public void SendMessageToPlayer(string playerId, NetworkMessage message)
    {
        lock (_lock)
        {
            if (_players.TryGetValue(playerId, out var connection))
            {
                try
                {
                    connection.SendMessage(message);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[NetworkRoom] Failed to send message to {playerId}: {ex.Message}");
                }
            }
        }
    }

    /// <summary>
    /// 获取所有玩家ID
    /// </summary>
    public List<string> GetAllPlayerIds()
    {
        lock (_lock)
        {
            return _players.Keys.ToList();
        }
    }

    /// <summary>
    /// 开始游戏
    /// </summary>
    public void StartGame()
    {
        lock (_lock)
        {
            IsInGame = true;
            Config.IsGameStarted = true;

            BroadcastMessage(new NetworkMessage
            {
                Type = "GameStarted",
                Payload = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    PlayerCount = PlayerCount
                }
            });

            _logger.LogInformation($"[NetworkRoom] Game started in room {RoomId}, players: {PlayerCount}");
        }
    }

    /// <summary>
    /// 结束游戏
    /// </summary>
    public void EndGame()
    {
        lock (_lock)
        {
            IsInGame = false;
            Config.IsGameStarted = false;

            BroadcastMessage(new NetworkMessage
            {
                Type = "GameEnded",
                Payload = new
                {
                    Timestamp = DateTime.Now.Ticks
                }
            });

            _logger.LogInformation($"[NetworkRoom] Game ended in room {RoomId}");
        }
    }

    /// <summary>
    /// 检查玩家是否在房间中
    /// </summary>
    public bool ContainsPlayer(string playerId)
    {
        lock (_lock)
        {
            return _players.ContainsKey(playerId);
        }
    }

    /// <summary>
    /// 获取玩家连接
    /// </summary>
    public NetworkConnection? GetPlayerConnection(string playerId)
    {
        lock (_lock)
        {
            _players.TryGetValue(playerId, out var connection);
            return connection;
        }
    }

    /// <summary>
    /// 获取房间状态
    /// </summary>
    public RoomStatus GetStatus()
    {
        lock (_lock)
        {
            return new RoomStatus
            {
                RoomId = RoomId,
                PlayerCount = PlayerCount,
                MaxPlayers = MaxPlayers,
                HostPlayerId = HostPlayerId,
                IsInGame = IsInGame,
                CreatedAt = CreatedAt,
                PlayerIds = _players.Keys.ToList()
            };
        }
    }
}

 // 网络房间类：管理房间内的玩家连接、消息广播和游戏状态

/// <summary>
/// 加入房间结果
/// </summary>
public class JoinResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }

    public static JoinResult Success()
    {
        return new JoinResult { Success = true };
    }

    public static JoinResult Failed(string errorMessage)
    {
        return new JoinResult { Success = false, ErrorMessage = errorMessage };
    }
}

/// <summary>
/// 房间配置
/// </summary>
public class RoomConfig
{
    public int MaxPlayers { get; set; } = 4;
    public bool IsGameStarted { get; set; } = false;
    public bool IsPublic { get; set; } = true;
    public string? Password { get; set; }
    public string? GameMode { get; set; }
    public string? Description { get; set; }

    public static RoomConfig Default()
    {
        return new RoomConfig { MaxPlayers = 4 };
    }
}

/// <summary>
/// 房间状态信息
/// </summary>
public class RoomStatus
{
    public string RoomId { get; set; } = string.Empty;
    public int PlayerCount { get; set; }
    public int MaxPlayers { get; set; }
    public string HostPlayerId { get; set; } = string.Empty;
    public bool IsInGame { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string> PlayerIds { get; set; } = [];
    public int Ping { get; set; }
}

/// <summary>
/// 房间列表过滤器
/// </summary>
public class RoomFilter
{
    public bool? IsPublic { get; set; }
    public int? MaxPlayers { get; set; }
    public bool? IsInGame { get; set; }
    public string? GameMode { get; set; }
}
