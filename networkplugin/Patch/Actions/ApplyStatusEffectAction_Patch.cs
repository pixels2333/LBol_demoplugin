using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using LBoL.Core.Battle.BattleActions;
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
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    /// <summary>
    /// 配置管理器（用于判断是否启用状态效果同步）。
    /// </summary>
    private static ConfigManager ConfigManager => ServiceProvider?.GetService<ConfigManager>();

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
            // 配置未开启或当前处于远端出牌管线时不发送。
            if (!ShouldBroadcastStatusEffect())
            {
                return;
            }

            // 解析同步管理器和网络玩家。
            if (!TryGetSyncContext(out ISynchronizationManager syncManager, out INetworkPlayer player))
            {
                return;
            }

            // 构建状态效果应用同步数据。
            Dictionary<string, object> statusData = CreateStatusData(
                player,
                "ApplyStatusEffect",
                statusEffectType,
                target,
                level,
                duration,
                count,
                limit,
                occupationTime,
                startAutoDecreasing,
                includeCount: true);

            AppendExistingStatusEffects(statusData, target);

            // 组装事件并发送。
            SendStatusEffectEvent(syncManager, player.userName, statusData);

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
            // 配置未开启或当前处于远端出牌管线时不发送。
            if (!ShouldBroadcastStatusEffect())
            {
                return;
            }

            // 解析同步管理器和网络玩家。
            if (!TryGetSyncContext(out ISynchronizationManager syncManager, out INetworkPlayer player))
            {
                return;
            }

            Type statusEffectType = typeof(T);

            // 构建泛型状态效果应用同步数据。
            Dictionary<string, object> statusData = CreateStatusData(
                player,
                "ApplyStatusEffectGeneric",
                statusEffectType,
                target,
                level,
                duration,
                count,
                limit,
                occupationTime,
                startAutoDecreasing,
                includeCount: false);

            statusData["GenericArgument"] = statusEffectType.Name;
            statusData["TargetType"] = target?.GetType().Name ?? "Unknown";

            AppendExistingStatusEffects(statusData, target);
            AppendEffectCategory(statusData, statusEffectType);

            // 组装事件并发送。
            SendStatusEffectEvent(syncManager, player.userName, statusData);

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
    /// 判断当前是否允许广播状态效果同步。
    /// </summary>
    /// <returns>允许发送时返回 true。</returns>
    private static bool ShouldBroadcastStatusEffect()
    {
        return ConfigManager?.EnableStatusEffectSync?.Value == true
            && !RemoteCardUsePatch.IsInRemoteCardPipeline;
    }

    /// <summary>
    /// 创建状态效果同步数据的公共字段。
    /// </summary>
    /// <param name="player">当前本地网络玩家。</param>
    /// <param name="actionType">动作类型标识。</param>
    /// <param name="statusEffectType">状态效果类型。</param>
    /// <param name="target">目标单位。</param>
    /// <param name="level">等级。</param>
    /// <param name="duration">持续回合。</param>
    /// <param name="count">层数/次数。</param>
    /// <param name="limit">上限。</param>
    /// <param name="occupationTime">动作占用时间。</param>
    /// <param name="startAutoDecreasing">是否开始自动衰减。</param>
    /// <param name="includeCount">是否写入 Count 字段。</param>
    /// <returns>初始化完成的同步数据字典。</returns>
    private static Dictionary<string, object> CreateStatusData(
        INetworkPlayer player,
        string actionType,
        Type statusEffectType,
        Unit target,
        int? level,
        int? duration,
        int? count,
        int? limit,
        float occupationTime,
        bool startAutoDecreasing,
        bool includeCount)
    {
        Dictionary<string, object> statusData = new()
        {
            ["UserName"] = player.userName,
            ["Timestamp"] = DateTime.Now.Ticks,
            ["ActionType"] = actionType,
            ["StatusEffectType"] = statusEffectType?.Name ?? "Unknown",
            ["StatusEffectFullName"] = statusEffectType?.FullName ?? "Unknown",
            ["TargetId"] = target?.Id ?? "",
            ["TargetName"] = target?.Name ?? "Unknown",
            ["Level"] = level ?? 0,
            ["Duration"] = duration ?? 0,
            ["Limit"] = limit ?? 0,
            ["OccupationTime"] = occupationTime,
            ["StartAutoDecreasing"] = startAutoDecreasing,
        };

        if (includeCount)
        {
            statusData["Count"] = count ?? 0;
        }

        return statusData;
    }

    /// <summary>
    /// 补充目标当前已有的状态效果列表。
    /// </summary>
    /// <param name="statusData">待写入的同步数据。</param>
    /// <param name="target">目标单位。</param>
    private static void AppendExistingStatusEffects(Dictionary<string, object> statusData, Unit target)
    {
        if (target?.StatusEffects == null)
        {
            return;
        }

        List<Dictionary<string, object>> existingStatusEffects = new();
        foreach (StatusEffect statusEffect in target.StatusEffects)
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

    /// <summary>
    /// 粗略判断状态效果类别（用于远端 UI 或统计）。
    /// </summary>
    /// <param name="statusData">待写入的同步数据。</param>
    /// <param name="statusEffectType">状态效果类型。</param>
    private static void AppendEffectCategory(Dictionary<string, object> statusData, Type statusEffectType)
    {
        string effectCategory = statusEffectType.GetInterfaces()
            .Select(interfaceType => interfaceType.Name)
            .FirstOrDefault(name => name.Contains("IBuff") || name.Contains("IDebuff"));

        if (effectCategory != null)
        {
            statusData["EffectCategory"] = effectCategory;
        }
    }

    /// <summary>
    /// 组装并发送状态效果同步事件。
    /// </summary>
    /// <param name="syncManager">同步管理器。</param>
    /// <param name="userName">发送方用户名。</param>
    /// <param name="statusData">同步数据。</param>
    private static void SendStatusEffectEvent(ISynchronizationManager syncManager, string userName, Dictionary<string, object> statusData)
    {
        GameEvent gameEvent = GameEventManager.CreateEvent(
            NetworkMessageTypes.OnStatusEffectApplied.ToString(),
            userName,
            statusData
        );

        syncManager.SendGameEvent(gameEvent);
    }

    /// <summary>
    /// 获取同步管理器。
    /// </summary>
    /// <returns>解析成功返回实例，否则返回 null。</returns>
    private static ISynchronizationManager GetSyncManager()
    {
        try
        {
            Plugin.LogSynchronizationManagerResolveFromPatch(nameof(ApplyStatusEffectAction_Patch), ServiceProvider);
            return ServiceProvider?.GetService<ISynchronizationManager>();
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
            return ServiceProvider?.GetService<INetworkManager>();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 尝试解析发送状态效果同步事件所需的上下文。
    /// </summary>
    /// <param name="syncManager">同步管理器。</param>
    /// <param name="player">当前本地网络玩家。</param>
    /// <returns>同步管理器和网络管理器都解析成功时返回 true。</returns>
    private static bool TryGetSyncContext(out ISynchronizationManager syncManager, out INetworkPlayer player)
    {
        syncManager = GetSyncManager();
        if (syncManager == null)
        {
            player = null;
            return false;
        }

        INetworkManager networkManager = GetNetworkManager();
        if (networkManager == null)
        {
            player = null;
            return false;
        }

        player = networkManager.GetSelf();
        return true;
    }

    #endregion
}
