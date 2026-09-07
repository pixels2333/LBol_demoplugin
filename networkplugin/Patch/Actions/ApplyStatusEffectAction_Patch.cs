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
using NetworkPlugin.Network.Services;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Event;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Network.NetworkPlayer;
using NetworkPlugin.Patch.Network;

namespace NetworkPlugin.Patch.Actions;

public class ApplyStatusEffectAction_Patch
{
    #region 依赖注入与配置

        private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

        private static ConfigManager ConfigManager => ServiceProvider?.GetService<ConfigManager>();

    #endregion

    #region 构造函数补丁（非泛型）

        [HarmonyPatch(typeof(ApplyStatusEffectAction), MethodType.Constructor,
        typeof(Type), typeof(Unit), typeof(int?), typeof(int?), typeof(int?), typeof(int?), typeof(float), typeof(bool))]
    [HarmonyPostfix]
    public static void Constructor_Postfix(ApplyStatusEffectAction __instance,
        Type statusEffectType, Unit target, int? level, int? duration, int? count, int? limit,
        float occupationTime, bool startAutoDecreasing)
    {
        try
        {

            if (!ShouldBroadcastStatusEffect())
            {
                return;
            }

            if (!TryGetSyncContext(out ISynchronizationManager syncManager, out INetworkPlayer player))
            {
                return;
            }

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

        [HarmonyPatch(typeof(ApplyStatusEffectAction<>), MethodType.Constructor,
        typeof(Unit), typeof(int?), typeof(int?), typeof(int?), typeof(int?), typeof(float), typeof(bool))]
    [HarmonyPostfix]
    public static void GenericConstructor_Postfix<T>(ApplyStatusEffectAction<T> __instance,
        Unit target, int? level, int? duration, int? count, int? limit,
        float occupationTime, bool startAutoDecreasing) where T : StatusEffect
    {
        try
        {

            if (!ShouldBroadcastStatusEffect())
            {
                return;
            }

            if (!TryGetSyncContext(out ISynchronizationManager syncManager, out INetworkPlayer player))
            {
                return;
            }

            Type statusEffectType = typeof(T);

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

        private static bool ShouldBroadcastStatusEffect()
    {
        return ConfigManager?.EnableStatusEffectSync?.Value == true
            && !RemoteCardUsePatch.IsInRemoteCardPipeline;
    }

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

        private static void SendStatusEffectEvent(ISynchronizationManager syncManager, string userName, Dictionary<string, object> statusData)
    {
        GameEvent gameEvent = GameEventManager.CreateEvent(
            NetworkMessageTypes.OnStatusEffectApplied.ToString(),
            userName,
            statusData
        );

        syncManager.SendGameEvent(gameEvent);
    }

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
