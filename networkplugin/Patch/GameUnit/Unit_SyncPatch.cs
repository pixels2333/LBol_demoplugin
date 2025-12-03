using System;
using HarmonyLib;
using LBoL.Core.Units;
using NetworkPlugin.Network;

namespace NetworkPlugin.Patch.GameUnit;

/// <summary>
/// Unit属性同步补丁 - 同步HP、Block、Shield等基础属性
/// 注意: 之前尝试的Unit_Patch.txt方法被废弃,因为会导致所有玩家同步到最后一个玩家的值
/// 此版本使用更精确的控制,仅在需要时同步
/// </summary>
public class Unit_SyncPatch
{
    private static IServiceProvider serviceProvider => ModService.ServiceProvider;

    /// <summary>
    /// 获取当前玩家的PlayerEntity实例
    /// </summary>
    private static PlayerEntity GetLocalPlayer()
    {
        // TODO: 实现获取本地玩家的逻辑
        // 需要从NetworkManager或其他服务获取
        return null;
    }

    /// <summary>
    /// 判断是否是玩家单位(不是敌人)
    /// </summary>
    private static bool IsPlayerUnit(Unit unit)
    {
        return unit is PlayerUnit;
    }

    /// <summary>
    /// 判断是否是本地玩家单位
    /// </summary>
    private static bool IsLocalPlayerUnit(Unit unit)
    {
        // 如果是玩家单位且BattleController的Player就是此单位
        if (unit is PlayerUnit && unit.Battle != null)
        {
            return unit.Battle.Player == unit;
        }
        return false;
    }

    /// <summary>
    /// HP变更同步补丁
    /// </summary>
    [HarmonyPatch(typeof(Unit), "set_Hp")]
    [HarmonyPostfix]
    public static void HpSetter_Postfix(Unit __instance, int value)
    {
        // 只在以下情况同步:
        // 1. 是本地玩家单位
        // 2. 在战斗中
        // 3. 网络服务已初始化
        if (!IsLocalPlayerUnit(__instance) || __instance.Battle == null || serviceProvider == null)
        {
            return;
        }

        try
        {
            // 这里不应该直接发送网络请求,因为HP可能在同一帧内多次变更
            // 应该让BattleController.Damage来处理同步
            // 此补丁主要用于记录日志和调试
            Plugin.Logger?.LogDebug($"[Unit_SyncPatch] Local player HP changed to {value}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[Unit_SyncPatch] Error in HpSetter_Postfix: {ex.Message}");
        }
    }

    /// <summary>
    /// Block变更同步补丁
    /// </summary>
    [HarmonyPatch(typeof(Unit), "set_Block")]
    [HarmonyPostfix]
    public static void BlockSetter_Postfix(Unit __instance, int value)
    {
        if (!IsLocalPlayerUnit(__instance) || __instance.Battle == null || serviceProvider == null)
        {
            return;
        }

        try
        {
            Plugin.Logger?.LogDebug($"[Unit_SyncPatch] Local player Block changed to {value}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[Unit_SyncPatch] Error in BlockSetter_Postfix: {ex.Message}");
        }
    }

    /// <summary>
    /// Shield变更同步补丁
    /// </summary>
    [HarmonyPatch(typeof(Unit), "set_Shield")]
    [HarmonyPostfix]
    public static void ShieldSetter_Postfix(Unit __instance, int value)
    {
        if (!IsLocalPlayerUnit(__instance) || __instance.Battle == null || serviceProvider == null)
        {
            return;
        }

        try
        {
            Plugin.Logger?.LogDebug($"[Unit_SyncPatch] Local player Shield changed to {value}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[Unit_SyncPatch] Error in ShieldSetter_Postfix: {ex.Message}");
        }
    }

    /// <summary>
    /// MaxHP变更同步补丁
    /// </summary>
    [HarmonyPatch(typeof(Unit), "set_MaxHp")]
    [HarmonyPostfix]
    public static void MaxHpSetter_Postfix(Unit __instance, int value)
    {
        if (!IsLocalPlayerUnit(__instance) || __instance.Battle == null || serviceProvider == null)
        {
            return;
        }

        try
        {
            Plugin.Logger?.LogDebug($"[Unit_SyncPatch] Local player MaxHP changed to {value}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[Unit_SyncPatch] Error in MaxHpSetter_Postfix: {ex.Message}");
        }
    }

    /// <summary>
    /// Status变更同步补丁
    /// </summary>
    [HarmonyPatch(typeof(Unit), "set_Status")]
    [HarmonyPostfix]
    public static void StatusSetter_Postfix(Unit __instance, UnitStatus value)
    {
        if (!IsLocalPlayerUnit(__instance) || __instance.Battle == null || serviceProvider == null)
        {
            return;
        }

        try
        {
            Plugin.Logger?.LogDebug($"[Unit_SyncPatch] Local player Status changed to {value}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[Unit_SyncPatch] Error in StatusSetter_Postfix: {ex.Message}");
        }
    }
}
