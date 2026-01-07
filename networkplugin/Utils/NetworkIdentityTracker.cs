using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;

namespace NetworkPlugin.Utils;

/// <summary>
/// 追踪服务器侧分配的 PlayerId / Host 信息（从 Welcome / PlayerListUpdate 等 GameEvent 中提取）。
/// 注意：NetworkServer 会在广播 GameEvent 时排除发送方，因此不要假设“自己发出的事件自己也能收到”。
/// </summary>
public static class NetworkIdentityTracker
{
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    private static readonly object SyncLock = new();
    private static bool _subscribed;
    private static INetworkClient _subscribedClient;

    private static string _selfPlayerId;
    private static bool _selfIsHost;
    private static readonly HashSet<string> _playerIds = new(StringComparer.Ordinal);

    private static readonly Action<string, object> OnGameEventReceivedHandler = OnGameEventReceived;
    private static readonly Action<bool> OnConnectionStateChangedHandler = OnConnectionStateChanged;

    public static INetworkClient TryGetClient()
    {
        try
        {
            return ServiceProvider?.GetService<INetworkClient>();
        }
        catch
        {
            return null;
        }
    }

    public static void EnsureSubscribed(INetworkClient client)
    {
        if (client == null)
        {
            return;
        }

        lock (SyncLock)
        {
            if (_subscribed && ReferenceEquals(_subscribedClient, client))
            {
                return;
            }
        }

        try
        {
            if (_subscribedClient != null)
            {
                _subscribedClient.OnGameEventReceived -= OnGameEventReceivedHandler;
                _subscribedClient.OnConnectionStateChanged -= OnConnectionStateChangedHandler;
            }
        }
        catch
        {
            // ignored
        }

        try
        {
            client.OnGameEventReceived += OnGameEventReceivedHandler;
            client.OnConnectionStateChanged += OnConnectionStateChangedHandler;
            lock (SyncLock)
            {
                _subscribedClient = client;
                _subscribed = true;
            }
        }
        catch
        {
            lock (SyncLock)
            {
                _subscribedClient = null;
                _subscribed = false;
            }
        }
    }

    public static string GetSelfPlayerId()
    {
        lock (SyncLock)
        {
            return _selfPlayerId;
        }
    }

    public static bool GetSelfIsHost()
    {
        lock (SyncLock)
        {
            return _selfIsHost;
        }
    }

    public static HashSet<string> GetPlayerIdsSnapshot()
    {
        lock (SyncLock)
        {
            return new HashSet<string>(_playerIds, StringComparer.Ordinal);
        }
    }

    private static void OnConnectionStateChanged(bool connected)
    {
        if (connected)
        {
            return;
        }

        lock (SyncLock)
        {
            _selfPlayerId = null;
            _selfIsHost = false;
            _playerIds.Clear();
        }
    }

    private static void OnGameEventReceived(string eventType, object payload)
    {
        if (!TryGetJsonElement(payload, out JsonElement root))
        {
            return;
        }

        switch (eventType)
        {
            case NetworkMessageTypes.Welcome:
                HandleWelcome(root);
                return;
            case NetworkMessageTypes.HostChanged:
                HandleHostChanged(root);
                return;
            case NetworkMessageTypes.PlayerListUpdate:
                HandlePlayerListUpdate(root);
                return;
            case NetworkMessageTypes.PlayerJoined:
                HandlePlayerJoined(root);
                return;
            case NetworkMessageTypes.PlayerLeft:
                HandlePlayerLeft(root);
                return;
        }
    }

    private static void HandleWelcome(JsonElement root)
    {
        try
        {
            string playerId = GetString(root, "PlayerId");
            bool isHost = GetBool(root, "IsHost");

            // NetworkServer.Welcome 使用 PlayerList 字段；其他代码使用 Players 字段，兼容两者。
            JsonElement listElem;
            bool hasList = root.TryGetProperty("Players", out listElem) && listElem.ValueKind == JsonValueKind.Array;
            if (!hasList)
            {
                hasList = root.TryGetProperty("PlayerList", out listElem) && listElem.ValueKind == JsonValueKind.Array;
            }

            lock (SyncLock)
            {
                _selfPlayerId = playerId;
                _selfIsHost = isHost;
                _playerIds.Clear();
                if (hasList)
                {
                    foreach (JsonElement p in listElem.EnumerateArray())
                    {
                        string id = GetString(p, "PlayerId");
                        if (!string.IsNullOrWhiteSpace(id))
                        {
                            _playerIds.Add(id);
                        }
                    }
                }
            }
        }
        catch
        {
            // ignored
        }
    }

    private static void HandleHostChanged(JsonElement root)
    {
        try
        {
            string newHostId = GetString(root, "NewHostId");
            if (string.IsNullOrWhiteSpace(newHostId))
            {
                return;
            }

            lock (SyncLock)
            {
                _selfIsHost = string.Equals(_selfPlayerId, newHostId, StringComparison.Ordinal);
            }
        }
        catch
        {
            // ignored
        }
    }

    private static void HandlePlayerListUpdate(JsonElement root)
    {
        if (!root.TryGetProperty("Players", out JsonElement playersElem) || playersElem.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        lock (SyncLock)
        {
            _playerIds.Clear();
            foreach (JsonElement p in playersElem.EnumerateArray())
            {
                string id = GetString(p, "PlayerId");
                if (!string.IsNullOrWhiteSpace(id))
                {
                    _playerIds.Add(id);
                }
            }
        }
    }

    private static void HandlePlayerJoined(JsonElement root)
    {
        string id = GetString(root, "PlayerId");
        if (string.IsNullOrWhiteSpace(id))
        {
            return;
        }

        lock (SyncLock)
        {
            _playerIds.Add(id);
        }
    }

    private static void HandlePlayerLeft(JsonElement root)
    {
        string id = GetString(root, "PlayerId");
        if (string.IsNullOrWhiteSpace(id))
        {
            return;
        }

        lock (SyncLock)
        {
            _playerIds.Remove(id);
        }
    }

    private static bool TryGetJsonElement(object payload, out JsonElement root)
    {
        try
        {
            if (payload is JsonElement je)
            {
                root = je;
                return true;
            }

            if (payload is string s)
            {
                root = JsonDocument.Parse(s).RootElement;
                return true;
            }
        }
        catch
        {
            // ignored
        }

        root = default;
        return false;
    }

    private static string GetString(JsonElement elem, string property)
    {
        try
        {
            if (elem.ValueKind != JsonValueKind.Object || !elem.TryGetProperty(property, out JsonElement p))
            {
                return null;
            }

            return p.ValueKind == JsonValueKind.String ? p.GetString() : p.GetRawText();
        }
        catch
        {
            return null;
        }
    }

    private static bool GetBool(JsonElement elem, string property)
    {
        try
        {
            if (elem.ValueKind != JsonValueKind.Object || !elem.TryGetProperty(property, out JsonElement p))
            {
                return false;
            }

            return p.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.String => bool.TryParse(p.GetString(), out bool b) && b,
                _ => false,
            };
        }
        catch
        {
            return false;
        }
    }
}

