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
/// 状态效果应用动作同步补丁。
/// </summary>
/// <remarks>
/// 目标：在 <see cref="ApplyStatusEffectAction"/> 构造时，将“状态效果被施加”的信息同步到联机层。
/// 注意：此处是“动作层级”的同步，适用于远端复现/记录；是否启用由配置控制。
/// </remarks>
public class ApplyStatusEffectAction_Patch
{
    #region 依赖注入与配置

    /// <summary>
    /// 依赖注入服务提供者（用于解析网络/配置服务）。
    /// </summary>
    private static IServiceProvider serviceProvider => ModService.ServiceProvider;

    /// <summary>
    /// 配置管理器（用于判断是否启用状态效果同步）。
    /// </summary>
    private static ConfigManager ConfigManager => serviceProvider?.GetService<ConfigManager>();

    #endregion

    #region 构造函数补丁（非泛型）

    /// <summary>
    /// 构造函数后置：拦截 <see cref="ApplyStatusEffectAction"/> (Type, Unit, ...) 并同步。
    /// </summary>
    /// <param name="__instance">动作实例（Harmony 注入）。</param>
    /// <param name="statusEffectType">状态效果类型。</param>
    /// <param name="target">目标单位。</param>
    /// <param name="level">等级。</param>
    /// <param name="duration">持续回合。</param>
    /// <param name="count">层数/次数。</param>
    /// <param name="limit">上限。</param>
    /// <param name="occupationTime">动作占用时间。</param>
    /// <param name="startAutoDecreasing">是否开始自动衰减。</param>
    [HarmonyPatch(typeof(ApplyStatusEffectAction), MethodType.Constructor,
        typeof(Type), typeof(Unit), typeof(int?), typeof(int?), typeof(int?), typeof(int?), typeof(float), typeof(bool))]
    [HarmonyPostfix]
    public static void Constructor_Postfix(ApplyStatusEffectAction __instance,
        Type statusEffectType, Unit target, int? level, int? duration, int? count, int? limit,
        float occupationTime, bool startAutoDecreasing)
    {
        try
        {
            // 配置未开启时不发送。
            if (ConfigManager?.EnableStatusEffectSync?.Value != true)
            {
                return;
            }

            // 远程出牌管线中的动作由远端驱动，本地不应再次广播。
            if (RemoteCardUsePatch.IsInRemoteCardPipeline)
            {
                return;
            }

            // 解析同步管理器。
            ISynchronizationManager syncManager = GetSyncManager();
            if (syncManager == null)
            {
                return;
            }

            // 解析网络管理器。
            INetworkManager networkManager = GetNetworkManager();
            if (networkManager == null)
            {
                return;
            }

            INetworkPlayer player = networkManager.GetSelf();

            // 构建状态效果应用同步数据。
            Dictionary<string, object> statusData = new()
            {
                ["UserName"] = player.userName,
                ["Timestamp"] = DateTime.Now.Ticks,
                ["ActionType"] = "ApplyStatusEffect",
                ["StatusEffectType"] = statusEffectType?.Name ?? "Unknown",
                ["StatusEffectFullName"] = statusEffectType?.FullName ?? "Unknown",
                ["TargetId"] = target?.Id ?? "",
                ["TargetName"] = target?.Name ?? "Unknown",
                ["Level"] = level ?? 0,
                ["Duration"] = duration ?? 0,
                ["Count"] = count ?? 0,
                ["Limit"] = limit ?? 0,
                ["OccupationTime"] = occupationTime,
                ["StartAutoDecreasing"] = startAutoDecreasing,
            };

            // 补充目标当前已有的状态效果列表（便于远端校验）。
            if (target != null && target.StatusEffects != null)
            {
                List<Dictionary<string, object>> existingStatusEffects = new();
                foreach (var statusEffect in target.StatusEffects)
                {
                    if (statusEffect == null)
                    {
                        continue;
                    }

                    existingStatusEffects.Add(new Dictionary<string, object>
                    {
                        ["StatusType"] = statusEffect.GetType().Name,
                        ["Level"] = statusEffect.Level,
                        ["Duration"] = statusEffect.Duration,
                        ["Count"] = statusEffect.Count,
                    });
                }

                statusData["ExistingStatusEffects"] = existingStatusEffects;
                statusData["ExistingStatusCount"] = target.StatusEffects.Count;
            }

            // 组装事件并发送。
            GameEvent gameEvent = GameEventManager.CreateEvent(
                NetworkMessageTypes.OnStatusEffectApplied.ToString(),
                player.userName,
                statusData
            );

            syncManager.SendGameEvent(gameEvent);

            Plugin.Logger?.LogInfo(
                $"[StatusEffectSync] 应用状态效果: {statusEffectType?.Name} -> {target?.Name} (等级: {level}, 持续: {duration}, 数量: {count})");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[StatusEffectSync] Constructor_Postfix 错误: {ex.Message}");
        }
    }

