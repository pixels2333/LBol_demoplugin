using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;

namespace NetworkPlugin.Patch.Network;

/// <summary>
/// 完整的敌人同步补丁 - 实现所有敌人状态的网络同步
/// 包括：HP、Block、Shield、StatusEffects、Intentions、死亡状态
/// 参考: 杀戮尖塔Together in Spire的CreatureSyncPatches
/// </summary>
public class EnemySyncPatch
{
    private static IServiceProvider serviceProvider => ModService.ServiceProvider;

    /// <summary>
    /// 敌人生命值变更同步 - 当敌人HP改变时触发
    /// </summary>
    [HarmonyPatch(typeof(EnemyUnit), "Hp", MethodType.Setter)]
    [HarmonyPostfix]
    public static void EnemyHpChanged_Postfix(EnemyUnit __instance, int hp, int __state)
    {
        try
        {
            if (serviceProvider == null)
            {
                return;
            }

            var networkClient = serviceProvider.GetService<INetworkClient>();
            if (networkClient == null || !networkClient.IsConnected)
            {
                return;
            }

            // 只同步在战斗中的敌人
            if (__instance.Battle == null)
            {
                return;
            }

            // 检查HP是否真的改变
            int oldHp = __state;
            if (oldHp == hp)
            {
                return;
            }

            var enemyData = BuildEnemyUpdateData(__instance, "HpChanged", new
            {
                OldHp = oldHp,
                NewHp = hp,
                HpDifference = hp - oldHp
            });

            var json = JsonSerializer.Serialize(enemyData);
            networkClient.SendRequest("EnemyStateUpdate", json);

            Plugin.Logger?.LogInfo($"[EnemySync] Enemy {__instance.Name} HP: {oldHp} -> {hp}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[EnemySync] Error in EnemyHpChanged: {ex.Message}");
        }
    }

    /// <summary>
    /// Record the original HP value before change
    /// </summary>
    [HarmonyPatch(typeof(EnemyUnit), "Hp", MethodType.Setter)]
    [HarmonyPrefix]
    public static void EnemyHpChanged_Prefix(EnemyUnit __instance, ref int __state)
    {
        __state = __instance.Hp;
    }

    /// <summary>
    /// 敌人Block变更同步
    /// </summary>
    [HarmonyPatch(typeof(EnemyUnit), "Block", MethodType.Setter)]
    [HarmonyPostfix]
    public static void EnemyBlockChanged_Postfix(EnemyUnit __instance, int block, int __state)
    {
        try
        {
            if (serviceProvider == null)
            {
                return;
            }

            var networkClient = serviceProvider.GetService<INetworkClient>();
            if (networkClient == null || !networkClient.IsConnected)
            {
                return;
            }

            if (__instance.Battle == null)
            {
                return;
            }

            int oldBlock = __state;
            if (oldBlock == block)
            {
                return;
            }

            var enemyData = BuildEnemyUpdateData(__instance, "BlockChanged", new
            {
                OldBlock = oldBlock,
                NewBlock = block,
                BlockDifference = block - oldBlock
            });

            var json = JsonSerializer.Serialize(enemyData);
            networkClient.SendRequest("EnemyStateUpdate", json);

            Plugin.Logger?.LogDebug($"[EnemySync] Enemy {__instance.Name} Block: {oldBlock} -> {block}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[EnemySync] Error in EnemyBlockChanged: {ex.Message}");
        }
    }

    [HarmonyPatch(typeof(EnemyUnit), "Block", MethodType.Setter)]
    [HarmonyPrefix]
    public static void EnemyBlockChanged_Prefix(EnemyUnit __instance, ref int __state)
    {
        __state = __instance.Block;
    }

