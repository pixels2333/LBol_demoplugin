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
using NetworkPlugin.Utils;

namespace NetworkPlugin.Patch.Network;

/// <summary>
/// 敌人完整状态同步补丁。
/// </summary>
/// <remarks>
/// 同步范围（主要面向“观察/复现”）：
/// - 敌人 HP / Block / Shield
/// - 敌人状态效果（增减）
/// - 敌人意图（CreateEnemyIntention）
/// - 敌人死亡
/// </remarks>
public class EnemySyncPatch
{
    #region 依赖注入

    /// <summary>
    /// 依赖注入服务提供者，用于解析网络客户端。
    /// </summary>
    private static IServiceProvider serviceProvider => ModService.ServiceProvider;

    #endregion

    #region HP 同步

    /// <summary>
    /// HP Setter 前置：记录变更前的 HP。
    /// </summary>
    /// <param name="__instance">敌人单位。</param>
    /// <param name="__state">用于保存变更前的 HP。</param>
    [HarmonyPatch(typeof(EnemyUnit), "Hp", MethodType.Setter)]
    [HarmonyPrefix]
    public static void EnemyHpChanged_Prefix(EnemyUnit __instance, ref int __state)
    {
        // 记录旧值供后置比较。
        __state = __instance.Hp;
    }

    /// <summary>
    /// HP Setter 后置：若 HP 发生变化则发送同步。
    /// </summary>
    /// <param name="__instance">敌人单位。</param>
    /// <param name="hp">设置后的 HP 值。</param>
    /// <param name="__state">前置记录的旧 HP。</param>
    [HarmonyPatch(typeof(EnemyUnit), "Hp", MethodType.Setter)]
    [HarmonyPostfix]
    public static void EnemyHpChanged_Postfix(EnemyUnit __instance, int hp, int __state)
    {
        try
        {
            // 服务未就绪直接跳过。
            if (serviceProvider == null)
            {
                return;
            }

            // 客户端未连接则不发送。
            var networkClient = serviceProvider.GetService<INetworkClient>();
            if (networkClient == null || !networkClient.IsConnected)
            {
                return;
            }

            // 只同步战斗中的敌人（非战斗对象忽略）。
            if (__instance.Battle == null)
            {
                return;
            }

            // 没有变化则不发送。
            int oldHp = __state;
            if (oldHp == hp)
            {
                return;
            }

            // 构建事件数据并发送。
            object enemyData = BuildEnemyUpdateData(__instance, "HpChanged", new
            {
                OldHp = oldHp,
                NewHp = hp,
                HpDifference = hp - oldHp,
            });

            string json = JsonCompat.Serialize(enemyData);
            networkClient.SendRequest("EnemyStateUpdate", json);

            Plugin.Logger?.LogInfo($"[EnemySync] 敌人 {__instance.Name} HP: {oldHp} -> {hp}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[EnemySync] EnemyHpChanged 异常: {ex.Message}");
        }
    }

    #endregion

    #region Block 同步

    /// <summary>
    /// Block Setter 前置：记录变更前的 Block。
    /// </summary>
    /// <param name="__instance">敌人单位。</param>
    /// <param name="__state">用于保存变更前的 Block。</param>
    [HarmonyPatch(typeof(EnemyUnit), "Block", MethodType.Setter)]
    [HarmonyPrefix]
    public static void EnemyBlockChanged_Prefix(EnemyUnit __instance, ref int __state)
    {
        // 记录旧值供后置比较。
        __state = __instance.Block;
    }

    /// <summary>
    /// Block Setter 后置：若 Block 发生变化则发送同步。
    /// </summary>
    /// <param name="__instance">敌人单位。</param>
    /// <param name="block">设置后的 Block 值。</param>
    /// <param name="__state">前置记录的旧 Block。</param>
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

            object enemyData = BuildEnemyUpdateData(__instance, "BlockChanged", new
            {
                OldBlock = oldBlock,
                NewBlock = block,
                BlockDifference = block - oldBlock,
            });

            string json = JsonCompat.Serialize(enemyData);
            networkClient.SendRequest("EnemyStateUpdate", json);