    #endregion

    #region 构造函数补丁（泛型）

    /// <summary>
    /// 构造函数后置：拦截 <see cref="ApplyStatusEffectAction{TEffect}"/> (Unit, ...) 并同步。
    /// </summary>
    /// <typeparam name="T">状态效果类型参数。</typeparam>
    /// <param name="__instance">动作实例（Harmony 注入）。</param>
    /// <param name="target">目标单位。</param>
    /// <param name="level">等级。</param>
    /// <param name="duration">持续回合。</param>
    /// <param name="count">层数/次数。</param>
    /// <param name="limit">上限。</param>
    /// <param name="occupationTime">动作占用时间。</param>
    /// <param name="startAutoDecreasing">是否开始自动衰减。</param>
    [HarmonyPatch(typeof(ApplyStatusEffectAction<>), MethodType.Constructor,
        typeof(Unit), typeof(int?), typeof(int?), typeof(int?), typeof(int?), typeof(float), typeof(bool))]
    [HarmonyPostfix]
    public static void GenericConstructor_Postfix<T>(ApplyStatusEffectAction<T> __instance,
        Unit target, int? level, int? duration, int? count, int? limit,
        float occupationTime, bool startAutoDecreasing) where T : StatusEffect
    {
        try
        {
            // 配置未开启时不发送。
            if (ConfigManager?.EnableStatusEffectSync?.Value != true)
            {
                return;
            }

            // 远程出牌管线中的动作由远端驱动，本地不应再次广播。
            if (RemoteCardUsePatch.IsInRemoteCardPipeline)
            {
                return;
            }

            // 解析同步管理器。
            ISynchronizationManager syncManager = GetSyncManager();
            if (syncManager == null)
            {
                return;
            }

            // 解析网络管理器。
            INetworkManager networkManager = GetNetworkManager();
            if (networkManager == null)
            {
                return;
            }

            INetworkPlayer player = networkManager.GetSelf();

            Type statusEffectType = typeof(T);

            // 构建泛型状态效果应用同步数据。
            Dictionary<string, object> statusData = new()
            {
                ["UserName"] = player.userName,
                ["Timestamp"] = DateTime.Now.Ticks,
                ["ActionType"] = "ApplyStatusEffectGeneric",
                ["StatusEffectType"] = statusEffectType.Name,
                ["StatusEffectFullName"] = statusEffectType.FullName ?? "Unknown",
                ["GenericArgument"] = typeof(T).Name,
                ["TargetId"] = target?.Id ?? "",
                ["TargetName"] = target?.Name ?? "Unknown",
                ["TargetType"] = target?.GetType().Name ?? "Unknown",
                ["Level"] = level ?? 0,
                ["Duration"] = duration ?? 0,
                ["Limit"] = limit ?? 0,
                ["OccupationTime"] = occupationTime,
                ["StartAutoDecreasing"] = startAutoDecreasing,
            };

            // 补充目标当前已有的状态效果列表。
            if (target != null && target.StatusEffects != null)
            {
                List<Dictionary<string, object>> existingStatusEffects = new();
                foreach (var statusEffect in target.StatusEffects)
                {
                    if (statusEffect == null)
                    {
                        continue;
                    }

                    existingStatusEffects.Add(new Dictionary<string, object>
                    {
                        ["StatusType"] = statusEffect.GetType().Name,
                        ["Level"] = statusEffect.Level,
                        ["Duration"] = statusEffect.Duration,
                        ["Count"] = statusEffect.Count,
                    });
                }

                statusData["ExistingStatusEffects"] = existingStatusEffects;
                statusData["ExistingStatusCount"] = target.StatusEffects.Count;
            }

            // 粗略判断增益/减益类别（用于远端 UI 或统计）。
            if (typeof(T).GetInterfaces().Any(i => i.Name.Contains("IBuff") || i.Name.Contains("IDebuff")))
            {
                statusData["EffectCategory"] = typeof(T).GetInterfaces()
                    .Where(i => i.Name.Contains("IBuff") || i.Name.Contains("IDebuff"))
                    .Select(i => i.Name)
                    .FirstOrDefault() ?? "Unknown";
            }

            // 组装事件并发送。
            GameEvent gameEvent = GameEventManager.CreateEvent(
                NetworkMessageTypes.OnStatusEffectApplied.ToString(),
                player.userName,
                statusData
            );

            syncManager.SendGameEvent(gameEvent);

            Plugin.Logger?.LogInfo(
                $"[StatusEffectSync] 应用泛型状态效果: {typeof(T).Name} -> {target?.Name} (等级: {level}, 持续: {duration}, 数量: {count})");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[StatusEffectSync] GenericConstructor_Postfix 错误: {ex.Message}");
        }
    }

