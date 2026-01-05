using System;
using System.Text.Json;
using HarmonyLib;
using LBoL.Core.Dialogs;
using LBoL.EntityLib.Adventures;
using LBoL.Presentation.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;

namespace NetworkPlugin.Patch.Network;

/// <summary>
/// Eirin(Erin) 开局奖励(= Debut Adventure bonus)同步补丁。
/// 参考 Together in Spire 的 NeowBlessingPatches：在联机时强制多人一致的“开局奖励随机结果”。
///
/// LBoL 对应逻辑：LBoL.EntityLib.Adventures.Debut.RollBonus()
/// - 使用 GameRun.DebutRng 随机抽取 2 个 bonus 选项（0..5）。
/// - 结果写入 Yarn DialogStorage：$bonusNo1/2, $bonusOption1/2, $bonusTarget1/2, $tip*
///
/// 当前实现策略：
/// - 主机：RollBonus 结束后广播 OnDebutBonusRolled（包含 bonusNo1/2）
/// - 客户端：收到广播后，若当前仍在 Debut 流程中，则覆盖本地 Storage 变量，保证最终一致
/// </summary>
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
            // ignored
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
            case "Welcome":
                HandleWelcome(root);
                return;
            case "HostChanged":
                HandleHostChanged(root);
                return;
            case "OnDebutBonusRolled":
                HandleDebutBonusRolled(root);
                return;
        }
    }

    private static void HandleWelcome(JsonElement root)
    {
        try
        {
            string playerId = root.TryGetProperty("PlayerId", out JsonElement idEl) && idEl.ValueKind == JsonValueKind.String
                ? idEl.GetString()
                : null;
            bool isHost = root.TryGetProperty("IsHost", out JsonElement hostEl) &&
                          (hostEl.ValueKind == JsonValueKind.True || hostEl.ValueKind == JsonValueKind.False) &&
                          hostEl.GetBoolean();

            lock (_syncLock)
            {
                _selfPlayerId = playerId;
                _selfIsHost = isHost;
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
            string newHostId = root.TryGetProperty("NewHostId", out JsonElement idEl) && idEl.ValueKind == JsonValueKind.String
                ? idEl.GetString()
                : null;

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
            // ignored
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

            long ts = root.TryGetProperty("Timestamp", out JsonElement tsEl) && tsEl.ValueKind == JsonValueKind.Number &&
                      tsEl.TryGetInt64(out long tsValue)
                ? tsValue
                : DateTime.Now.Ticks;

            string sender = root.TryGetProperty("PlayerId", out JsonElement senderEl) && senderEl.ValueKind == JsonValueKind.String
                ? senderEl.GetString()
                : null;

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
            // ignored
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

    private static bool TryGetInt(JsonElement root, string prop, out int value)
    {
        value = default;
        if (!root.TryGetProperty(prop, out JsonElement el))
        {
            return false;
        }

        return el.ValueKind switch
        {
            JsonValueKind.Number => el.TryGetInt32(out value),
            JsonValueKind.String => int.TryParse(el.GetString(), out value),
            _ => false,
        };
    }

    private static bool IsConnectedAndMultiplayer(out INetworkClient client)
    {
        client = TryGetNetworkClient();
        return client != null && client.IsConnected;
    }

    private static bool IsSelfHost()
    {
        lock (_syncLock)
        {
            return _selfIsHost;
        }
    }

    [HarmonyPatch(typeof(Debut), nameof(Debut.RollBonus))]
    private static class Debut_RollBonus_Sync
    {
        [HarmonyPostfix]
        public static void Postfix(Debut __instance)
        {
            try
            {
                if (!IsConnectedAndMultiplayer(out INetworkClient client))
                {
                    return;
                }

                if (__instance?.Storage == null)
                {
                    return;
                }

                if (IsSelfHost())
                {
                    if (!TryGetFloat(__instance.Storage, "$bonusNo1", out float b1f) ||
                        !TryGetFloat(__instance.Storage, "$bonusNo2", out float b2f))
                    {
                        return;
                    }

                    int b1 = (int)b1f;
                    int b2 = (int)b2f;

                    client.SendGameEventData("OnDebutBonusRolled", new
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

        // mirror Debut.RollBonus() variable writes
        debut.Storage.SetValue("$bonusNo1", (float)bonusNos[0]);
        debut.Storage.SetValue("$bonusNo2", (float)bonusNos[1]);

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
                    debut.Storage.SetValue("$tipUncommonCard", (float)(j + 3));
                    break;
                case 1:
                    debut.Storage.SetValue("$tipRareCard", (float)(j + 3));
                    break;
                case 2:
                    debut.Storage.SetValue("$tipRareExhibit", (float)(j + 3));
                    break;
                case 5:
                    debut.Storage.SetValue("$tipTransformCard", (float)(j + 3));
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
