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
[HarmonyPatch]
public class EnemySyncPatch
{
    #region 依赖注入

    /// <summary>
    /// 依赖注入服务提供者，用于解析网络客户端。
    /// </summary>
    private static IServiceProvider serviceProvider => ModService.ServiceProvider;

    // 当 EnemyStateReceivePatch 正在应用远端敌人状态时置 true，
    // 此时本地 setter 不应再广播，避免"收到→应用→再广播"的回环。
    // 参考 sts2 lockstep 模式：动作经主机广播后各端本地执行，
    // 远端驱动的状态变更不应再回传。
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
        // 允许任何已连接客户端（含非房主）广播敌人状态变化，
        // 使非房主打敌人时所有人都能同步收到。
        // 正在应用远端状态时跳过（防止回环）。
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

    /// <summary>
    /// HP Setter 前置：记录变更前的 HP。
    /// </summary>
    /// <param name="__instance">敌人单位。</param>
    /// <param name="__state">用于保存变更前的 HP。</param>
    [HarmonyPatch(typeof(Unit), "Hp", MethodType.Setter)]
    [HarmonyPrefix]
    public static void EnemyHpChanged_Prefix(Unit __instance, ref int __state)
    {
        if (__instance is not EnemyUnit) return;
        __state = __instance.Hp;
    }

    /// <summary>
    /// HP Setter 后置：若 HP 发生变化则发送同步。
    /// </summary>
    /// <param name="__instance">敌人单位。</param>
    /// <param name="value">设置后的 HP 值。</param>
    /// <param name="__state">前置记录的旧 HP。</param>
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

    /// <summary>
    /// Block Setter 前置：记录变更前的 Block。
    /// </summary>
    /// <param name="__instance">敌人单位。</param>
    /// <param name="__state">用于保存变更前的 Block。</param>
    [HarmonyPatch(typeof(Unit), "Block", MethodType.Setter)]
    [HarmonyPrefix]
    public static void EnemyBlockChanged_Prefix(Unit __instance, ref int __state)
    {
        if (__instance is not EnemyUnit) return;
        __state = __instance.Block;
    }

    /// <summary>
    /// Block Setter 后置：若 Block 发生变化则发送同步。
    /// </summary>
    /// <param name="__instance">敌人单位。</param>
    /// <param name="value">设置后的 Block 值。</param>
    /// <param name="__state">前置记录的旧 Block。</param>
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

    /// <summary>
    /// Shield Setter 前置：记录变更前的 Shield。
    /// </summary>
    /// <param name="__instance">敌人单位。</param>
    /// <param name="__state">用于保存变更前的 Shield。</param>
    [HarmonyPatch(typeof(Unit), "Shield", MethodType.Setter)]
    [HarmonyPrefix]
    public static void EnemyShieldChanged_Prefix(Unit __instance, ref int __state)
    {
        if (__instance is not EnemyUnit) return;
        __state = __instance.Shield;
    }

    /// <summary>
    /// Shield Setter 后置：若 Shield 发生变化则发送同步。
    /// </summary>
    /// <param name="__instance">敌人单位。</param>
    /// <param name="value">设置后的 Shield 值。</param>
    /// <param name="__state">前置记录的旧 Shield。</param>
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
            INetworkClient networkClient = TryGetSyncNetworkClient();
            if (networkClient == null)
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
            SendEnemyStateUpdate(networkClient, json);

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
            INetworkClient networkClient = TryGetSyncNetworkClient();
            if (networkClient == null)
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
            SendEnemyStateUpdate(networkClient, json);

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
    /// <param name="__instance">敌人单位实例。</param>
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
