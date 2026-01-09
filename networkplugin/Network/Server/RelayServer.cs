// 中继服务器：提供房间/中继转发能力，并基于 ServerCore 统一底层网络收发、鉴权与队列调度。
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
using NetworkPlugin.Network.Server.Core;

namespace NetworkPlugin.Network.Server;

public class RelayServer
{
    private readonly ILogger<RelayServer> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ConfigManager _configManager;

    private readonly object _lock = new();
    private readonly Dictionary<string, NetworkRoom> _rooms = [];
    private readonly Dictionary<NetPeer, PlayerSession> _sessionsByPeer = [];
    private readonly Dictionary<string, PlayerSession> _sessionsByPlayerId = [];
    private readonly Dictionary<string, NetworkConnection> _connectionsByPlayerId = [];

    private readonly ServerCore _core;

    public RelayServer(ILogger<RelayServer> logger, IServiceProvider serviceProvider, ConfigManager configManager)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configManager = configManager;

        _core = new ServerCore(
            new ServerOptions
            {
                Port = _configManager.RelayServerPort.Value,
                MaxConnections = _configManager.RelayServerMaxConnections.Value,
                ConnectionKey = _configManager.RelayServerConnectionKey.Value,
                DisconnectTimeoutMs = _configManager.NetworkTimeoutSeconds.Value * 1000,
                PingIntervalMs = 1000,
                MaxQueueSize = 2000,
                UseBackgroundThread = true,
                BackgroundThreadSleepMs = 15,
            },
            new MicrosoftServerLogger(_logger));

