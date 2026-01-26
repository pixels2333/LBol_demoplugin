using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Utils;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Network.Snapshot;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Network.RoomSync;

/// <summary>
/// 房间/战斗残局同步：
/// - 客户端：进入节点后向主机请求 RoomStateSnapshot，并在战斗开始/回合结束/战斗结束时上传。
/// - 主机：缓存每个 RoomKey 的最新状态，并对请求方定向响应。
/// </summary>
public static class RoomSyncManager
{
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    private static readonly object _lock = new();

    private static INetworkClient _subscribedClient;
    private static bool _subscribed;

    // Host-only: RoomKey -> latest snapshot
    private static readonly Dictionary<string, RoomStateSnapshot> _hostRoomStates = new(StringComparer.Ordinal);

    // Client-only: pending response cache (latest per RoomKey)
    private static readonly Dictionary<string, RoomStateSnapshot> _clientRoomStates = new(StringComparer.Ordinal);

    // Local helper: last entered node (for computing room key & metadata)
    private static (string roomKey, int act, int x, int y, string stationType, long atUtcTicks)? _lastEntered;

    public static void EnsureSubscribed(INetworkClient client)
    {
        if (client == null)
        {
            return;
        }

        lock (_lock)
        {
            if (_subscribed && ReferenceEquals(_subscribedClient, client))
            {
                return;
            }

            if (_subscribedClient != null)
            {
                _subscribedClient.OnGameEventReceived -= OnGameEventReceived;
            }

            _subscribedClient = client;
            _subscribedClient.OnGameEventReceived += OnGameEventReceived;
            _subscribed = true;
        }
    }

    public static void Unsubscribe()
    {
        lock (_lock)
        {
            if (_subscribedClient != null)
            {
                _subscribedClient.OnGameEventReceived -= OnGameEventReceived;
            }

            _subscribedClient = null;
            _subscribed = false;
        }
    }

    /// <summary>
    /// RoomKey 口径：Act:X:Y:StationType。
    /// </summary>
    public static string BuildRoomKey(int act, int x, int y, string stationType)
    {
        return $"{act}:{x}:{y}:{stationType}";
    }

    public static void SetLastEnteredNode(int act, int x, int y, string stationType)
    {
        try
        {
            string key = BuildRoomKey(act, x, y, stationType ?? string.Empty);
            _lastEntered = (key, act, x, y, stationType ?? string.Empty, DateTime.UtcNow.Ticks);
        }
        catch
        {
            // ignored
        }
    }

    public static string GetLastEnteredRoomKey()
    {
        return _lastEntered.HasValue ? _lastEntered.Value.roomKey : null;
    }

    public static RoomStateSnapshot TryGetClientRoomState(string roomKey)
    {
        if (string.IsNullOrWhiteSpace(roomKey))
        {
            return null;
        }

        lock (_lock)
        {
            return _clientRoomStates.TryGetValue(roomKey, out var s) ? s : null;
        }
    }