    #endregion

    #region 辅助方法

    /// <summary>
    /// 获取同步管理器。
    /// </summary>
    /// <returns>解析成功返回实例，否则返回 null。</returns>
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
    /// 获取网络管理器。
    /// </summary>
    /// <returns>解析成功返回实例，否则返回 null。</returns>
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
    /// 获取状态效果的显示名称（尽力从特性中提取）。
    /// </summary>
    /// <param name="statusEffectType">状态效果类型。</param>
    /// <returns>显示名称。</returns>
    private static string GetStatusEffectDisplayName(Type statusEffectType)
    {
        if (statusEffectType == null)
        {
            return "Unknown";
        }

        // 尝试获取带有 Name/DisplayName 属性的特性。
        object displayAttribute = statusEffectType.GetCustomAttributes(false)
            .FirstOrDefault(attr => attr.GetType().Name.Contains("Display") || attr.GetType().Name.Contains("Name"));

        if (displayAttribute != null)
        {
            var nameProperty = displayAttribute.GetType().GetProperty("Name") ??
                               displayAttribute.GetType().GetProperty("DisplayName");
            if (nameProperty != null && nameProperty.GetValue(displayAttribute) is string name)
            {
                return name;
            }
        }

        return statusEffectType.Name;
    }

    /// <summary>
    /// 判断状态效果是否为增益（仅基于命名/接口的启发式判断）。
    /// </summary>
    /// <param name="statusEffectType">状态效果类型。</param>
    /// <returns>是增益返回 true，否则 false。</returns>
    private static bool IsBuffEffect(Type statusEffectType)
    {
        if (statusEffectType == null)
        {
            return false;
        }

        return statusEffectType.Name.Contains("Buff")
               || statusEffectType.Name.Contains("Boost")
               || statusEffectType.GetInterfaces().Any(i => i.Name.Contains("IBuff"));
    }

    /// <summary>
    /// 判断状态效果是否为减益（仅基于命名/接口的启发式判断）。
    /// </summary>
    /// <param name="statusEffectType">状态效果类型。</param>
    /// <returns>是减益返回 true，否则 false。</returns>
    private static bool IsDebuffEffect(Type statusEffectType)
    {
        if (statusEffectType == null)
        {
            return false;
        }

        return statusEffectType.Name.Contains("Debuff")
               || statusEffectType.Name.Contains("Negative")
               || statusEffectType.GetInterfaces().Any(i => i.Name.Contains("IDebuff"));
    }

    #endregion
}
