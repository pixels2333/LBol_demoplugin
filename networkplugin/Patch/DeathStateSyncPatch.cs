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
    /// <param name="__instance">单位实例（实际只处理 PlayerUnit）。</param>
    /// <param name="value">被设置的新状态值（Harmony 注入）。</param>
    // 注意：Status 属性定义在 Unit 上，而不是 PlayerUnit。
    // 如果错误地 patch PlayerUnit.Status，会导致 PatchAll 期间崩溃（original method 为空）。
    [HarmonyPatch(typeof(Unit), "Status", MethodType.Setter)]
    [HarmonyPostfix]
    public static void OnUnitStatusChanged(Unit __instance, UnitStatus value)
    {
        try
        {
            if (__instance is not PlayerUnit player)
            {
                return;
            }

            // 只在联机模式下工作。
            if (NetworkClient == null || !NetworkClient.IsConnected)
            {
                return;
            }

            // 进入死亡状态：记录假死（由 AllowRealDeath 控制是否真死）。
            if (value == UnitStatus.Dead && player.IsDead)
            {
                DeathManagementService.NotifyFakeDeath(player);
                Plugin.Logger?.LogDebug($"[DeathStateSync] 玩家 {player.Id} 死亡状态已记录");
            }

            // 恢复为存活：记录复活。
            if (value == UnitStatus.Alive && !player.IsDead)
            {
                DeathManagementService.NotifyResurrection(player);
                Plugin.Logger?.LogDebug($"[DeathStateSync] 玩家 {player.Id} 复活状态已记录");
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[DeathStateSync] OnUnitStatusChanged 异常: {ex.Message}");
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
    // LBoL.Core.Battle.BattleController 并不是 Unity MonoBehaviour，没有 Start()。
    // 正确的战斗开始入口是 StartBattle()，否则会导致 PatchAll 期间 original method 为空并崩溃。
    [HarmonyPatch(typeof(BattleController), "StartBattle")]
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
    // BattleController 同样没有 Destroy()。这里改为 patch Leave()，它负责单位/卡牌离场与战斗统计收尾。
    [HarmonyPatch(typeof(BattleController), "Leave")]
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

    /// <summary>
    /// 战斗结束后置（兜底）：如果流程走到 EndBattle()，同样清理假死记录。
    /// </summary>
    /// <param name="__instance">战斗控制器实例。</param>
    [HarmonyPatch(typeof(BattleController), "EndBattle")]
    [HarmonyPostfix]
    public static void OnBattleEndBattle(BattleController __instance)
    {
        try
        {
            DeathManagementService.Instance.ClearAllFakeDead();
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[GameEventDeathSync] OnBattleEndBattle 异常: {ex.Message}");
        }
    }

    #endregion
}
