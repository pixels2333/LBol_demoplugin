using System;
using System.Collections.Generic;
using System.Text.Json;
using LBoL.Presentation;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Utils;
using UnityEngine;

namespace NetworkPlugin.Patch.UI;

public static partial class OtherPlayersOverlayPatch
{
    #region 网络客户端操作

    private static INetworkClient TryGetNetworkClient()
    {
        try
        {
            return ServiceProvider?.GetService(typeof(INetworkClient)) as INetworkClient;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogDebug($"[OtherPlayersOverlayPatch] 获取 INetworkClient 失败: {ex.Message}");
            return null;
        }
    }

    private static void EnsureSubscribed(INetworkClient client)
    {
        if (_subscribed && ReferenceEquals(_subscribedClient, client))
        {
            return;
        }

        try
        {
            if (_subscribedClient != null)
            {
                _subscribedClient.OnGameEventReceived -= _onGameEventReceived;
                _subscribedClient.OnConnectionStateChanged -= _onConnectionStateChanged;
            }
        }
        catch
        {
        }

        try
        {
            client.OnGameEventReceived += _onGameEventReceived;
            client.OnConnectionStateChanged += _onConnectionStateChanged;
            _subscribedClient = client;
            _subscribed = true;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogWarning($"[OtherPlayersOverlayPatch] 订阅网络事件失败: {ex.Message}");
            _subscribed = false;
            _subscribedClient = null;
        }
    }

    #endregion

    #region 网络事件处理

    private static void OnConnectionStateChanged(bool connected)
    {
        if (connected)
        {
            return;
        }

        lock (_syncLock)
        {
            _players.Clear();
        }

        _selfPlayerId = null;
        ClearRemoteCharacters();
        ClearMapIcons();
    }

