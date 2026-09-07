using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Network.Server;

namespace NetworkPlugin.Network.Room;

public class NetworkRoom(string roomId, RoomConfig config, ILogger logger)
{
    #region 字段与属性

    private readonly ILogger _logger = logger;
    private readonly object _lock = new();

        public string RoomId { get; } = roomId;

        public RoomConfig Config { get; } = config;

        private readonly Dictionary<string, NetworkConnection> _players = [];

        public DateTime CreatedAt { get; } = DateTime.UtcNow;

        public string HostPlayerId { get; private set; } = string.Empty;

        public bool IsFull => _players.Count >= Config.MaxPlayers;

        public int PlayerCount => _players.Count;

        public int MaxPlayers => Config.MaxPlayers;

        public bool IsInGame { get; private set; }

    #endregion

    #region 玩家管理

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

            if (string.IsNullOrEmpty(HostPlayerId))
            {
                HostPlayerId = playerId;
                _logger.LogInformation($"[NetworkRoom] Player {playerId} became host of room {RoomId}");
            }

            _logger.LogInformation($"[NetworkRoom] Player {playerId} joined room {RoomId}, current count: {PlayerCount}");

            return JoinResult.Success();
        }
    }

        public void RemovePlayer(string playerId)
    {
        lock (_lock)
        {
            if (!_players.ContainsKey(playerId))
            {
                return;
            }

            _players.Remove(playerId);

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

        public List<string> GetAllPlayerIds()
    {
        lock (_lock)
        {
            return [.. _players.Keys];
        }
    }

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

        public bool ContainsPlayer(string playerId)
    {
        lock (_lock)
        {
            return _players.ContainsKey(playerId);
        }
    }

        public NetworkConnection? GetPlayerConnection(string playerId)
    {
        lock (_lock)
        {
            _players.TryGetValue(playerId, out var connection);
            return connection;
        }
    }

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

    #endregion
}
