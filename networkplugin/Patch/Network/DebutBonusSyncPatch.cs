using System;
using System.Text.Json;
using HarmonyLib;
using LBoL.Core.Dialogs;
using LBoL.EntityLib.Adventures;
using LBoL.Presentation.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network.Services;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Patch.Network;

public static class DebutBonusSyncPatch
{
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    private static bool _subscribed;
    private static INetworkClient _subscribedClient;
    private static readonly Action<string, object> _onGameEventReceived = OnGameEventReceived;
    private static readonly Action<bool> _onConnectionStateChanged = OnConnectionStateChanged;

    private static string _selfPlayerId;
    private static bool _selfIsHost;

    private static readonly object _syncLock = new();
    private static PendingBonusRoll _pending;

    private sealed class PendingBonusRoll
    {
        public long Timestamp { get; set; }
        public int BonusNo1 { get; set; }
        public int BonusNo2 { get; set; }
        public string SenderPlayerId { get; set; }
    }

    private static INetworkClient TryGetNetworkClient()
        => NetworkEventHelper.TryGetNetworkClient();

    [HarmonyPatch(typeof(GameDirector), "Update")]
    private static class SubscribeHook
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            INetworkClient client = TryGetNetworkClient();
            if (client == null)
            {
                return;
            }

            EnsureSubscribed(client);
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
        catch
        {
            _subscribedClient = null;
            _subscribed = false;
        }
    }

    private static void OnConnectionStateChanged(bool connected)
    {
        if (connected)
        {
            return;
        }

        lock (_syncLock)
        {
            _selfPlayerId = null;
            _selfIsHost = false;
            _pending = null;
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
            case NetworkMessageTypes.OnDebutBonusRolled:
                HandleDebutBonusRolled(root);
                return;
        }
    }

    private static void HandleWelcome(JsonElement root)
    {
        try
        {
            TryGetString(root, "PlayerId", out string playerId);
            bool isHost = TryGetBool(root, "IsHost", out bool hostValue) && hostValue;

            lock (_syncLock)
            {
                _selfPlayerId = playerId;
                _selfIsHost = isHost;
            }
        }
        catch
        {

        }
    }

    private static void HandleHostChanged(JsonElement root)
    {
        try
        {
            TryGetString(root, "NewHostId", out string newHostId);

            lock (_syncLock)
            {
                if (!string.IsNullOrEmpty(_selfPlayerId) && !string.IsNullOrEmpty(newHostId))
                {
                    _selfIsHost = string.Equals(_selfPlayerId, newHostId, StringComparison.Ordinal);
                }
            }
        }
        catch
        {

        }
    }

    private static void HandleDebutBonusRolled(JsonElement root)
    {
        try
        {
            if (!TryGetInt(root, "BonusNo1", out int b1) || !TryGetInt(root, "BonusNo2", out int b2))
            {
                return;
            }

            long ts = TryGetLong(root, "Timestamp", out long tsValue) ? tsValue : DateTime.Now.Ticks;
            TryGetString(root, "PlayerId", out string sender);

            lock (_syncLock)
            {
                _pending = new PendingBonusRoll
                {
                    Timestamp = ts,
                    BonusNo1 = b1,
                    BonusNo2 = b2,
                    SenderPlayerId = sender,
                };
            }
        }
        catch
        {

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
                using JsonDocument doc = JsonDocument.Parse(s);
                root = doc.RootElement.Clone();
                return true;
            }
        }
        catch
        {

        }

        root = default;
        return false;
    }

    private static bool TryGetProperty(JsonElement root, string prop, out JsonElement el)
    {
        el = default;
        return root.ValueKind == JsonValueKind.Object && root.TryGetProperty(prop, out el);
    }

    private static bool TryGetString(JsonElement root, string prop, out string value)
    {
        value = null;
        return TryGetProperty(root, prop, out JsonElement el)
            && el.ValueKind == JsonValueKind.String
            && (value = el.GetString()) != null;
    }

    private static bool TryGetBool(JsonElement root, string prop, out bool value)
    {
        value = default;
        if (!TryGetProperty(root, prop, out JsonElement el) ||
            (el.ValueKind != JsonValueKind.True && el.ValueKind != JsonValueKind.False))
        {
            return false;
        }

        value = el.GetBoolean();
        return true;
    }

    private static bool TryGetInt(JsonElement root, string name, out int value)
        => NetworkEventHelper.TryGetInt(root, name, out value);

    private static bool TryGetLong(JsonElement root, string name, out long value)
        => NetworkEventHelper.TryGetLong(root, name, out value);

    [HarmonyPatch(typeof(Debut), nameof(Debut.RollBonus))]
    private static class Debut_RollBonus_Sync
    {
        [HarmonyPostfix]
        public static void Postfix(Debut __instance)
        {
            try
            {
                INetworkClient client = TryGetNetworkClient();
                if (client?.IsConnected != true)
                {
                    return;
                }

                if (__instance?.Storage == null)
                {
                    return;
                }

                bool isSelfHost;
                lock (_syncLock)
                {
                    isSelfHost = _selfIsHost;
                }

                if (isSelfHost)
                {
                    if (!TryGetFloat(__instance.Storage, "$bonusNo1", out float b1f) ||
                        !TryGetFloat(__instance.Storage, "$bonusNo2", out float b2f))
                    {
                        return;
                    }

                    int b1 = (int)b1f;
                    int b2 = (int)b2f;

                    client.SendGameEventData(NetworkMessageTypes.OnDebutBonusRolled, new
                    {
                        Timestamp = DateTime.Now.Ticks,
                        PlayerId = _selfPlayerId,
                        BonusNo1 = b1,
                        BonusNo2 = b2,
                    });
                    return;
                }

                PendingBonusRoll pending;
                lock (_syncLock)
                {
                    pending = _pending;
                }

                if (pending == null)
                {
                    return;
                }

                ApplyBonusRoll(__instance, pending.BonusNo1, pending.BonusNo2);
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[DebutBonusSync] Error in RollBonus postfix: {ex.Message}");
            }
        }
    }

    private static void ApplyBonusRoll(Debut debut, int bonusNo1, int bonusNo2)
    {
        if (debut?.Storage == null)
        {
            return;
        }

        int[] bonusNos = new[] { bonusNo1, bonusNo2 };

        debut.Storage.SetValue("$bonusNo1", bonusNos[0]);
        debut.Storage.SetValue("$bonusNo2", bonusNos[1]);

        string[] optionTitles = new string[6];
        for (int i = 0; i < 6; i++)
        {
            optionTitles[i] = TryGetString(debut.Storage, string.Format("$option{0}Source", i + 1));
        }

        debut.Storage.SetValue("$bonusOption1", optionTitles[bonusNos[0]]);
        debut.Storage.SetValue("$bonusOption2", optionTitles[bonusNos[1]]);

        for (int j = 0; j < 2; j++)
        {
            debut.Storage.SetValue(string.Format("$bonusTarget{0}", j + 1), string.Format("Bonus{0}", bonusNos[j] + 1));
            switch (bonusNos[j])
            {
                case 0:
                    debut.Storage.SetValue("$tipUncommonCard", j + 3);
                    break;
                case 1:
                    debut.Storage.SetValue("$tipRareCard", j + 3);
                    break;
                case 2:
                    debut.Storage.SetValue("$tipRareExhibit", j + 3);
                    break;
                case 5:
                    debut.Storage.SetValue("$tipTransformCard", j + 3);
                    break;
            }
        }

        lock (_syncLock)
        {
            _pending = null;
        }

        Plugin.Logger?.LogInfo($"[DebutBonusSync] Applied host bonus roll: {bonusNo1}, {bonusNo2}");
    }

    private static bool TryGetFloat(DialogStorage storage, string key, out float value)
    {
        value = default;
        return storage != null && storage.TryGetValue(key, out value);
    }

    private static string TryGetString(DialogStorage storage, string key)
    {
        if (storage == null)
        {
            return null;
        }

        return storage.TryGetValue(key, out string s) ? s : null;
    }
}