    private static void OnGameEventReceived(string eventType, object payload)
    {
        try
        {
            if (!TryGetJsonElement(payload, out JsonElement root))
            {
                return;
            }

            switch (eventType)
            {
                case NetworkMessageTypes.Welcome:
                    HandleWelcome(root);
                    break;
                case NetworkMessageTypes.PlayerListUpdate:
                    HandlePlayerListUpdate(root);
                    break;
                case NetworkMessageTypes.PlayerJoined:
                    HandlePlayerJoined(root);
                    break;
                case NetworkMessageTypes.PlayerLeft:
                    HandlePlayerLeft(root);
                    break;
                case NetworkMessageTypes.HostChanged:
                    HandleHostChanged(root);
                    break;
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogDebug($"[OtherPlayersOverlayPatch] 处理网络事件失败: {eventType}, {ex.Message}");
        }
    }

    private static void HandlePlayerListUpdate(JsonElement root)
    {
        if (!root.TryGetProperty("Players", out JsonElement playersElem) || playersElem.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        ReplacePlayersFromArray(playersElem);
    }

    private static void HandlePlayerJoined(JsonElement root)
    {
        string playerId = GetString(root, "PlayerId");
        if (string.IsNullOrWhiteSpace(playerId))
        {
            return;
        }

        bool hasConnectedField = root.ValueKind == JsonValueKind.Object && root.TryGetProperty("IsConnected", out _);

        lock (_syncLock)
        {
            _players[playerId] = new PlayerSummary
            {
                PlayerId = playerId,
                PlayerName = GetString(root, "PlayerName") ?? playerId,
                IsHost = GetBool(root, "IsHost"),
                IsConnected = hasConnectedField ? GetBool(root, "IsConnected") : true,
                CharacterId = GetString(root, "CharacterId"),
                LocationX = GetInt(root, "LocationX", -1),
                LocationY = GetInt(root, "LocationY", -1),
                Stage = GetInt(root, "Stage", -1),
                LocationName = GetString(root, "LocationName"),
                LastUpdateTime = Time.unscaledTime,
            };
        }
    }

    private static void HandleHostChanged(JsonElement root)
    {
        string newHostId = GetString(root, "NewHostId");
        if (string.IsNullOrWhiteSpace(newHostId))
        {
            return;
        }

        lock (_syncLock)
        {
            foreach (KeyValuePair<string, PlayerSummary> kv in _players)
            {
                kv.Value.IsHost = kv.Key == newHostId;
            }
        }
    }

    private static void HandleWelcome(JsonElement root)
    {
        string selfId = GetString(root, "PlayerId");
        if (!string.IsNullOrWhiteSpace(selfId))
        {
            _selfPlayerId = selfId;
        }

        if (root.TryGetProperty("PlayerList", out JsonElement playersElem) && playersElem.ValueKind == JsonValueKind.Array)
        {
            ReplacePlayersFromArray(playersElem);
        }
    }

    private static void HandlePlayerLeft(JsonElement root)
    {
        string playerId = GetString(root, "PlayerId");
        if (string.IsNullOrWhiteSpace(playerId))
        {
            return;
        }

        lock (_syncLock)
        {
            _players.Remove(playerId);
        }

        RemoveRemoteCharacter(playerId);
        HideMapIcon(playerId);
    }

    private static void ReplacePlayersFromArray(JsonElement playersElem)
    {
        var incoming = new Dictionary<string, PlayerSummary>();
        foreach (JsonElement p in playersElem.EnumerateArray())
        {
            string playerId = GetString(p, "PlayerId");
            if (string.IsNullOrWhiteSpace(playerId))
            {
                continue;
            }

            bool hasConnectedField = p.ValueKind == JsonValueKind.Object && p.TryGetProperty("IsConnected", out _);
            incoming[playerId] = new PlayerSummary
            {
                PlayerId = playerId,
                PlayerName = GetString(p, "PlayerName") ?? playerId,
                IsHost = GetBool(p, "IsHost"),
                IsConnected = hasConnectedField ? GetBool(p, "IsConnected") : true,
                CharacterId = GetString(p, "CharacterId"),
                LocationX = GetInt(p, "LocationX", -1),
                LocationY = GetInt(p, "LocationY", -1),
                Stage = GetInt(p, "Stage", -1),
                LocationName = GetString(p, "LocationName"),
                LastUpdateTime = Time.unscaledTime,
            };
        }

        lock (_syncLock)
        {
            _players.Clear();
            foreach (var kv in incoming)
            {
                _players[kv.Key] = kv.Value;
            }
        }
    }

    #endregion

    #region JSON工具

    private static bool TryGetJsonElement(object payload, out JsonElement element)
    {
        if (payload is JsonElement je)
        {
            element = je;
            return true;
        }

        if (payload is string s && !string.IsNullOrWhiteSpace(s))
        {
            using JsonDocument doc = JsonDocument.Parse(s);
            element = doc.RootElement.Clone();
            return true;
        }

        if (payload != null)
        {
            string json = JsonCompat.Serialize(payload);
            if (!string.IsNullOrWhiteSpace(json))
            {
                using JsonDocument doc = JsonDocument.Parse(json);
                element = doc.RootElement.Clone();
                return true;
            }
        }

        element = default;
        return false;
    }

    private static string GetString(JsonElement elem, string property)
    {
        if (elem.ValueKind != JsonValueKind.Object || !elem.TryGetProperty(property, out JsonElement p))
        {
            return null;
        }

        return p.ValueKind switch
        {
            JsonValueKind.String => p.GetString(),
            JsonValueKind.Number => p.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => null,
        };
    }

    private static bool GetBool(JsonElement elem, string property)
    {
        if (elem.ValueKind != JsonValueKind.Object || !elem.TryGetProperty(property, out JsonElement p))
        {
            return false;
        }

        return p.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Number => p.TryGetInt32(out int v) && v != 0,
            JsonValueKind.String => bool.TryParse(p.GetString(), out bool b) && b,
            _ => false,
        };
    }

    private static int GetInt(JsonElement elem, string property, int defaultValue = 0)
    {
        if (elem.ValueKind != JsonValueKind.Object || !elem.TryGetProperty(property, out JsonElement p))
        {
            return defaultValue;
        }

        return p.ValueKind switch
        {
            JsonValueKind.Number => p.TryGetInt32(out int v) ? v : defaultValue,
            JsonValueKind.String => int.TryParse(p.GetString(), out int v) ? v : defaultValue,
            JsonValueKind.True => 1,
            JsonValueKind.False => 0,
            _ => defaultValue,
        };
    }

    #endregion
}