        RegisterCoreEvents();
    }

    public void Start()
    {
        _core.Start();
        _logger.LogInformation($"[RelayServer] Server started on port {_configManager.RelayServerPort.Value}");
    }

    public void Stop()
    {
        _logger.LogInformation("[RelayServer] Stopping server...");
        _core.Stop();

        lock (_lock)
        {
            _connectionsByPlayerId.Clear();
            _sessionsByPlayerId.Clear();
            _sessionsByPeer.Clear();
            _rooms.Clear();
        }

        _logger.LogInformation("[RelayServer] Server stopped");
    }

    private void RegisterCoreEvents()
    {
        _core.PeerConnected += peer =>
        {
            lock (_lock)
            {
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

                _sessionsByPeer[peer] = session;
                _sessionsByPlayerId[playerId] = session;
                _connectionsByPlayerId[playerId] = new NetworkConnection(peer, playerId);

                SendMessageToPeer(peer, new NetworkMessage
                {
                    Type = "Welcome",
                    Payload = new
                    {
                        PlayerId = playerId,
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
                if (!_sessionsByPeer.TryGetValue(peer, out var session))
                {
                    return;
                }

                _logger.LogInformation($"[RelayServer] Client disconnected: {peer.EndPoint}, Reason: {disconnectInfo.Reason}, PlayerId={session.PlayerId}");

                session.IsConnected = false;

                if (!string.IsNullOrEmpty(session.CurrentRoomId) && _rooms.TryGetValue(session.CurrentRoomId, out var room))
                {
                    room.RemovePlayer(session.PlayerId);
                    BroadcastPlayerList(room);

                    if (room.PlayerCount == 0)
                    {
                        _rooms.Remove(room.RoomId);
                        _logger.LogInformation($"[RelayServer] Room {room.RoomId} destroyed (empty)");
                    }
                }

                _connectionsByPlayerId.Remove(session.PlayerId);
                _sessionsByPlayerId.Remove(session.PlayerId);
                _sessionsByPeer.Remove(peer);
            }
        };

        _core.PeerLatencyUpdated += (peer, latency) =>
        {
            lock (_lock)
            {
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
                NetworkMessage message = new()
                {
                    Type = inbound.Type,
                    Payload = inbound.JsonPayload,
                    SenderPlayerId = GetPlayerId(inbound.FromPeer)
                };

                ProcessMessage(inbound.FromPeer, message, inbound.DeliveryMethod);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[RelayServer] Error processing message from {inbound.FromPeer?.EndPoint}");
            }
        };
    }

    private void ProcessMessage(NetPeer fromPeer, NetworkMessage message, DeliveryMethod deliveryMethod)
    {
        lock (_lock)
        {
            if (!_sessionsByPeer.TryGetValue(fromPeer, out var session))
            {
                return;
            }

            session.UpdateMessageTime();

            switch (message.Type)
            {
                case "CreateRoom":
                    HandleCreateRoom(session, message);
                    return;
                case "JoinRoom":
                    HandleJoinRoom(session, message);
                    return;
                case "LeaveRoom":
                    HandleLeaveRoom(session);
                    return;
                case "RoomMessage":
                    HandleRoomMessage(session, message, deliveryMethod);
                    return;
                case "DirectMessage":
                    HandleDirectMessage(session, message, deliveryMethod);
                    return;
                case "Heartbeat":
                    HandleHeartbeat(session, fromPeer);
                    return;
                case "GetRoomList":
                    HandleGetRoomList(fromPeer);
                    return;
                case "KickPlayer":
                    HandleKickPlayer(session, message);
                    return;

                // 兼容 Host 模式依赖的系统消息（按房间作用域生效）
                case "PlayerJoined":
                    HandlePlayerJoined(session, message);
                    return;
                case "GetSelf_REQUEST":
                    HandleGetSelfRequest(session, fromPeer);
                    return;
                case "UpdatePlayerLocation":
                    HandleUpdatePlayerLocation(session, message);
                    return;
            }

            if (IsGameEvent(message.Type))
            {
                ForwardGameEventToRoom(session, message.Type, message.Payload, deliveryMethod);
                return;
            }

            _logger.LogWarning($"[RelayServer] Unknown message type: {message.Type}");
        }
    }

    private void HandleHeartbeat(PlayerSession session, NetPeer peer)
    {
        session.UpdateHeartbeat();
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

    private void HandleGetRoomList(NetPeer peer)
    {
        var rooms = _rooms.Values.Select(r => r.GetStatus()).ToList();
        SendMessageToPeer(peer, new NetworkMessage
        {
            Type = "RoomList",
            Payload = new { Rooms = rooms },
            SenderPlayerId = "SERVER"
        }, DeliveryMethod.ReliableOrdered);
    }

    private void HandleCreateRoom(PlayerSession session, NetworkMessage message)
    {
        if (!string.IsNullOrEmpty(session.CurrentRoomId))
        {
            HandleLeaveRoom(session);
        }

        RoomConfig roomConfig = message.GetRoomConfigPayload();
        string roomId = GenerateRoomId();

        NetworkRoom room = new(roomId, roomConfig, _logger);
        _rooms[roomId] = room;

        if (!_connectionsByPlayerId.TryGetValue(session.PlayerId, out var connection))
        {
            connection = new NetworkConnection(session.Peer, session.PlayerId);
            _connectionsByPlayerId[session.PlayerId] = connection;
        }

        var joinResult = room.AddPlayer(session.PlayerId, connection);
        if (!joinResult.IsSuccess)
        {
            SendErrorMessage(session.Peer, "CreateRoomFailed", joinResult.ErrorMessage ?? "Unknown error");
            return;
        }

        session.CurrentRoomId = roomId;
        connection.CurrentRoomId = roomId;

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

        BroadcastPlayerList(room);
        _logger.LogInformation($"[RelayServer] Room created: {roomId} by player {session.PlayerId}");
    }

    private void HandleJoinRoom(PlayerSession session, NetworkMessage message)
    {
        string roomId = TryGetStringProperty(message.Payload, "RoomId") ?? session.CurrentRoomId;
        if (string.IsNullOrEmpty(roomId))
        {
            SendErrorMessage(session.Peer, "JoinRoomFailed", "Missing RoomId");
            return;
        }

        if (!_rooms.TryGetValue(roomId, out var room))
        {
            SendErrorMessage(session.Peer, "JoinRoomFailed", $"Room not found: {roomId}");
            return;
        }

        if (room.IsFull)
        {
            SendErrorMessage(session.Peer, "JoinRoomFailed", "Room is full");
            return;
        }

        if (!string.IsNullOrEmpty(session.CurrentRoomId))
        {
            HandleLeaveRoom(session);
        }

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

        session.CurrentRoomId = roomId;
        connection.CurrentRoomId = roomId;

        SendMessageToPeer(session.Peer, new NetworkMessage
        {
            Type = "RoomJoined",
            Payload = new { RoomId = roomId, PlayerId = session.PlayerId },
            SenderPlayerId = "SERVER"
        }, DeliveryMethod.ReliableOrdered);

        BroadcastPlayerList(room);
    }

    private void HandleLeaveRoom(PlayerSession session)
    {
        if (string.IsNullOrEmpty(session.CurrentRoomId))
        {
            return;
        }

        if (_rooms.TryGetValue(session.CurrentRoomId, out var room))
        {
            room.RemovePlayer(session.PlayerId);
            BroadcastPlayerList(room);

            if (room.PlayerCount == 0)
            {
                _rooms.Remove(room.RoomId);
                _logger.LogInformation($"[RelayServer] Room {room.RoomId} destroyed (empty)");
            }
        }

        if (_connectionsByPlayerId.TryGetValue(session.PlayerId, out var connection))
        {
            connection.CurrentRoomId = string.Empty;
        }

        session.CurrentRoomId = string.Empty;
    }

    private void HandleRoomMessage(PlayerSession session, NetworkMessage message, DeliveryMethod deliveryMethod)
    {
        string roomId = TryGetStringProperty(message.Payload, "RoomId") ?? session.CurrentRoomId;
        if (string.IsNullOrEmpty(roomId) || !_rooms.TryGetValue(roomId, out var room))
        {
            SendErrorMessage(session.Peer, "RoomMessageFailed", "Not in room");
            return;
        }

        string innerType = TryGetStringProperty(message.Payload, "Type") ?? "RoomMessage";
        object innerPayload = TryGetObjectProperty(message.Payload, "Payload") ?? message.Payload;

        room.BroadcastMessage(new NetworkMessage
        {
            Type = innerType,
            Payload = innerPayload,
            SenderPlayerId = session.PlayerId
        }, excludePlayerId: session.PlayerId);
    }

    private void HandleDirectMessage(PlayerSession session, NetworkMessage message, DeliveryMethod deliveryMethod)
    {
        string targetPlayerId = TryGetStringProperty(message.Payload, "TargetPlayerId");
        if (string.IsNullOrEmpty(targetPlayerId) || !_connectionsByPlayerId.TryGetValue(targetPlayerId, out var connection))
        {
            SendErrorMessage(session.Peer, "DirectMessageFailed", "Target not found");
            return;
        }

        string innerType = TryGetStringProperty(message.Payload, "Type") ?? "DirectMessage";
        object innerPayload = TryGetObjectProperty(message.Payload, "Payload") ?? message.Payload;

        connection.SendMessage(new NetworkMessage
        {
            Type = innerType,
            Payload = innerPayload,
            SenderPlayerId = session.PlayerId
        }, deliveryMethod);
    }

    private void HandleKickPlayer(PlayerSession session, NetworkMessage message)
    {
        if (string.IsNullOrEmpty(session.CurrentRoomId) || !_rooms.TryGetValue(session.CurrentRoomId, out var room))
        {
            SendErrorMessage(session.Peer, "KickPlayerFailed", "Not in room");
            return;
        }

        if (!string.Equals(room.HostPlayerId, session.PlayerId, StringComparison.Ordinal))
        {
            SendErrorMessage(session.Peer, "KickPlayerFailed", "Only host can kick");
            return;
        }

        string targetPlayerId = TryGetStringProperty(message.Payload, "TargetPlayerId");
        if (string.IsNullOrEmpty(targetPlayerId) || !room.ContainsPlayer(targetPlayerId))
        {
            SendErrorMessage(session.Peer, "KickPlayerFailed", "Target not in room");
            return;
        }

        room.RemovePlayer(targetPlayerId);
        BroadcastPlayerList(room);
    }

    private void HandlePlayerJoined(PlayerSession session, NetworkMessage message)
    {
        try
        {
            JsonElement root = GetJsonElement(message.Payload);
            if (root.ValueKind == JsonValueKind.Object)
            {
                if (root.TryGetProperty("PlayerName", out var nameElem) && nameElem.ValueKind == JsonValueKind.String)
                {
                    session.PlayerName = nameElem.GetString();
                }
                if (root.TryGetProperty("CharacterId", out var charElem) && charElem.ValueKind == JsonValueKind.String)
                {
                    session.Metadata["CharacterId"] = charElem.GetString();
                }
            }

            if (!string.IsNullOrEmpty(session.CurrentRoomId) && _rooms.TryGetValue(session.CurrentRoomId, out var room))
            {
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

                BroadcastPlayerList(room);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[RelayServer] Error handling PlayerJoined");
        }
    }

    private void HandleGetSelfRequest(PlayerSession session, NetPeer peer)
    {
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

    private void HandleUpdatePlayerLocation(PlayerSession session, NetworkMessage message)
    {
        try
        {
            JsonElement root = GetJsonElement(message.Payload);
            if (root.ValueKind != JsonValueKind.Object)
            {
                return;
            }

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
                BroadcastPlayerList(room);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[RelayServer] Error handling UpdatePlayerLocation");
        }
    }

    private void ForwardGameEventToRoom(PlayerSession session, string eventType, object payload, DeliveryMethod deliveryMethod)
    {
        if (string.IsNullOrEmpty(session.CurrentRoomId) || !_rooms.TryGetValue(session.CurrentRoomId, out var room))
        {
            return;
        }

        room.BroadcastMessage(new NetworkMessage
        {
            Type = eventType,
            Payload = payload,
            SenderPlayerId = session.PlayerId
        }, excludePlayerId: session.PlayerId);
    }

    private void BroadcastPlayerList(NetworkRoom room)
    {
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

        room.BroadcastMessage(new NetworkMessage
        {
            Type = "PlayerListUpdate",
            Payload = new { Players = players },
            SenderPlayerId = "SERVER"
        });
    }

    private bool IsRoomHost(PlayerSession session)
    {
        return !string.IsNullOrEmpty(session.CurrentRoomId)
               && _rooms.TryGetValue(session.CurrentRoomId, out var room)
               && string.Equals(room.HostPlayerId, session.PlayerId, StringComparison.Ordinal);
    }

    private void SendMessageToPeer(NetPeer peer, NetworkMessage message, DeliveryMethod deliveryMethod)
    {
        try
        {
            NetDataWriter writer = new();
            writer.Put(message.Type);
            writer.Put(JsonSerializer.Serialize(message.Payload));
            peer.Send(writer, deliveryMethod);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[RelayServer] Failed to send message to peer");
        }
    }

    private void SendErrorMessage(NetPeer peer, string errorType, string errorMessage)
    {
        SendMessageToPeer(peer, new NetworkMessage
        {
            Type = "Error",
            Payload = new { ErrorType = errorType, Message = errorMessage },
            SenderPlayerId = "SERVER"
        }, DeliveryMethod.ReliableOrdered);
    }

    private string GetPlayerId(NetPeer peer)
    {
        return _sessionsByPeer.TryGetValue(peer, out var session) ? session.PlayerId : "unknown";
    }

    private static bool IsGameEvent(string messageType)
    {
        return messageType.StartsWith("On", StringComparison.Ordinal) ||
               messageType.StartsWith("Mana", StringComparison.Ordinal) ||
               messageType.StartsWith("Gap", StringComparison.Ordinal) ||
               messageType.StartsWith("Battle", StringComparison.Ordinal) ||
               messageType == "StateSyncRequest";
    }

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

    private static JsonElement GetJsonElement(object payload)
    {
        try
        {
            return payload switch
            {
                JsonElement je => je,
                string json => JsonSerializer.Deserialize<JsonElement>(json),
                _ => JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(payload))
            };
        }
        catch
        {
            return default;
        }
    }

    private static string GeneratePlayerId()
    {
        return Guid.NewGuid().ToString("N");
    }

    private static string GenerateRoomId()
    {
        Random random = new();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, 6).Select(s => s[random.Next(s.Length)]).ToArray());
    }
}

