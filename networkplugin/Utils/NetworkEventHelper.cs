using System;
using System.Text.Json;
using LBoL.Core.Battle;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
namespace NetworkPlugin.Utils;
public static class NetworkEventHelper
{
    public static INetworkClient TryGetNetworkClient()
        => ModService.ServiceProvider?.GetService<INetworkClient>();
    public static INetworkClient TryGetConnectedClient(bool requireHost = false)
    {
        var client = TryGetNetworkClient();
        if (client == null || !client.IsConnected)
            return null;
        NetworkIdentityTracker.EnsureSubscribed(client);
        if (requireHost && !NetworkIdentityTracker.GetSelfIsHost())
            return null;
        return client;
    }
    public static bool ShouldSyncLocalBattle(BattleController battle)
        => battle != null && battle.Player != null && battle.Player == GameStateUtils.GetCurrentPlayer();
    public static void SendGameEvent(string eventType, object payload, string logPrefix = null)
    {
        try
        {
            var client = TryGetConnectedClient();
            if (client == null)
                return;
            client.SendGameEventData(eventType, payload);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[{logPrefix ?? "Sync"}] Error sending {eventType}: {ex.Message}");
        }
    }
    public static void SendRequest(string eventType, string json, string logPrefix = null)
    {
        try
        {
            var client = TryGetConnectedClient();
            if (client == null)
                return;
            client.SendRequest(eventType, json);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[{logPrefix ?? "Sync"}] Error sending {eventType}: {ex.Message}");
        }
    }
    public static bool TryGetJsonElement(object payload, out JsonElement root)
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
                using JsonDocument doc = JsonDocument.Parse(s);
                root = doc.RootElement.Clone();
                return true;
            }
        }
        catch
        {
            root = default;
            return false;
        }
        root = default;
        return false;
    }
    public static string GetString(JsonElement elem, string property)
    {
        if (elem.ValueKind != JsonValueKind.Object || !elem.TryGetProperty(property, out JsonElement prop))
            return null;
        return prop.ValueKind switch
        {
            JsonValueKind.String => prop.GetString(),
            JsonValueKind.Number => prop.GetRawText(),
            _ => null
        };
    }
    public static bool GetBool(JsonElement elem, string property, bool fallback = false)
    {
        if (elem.ValueKind != JsonValueKind.Object || !elem.TryGetProperty(property, out JsonElement prop))
            return fallback;
        return prop.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.String => bool.TryParse(prop.GetString(), out bool r) && r,
            _ => fallback
        };
    }
    public static bool GetBool(JsonElement el)
    {
        return el.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.String => bool.TryParse(el.GetString(), out bool r) && r,
            _ => false
        };
    }
    public static bool TryGetInt(JsonElement elem, string property, out int value)
    {
        value = 0;
        if (elem.ValueKind != JsonValueKind.Object || !elem.TryGetProperty(property, out JsonElement prop))
            return false;
        if (prop.ValueKind == JsonValueKind.Number && prop.TryGetInt32(out value))
            return true;
        if (prop.ValueKind == JsonValueKind.String && int.TryParse(prop.GetString(), out value))
            return true;
        return false;
    }
    public static bool TryGetLong(JsonElement elem, string property, out long value)
    {
        value = 0;
        if (elem.ValueKind != JsonValueKind.Object || !elem.TryGetProperty(property, out JsonElement prop))
            return false;
        if (prop.ValueKind == JsonValueKind.Number && prop.TryGetInt64(out value))
            return true;
        if (prop.ValueKind == JsonValueKind.String && long.TryParse(prop.GetString(), out value))
            return true;
        return false;
    }
}
