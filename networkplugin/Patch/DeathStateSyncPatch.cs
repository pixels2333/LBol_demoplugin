using System;
using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Units;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.UI.Panels;

namespace NetworkPlugin.Patch;

/// <summary>
/// 死亡状态同步补丁：用于把“假死/复活”状态变化同步到状态管理器与 UI。
/// </summary>
/// <remarks>
/// 这份补丁本身不负责“假死机制”的具体实现（由 <see cref="DeathPatches"/> 处理），
/// 而是监听玩家状态/HP 的变化，在必要时调用 <see cref="DeathManagementService"/> 进行记录与通知。
/// </remarks>
[HarmonyPatch]
public class DeathStateSyncPatch
{
    #region 依赖注入

    /// <summary>
    /// 网络客户端（用于判断是否处于联机模式）。
    /// </summary>
    private static INetworkClient NetworkClient
        => ModService.ServiceProvider?.GetService(typeof(INetworkClient)) as INetworkClient;

    #endregion

    #region 玩家状态变化监听

    /// <summary>
    /// 玩家状态（<see cref="Unit.Status"/>）变化后置：在进入死亡/恢复存活时，更新假死/复活记录。
    /// </summary>
    /// <param name="__instance">玩家单位实例。</param>
    /// <param name="value">被设置的新状态值（Harmony 注入）。</param>
    [HarmonyPatch(typeof(PlayerUnit), "Status", MethodType.Setter)]
    [HarmonyPostfix]
    public static void OnPlayerStatusChanged(PlayerUnit __instance, ref UnitStatus value)
    {
        try
        {
            if (__instance == null)
            {
                return;
            }

            // 只在联机模式下工作。
            if (NetworkClient == null || !NetworkClient.IsConnected)
            {
                return;
            }

            // 进入死亡状态：记录假死（由 AllowRealDeath 控制是否真死）。
            if (value == UnitStatus.Dead && __instance.IsDead)
            {
                DeathManagementService.NotifyFakeDeath(__instance);
                Plugin.Logger?.LogDebug($"[DeathStateSync] 玩家 {__instance.Id} 死亡状态已记录");
            }

            // 恢复为存活：记录复活。
            if (value == UnitStatus.Alive && !__instance.IsDead)
            {
                DeathManagementService.NotifyResurrection(__instance);
                Plugin.Logger?.LogDebug($"[DeathStateSync] 玩家 {__instance.Id} 复活状态已记录");
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[DeathStateSync] OnPlayerStatusChanged 异常: {ex.Message}");
        }
    }

    #endregion

    #region 玩家 HP 变化监听

    /// <summary>
    /// 玩家 HP 变化后置：当 HP 变化触发假死/复活条件时，更新记录并触发 UI/状态管理。
    /// </summary>
    /// <param name="__instance">发生 HP 变化的单位。</param>
    /// <param name="value">被设置的新 HP 值。</param>
    [HarmonyPatch(typeof(Unit), "Hp", MethodType.Setter)]
    [HarmonyPostfix]
    public static void OnPlayerHpChanged(Unit __instance, int value)
    {
        try
        {
            // 只处理玩家单位。
            if (__instance is not PlayerUnit player)
            {
                return;
            }

            // 只在联机模式下工作。
            if (NetworkClient == null || !NetworkClient.IsConnected)
            {
                return;
            }

            // HP 降到 0/以下且已死：在不允许真死时记录假死。
            if (value <= 0 && player.IsDead)
            {
                if (!DeathPatches.AllowRealDeath)
                {
                    DeathManagementService.NotifyFakeDeath(player);
                    Plugin.Logger?.LogDebug($"[DeathStateSync] 玩家 {player.Id} HP 变为 0，假死状态已记录");
                }
            }

            // HP 恢复到正数且此前被记录为假死：记录复活。
            if (value > 0 && DeathManagementService.Instance.IsFakeDead(player))
            {
                DeathManagementService.NotifyResurrection(player);
                Plugin.Logger?.LogDebug($"[DeathStateSync] 玩家 {player.Id} HP 恢复为 {value}，复活状态已记录");
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[DeathStateSync] OnPlayerHpChanged 异常: {ex.Message}");
        }
    }

    #endregion
}

/// <summary>
/// 游戏事件监听补丁：在战斗开始/结束时清理假死记录，避免跨战斗污染。
/// </summary>
[HarmonyPatch]
public class GameEventDeathSyncPatch
{
    #region 战斗生命周期

    /// <summary>
    /// 战斗开始后置：清空上一场战斗的假死记录，并重置“允许真死”标记。
    /// </summary>
    /// <param name="__instance">战斗控制器实例。</param>
    [HarmonyPatch(typeof(BattleController), "Start")]
    [HarmonyPostfix]
    public static void OnBattleStart(BattleController __instance)
    {
        try
        {
            DeathManagementService.Instance.ClearAllFakeDead();
            DeathPatches.AllowRealDeath = false;

            Plugin.Logger?.LogDebug("[GameEventDeathSync] 战斗开始，死亡状态已重置");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[GameEventDeathSync] OnBattleStart 异常: {ex.Message}");
        }
    }

    /// <summary>
    /// 战斗结束后置：清空假死记录。
    /// </summary>
    /// <param name="__instance">战斗控制器实例。</param>
    [HarmonyPatch(typeof(BattleController), "Destroy")]
    [HarmonyPostfix]
    public static void OnBattleEnd(BattleController __instance)
    {
        try
        {
            DeathManagementService.Instance.ClearAllFakeDead();
            Plugin.Logger?.LogDebug("[GameEventDeathSync] 战斗结束，死亡状态已清理");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[GameEventDeathSync] OnBattleEnd 异常: {ex.Message}");
        }
    }

    #endregion
}