    /// <summary>
    /// 敌人Shield变更同步
    /// </summary>
    [HarmonyPatch(typeof(EnemyUnit), "Shield", MethodType.Setter)]
    [HarmonyPostfix]
    public static void EnemyShieldChanged_Postfix(EnemyUnit __instance, int shield, int __state)
    {
        try
        {
            if (serviceProvider == null)
            {
                return;
            }

            var networkClient = serviceProvider.GetService<INetworkClient>();
            if (networkClient == null || !networkClient.IsConnected)
            {
                return;
            }

            if (__instance.Battle == null)
            {
                return;
            }

            int oldShield = __state;
            if (oldShield == shield)
            {
                return;
            }

            var enemyData = BuildEnemyUpdateData(__instance, "ShieldChanged", new
            {
                OldShield = oldShield,
                NewShield = shield,
                ShieldDifference = shield - oldShield
            });

            var json = JsonSerializer.Serialize(enemyData);
            networkClient.SendRequest("EnemyStateUpdate", json);

            Plugin.Logger?.LogDebug($"[EnemySync] Enemy {__instance.Name} Shield: {oldShield} -> {shield}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[EnemySync] Error in EnemyShieldChanged: {ex.Message}");
        }
    }

    [HarmonyPatch(typeof(EnemyUnit), "Shield", MethodType.Setter)]
    [HarmonyPrefix]
    public static void EnemyShieldChanged_Prefix(EnemyUnit __instance, ref int __state)
    {
        __state = __instance.Shield;
    }

    /// <summary>
    /// 敌人状态效果添加同步
    /// </summary>
    [HarmonyPatch(typeof(BattleController), "TryAddStatusEffect")]
    [HarmonyPostfix]
    public static void EnemyStatusEffectAdded_Postfix(BattleController __instance, Unit target)
    {
        try
        {
            if (serviceProvider == null)
            {
                return;
            }

            var networkClient = serviceProvider.GetService<INetworkClient>();
            if (networkClient == null || !networkClient.IsConnected)
            {
                return;
            }

            if (target is not EnemyUnit enemy)
            {
                return;
            }

            var statusEffects = GetEnemyStatusEffects(enemy);

            var enemyData = BuildEnemyUpdateData(enemy, "StatusAdded", new
            {
                statusEffects,
                StatusEffectCount = statusEffects.Count
            });

            var json = JsonSerializer.Serialize(enemyData);
            networkClient.SendRequest("EnemyStateUpdate", json);

            Plugin.Logger?.LogInfo($"[EnemySync] Enemy {enemy.Name} status effects updated, count: {statusEffects.Count}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[EnemySync] Error in EnemyStatusEffectAdded: {ex.Message}");
        }
    }

    /// <summary>
    /// 敌人状态效果移除同步
    /// </summary>
    [HarmonyPatch(typeof(BattleController), "RemoveStatusEffect")]
    [HarmonyPostfix]
    public static void EnemyStatusEffectRemoved_Postfix(BattleController __instance, Unit target)
    {
        try
        {
            if (serviceProvider == null)
            {
                return;
            }

            var networkClient = serviceProvider.GetService<INetworkClient>();
            if (networkClient == null || !networkClient.IsConnected)
            {
                return;
            }

            if (target is not EnemyUnit enemy)
            {
                return;
            }

            var statusEffects = GetEnemyStatusEffects(enemy);

            var enemyData = BuildEnemyUpdateData(enemy, "StatusRemoved", new
            {
                StatusEffects = statusEffects,
                StatusEffectCount = statusEffects.Count
            });

            var json = JsonSerializer.Serialize(enemyData);
            networkClient.SendRequest("EnemyStateUpdate", json);

            Plugin.Logger?.LogInfo($"[EnemySync] Enemy {enemy.Name} status effects removed, remaining: {statusEffects.Count}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[EnemySync] Error in EnemyStatusEffectRemoved: {ex.Message}");
        }
    }

