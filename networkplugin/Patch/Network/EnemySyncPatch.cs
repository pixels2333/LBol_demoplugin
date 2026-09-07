using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network.Services;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Patch.Network;

[HarmonyPatch]
public class EnemySyncPatch
{
    #region 依赖注入

        private static IServiceProvider serviceProvider => ModService.ServiceProvider;

    [ThreadStatic]
    private static bool _isApplyingRemoteState;
    internal static bool IsApplyingRemoteState => _isApplyingRemoteState;
    internal static IDisposable EnterApplyRemoteStateScope() => new ApplyRemoteStateScope();

    private sealed class ApplyRemoteStateScope : IDisposable
    {
        private bool _disposed;
        public ApplyRemoteStateScope() => _isApplyingRemoteState = true;
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _isApplyingRemoteState = false;
        }
    }

    private static INetworkClient TryGetSyncNetworkClient()
    {

        if (_isApplyingRemoteState) return null;
        var client = SendSyncHelper.TryGetClient();
        if (client?.IsConnected != true) return null;
        return client;
    }

    private static void SendEnemyStateUpdate(INetworkClient networkClient, string json)
    {
        networkClient.SendRequest(NetworkMessageTypes.BattleEnemyStateChanged, json);
        networkClient.SendRequest(NetworkMessageTypes.EnemyStateUpdate, json);
    }

    #endregion

    #region HP 同步

        [HarmonyPatch(typeof(Unit), "Hp", MethodType.Setter)]
    [HarmonyPrefix]
    public static void EnemyHpChanged_Prefix(Unit __instance, ref int __state)
    {
        if (__instance is not EnemyUnit) return;
        __state = __instance.Hp;
    }

        [HarmonyPatch(typeof(Unit), "Hp", MethodType.Setter)]
    [HarmonyPostfix]
    public static void EnemyHpChanged_Postfix(Unit __instance, int value, int __state)
    {
        if (__instance is not EnemyUnit enemy) return;
        try
        {
            INetworkClient networkClient = TryGetSyncNetworkClient();
            if (networkClient == null)
            {
                return;
            }

            if (enemy.Battle == null)
            {
                return;
            }

            int oldHp = __state;
            if (oldHp == value)
            {
                return;
            }

            object enemyData = BuildEnemyUpdateData(enemy, "HpChanged", new
            {
                OldHp = oldHp,
                NewHp = value,
                HpDifference = value - oldHp,
            });

            string json = JsonCompat.Serialize(enemyData);
            SendEnemyStateUpdate(networkClient, json);

            Plugin.Logger?.LogInfo($"[EnemySync] 敌人 {enemy.Name} HP: {oldHp} -> {value}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[EnemySync] EnemyHpChanged 异常: {ex.Message}");
        }
    }

    #endregion

    #region Block 同步

        [HarmonyPatch(typeof(Unit), "Block", MethodType.Setter)]
    [HarmonyPrefix]
    public static void EnemyBlockChanged_Prefix(Unit __instance, ref int __state)
    {
        if (__instance is not EnemyUnit) return;
        __state = __instance.Block;
    }

        [HarmonyPatch(typeof(Unit), "Block", MethodType.Setter)]
    [HarmonyPostfix]
    public static void EnemyBlockChanged_Postfix(Unit __instance, int value, int __state)
    {
        if (__instance is not EnemyUnit enemy) return;
        try
        {
            INetworkClient networkClient = TryGetSyncNetworkClient();
            if (networkClient == null)
            {
                return;
            }

            if (enemy.Battle == null)
            {
                return;
            }

            int oldBlock = __state;
            if (oldBlock == value)
            {
                return;
            }

            object enemyData = BuildEnemyUpdateData(enemy, "BlockChanged", new
            {
                OldBlock = oldBlock,
                NewBlock = value,
                BlockDifference = value - oldBlock,
            });

            string json = JsonCompat.Serialize(enemyData);
            SendEnemyStateUpdate(networkClient, json);

            Plugin.Logger?.LogInfo($"[EnemySync] 敌人 {enemy.Name} Block: {oldBlock} -> {value}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[EnemySync] EnemyBlockChanged 异常: {ex.Message}");
        }
    }

    #endregion

    #region Shield 同步

        [HarmonyPatch(typeof(Unit), "Shield", MethodType.Setter)]
    [HarmonyPrefix]
    public static void EnemyShieldChanged_Prefix(Unit __instance, ref int __state)
    {
        if (__instance is not EnemyUnit) return;
        __state = __instance.Shield;
    }

        [HarmonyPatch(typeof(Unit), "Shield", MethodType.Setter)]
    [HarmonyPostfix]
    public static void EnemyShieldChanged_Postfix(Unit __instance, int value, int __state)
    {
        if (__instance is not EnemyUnit enemy) return;
        try
        {
            INetworkClient networkClient = TryGetSyncNetworkClient();
            if (networkClient == null)
            {
                return;
            }

            if (enemy.Battle == null)
            {
                return;
            }

            int oldShield = __state;
            if (oldShield == value)
            {
                return;
            }

            object enemyData = BuildEnemyUpdateData(enemy, "ShieldChanged", new
            {
                OldShield = oldShield,
                NewShield = value,
                ShieldDifference = value - oldShield,
            });

            string json = JsonCompat.Serialize(enemyData);
            SendEnemyStateUpdate(networkClient, json);

            Plugin.Logger?.LogDebug($"[EnemySync] 敌人 {enemy.Name} Shield: {oldShield} -> {value}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[EnemySync] EnemyShieldChanged 异常: {ex.Message}");
        }
    }

    #endregion

    #region 状态效果同步

        [HarmonyPatch(typeof(BattleController), "TryAddStatusEffect")]
    [HarmonyPostfix]
    public static void EnemyStatusEffectAdded_Postfix(BattleController __instance, Unit target)
    {
        try
        {
            INetworkClient networkClient = TryGetSyncNetworkClient();
            if (networkClient == null)
            {
                return;
            }

            if (target is not EnemyUnit enemy)
            {
                return;
            }

            var statusEffects = GetEnemyStatusEffects(enemy);

            object enemyData = BuildEnemyUpdateData(enemy, "StatusAdded", new
            {
                StatusEffects = statusEffects,
                StatusEffectCount = statusEffects.Count,
            });

            string json = JsonCompat.Serialize(enemyData);
            SendEnemyStateUpdate(networkClient, json);

            Plugin.Logger?.LogInfo($"[EnemySync] 敌人 {enemy.Name} 状态效果已更新，数量: {statusEffects.Count}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[EnemySync] EnemyStatusEffectAdded 异常: {ex.Message}");
        }
    }

        [HarmonyPatch(typeof(BattleController), "RemoveStatusEffect")]
    [HarmonyPostfix]
    public static void EnemyStatusEffectRemoved_Postfix(BattleController __instance, Unit target)
    {
        try
        {
            INetworkClient networkClient = TryGetSyncNetworkClient();
            if (networkClient == null)
            {
                return;
            }

            if (target is not EnemyUnit enemy)
            {
                return;
            }

            var statusEffects = GetEnemyStatusEffects(enemy);

            object enemyData = BuildEnemyUpdateData(enemy, "StatusRemoved", new
            {
                StatusEffects = statusEffects,
                StatusEffectCount = statusEffects.Count,
            });

            string json = JsonCompat.Serialize(enemyData);
            SendEnemyStateUpdate(networkClient, json);

            Plugin.Logger?.LogInfo($"[EnemySync] 敌人 {enemy.Name} 状态效果移除后剩余: {statusEffects.Count}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[EnemySync] EnemyStatusEffectRemoved 异常: {ex.Message}");
        }
    }

    internal static void SyncEnemyStatusEffectChanged(EnemyUnit enemy)
    {
        if (enemy == null || _isApplyingRemoteState)
        {
            return;
        }

        try
        {
            INetworkClient networkClient = TryGetSyncNetworkClient();
            if (networkClient == null)
            {
                return;
            }

            var statusEffects = GetEnemyStatusEffects(enemy);

            object enemyData = BuildEnemyUpdateData(enemy, "StatusChanged", new
            {
                StatusEffects = statusEffects,
                StatusEffectCount = statusEffects.Count,
            });

            string json = JsonCompat.Serialize(enemyData);
            SendEnemyStateUpdate(networkClient, json);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[EnemySync] SyncEnemyStatusEffectChanged 异常: {ex.Message}");
        }
    }

    #endregion

    #region 意图同步

        [HarmonyPatch(typeof(EnemyUnit), nameof(EnemyUnit.NotifyIntentionsChanged))]
    [HarmonyPostfix]
    public static void EnemyIntentionCreated_Postfix(EnemyUnit __instance)
    {
        EnemyUnit enemy = __instance;
        try
        {
            INetworkClient networkClient = TryGetSyncNetworkClient();
            if (networkClient == null)
            {
                return;
            }

            if (enemy == null || !enemy.Intentions.Any())
            {
                return;
            }

            var intentionData = GetEnemyIntention(enemy);

            object enemyData = BuildEnemyUpdateData(enemy, "IntentionChanged", new
            {
                Intention = intentionData,
            });

            string json = JsonCompat.Serialize(enemyData);
            SendEnemyStateUpdate(networkClient, json);

            Plugin.Logger?.LogDebug($"[EnemySync] 敌人 {enemy.Name} 意图已更新: {intentionData.Type}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[EnemySync] EnemyIntentionCreated 异常: {ex.Message}");
        }
    }

    #endregion

    #region 死亡同步

        [HarmonyPatch(typeof(BattleController), "Die")]
    [HarmonyPostfix]
    public static void EnemyDied_Postfix(BattleController __instance, Unit unit)
    {
        try
        {
            INetworkClient networkClient = TryGetSyncNetworkClient();
            if (networkClient == null)
            {
                return;
            }

            if (unit is not EnemyUnit enemy)
            {
                return;
            }

            object enemyData = BuildEnemyUpdateData(enemy, "Died", new
            {
                DeathTime = DateTime.Now.Ticks,
                enemy.Hp,
                enemy.IsDying,
                enemy.IsAlive,
            });

            string json = JsonCompat.Serialize(enemyData);
            SendEnemyStateUpdate(networkClient, json);

            Plugin.Logger?.LogInfo($"[EnemySync] 敌人 {enemy.Name} 已死亡");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[EnemySync] EnemyDied 异常: {ex.Message}");
        }
    }

    #endregion

    #region 数据构建与提取

        private static object BuildEnemyUpdateData(EnemyUnit enemy, string updateType, object additionalData)
    {

        SpawnedEnemySyncPatch.TryGetSpawnId(enemy, out string spawnId);

        return new
        {
            UpdateType = updateType,
            Timestamp = DateTime.Now.Ticks,
            BattleId = enemy.Battle?.GetHashCode().ToString() ?? "unknown",
            Enemy = new
            {
                SpawnId = spawnId,
                enemy.Id,
                enemy.Name,
                Type = enemy.GetType().Name,
                enemy.RootIndex,
                Index = Traverse.Create(enemy).Property("Index")?.GetValue<int>() ?? 0,
                enemy.MaxHp,
                CurrentHp = enemy.Hp,
                enemy.Block,
                enemy.Shield,
                Status = enemy.Status.ToString(),
                enemy.IsAlive,
                enemy.IsDying,
            },
            UpdateData = additionalData,
        };
    }

        private static List<EnemyStatusEffectInfo> GetEnemyStatusEffects(EnemyUnit enemy)
    {
        List<EnemyStatusEffectInfo> effects = [];

        try
        {
            var statusEffects = Traverse.Create(enemy)
                .Field("_statusEffects")?
                .GetValue<OrderedList<StatusEffect>>();

            if (statusEffects == null)
            {
                return effects;
            }

            foreach (var effect in statusEffects)
            {
                if (effect == null)
                {
                    continue;
                }

                int level = 0;
                if (effect.HasLevel)
                {
                    level = effect.Level;
                }
                else if (effect.HasCount)
                {
                    level = effect.Count;
                }

                int duration = 0;
                if (effect.HasDuration)
                {
                    duration = effect.Duration;
                }

                effects.Add(new EnemyStatusEffectInfo
                {
                    Id = effect.Id,
                    Name = effect.Name,
                    Type = effect.GetType().Name,
                    Level = level,
                    Duration = duration,
                    IsDebuff = effect.Type == LBoL.Base.StatusEffectType.Negative,
                });
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[EnemySync] 获取状态效果列表失败: {ex.Message}");
        }

        return effects;
    }

        private static EnemyIntentionInfo GetEnemyIntention(EnemyUnit enemy)
    {
        if (enemy.Intentions == null)
        {
            return new EnemyIntentionInfo { Type = "None" };
        }

        try
        {
            var intention = enemy.Intentions.FirstOrDefault();

            return new EnemyIntentionInfo
            {
                Type = intention.GetType().Name,
                Name = intention.Name,
                Description = intention.Description,
            };
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[EnemySync] 获取意图失败: {ex.Message}");
            return new EnemyIntentionInfo { Type = "Error" };
        }
    }

    #endregion

    #region 数据结构

        private class EnemyStatusEffectInfo
    {
                public string Id { get; set; }

                public string Name { get; set; }

                public string Type { get; set; }

                public int Level { get; set; }

                public int Duration { get; set; }

                public bool IsDebuff { get; set; }
    }

        private class EnemyIntentionInfo
    {
                public string Type { get; set; }

                public string Name { get; set; }

                public string Description { get; set; }

                public string TargetId { get; set; }

                public string TargetType { get; set; }

                public int Value { get; set; }
    }

    #endregion
}
