using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using HarmonyLib;
using LBoL.Core.Battle;
using LBoL.Core.Units;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Patch.Network;

/// <summary>
/// 接收并落地敌人意图同步：
/// - 订阅 <see cref="INetworkClient.OnGameEventReceived"/>，处理 "BattleEnemyIntentChanged"；
/// - 在非 Host 客户端，将远端提供的意图列表写入本地 <see cref="EnemyUnit.Intentions"/> 并触发 UI 刷新。
///
/// 设计边界：仅用于“展示正确意图 UI”，不改动敌人 AI/_turnMoves。
/// </summary>
[HarmonyPatch]
public static class EnemyIntentReceivePatch
{
    private static readonly string EventType = NetworkMessageTypes.BattleEnemyIntentChanged;

    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    private static bool _subscribed;
    private static INetworkClient _subscribedClient;
    private static readonly Action<string, object> _onGameEventReceived = OnGameEventReceived;
    private static readonly Action<bool> _onConnectionStateChanged = OnConnectionStateChanged;

    private static readonly object _lock = new();
    private static readonly Dictionary<string, PendingIntent> _pendingByEnemyKey = new(StringComparer.Ordinal);

    private sealed class PendingIntent
    {
        public long Timestamp;
        public string EnemyGroupId;
        public int Round;
        public int RootIndex;
        public string EnemyId;
        public string SpawnId;
        public List<Intention> Intentions;
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
            NetworkIdentityTracker.EnsureSubscribed(client);
        }
    }

    [HarmonyPatch(typeof(EnemyUnit), nameof(EnemyUnit.UpdateTurnMoves))]
    private static class EnemyUnit_UpdateTurnMoves_ApplyRemote
    {
        [HarmonyPostfix]
        public static void Postfix(EnemyUnit __instance)
        {
            try
            {
                if (__instance == null || __instance.Battle == null)
                {
                    return;
                }

                // Host should never apply remote intentions.
                if (NetworkIdentityTracker.GetSelfIsHost())
                {
                    return;
                }

                TryApplyPendingToEnemy(__instance);
            }
            catch
            {
                // ignored
            }
        }
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

        lock (_lock)
        {
            _pendingByEnemyKey.Clear();
        }
    }

    private static void OnGameEventReceived(string eventType, object payload)
    {
        if (!string.Equals(eventType, EventType, StringComparison.Ordinal))
        {
            return;
        }

        // Host doesn't need to apply remote intentions.
        if (NetworkIdentityTracker.GetSelfIsHost())
        {
            return;
        }

        if (!TryGetJsonElement(payload, out JsonElement root))
        {
            return;
        }

        try
        {
            long ts = TryGetLong(root, "Timestamp", out long t) ? t : DateTime.Now.Ticks;
            string groupId = GetString(root, "EnemyGroupId");
            int round = TryGetInt(root, "Round", out int r) ? r : -1;

            JsonElement enemyElem;
            if (!root.TryGetProperty("Enemy", out enemyElem) || enemyElem.ValueKind != JsonValueKind.Object)
            {
                return;
            }

            int rootIndex = TryGetInt(enemyElem, "RootIndex", out int ri) ? ri : -1;
            string enemyId = GetString(enemyElem, "Id");
            string spawnId = GetString(enemyElem, "SpawnId");

            List<Intention> intentions = ParseIntentions(root);
            if (intentions == null)
            {
                return;
            }

            PendingIntent pending = new()
            {
                Timestamp = ts,
                EnemyGroupId = groupId,
                Round = round,
                RootIndex = rootIndex,
                EnemyId = enemyId,
                SpawnId = spawnId,
                Intentions = intentions,
            };

            string spawnKey = BuildSpawnKey(groupId, spawnId, rootIndex, enemyId);
            string legacyKey = BuildLegacyKey(groupId, rootIndex, enemyId);

            lock (_lock)
            {
                UpsertPending(spawnKey, pending);

                if (!string.Equals(spawnKey, legacyKey, StringComparison.Ordinal))
                {
                    UpsertPending(legacyKey, pending);
                }
            }

            // Best-effort immediate apply.
            TryApplyPendingToBattle(groupId);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[EnemyIntentRecv] Error handling {EventType}: {ex.Message}");
        }
    }

    private static void TryApplyPendingToBattle(string enemyGroupId)
    {
        try
        {
            BattleController battle = TryGetCurrentBattle();
            if (battle?.EnemyGroup == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(enemyGroupId) &&
                !string.Equals(battle.EnemyGroup.Id, enemyGroupId, StringComparison.Ordinal))
            {
                return;
            }

            foreach (EnemyUnit enemy in battle.EnemyGroup.Alives)
            {
                TryApplyPendingToEnemy(enemy);
            }
        }
        catch
        {
            // ignored
        }
    }

    private static void TryApplyPendingToEnemy(EnemyUnit enemy)
    {
        if (enemy == null || enemy.Battle?.EnemyGroup == null)
        {
            return;
        }

        string groupId = enemy.Battle.EnemyGroup.Id;
        string spawnId = SpawnedEnemySyncPatch.TryGetSpawnId(enemy, out string sid) ? sid : null;
        string spawnKey = BuildSpawnKey(groupId, spawnId, enemy.RootIndex, enemy.Id);
        string legacyKey = BuildLegacyKey(groupId, enemy.RootIndex, enemy.Id);

        PendingIntent pending;
        lock (_lock)
        {
            bool found = _pendingByEnemyKey.TryGetValue(spawnKey, out pending);
            if (!found)
            {
                found = _pendingByEnemyKey.TryGetValue(legacyKey, out pending);
            }

            if (!found || pending == null)
            {
                return;
            }
        }

        ApplyIntentionsToEnemy(enemy, pending.Intentions);
    }

    private static void ApplyIntentionsToEnemy(EnemyUnit enemy, List<Intention> intentions)
    {
        if (enemy == null || intentions == null)
        {
            return;
        }

        try
        {
            EnemyIntentSyncPatch.PauseIntentSync = true;

            enemy.Intentions.Clear();
            foreach (Intention i in intentions)
            {
                if (i == null)
                {
                    continue;
                }

                TrySetSource(i, enemy);
                enemy.Intentions.Add(i);
            }

            enemy.NotifyIntentionsChanged();
        }
        catch
        {
            // ignored
        }
        finally
        {
            EnemyIntentSyncPatch.PauseIntentSync = false;
        }
    }

    private static BattleController TryGetCurrentBattle()
    {
        try
        {
            var playBoard = UiManager.GetPanel<PlayBoard>();
            if (playBoard == null)
            {
                return null;
            }

            return Traverse.Create(playBoard).Property("Battle").GetValue<BattleController>();
        }
        catch
        {
            return null;
        }
    }

    private static List<Intention> ParseIntentions(JsonElement root)
    {
        try
        {
            if (!root.TryGetProperty("Intentions", out JsonElement listElem) || listElem.ValueKind != JsonValueKind.Array)
            {
                return new List<Intention>();
            }

            List<Intention> list = new();
            foreach (JsonElement item in listElem.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                Intention i = TryBuildIntention(item);
                if (i != null)
                {
                    list.Add(i);
                }
            }

            return list;
        }
        catch
        {
            return null;
        }
    }

    private static Intention TryBuildIntention(JsonElement item)
    {
        try
        {
            string cls = GetString(item, "Class");
            string moveName = GetString(item, "MoveName");
            bool hiddenByEnemy = GetBool(item, "HiddenByEnemy");
            bool showByEnemyTurn = GetBool(item, "ShowByEnemyTurn");

            JsonElement specific;
            bool hasSpecific = item.TryGetProperty("Specific", out specific) && specific.ValueKind == JsonValueKind.Object;

            Intention intention = cls switch
            {
                "AttackIntention" => BuildAttackIntention(hasSpecific ? specific : default),
                "ExplodeIntention" => BuildExplodeIntention(hasSpecific ? specific : default),
                "SpellCardIntention" => BuildSpellCardIntention(hasSpecific ? specific : default, moveName),
                "CountDownIntention" => BuildCountDownIntention(hasSpecific ? specific : default),
                "KokoroDarkIntention" => BuildKokoroDarkIntention(hasSpecific ? specific : default),
                "DefendIntention" => Intention.Defend(),
                "GrazeIntention" => Intention.Graze(),
                "PositiveEffectIntention" => Intention.PositiveEffect(),
                "NegativeEffectIntention" => Intention.NegativeEffect(null),
                "SpawnIntention" => Intention.Spawn(),
                "SpawnDroneIntention" => Intention.SpawnDrone(),
                "SleepIntention" => Intention.Sleep(),
                "StunIntention" => Intention.Stun(),
                "EscapeIntention" => Intention.Escape(),
                "ExplodeAllyIntention" => Intention.ExplodeAlly(),
                "ChargeIntention" => Intention.Charge(),
                "AddCardIntention" => Intention.AddCard(),
                "HealIntention" => Intention.Heal(),
                "RepairIntention" => Intention.Repair(),
                "ClearIntention" => Intention.Clear(),
                "HexIntention" => Intention.Hex(),
                "DoNothingIntention" => Intention.DoNothing(),
                "UnknownIntention" => Intention.Unknown(),
                _ => Intention.Unknown(),
            };

            if (!string.IsNullOrWhiteSpace(moveName))
            {
                intention.WithMoveName(moveName);
            }

            intention.HiddenByEnemy = hiddenByEnemy;
            intention.ShowByEnemyTurn = showByEnemyTurn;

            return intention;
        }
        catch
        {
            return null;
        }
    }

    private static Intention BuildAttackIntention(JsonElement specific)
    {
        int dmg = TryGetInt(specific, "Damage", out int d) ? d : 0;
        int times = TryGetInt(specific, "Times", out int t) ? t : 1;
        bool acc = GetBool(specific, "IsAccuracy");
        return Intention.Attack(dmg, times, acc);
    }

    private static Intention BuildExplodeIntention(JsonElement specific)
    {
        int dmg = TryGetInt(specific, "Damage", out int d) ? d : 0;
        return Intention.Explode(dmg);
    }

    private static Intention BuildSpellCardIntention(JsonElement specific, string moveName)
    {
        int? dmg = TryGetInt(specific, "Damage", out int d) ? d : (int?)null;
        int? times = TryGetInt(specific, "Times", out int t) ? t : (int?)null;
        bool acc = GetBool(specific, "IsAccuracy");
        string iconName = GetString(specific, "IconName");

        // The sender uses MoveName to carry the spell name.
        string name = string.IsNullOrWhiteSpace(moveName) ? null : moveName;

        // Use iconName overload when available.
        if (!string.IsNullOrWhiteSpace(iconName))
        {
            return Intention.SpellCard(name, iconName, dmg, times, acc);
        }

        return Intention.SpellCard(name, dmg, times, acc);
    }

    private static Intention BuildCountDownIntention(JsonElement specific)
    {
        int counter = TryGetInt(specific, "Counter", out int c) ? c : 0;
        return Intention.CountDown(counter);
    }

    private static Intention BuildKokoroDarkIntention(JsonElement specific)
    {
        int dmg = TryGetInt(specific, "Damage", out int d) ? d : 0;
        int count = TryGetInt(specific, "Count", out int c) ? c : 0;
        return Intention.KokoroDark(dmg, count);
    }

    private static void TrySetSource(Intention intention, EnemyUnit enemy)
    {
        try
        {
            if (intention == null || enemy == null)
            {
                return;
            }

            // Intention.SetSource is internal, use reflection.
            MethodInfo m = AccessTools.Method(intention.GetType(), "SetSource") ?? AccessTools.Method(typeof(Intention), "SetSource");
            m?.Invoke(intention, new object[] { enemy });
        }
        catch
        {
            // ignored
        }
    }

    private static string BuildSpawnKey(string enemyGroupId, string spawnId, int rootIndex, string enemyId)
    {
        if (!string.IsNullOrWhiteSpace(spawnId))
        {
            return $"{enemyGroupId ?? ""}|spawn:{spawnId}";
        }

        return BuildLegacyKey(enemyGroupId, rootIndex, enemyId);
    }

    private static string BuildLegacyKey(string enemyGroupId, int rootIndex, string enemyId)
    {
        return $"{enemyGroupId ?? ""}|root:{rootIndex}|id:{enemyId ?? ""}";
    }

    private static void UpsertPending(string key, PendingIntent pending)
    {
        if (_pendingByEnemyKey.TryGetValue(key, out PendingIntent existing) && existing != null && existing.Timestamp >= pending.Timestamp)
        {
            return;
        }

        _pendingByEnemyKey[key] = pending;
    }

    private static bool TryGetJsonElement(object payload, out JsonElement root)
    {
        root = default;

        try
        {
            if (payload is JsonElement el)
            {
                root = el;
                return true;
            }

            if (payload is string s && !string.IsNullOrWhiteSpace(s))
            {
                using JsonDocument doc = JsonDocument.Parse(s);
                root = doc.RootElement.Clone();
                return true;
            }

            // anonymous object / dictionary fallback
            string json = JsonCompat.Serialize(payload);
            if (!string.IsNullOrWhiteSpace(json))
            {
                using JsonDocument doc = JsonDocument.Parse(json);
                root = doc.RootElement.Clone();
                return true;
            }
        }
        catch
        {
            // ignored
        }

        return false;
    }

    private static string GetString(JsonElement root, string name)
    {
        try
        {
            return root.ValueKind == JsonValueKind.Object && root.TryGetProperty(name, out JsonElement el) && el.ValueKind == JsonValueKind.String
                ? el.GetString()
                : null;
        }
        catch
        {
            return null;
        }
    }

    private static bool GetBool(JsonElement root, string name)
    {
        try
        {
            if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty(name, out JsonElement el))
            {
                return false;
            }

            return el.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.String => bool.TryParse(el.GetString(), out bool b) && b,
                _ => false,
            };
        }
        catch
        {
            return false;
        }
    }

    private static bool GetBool(JsonElement el)
    {
        try
        {
            return el.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.String => bool.TryParse(el.GetString(), out bool b) && b,
                _ => false,
            };
        }
        catch
        {
            return false;
        }
    }

    private static bool TryGetInt(JsonElement root, string name, out int value)
    {
        value = default;
        try
        {
            if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty(name, out JsonElement el) || el.ValueKind != JsonValueKind.Number)
            {
                return false;
            }

            return el.TryGetInt32(out value);
        }
        catch
        {
            return false;
        }
    }

    private static bool TryGetLong(JsonElement root, string name, out long value)
    {
        value = default;
        try
        {
            if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty(name, out JsonElement el) || el.ValueKind != JsonValueKind.Number)
            {
                return false;
            }

            return el.TryGetInt64(out value);
        }
        catch
        {
            return false;
        }
    }
}