    /// <summary>
    /// 敌人意图同步 - 当敌人创建或更新意图时触发
    /// </summary>
    [HarmonyPatch(typeof(BattleController), "CreateEnemyIntention")]
    [HarmonyPostfix]
    public static void EnemyIntentionCreated_Postfix(BattleController __instance, EnemyUnit enemy)
    {
        try
        {
            if (serviceProvider == null)
            {
                return;
            }

            var networkClient = serviceProvider.GetService<INetworkClient>();
            if (networkClient == null || !networkClient.IsConnected)
            {
                return;
            }

            if (enemy == null || !enemy.Intentions.Any())
            {
                return;
            }

            var intentionData = GetEnemyIntention(enemy);

            var enemyData = BuildEnemyUpdateData(enemy, "IntentionChanged", new
            {
                Intention = intentionData
            });

            var json = JsonSerializer.Serialize(enemyData);
            networkClient.SendRequest("EnemyStateUpdate", json);

            Plugin.Logger?.LogDebug($"[EnemySync] Enemy {enemy.Name} intention updated: {intentionData.Type}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[EnemySync] Error in EnemyIntentionCreated: {ex.Message}");
        }
    }

    /// <summary>
    /// 敌人死亡同步
    /// </summary>
    [HarmonyPatch(typeof(BattleController), "Die")]
    [HarmonyPostfix]
    public static void EnemyDied_Postfix(BattleController __instance, Unit unit)
    {
        try
        {
            if (serviceProvider == null)
            {
                return;
            }

            var networkClient = serviceProvider.GetService<INetworkClient>();
            if (networkClient == null || !networkClient.IsConnected)
            {
                return;
            }

            if (unit is not EnemyUnit enemy)
            {
                return;
            }

            var enemyData = BuildEnemyUpdateData(enemy, "Died", new
            {
                DeathTime = DateTime.Now.Ticks,
                enemy.Hp,
                enemy.IsDying,
                enemy.IsAlive
            });

            var json = JsonSerializer.Serialize(enemyData);
            networkClient.SendRequest("EnemyStateUpdate", json);

            Plugin.Logger?.LogInfo($"[EnemySync] Enemy {enemy.Name} died");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[EnemySync] Error in EnemyDied: {ex.Message}");
        }
    }

    /// <summary>
    /// 构建敌人状态更新数据
    /// </summary>
    private static object BuildEnemyUpdateData(EnemyUnit enemy, string updateType, object additionalData)
    {
        return new
        {
            UpdateType = updateType,
            Timestamp = DateTime.Now.Ticks,
            BattleId = enemy.Battle?.GetHashCode().ToString() ?? "unknown",
            Enemy = new
            {
                enemy.Id,
                enemy.Name,
                Type = enemy.GetType().Name,
                enemy.MaxHp,
                CurrentHp = enemy.Hp,
                enemy.Block,
                enemy.Shield,
                Status = enemy.Status.ToString(),
                enemy.IsAlive,
                enemy.IsDying
            },
            UpdateData = additionalData
        };
    }

    /// <summary>
    /// 获取敌人状态效果列表
    /// </summary>
    private static List<EnemyStatusEffectInfo> GetEnemyStatusEffects(EnemyUnit enemy)
    {
        var effects = new List<EnemyStatusEffectInfo>();

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
                effects.Add(new EnemyStatusEffectInfo
                {
                    Id = effect.Id,
                    Name = effect.Name,
                    Type = effect.GetType().Name,
                    Level = effect.Level,
                    Duration = effect.Duration,
                    IsDebuff = effect.IsDebuff
                });
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[EnemySync] Error getting status effects: {ex.Message}");
        }

        return effects;
    }

    /// <summary>
    /// 获取敌人当前意图
    /// </summary>
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
                Description = intention.Description
            };
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[EnemySync] Error getting intention: {ex.Message}");
            return new EnemyIntentionInfo { Type = "Error" };
        }
    }

    /// <summary>
    /// 状态效果信息
    /// </summary>
    private class EnemyStatusEffectInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public int Level { get; set; }
        public int Duration { get; set; }
        public bool IsDebuff { get; set; }
    }

    /// <summary>
    /// 意图信息
    /// </summary>
    private class EnemyIntentionInfo
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string TargetId { get; set; }
        public string TargetType { get; set; }
        public int Value { get; set; }
    }
}
