using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Configuration;
using NetworkPlugin.Core;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Event;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Network.NetworkPlayer;
using NetworkPlugin.Patch.Network;

namespace NetworkPlugin.Patch.Actions;

/// <summary>
/// 状态效果应用动作同步补丁类
/// 同步游戏中的状态效果应用事件，包括增益、减益、特殊效果等
/// </summary>
public class ApplyStatusEffectAction_Patch
{
    /// <summary>
    /// 服务提供者实例
    /// </summary>
    private static IServiceProvider serviceProvider => ModService.ServiceProvider;

    /// <summary>
    /// 配置管理器实例
    /// </summary>
    private static ConfigManager ConfigManager => serviceProvider?.GetService<ConfigManager>();

    // ========================================
    // 构造函数补丁
    // ========================================

    /// <summary>
    /// ApplyStatusEffectAction构造函数同步补丁
    /// 拦截：ApplyStatusEffectAction(Type statusEffectType, Unit target, ...)
    /// </summary>
    [HarmonyPatch(typeof(ApplyStatusEffectAction), MethodType.Constructor,
        typeof(Type), typeof(Unit), typeof(int?), typeof(int?), typeof(int?), typeof(int?), typeof(float), typeof(bool))]
    [HarmonyPostfix]
    public static void Constructor_Postfix(ApplyStatusEffectAction __instance,
        Type statusEffectType, Unit target, int? level, int? duration, int? count, int? limit,
        float occupationTime, bool startAutoDecreasing)
    {
        try
        {
            // 检查状态效果同步是否启用
            if (ConfigManager?.EnableStatusEffectSync?.Value != true)
                return;

            if (RemoteCardUsePatch.IsInRemoteCardPipeline)
                return;

            ISynchronizationManager syncManager = GetSyncManager();
            if (syncManager == null)
                return;

            INetworkManager networkManager = GetNetworkManager();
            if (networkManager == null)
                return;

            INetworkPlayer player = networkManager.GetSelf();

            // 构建状态效果应用同步数据
            Dictionary<string, object> statusData = new()
            {
                ["UserName"] = player.userName,
                ["Timestamp"] = DateTime.Now.Ticks,
                ["ActionType"] = "ApplyStatusEffect",
                ["StatusEffectType"] = statusEffectType?.Name ?? "Unknown",
                ["StatusEffectFullName"] = statusEffectType?.FullName ?? "Unknown",
                ["TargetId"] = target?.Id ?? "",
                ["TargetName"] = target?.Name ?? "Unknown",
                // ["TargetType"] = target?.GetType().Name ?? "Unknown",
                ["Level"] = level ?? 0,
                ["Duration"] = duration ?? 0,
                ["Count"] = count ?? 0,
                ["Limit"] = limit ?? 0,
                ["OccupationTime"] = occupationTime,
                ["StartAutoDecreasing"] = startAutoDecreasing
            };

            // 获取目标当前的状态效果信息
            if (target != null && target.StatusEffects != null)
            {
                List<Dictionary<string, object>> existingStatusEffects = new();
                foreach (var statusEffect in target.StatusEffects)
                {
                    if (statusEffect != null)
                    {
                        existingStatusEffects.Add(new Dictionary<string, object>
                        {
                            ["StatusType"] = statusEffect.GetType().Name,
                            ["Level"] = statusEffect.Level,
                            ["Duration"] = statusEffect.Duration,
                            ["Count"] = statusEffect.Count
                        });
                    }
                }
                statusData.Add("ExistingStatusEffects", existingStatusEffects);
                statusData.Add("ExistingStatusCount", target.StatusEffects.Count);
            }

            GameEvent gameEvent = GameEventManager.CreateEvent(
                NetworkMessageTypes.OnStatusEffectApplied.ToString(),
                player.userName,
                statusData
            );

            syncManager.SendGameEvent(gameEvent);

            Plugin.Logger?.LogInfo($"[StatusEffectSync] 应用状态效果: {statusEffectType?.Name} -> {target?.Name} " +
                $"(等级: {level}, 持续: {duration}, 数量: {count})");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[StatusEffectSync] Constructor_Postfix错误: {ex.Message}");
        }
    }

