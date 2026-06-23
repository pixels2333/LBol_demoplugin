using System;
using System.Collections.Generic;
using System.Text.Json;
using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network.Services;
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
        => NetworkEventHelper.TryGetNetworkClient();

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
            INetworkClient client = TryGetNetworkClient();
            if (client != null)
            {
                EnsureSubscribed(client);
                NetworkIdentityTracker.EnsureSubscribed(client);
            }

            if (__instance == null || __instance.Battle == null)
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

        Plugin.Logger?.LogInfo($"[EnemyStateReceive] Received event {eventType} (SelfIsHost: {NetworkIdentityTracker.GetSelfIsHost()})");

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

        // 不校验 battleId：host 与 client 的 BattleController 实例不同，
        // GetHashCode().ToString() 必然不同，校验会导致跨端 pending 永远不应用。
        // 每端同一时刻只有一个活跃战斗，直接尝试应用到所有敌人。
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
        if (enemy == null || !enemy.IsAlive)
        {
            return;
        }

        int oldHp = enemy.Hp;
        int oldBlock = enemy.Block;
        int oldShield = enemy.Shield;

        int newHp = Math.Max(0, pending.CurrentHp);
        int newBlock = Math.Max(0, pending.Block);
        int newShield = Math.Max(0, pending.Shield);

        if (!pending.IsAlive || pending.IsDying)
        {
            newHp = 0;
        }

        Plugin.Logger?.LogInfo($"[EnemyStateReceive] ApplyState: {enemy.Name} Hp: {oldHp}->{newHp}, Block: {oldBlock}->{newBlock}, Shield: {oldShield}->{newShield}");

        // 应用远端状态时抑制 EnemySyncPatch 广播回环
        using (EnemySyncPatch.EnterApplyRemoteStateScope())
        {
            TrySetEnemyProperty(enemy, "Hp", newHp);
            TrySetEnemyProperty(enemy, "Block", newBlock);
            TrySetEnemyProperty(enemy, "Shield", newShield);
        }

        // 若 HP 变为 0，则触发死亡流
        if (newHp == 0)
        {
            var battle = enemy.Battle;
            if (battle != null)
            {
                battle.RequestDebugAction(new ForceKillAction(battle.Player, enemy), "RemoteForceKill");
            }
            return;
        }

        // 触发 UI 与 View 视图层更新
        try
        {
            var view = GameDirector.GetEnemy(enemy);
            if (view != null)
            {
                int hpDamage = oldHp - newHp;
                int blockDamage = Math.Max(0, oldBlock - newBlock);
                int shieldDamage = Math.Max(0, oldShield - newShield);
                int healAmount = newHp - oldHp;

                if (hpDamage > 0 || blockDamage > 0 || shieldDamage > 0)
                {
                    DamageInfo damageInfo = DamageInfo.Attack(hpDamage);
                    damageInfo.DamageBlocked = blockDamage;
                    damageInfo.DamageShielded = shieldDamage;

                    view.ComingDamage = damageInfo;
                    view.Hit(ignoreCoolDown: true);

                    // 触发漂浮伤害数值
                    if (PopupHud.Instance != null)
                    {
                        PopupHud.Instance.DamagePopupFromScene(damageInfo, view.transform.position, sourceIsPlayer: true);
                    }

                    // 更新 HP Bar UI
                    view.OnDamageReceived(damageInfo);
                }
                else if (healAmount > 0)
                {
                    // 触发漂浮治疗数值
                    if (PopupHud.Instance != null)
                    {
                        PopupHud.Instance.HealPopupFromScene(healAmount, view.transform.position);
                    }

                    // 更新 HP Bar UI
                    view.OnHealingReceived(healAmount);
                }
                else if (newBlock != oldBlock || newShield != oldShield)
                {
                    // 仅 Block/Shield 变化，通过反射获取 _statusWidget 并触发 HP Bar UI 状态刷新
                    var widget = Traverse.Create(view).Field("_statusWidget").GetValue();
                    if (widget != null)
                    {
                        Traverse.Create(widget).Method("OnBlockShieldChanged").GetValue();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[EnemyStateReceivePatch] ApplyState presentation logic failed: {ex}");
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
        // key 不含 battleId：host 与 client 的 BattleController 实例不同，
        // GetHashCode().ToString() 必然不同，会导致跨端 pending 永远匹配不上。
        // 每端同一时刻只有一个活跃战斗，spawnId 或 rootIndex|enemyId 已足够唯一。
        if (!string.IsNullOrWhiteSpace(spawnId))
        {
            return $"spawn:{spawnId}";
        }

        return BuildLegacyKey(battleId, rootIndex, enemyId);
    }

    private static string BuildLegacyKey(string battleId, int rootIndex, string enemyId)
    {
        return $"root:{rootIndex}|id:{enemyId ?? ""}";
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
        => NetworkEventHelper.TryGetJsonElement(payload, out root);

    private static string GetString(JsonElement root, string name)
        => NetworkEventHelper.GetString(root, name);

    private static bool GetBool(JsonElement root, string name)
        => NetworkEventHelper.GetBool(root, name);

    private static bool TryGetInt(JsonElement root, string name, out int value)
        => NetworkEventHelper.TryGetInt(root, name, out value);

    private static bool TryGetLong(JsonElement root, string name, out long value)
        => NetworkEventHelper.TryGetLong(root, name, out value);
}
