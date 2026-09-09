using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network.Services;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Network.Snapshot;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Patch.Network;

public static class EnemyStateReceivePatch
{
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    private static bool _subscribed;
    private static INetworkClient _subscribedClient;
    private static readonly Action<string, object> _onGameEventReceived = OnGameEventReceived;
    private static readonly Action<bool> _onConnectionStateChanged = OnConnectionStateChanged;

    private static readonly object _lock = new();
    private static readonly Dictionary<string, PendingState> _pendingByEnemyKey = new(StringComparer.Ordinal);
    private static readonly HashSet<EnemyUnit> _killedEnemiesInBattle = new();

    [HarmonyPatch(typeof(BattleController), "StartBattle")]
    private static class BattleController_StartBattle_ClearKilled
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            lock (_lock)
            {
                _killedEnemiesInBattle.Clear();
                _pendingByEnemyKey.Clear();
            }
        }
    }

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
        public List<RemoteStatusEffectInfo> StatusEffects;
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

        List<RemoteStatusEffectInfo> parsedEffects = null;
        if (root.TryGetProperty("UpdateData", out JsonElement updateDataElem) && updateDataElem.ValueKind == JsonValueKind.Object)
        {
            if ((updateDataElem.TryGetProperty("StatusEffects", out JsonElement seElem) || updateDataElem.TryGetProperty("statusEffects", out seElem)) && seElem.ValueKind == JsonValueKind.Array)
            {
                parsedEffects = ParseStatusEffects(seElem);
            }
        }

        if (parsedEffects == null && (enemyElem.TryGetProperty("StatusEffects", out JsonElement seElemDirect) || enemyElem.TryGetProperty("statusEffects", out seElemDirect)) && seElemDirect.ValueKind == JsonValueKind.Array)
        {
            parsedEffects = ParseStatusEffects(seElemDirect);
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
            StatusEffects = parsedEffects,
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
        Plugin.RunOnMainThread(() =>
        {
            try
            {
                BattleController battle = TryGetCurrentBattle();
                if (battle?.EnemyGroup == null)
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
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[EnemyStateReceivePatch] TryApplyPendingToBattle 异常: {ex.Message}");
            }
        });
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
        if (enemy == null || !enemy.IsAlive || enemy.IsDying || enemy.Hp <= 0)
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

        using (EnemySyncPatch.EnterApplyRemoteStateScope())
        {
            TrySetEnemyProperty(enemy, "Hp", newHp);
            TrySetEnemyProperty(enemy, "Block", newBlock);
            TrySetEnemyProperty(enemy, "Shield", newShield);
        }

        if (newHp == 0)
        {
            var battle = enemy.Battle;
            if (battle != null)
            {
                lock (_lock)
                {
                    if (_killedEnemiesInBattle.Add(enemy))
                    {
                        Plugin.Logger?.LogInfo($"[EnemyStateReceive] 触发远程斩杀 ForceKillAction: {enemy.Name}");
                        battle.RequestDebugAction(new ForceKillAction(battle.Player, enemy), "RemoteForceKill");
                    }
                }
            }
            return;
        }

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

                    if (!RemoteCardPlaybackTracker.IsInCardPlaybackWindow)
                    {
                        view.ComingDamage = damageInfo;
                        view.Hit(ignoreCoolDown: true);

                        if (PopupHud.Instance != null)
                        {
                            PopupHud.Instance.DamagePopupFromScene(damageInfo, view.transform.position, sourceIsPlayer: true);
                        }

                        view.OnDamageReceived(damageInfo);
                    }
                    else
                    {
                        Plugin.Logger?.LogDebug($"[EnemyStateReceive] Suppressed preemptive damage popup/hit for {enemy.Name} during card playback window.");
                    }
                }
                else if (healAmount > 0)
                {

                    if (PopupHud.Instance != null)
                    {
                        PopupHud.Instance.HealPopupFromScene(healAmount, view.transform.position);
                    }

                    view.OnHealingReceived(healAmount);
                }
                else if (newBlock != oldBlock || newShield != oldShield)
                {

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

        if (pending.StatusEffects != null)
        {
            ApplyRemoteStatusEffectsToEnemy(enemy, pending.StatusEffects);
        }
    }

    private static List<RemoteStatusEffectInfo> ParseStatusEffects(JsonElement seArrayElem)
    {
        List<RemoteStatusEffectInfo> list = new();
        foreach (JsonElement elem in seArrayElem.EnumerateArray())
        {
            if (elem.ValueKind != JsonValueKind.Object) continue;
            list.Add(new RemoteStatusEffectInfo
            {
                Id = GetString(elem, "Id"),
                Type = GetString(elem, "Type"),
                Level = TryGetInt(elem, "Level", out int lvl) ? lvl : 0,
                Duration = TryGetInt(elem, "Duration", out int dur) ? dur : 0,
            });
        }
        return list;
    }

    private static void ApplyRemoteStatusEffectsToEnemy(EnemyUnit enemy, List<RemoteStatusEffectInfo> remoteEffects)
    {
        try
        {
            if (enemy == null || enemy.Battle == null || !enemy.IsAlive)
            {
                return;
            }

            using (EnemySyncPatch.EnterApplyRemoteStateScope())
            {
                var currentEffects = Traverse.Create(enemy).Field("_statusEffects")?.GetValue<OrderedList<StatusEffect>>();
                if (currentEffects == null)
                {
                    return;
                }

                var view = GameDirector.GetEnemy(enemy);

                HashSet<string> remoteIds = new(StringComparer.Ordinal);
                foreach (var rInfo in remoteEffects)
                {
                    if (string.IsNullOrWhiteSpace(rInfo.Id) && string.IsNullOrWhiteSpace(rInfo.Type))
                    {
                        continue;
                    }

                    string effectId = !string.IsNullOrWhiteSpace(rInfo.Id) ? rInfo.Id : rInfo.Type;
                    remoteIds.Add(effectId);

                    StatusEffect existing = currentEffects.FirstOrDefault(se =>
                        string.Equals(se.Id, effectId, StringComparison.Ordinal) ||
                        string.Equals(se.GetType().Name, effectId, StringComparison.Ordinal));

                    if (existing != null)
                    {
                        if (existing.HasLevel)
                        {
                            existing.Level = rInfo.Level;
                        }
                        else if (existing.HasCount)
                        {
                            existing.Count = rInfo.Level;
                        }

                        if (existing.HasDuration)
                        {
                            existing.Duration = rInfo.Duration;
                        }

                        if (view != null && !string.IsNullOrEmpty(existing.UnitEffectName))
                        {
                            view.SendEffectMessage(existing.UnitEffectName, "OnPropertyChanged", existing);
                        }
                    }
                    else
                    {
                        StatusEffect newEffect = Library.TryCreateStatusEffect(effectId);
                        if (newEffect == null && !string.IsNullOrWhiteSpace(rInfo.Type))
                        {
                            newEffect = Library.TryCreateStatusEffect(rInfo.Type);
                        }

                        if (newEffect != null)
                        {
                            Traverse.Create(newEffect).Property("GameRun").SetValue(enemy.GameRun ?? enemy.Battle?.GameRun);

                            if (newEffect.HasLevel && rInfo.Level > 0)
                            {
                                newEffect.Level = rInfo.Level;
                            }
                            else if (newEffect.HasCount && rInfo.Level > 0)
                            {
                                newEffect.Count = rInfo.Level;
                            }

                            if (newEffect.HasDuration && rInfo.Duration > 0)
                            {
                                newEffect.Duration = rInfo.Duration;
                            }

                            if (enemy.Battle != null)
                            {
                                Traverse.Create(enemy.Battle).Method("TryAddStatusEffect", enemy, newEffect).GetValue();
                            }

                            if (!string.IsNullOrEmpty(newEffect.UnitEffectName) && view != null)
                            {
                                view.TryPlayEffectLoop(newEffect.UnitEffectName);
                                view.SendEffectMessage(newEffect.UnitEffectName, "OnPropertyChanged", newEffect);
                            }
                        }
                    }
                }

                List<StatusEffect> toRemove = currentEffects.Where(se => !remoteIds.Contains(se.Id) && !remoteIds.Contains(se.GetType().Name)).ToList();
                foreach (var se in toRemove)
                {
                    if (!string.IsNullOrEmpty(se.UnitEffectName) && view != null)
                    {
                        view.EndEffectLoop(se.UnitEffectName, true);
                    }

                    if (enemy.Battle != null)
                    {
                        Traverse.Create(enemy.Battle).Method("RemoveStatusEffect", enemy, se).GetValue();
                    }
                }

                if (view != null)
                {
                    var widget = Traverse.Create(view).Field("_statusWidget").GetValue();
                    if (widget != null)
                    {
                        Traverse.Create(widget).Method("SetStatusEffects").GetValue();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogWarning($"[EnemyStateReceivePatch] ApplyRemoteStatusEffectsToEnemy 异常: {ex.Message}");
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