            Plugin.Logger?.LogDebug($"[EnemySync] 敌人 {__instance.Name} Block: {oldBlock} -> {block}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[EnemySync] EnemyBlockChanged 异常: {ex.Message}");
        }
    }

    #endregion

    #region Shield 同步

    /// <summary>
    /// Shield Setter 前置：记录变更前的 Shield。
    /// </summary>
    /// <param name="__instance">敌人单位。</param>
    /// <param name="__state">用于保存变更前的 Shield。</param>
    [HarmonyPatch(typeof(EnemyUnit), "Shield", MethodType.Setter)]
    [HarmonyPrefix]
    public static void EnemyShieldChanged_Prefix(EnemyUnit __instance, ref int __state)
    {
        // 记录旧值供后置比较。
        __state = __instance.Shield;
    }

    /// <summary>
    /// Shield Setter 后置：若 Shield 发生变化则发送同步。
    /// </summary>
    /// <param name="__instance">敌人单位。</param>
    /// <param name="shield">设置后的 Shield 值。</param>
    /// <param name="__state">前置记录的旧 Shield。</param>
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

            object enemyData = BuildEnemyUpdateData(__instance, "ShieldChanged", new
            {
                OldShield = oldShield,
                NewShield = shield,
                ShieldDifference = shield - oldShield,
            });

            string json = JsonCompat.Serialize(enemyData);
            networkClient.SendRequest("EnemyStateUpdate", json);

            Plugin.Logger?.LogDebug($"[EnemySync] 敌人 {__instance.Name} Shield: {oldShield} -> {shield}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[EnemySync] EnemyShieldChanged 异常: {ex.Message}");
        }
    }

    #endregion

    #region 状态效果同步

    /// <summary>
    /// 敌人添加状态效果后置：同步敌人完整状态效果列表。
    /// </summary>
    /// <param name="__instance">战斗控制器实例。</param>
    /// <param name="target">被添加状态效果的目标。</param>
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

            // 只处理敌人目标。
            if (target is not EnemyUnit enemy)
            {
                return;
            }

            var statusEffects = GetEnemyStatusEffects(enemy);

            object enemyData = BuildEnemyUpdateData(enemy, "StatusAdded", new
            {
                statusEffects,
                StatusEffectCount = statusEffects.Count,
            });

            string json = JsonCompat.Serialize(enemyData);
            networkClient.SendRequest("EnemyStateUpdate", json);

            Plugin.Logger?.LogInfo($"[EnemySync] 敌人 {enemy.Name} 状态效果已更新，数量: {statusEffects.Count}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[EnemySync] EnemyStatusEffectAdded 异常: {ex.Message}");
        }
    }

    /// <summary>
    /// 敌人移除状态效果后置：同步敌人完整状态效果列表。
    /// </summary>
    /// <param name="__instance">战斗控制器实例。</param>
    /// <param name="target">被移除状态效果的目标。</param>
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

            // 只处理敌人目标。
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
            networkClient.SendRequest("EnemyStateUpdate", json);

            Plugin.Logger?.LogInfo($"[EnemySync] 敌人 {enemy.Name} 状态效果移除后剩余: {statusEffects.Count}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[EnemySync] EnemyStatusEffectRemoved 异常: {ex.Message}");
        }
    }

    #endregion

    #region 意图同步

    /// <summary>
    /// 敌人创建/更新意图后置：同步敌人当前意图。
    /// </summary>
    /// <param name="__instance">战斗控制器实例。</param>
    /// <param name="enemy">敌人单位。</param>
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

            // 没有意图不处理。
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
            networkClient.SendRequest("EnemyStateUpdate", json);

            Plugin.Logger?.LogDebug($"[EnemySync] 敌人 {enemy.Name} 意图已更新: {intentionData.Type}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[EnemySync] EnemyIntentionCreated 异常: {ex.Message}");
        }
    }

    #endregion

    #region 死亡同步

    /// <summary>
    /// 敌人死亡后置：同步敌人死亡状态。
    /// </summary>
    /// <param name="__instance">战斗控制器实例。</param>
    /// <param name="unit">死亡单位。</param>
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

            object enemyData = BuildEnemyUpdateData(enemy, "Died", new
            {
                DeathTime = DateTime.Now.Ticks,
                enemy.Hp,
                enemy.IsDying,
                enemy.IsAlive,
            });

            string json = JsonCompat.Serialize(enemyData);
            networkClient.SendRequest("EnemyStateUpdate", json);

            Plugin.Logger?.LogInfo($"[EnemySync] 敌人 {enemy.Name} 已死亡");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[EnemySync] EnemyDied 异常: {ex.Message}");
        }
    }

    #endregion

    #region 数据构建与提取

    /// <summary>
    /// 构建敌人状态更新包。
    /// </summary>
    /// <param name="enemy">敌人单位。</param>
    /// <param name="updateType">更新类型标记。</param>
    /// <param name="additionalData">附加数据。</param>
    /// <returns>可序列化对象。</returns>
    private static object BuildEnemyUpdateData(EnemyUnit enemy, string updateType, object additionalData)
    {
        // 尝试获取 spawnId（若该敌人来自 SpawnedEnemySyncPatch 的生成逻辑）。
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

    /// <summary>
    /// 获取敌人状态效果列表（通过 Traverse 读取私有字段）。
    /// </summary>
    /// <param name="enemy">敌人单位。</param>
    /// <returns>状态效果信息列表。</returns>
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
                effects.Add(new EnemyStatusEffectInfo
                {
                    Id = effect.Id,
                    Name = effect.Name,
                    Type = effect.GetType().Name,
                    Level = effect.Level,
                    Duration = effect.Duration,
                    IsDebuff = false,
                });
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[EnemySync] 获取状态效果列表失败: {ex.Message}");
        }

        return effects;
    }

    /// <summary>
    /// 获取敌人当前意图（取第一条意图作为展示/同步对象）。
    /// </summary>
    /// <param name="enemy">敌人单位。</param>
    /// <returns>意图信息。</returns>
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

    /// <summary>
    /// 用于网络传输的状态效果信息。
    /// </summary>
    private class EnemyStatusEffectInfo
    {
        /// <summary>状态效果 Id。</summary>
        public string Id { get; set; }

        /// <summary>状态效果名称。</summary>
        public string Name { get; set; }

        /// <summary>状态效果类型名。</summary>
        public string Type { get; set; }

        /// <summary>等级。</summary>
        public int Level { get; set; }

        /// <summary>持续回合。</summary>
        public int Duration { get; set; }

        /// <summary>是否为减益（此处为占位，需更准确分类时补充）。</summary>
        public bool IsDebuff { get; set; }
    }

    /// <summary>
    /// 用于网络传输的敌人意图信息。
    /// </summary>
    private class EnemyIntentionInfo
    {
        /// <summary>意图类型名。</summary>
        public string Type { get; set; }

        /// <summary>意图显示名称。</summary>
        public string Name { get; set; }

        /// <summary>意图描述。</summary>
        public string Description { get; set; }

        /// <summary>目标单位 Id（预留）。</summary>
        public string TargetId { get; set; }

        /// <summary>目标类型（预留）。</summary>
        public string TargetType { get; set; }

        /// <summary>意图数值（预留）。</summary>
        public int Value { get; set; }
    }

    #endregion
}