    /// <summary>
    /// ApplyStatusEffectAction<TEffect>构造函数同步补丁
    /// 拦截：ApplyStatusEffectAction<TEffect>(Unit target, int? level, ...)
    /// </summary>
    [HarmonyPatch(typeof(ApplyStatusEffectAction<>), MethodType.Constructor,
        typeof(Unit), typeof(int?), typeof(int?), typeof(int?), typeof(int?), typeof(float), typeof(bool))]
    [HarmonyPostfix]
    public static void GenericConstructor_Postfix<T>(ApplyStatusEffectAction<T> __instance,
        Unit target, int? level, int? duration, int? count, int? limit,
        float occupationTime, bool startAutoDecreasing) where T : StatusEffect
    {
        try
        {
            // 检查状态效果同步是否启用
            if (ConfigManager?.EnableStatusEffectSync?.Value != true)
                return;

            if (RemoteCardUsePatch.IsInRemoteCardPipeline)
                return;

            ISynchronizationManager syncManager = GetSyncManager();
            if (syncManager == null)
                return;

            INetworkManager networkManager = GetNetworkManager();
            if (networkManager == null)
                return;

            INetworkPlayer player = networkManager.GetSelf();

            Type statusEffectType = typeof(T);

            // 构建泛型状态效果应用同步数据
            Dictionary<string, object> statusData = new()
            {
                ["UserName"] = player.userName,
                ["Timestamp"] = DateTime.Now.Ticks,
                ["ActionType"] = "ApplyStatusEffectGeneric",
                ["StatusEffectType"] = statusEffectType?.Name ?? "Unknown",
                ["StatusEffectFullName"] = statusEffectType?.FullName ?? "Unknown",
                ["GenericArgument"] = typeof(T).Name,
                ["TargetId"] = target?.Id ?? "",
                ["TargetName"] = target?.Name ?? "Unknown",
                ["TargetType"] = target?.GetType().Name ?? "Unknown",
                // ["TargetCurrentHp"] = target?.Hp ?? 0,
                ["Level"] = level ?? 0,
                ["Duration"] = duration ?? 0,
                // ["Count"] = count ?? 0,
                ["Limit"] = limit ?? 0,
                ["OccupationTime"] = occupationTime,
                ["StartAutoDecreasing"] = startAutoDecreasing
            };

            // 获取目标当前的状态效果信息
            if (target != null && target.StatusEffects != null)
            {
                List<Dictionary<string, object>> existingStatusEffects = new();
                foreach (var statusEffect in target.StatusEffects)
                {
                    if (statusEffect != null)
                    {
                        existingStatusEffects.Add(new Dictionary<string, object>
                        {
                            ["StatusType"] = statusEffect.GetType().Name,
                            ["Level"] = statusEffect.Level,
                            ["Duration"] = statusEffect.Duration,
                            ["Count"] = statusEffect.Count,
                            // ["IsActive"] = statusEffect.IsActive
                        });
                    }
                }
                statusData["ExistingStatusEffects"] = existingStatusEffects;
                statusData["ExistingStatusCount"] = target.StatusEffects.Count;
            }

            // 检查是否为特定类型的增益效果
            if (typeof(T).GetInterfaces().Any(i => i.Name.Contains("IBuff") || i.Name.Contains("IDebuff")))
            {
                statusData["EffectCategory"] = typeof(T).GetInterfaces()
                    .Where(i => i.Name.Contains("IBuff") || i.Name.Contains("IDebuff"))
                    .Select(i => i.Name)
                    .FirstOrDefault() ?? "Unknown";
            }

            GameEvent gameEvent = GameEventManager.CreateEvent(
                NetworkMessageTypes.OnStatusEffectApplied.ToString(),
                player.userName,
                statusData
            );

            syncManager.SendGameEvent(gameEvent);

            Plugin.Logger?.LogInfo($"[StatusEffectSync] 应用泛型状态效果: {typeof(T).Name} -> {target?.Name} " +
                $"(等级: {level}, 持续: {duration}, 数量: {count})");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[StatusEffectSync] GenericConstructor_Postfix错误: {ex.Message}");
        }
    }

    // ========================================
    // 辅助方法
    // ========================================

    /// <summary>
    /// 获取同步管理器
    /// </summary>
    private static ISynchronizationManager GetSyncManager()
    {
        try
        {
            return serviceProvider?.GetService<ISynchronizationManager>();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 获取网络管理器
    /// </summary>
    private static INetworkManager GetNetworkManager()
    {
        try
        {
            return serviceProvider?.GetService<INetworkManager>();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 获取状态效果的显示名称
    /// </summary>
    /// <param name="statusEffectType">状态效果类型</param>
    /// <returns>显示名称</returns>
    private static string GetStatusEffectDisplayName(Type statusEffectType)
    {
        if (statusEffectType == null)
            return "Unknown";

        // 尝试获取状态的显示名称属性或自定义属性
        object displayAttribute = statusEffectType.GetCustomAttributes(false)
            .FirstOrDefault(attr => attr.GetType().Name.Contains("Display") || attr.GetType().Name.Contains("Name"));

        if (displayAttribute != null)
        {
            var nameProperty = displayAttribute.GetType().GetProperty("Name") ??
                               displayAttribute.GetType().GetProperty("DisplayName");
            if (nameProperty != null && nameProperty.GetValue(displayAttribute) is string name)
                return name;
        }

        return statusEffectType.Name;
    }

    /// <summary>
    /// 判断状态效果是否为增益效果
    /// </summary>
    /// <param name="statusEffectType">状态效果类型</param>
    /// <returns>是否为增益效果</returns>
    private static bool IsBuffEffect(Type statusEffectType)
    {
        if (statusEffectType == null)
            return false;

        // 通过类型名称或接口判断是否为增益效果
        return statusEffectType.Name.Contains("Buff") ||
               statusEffectType.Name.Contains("Boost") ||
               statusEffectType.GetInterfaces().Any(i => i.Name.Contains("IBuff"));
    }

    /// <summary>
    /// 判断状态效果是否为减益效果
    /// </summary>
    /// <param name="statusEffectType">状态效果类型</param>
    /// <returns>是否为减益效果</returns>
    private static bool IsDebuffEffect(Type statusEffectType)
    {
        if (statusEffectType == null)
            return false;

        // 通过类型名称或接口判断是否为减益效果
        return statusEffectType.Name.Contains("Debuff") ||
               statusEffectType.Name.Contains("Negative") ||
               statusEffectType.GetInterfaces().Any(i => i.Name.Contains("IDebuff"));
    }
}
