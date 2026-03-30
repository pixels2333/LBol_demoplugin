using System;
using System.Collections.Generic;
using System.Text.Json;
using HarmonyLib;
using LBoL.Core.Battle;
using LBoL.Core.Units;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Panels;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Patch.Network;

/// <summary>
/// 敌人状态接收补丁：
/// - 订阅 BattleEnemyStateChanged（兼容 EnemyStateUpdate）；
/// - 客机按 SpawnId 主键 + RootIndex/Id 兼容键匹配并落地 HP/Block/Shield。
/// </summary>
public static class EnemyStateReceivePatch
{
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    private static bool _subscribed;
    private static INetworkClient _subscribedClient;
    private static readonly Action<string, object> _onGameEventReceived = OnGameEventReceived;
    private static readonly Action<bool> _onConnectionStateChanged = OnConnectionStateChanged;

    private static readonly object _lock = new();
    private static readonly Dictionary<string, PendingState> _pendingByEnemyKey = new(StringComparer.Ordinal);

    private static INetworkClient TryGetNetworkClient()
        => ServiceProvider?.GetService<INetworkClient>();

    private static bool IsSelfHost()
        => NetworkIdentityTracker.GetSelfIsHost();

    private sealed class PendingState
    {
        public long Timestamp;
        public string BattleId;
        public int RootIndex;
        public string EnemyId;
        public string SpawnId;
        public int CurrentHp;
        public int Block;
        public int Shield;
        public bool IsAlive;
        public bool IsDying;
    }

    [HarmonyPatch(typeof(EnemyUnit), nameof(EnemyUnit.UpdateTurnMoves))]
    private static class EnemyUnit_UpdateTurnMoves_ApplyRemote
    {
        [HarmonyPostfix]
        public static void Postfix(EnemyUnit __instance)
        {
            INetworkClient client = TryGetNetworkClient();
            if (client != null)
            {
                EnsureSubscribed(client);
                NetworkIdentityTracker.EnsureSubscribed(client);
            }

            if (__instance == null || __instance.Battle == null || IsSelfHost())
            {
                return;
            }

            TryApplyPendingToEnemy(__instance);
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
        if (!string.Equals(eventType, NetworkMessageTypes.BattleEnemyStateChanged, StringComparison.Ordinal) &&
            !string.Equals(eventType, NetworkMessageTypes.EnemyStateUpdate, StringComparison.Ordinal))
        {
            return;
        }

        if (NetworkIdentityTracker.GetSelfIsHost())
        {
            return;
        }

        if (!TryGetJsonElement(payload, out JsonElement root))
        {
            return;
        }

        if (!root.TryGetProperty("Enemy", out JsonElement enemyElem) || enemyElem.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        long ts = TryGetLong(root, "Timestamp", out long t) ? t : DateTime.Now.Ticks;
        string battleId = GetString(root, "BattleId") ?? "unknown";
        int rootIndex = TryGetInt(enemyElem, "RootIndex", out int ri) ? ri : -1;
        string enemyId = GetString(enemyElem, "Id");
        string spawnId = GetString(enemyElem, "SpawnId");

        if (string.IsNullOrWhiteSpace(enemyId) && string.IsNullOrWhiteSpace(spawnId))
        {
            return;
        }

        PendingState pending = new()
        {
            Timestamp = ts,
            BattleId = battleId,
            RootIndex = rootIndex,
            EnemyId = enemyId,
            SpawnId = spawnId,
            CurrentHp = TryGetInt(enemyElem, "CurrentHp", out int hp) ? hp : 0,
            Block = TryGetInt(enemyElem, "Block", out int block) ? block : 0,
            Shield = TryGetInt(enemyElem, "Shield", out int shield) ? shield : 0,
            IsAlive = GetBool(enemyElem, "IsAlive"),
            IsDying = GetBool(enemyElem, "IsDying"),
        };

        string spawnKey = BuildSpawnKey(battleId, spawnId, rootIndex, enemyId);
        string legacyKey = BuildLegacyKey(battleId, rootIndex, enemyId);

        lock (_lock)
        {
            UpsertPending(spawnKey, pending);
            if (!string.Equals(spawnKey, legacyKey, StringComparison.Ordinal))
            {
                UpsertPending(legacyKey, pending);
            }
        }

        TryApplyPendingToBattle(battleId);
    }

    private static void TryApplyPendingToBattle(string battleId)
    {
        BattleController battle = TryGetCurrentBattle();
        if (battle?.EnemyGroup == null)
        {
            return;
        }

        string localBattleId = battle.GetHashCode().ToString();
        if (!string.IsNullOrWhiteSpace(battleId) && !string.Equals(battleId, "unknown", StringComparison.Ordinal) &&
            !string.Equals(localBattleId, battleId, StringComparison.Ordinal))
        {
            return;
        }

        foreach (EnemyUnit enemy in battle.EnemyGroup)
        {
            if (enemy == null)
            {
                continue;
            }

            TryApplyPendingToEnemy(enemy);
        }
    }

    private static void TryApplyPendingToEnemy(EnemyUnit enemy)
    {
        if (enemy?.Battle == null)
        {
            return;
        }

        if (!TryResolvePendingState(enemy, out PendingState pending))
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(pending.SpawnId))
        {
            SpawnedEnemySyncPatch.BindSpawnId(enemy, pending.SpawnId);
        }

        ApplyState(enemy, pending);
    }