    /// <summary>
    /// 客户端：进入节点后请求主机返回该房间的最新状态。
    /// Host 模式下，事件会在本地直接回调（参考 TradeSyncPatch 的模式）。
    /// </summary>
    public static void RequestRoomState(string roomKey, long knownVersion)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(roomKey))
            {
                return;
            }

            var client = ServiceProvider?.GetService<INetworkClient>();
            if (client == null || !client.IsConnected)
            {
                return;
            }

            NetworkIdentityTracker.EnsureSubscribed(client);
            EnsureSubscribed(client);

            string requesterId = NetworkIdentityTracker.GetSelfPlayerId();
            if (string.IsNullOrWhiteSpace(requesterId))
            {
                requesterId = GameStateUtils.GetCurrentPlayerId();
            }

            var payload = new
            {
                Timestamp = DateTime.UtcNow.Ticks,
                RequesterId = requesterId,
                TargetPlayerId = requesterId,
                RoomKey = roomKey,
                KnownVersion = knownVersion,
            };

            client.SendGameEventData(NetworkMessageTypes.RoomStateRequest, payload);

            // Host 发起请求时不会收到服务器转发，直接在本地走一次处理。
            if (NetworkIdentityTracker.GetSelfIsHost())
            {
                OnGameEventReceived(NetworkMessageTypes.RoomStateRequest, payload);
            }
        }
        catch
        {
            // ignored
        }
    }

    /// <summary>
    /// 上传房间状态（战斗开始/回合结束/战斗结束）。
    /// </summary>
    public static void UploadRoomState(RoomStateSnapshot snapshot)
    {
        try
        {
            if (snapshot == null || string.IsNullOrWhiteSpace(snapshot.RoomKey))
            {
                return;
            }

            var client = ServiceProvider?.GetService<INetworkClient>();
            if (client == null || !client.IsConnected)
            {
                return;
            }

            NetworkIdentityTracker.EnsureSubscribed(client);
            EnsureSubscribed(client);

            string uploaderId = NetworkIdentityTracker.GetSelfPlayerId();
            if (string.IsNullOrWhiteSpace(uploaderId))
            {
                uploaderId = GameStateUtils.GetCurrentPlayerId();
            }

            snapshot.OwnerPlayerId = string.IsNullOrWhiteSpace(snapshot.OwnerPlayerId) ? uploaderId : snapshot.OwnerPlayerId;
            snapshot.UpdatedAtUtcTicks = DateTime.UtcNow.Ticks;

            // 平铺为匿名对象，避免 JsonElement 解析时出现不稳定的复杂类型。
            var payload = new
            {
                Timestamp = snapshot.UpdatedAtUtcTicks,
                UploaderId = uploaderId,
                TargetPlayerId = uploaderId,
                RoomKey = snapshot.RoomKey,
                RoomVersion = snapshot.RoomVersion,
                Phase = snapshot.Phase.ToString(),
                Act = snapshot.Act,
                X = snapshot.X,
                Y = snapshot.Y,
                StationType = snapshot.StationType,
                BattleId = snapshot.BattleId,
                Enemies = snapshot.Enemies,
                Rewards = snapshot.Rewards,
            };

            client.SendGameEventData(NetworkMessageTypes.RoomStateUpload, payload);

            // Host 上传时同样本地处理一次。
            if (NetworkIdentityTracker.GetSelfIsHost())
            {
                OnGameEventReceived(NetworkMessageTypes.RoomStateUpload, payload);
            }
        }
        catch
        {
            // ignored
        }
    }

    private static void OnGameEventReceived(string eventType, object payload)
    {
        if (string.IsNullOrWhiteSpace(eventType))
        {
            return;
        }

        // 仅处理 RoomState* 相关事件。
        if (!string.Equals(eventType, NetworkMessageTypes.RoomStateRequest, StringComparison.Ordinal) &&
            !string.Equals(eventType, NetworkMessageTypes.RoomStateResponse, StringComparison.Ordinal) &&
            !string.Equals(eventType, NetworkMessageTypes.RoomStateUpload, StringComparison.Ordinal) &&
            !string.Equals(eventType, NetworkMessageTypes.RoomStateBroadcast, StringComparison.Ordinal))
        {
            return;
        }

        if (!TryGetJsonElement(payload, out JsonElement root) || root.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        switch (eventType)
        {
            case NetworkMessageTypes.RoomStateRequest:
                HandleRoomStateRequest(root);
                return;
            case NetworkMessageTypes.RoomStateUpload:
                HandleRoomStateUpload(root);
                return;
            case NetworkMessageTypes.RoomStateResponse:
                HandleRoomStateResponse(root);
                return;
            case NetworkMessageTypes.RoomStateBroadcast:
                HandleRoomStateResponse(root);
                return;
        }
    }

    private static void HandleRoomStateRequest(JsonElement root)
    {
        // Only host answers.
        if (!NetworkIdentityTracker.GetSelfIsHost())
        {
            return;
        }

        string requesterId = TryGetString(root, "RequesterId") ?? TryGetString(root, "TargetPlayerId");
        string roomKey = TryGetString(root, "RoomKey");
        long knownVersion = TryGetLong(root, "KnownVersion") ?? 0;

        if (string.IsNullOrWhiteSpace(requesterId) || string.IsNullOrWhiteSpace(roomKey))
        {
            return;
        }

        RoomStateSnapshot snapshot;
        lock (_lock)
        {
            if (!_hostRoomStates.TryGetValue(roomKey, out snapshot!))
            {
                snapshot = new RoomStateSnapshot
                {
                    RoomKey = roomKey,
                    RoomVersion = 0,
                    Phase = RoomPhase.NotVisited,
                    UpdatedAtUtcTicks = DateTime.UtcNow.Ticks,
                };
            }
        }

        // 如果请求方已经是最新版本，可以选择不回复；但为了简单/可诊断，这里仍回复一次。
        if (snapshot.RoomVersion < knownVersion)
        {
            // 请求方版本更高（不太可能）：仍按主机为准回发。
        }

        SendRoomStateResponseTo(requesterId, snapshot);
    }

    private static void HandleRoomStateUpload(JsonElement root)
    {
        // Only host stores.
        if (!NetworkIdentityTracker.GetSelfIsHost())
        {
            return;
        }

        string uploaderId = TryGetString(root, "UploaderId") ?? TryGetString(root, "OwnerPlayerId") ?? TryGetString(root, "TargetPlayerId");
        string roomKey = TryGetString(root, "RoomKey");
        string phaseStr = TryGetString(root, "Phase");

        if (string.IsNullOrWhiteSpace(uploaderId) || string.IsNullOrWhiteSpace(roomKey))
        {
            return;
        }

        RoomPhase phase = RoomPhase.NotVisited;
        if (!string.IsNullOrWhiteSpace(phaseStr) && Enum.TryParse(phaseStr, ignoreCase: true, out RoomPhase parsed))
        {
            phase = parsed;
        }

        RoomStateSnapshot incoming = new()
        {
            RoomKey = roomKey,
            Phase = phase,
            OwnerPlayerId = uploaderId,
            UpdatedAtUtcTicks = TryGetLong(root, "Timestamp") ?? DateTime.UtcNow.Ticks,
            RoomVersion = TryGetLong(root, "RoomVersion") ?? 0,
            Act = (int)(TryGetLong(root, "Act") ?? 0),
            X = (int)(TryGetLong(root, "X") ?? 0),
            Y = (int)(TryGetLong(root, "Y") ?? 0),
            StationType = TryGetString(root, "StationType") ?? string.Empty,
            BattleId = TryGetString(root, "BattleId") ?? string.Empty,
            Rewards = TryDeserialize<BattleRewardSnapshot>(root, "Rewards") ?? new BattleRewardSnapshot(),
            Enemies = TryDeserializeEnemies(root) ?? new List<EnemyStateSnapshot>(),
        };

        RoomStateSnapshot stored;
        lock (_lock)
        {
            if (_hostRoomStates.TryGetValue(roomKey, out stored!))
            {
                // Owner arbitration: first uploader becomes owner.
                if (string.IsNullOrWhiteSpace(stored.OwnerPlayerId))
                {
                    stored.OwnerPlayerId = uploaderId;
                }

                // Ignore uploads from non-owner if owner already chosen.
                if (!string.Equals(stored.OwnerPlayerId, uploaderId, StringComparison.Ordinal))
                {
                    return;
                }

                // Bump version on host.
                stored.RoomVersion = Math.Max(stored.RoomVersion + 1, incoming.RoomVersion);
                stored.Phase = incoming.Phase;
                stored.UpdatedAtUtcTicks = incoming.UpdatedAtUtcTicks;
                stored.Act = incoming.Act;
                stored.X = incoming.X;
                stored.Y = incoming.Y;
                stored.StationType = incoming.StationType;
                stored.BattleId = incoming.BattleId;
                stored.Enemies = incoming.Enemies;
                stored.Rewards = incoming.Rewards;
            }
            else
            {
                incoming.RoomVersion = Math.Max(1, incoming.RoomVersion);
                _hostRoomStates[roomKey] = incoming;
                stored = incoming;
            }
        }

        // Optional: host can broadcast updates, but default to "request on enter".
    }

    private static void HandleRoomStateResponse(JsonElement root)
    {
        string roomKey = TryGetString(root, "RoomKey");
        if (string.IsNullOrWhiteSpace(roomKey))
        {
            return;
        }

        RoomPhase phase = RoomPhase.NotVisited;
        string phaseStr = TryGetString(root, "Phase");
        if (!string.IsNullOrWhiteSpace(phaseStr) && Enum.TryParse(phaseStr, ignoreCase: true, out RoomPhase parsed))
        {
            phase = parsed;
        }

        RoomStateSnapshot snapshot = new()
        {
            RoomKey = roomKey,
            Phase = phase,
            RoomVersion = TryGetLong(root, "RoomVersion") ?? 0,
            OwnerPlayerId = TryGetString(root, "OwnerPlayerId") ?? string.Empty,
            UpdatedAtUtcTicks = TryGetLong(root, "Timestamp") ?? DateTime.UtcNow.Ticks,
            Act = (int)(TryGetLong(root, "Act") ?? 0),
            X = (int)(TryGetLong(root, "X") ?? 0),
            Y = (int)(TryGetLong(root, "Y") ?? 0),
            StationType = TryGetString(root, "StationType") ?? string.Empty,
            BattleId = TryGetString(root, "BattleId") ?? string.Empty,
            Rewards = TryDeserialize<BattleRewardSnapshot>(root, "Rewards") ?? new BattleRewardSnapshot(),
            Enemies = TryDeserializeEnemies(root) ?? new List<EnemyStateSnapshot>(),
        };

        lock (_lock)
        {
            _clientRoomStates[roomKey] = snapshot;
        }
    }

    private static void SendRoomStateResponseTo(string targetPlayerId, RoomStateSnapshot snapshot)
    {
        try
        {
            var client = ServiceProvider?.GetService<INetworkClient>();
            if (client == null || !client.IsConnected)
            {
                return;
            }

            EnsureSubscribed(client);

            string hostId = NetworkIdentityTracker.GetSelfPlayerId();
            if (string.IsNullOrWhiteSpace(hostId))
            {
                hostId = GameStateUtils.GetCurrentPlayerId();
            }

            var payload = new
            {
                Timestamp = DateTime.UtcNow.Ticks,
                HostId = hostId,
                TargetPlayerId = targetPlayerId,
                RequesterId = targetPlayerId,
                RoomKey = snapshot.RoomKey,
                RoomVersion = snapshot.RoomVersion,
                Phase = snapshot.Phase.ToString(),
                OwnerPlayerId = snapshot.OwnerPlayerId,
                Act = snapshot.Act,
                X = snapshot.X,
                Y = snapshot.Y,
                StationType = snapshot.StationType,
                BattleId = snapshot.BattleId,
                Enemies = snapshot.Enemies,
                Rewards = snapshot.Rewards,
            };

            client.SendGameEventData(NetworkMessageTypes.RoomStateResponse, payload);
        }
        catch
        {
            // ignored
        }
    }

    private static List<EnemyStateSnapshot> TryDeserializeEnemies(JsonElement root)
    {
        try
        {
            if (!root.TryGetProperty("Enemies", out var enemiesEl) || enemiesEl.ValueKind != JsonValueKind.Array)
            {
                return null;
            }

            return JsonSerializer.Deserialize<List<EnemyStateSnapshot>>(enemiesEl.GetRawText());
        }
        catch
        {
            return null;
        }
    }

    private static T TryDeserialize<T>(JsonElement root, string property) where T : class
    {
        try
        {
            if (!root.TryGetProperty(property, out var el) || el.ValueKind == JsonValueKind.Undefined || el.ValueKind == JsonValueKind.Null)
            {
                return null;
            }

            return JsonSerializer.Deserialize<T>(el.GetRawText());
        }
        catch
        {
            return null;
        }
    }

    private static bool TryGetJsonElement(object payload, out JsonElement root)
    {
        if (payload is JsonElement je)
        {
            root = je;
            return true;
        }

        try
        {
            string json = JsonCompat.Serialize(payload);
            root = JsonDocument.Parse(json).RootElement;
            return true;
        }
        catch
        {
            root = default;
            return false;
        }
    }

    private static string TryGetString(JsonElement root, string property)
    {
        try
        {
            return root.TryGetProperty(property, out var p) && p.ValueKind == JsonValueKind.String ? p.GetString() : null;
        }
        catch
        {
            return null;
        }
    }

    private static long? TryGetLong(JsonElement root, string property)
    {
        try
        {
            if (!root.TryGetProperty(property, out var p))
            {
                return null;
            }

            if (p.ValueKind == JsonValueKind.Number && p.TryGetInt64(out long n))
            {
                return n;
            }

            if (p.ValueKind == JsonValueKind.String && long.TryParse(p.GetString(), out n))
            {
                return n;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    // RoomId（Relay 作用域）当前不从客户端侧主动填充：服务端可使用 session.CurrentRoomId 作为默认作用域。
}