    private static void ApplyState(EnemyUnit enemy, PendingState pending)
    {
        TrySetEnemyProperty(enemy, "Hp", Math.Max(0, pending.CurrentHp));
        TrySetEnemyProperty(enemy, "Block", Math.Max(0, pending.Block));
        TrySetEnemyProperty(enemy, "Shield", Math.Max(0, pending.Shield));

        if (!pending.IsAlive || pending.IsDying)
        {
            TrySetEnemyProperty(enemy, "Hp", 0);
        }
    }

    private static bool TryResolvePendingState(EnemyUnit enemy, out PendingState pending)
    {
        pending = null;

        string battleId = enemy.Battle.GetHashCode().ToString();
        string spawnId = SpawnedEnemySyncPatch.TryGetSpawnId(enemy, out string sid) ? sid : null;
        string spawnKey = BuildSpawnKey(battleId, spawnId, enemy.RootIndex, enemy.Id);
        string legacyKey = BuildLegacyKey(battleId, enemy.RootIndex, enemy.Id);

        lock (_lock)
        {
            if (_pendingByEnemyKey.TryGetValue(spawnKey, out pending) && pending != null)
            {
                return true;
            }

            return _pendingByEnemyKey.TryGetValue(legacyKey, out pending) && pending != null;
        }
    }

    private static void TrySetEnemyProperty(EnemyUnit enemy, string propertyName, int value)
    {
        try
        {
            Traverse.Create(enemy).Property(propertyName).SetValue(value);
        }
        catch
        {
            // ignored
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

    private static string BuildSpawnKey(string battleId, string spawnId, int rootIndex, string enemyId)
    {
        if (!string.IsNullOrWhiteSpace(spawnId))
        {
            return $"{battleId ?? ""}|spawn:{spawnId}";
        }

        return BuildLegacyKey(battleId, rootIndex, enemyId);
    }

    private static string BuildLegacyKey(string battleId, int rootIndex, string enemyId)
    {
        return $"{battleId ?? ""}|root:{rootIndex}|id:{enemyId ?? ""}";
    }

    private static void UpsertPending(string key, PendingState pending)
    {
        if (_pendingByEnemyKey.TryGetValue(key, out PendingState existing) && existing != null && existing.Timestamp >= pending.Timestamp)
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

    private static bool TryGetProperty(JsonElement root, string name, out JsonElement element)
    {
        element = default;
        return root.ValueKind == JsonValueKind.Object && root.TryGetProperty(name, out element);
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
            if (!TryGetProperty(root, name, out JsonElement el))
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

    private static bool TryGetInt(JsonElement root, string name, out int value)
    {
        value = default;
        try
        {
            if (!TryGetProperty(root, name, out JsonElement el))
            {
                return false;
            }

            if (el.ValueKind == JsonValueKind.Number && el.TryGetInt32(out value))
            {
                return true;
            }

            return el.ValueKind == JsonValueKind.String && int.TryParse(el.GetString(), out value);
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
            if (!TryGetProperty(root, name, out JsonElement el))
            {
                return false;
            }

            if (el.ValueKind == JsonValueKind.Number && el.TryGetInt64(out value))
            {
                return true;
            }

            return el.ValueKind == JsonValueKind.String && long.TryParse(el.GetString(), out value);
        }
        catch
        {
            return false;
        }
    }
}
