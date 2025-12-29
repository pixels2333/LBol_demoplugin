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
/// 死亡状态同步补丁 - 负责将死亡补丁与状态管理器连接
/// 当玩家进入或离开假死状态时，同步更新状态管理器
/// </summary>
[HarmonyPatch]
public class DeathStateSyncPatch
{
    private static INetworkClient NetworkClient => ModService.ServiceProvider?.GetService(typeof(INetworkClient)) as INetworkClient;

    /// <summary>
    /// 玩家死亡状态改变事件的补丁
    /// 当玩家状态变为 Dead 时，更新状态管理器
    /// </summary>
    [HarmonyPatch(typeof(PlayerUnit), "Status", MethodType.Setter)]
    [HarmonyPostfix]
    public static void OnPlayerStatusChanged(PlayerUnit __instance, ref UnitStatus value)
    {
        try
        {
            if (__instance == null) return;

            // 只处理联机模式
            if (NetworkClient == null || !NetworkClient.IsConnected)
            {
                return;
            }

            // 玩家进入死亡状态
            if (value == UnitStatus.Dead && __instance.IsDead)
            {
                DeathManagementService.NotifyFakeDeath(__instance);
                Plugin.Logger?.LogDebug($"[DeathStateSync] Player {__instance.Id} death state synced");
            }

            // 玩家状态恢复为活着
            if (value == UnitStatus.Alive && !__instance.IsDead)
            {
                DeathManagementService.NotifyResurrection(__instance);
                Plugin.Logger?.LogDebug($"[DeathStateSync] Player {__instance.Id} resurrection state synced");
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[DeathStateSync] Error in OnPlayerStatusChanged: {ex.Message}");
        }
    }

    /// <summary>
    /// 玩家 HP 变化同步补丁
    /// 如果 HP 变化导致玩家进入假死状态，则更新 UI
    /// </summary>
    [HarmonyPatch(typeof(Unit), "Hp", MethodType.Setter)]
    [HarmonyPostfix]
    public static void OnPlayerHpChanged(Unit __instance, int value)
    {
        try
        {
            // 只处理玩家单位
            if (__instance is not PlayerUnit player) return;

            // 只处理联机模式
            if (NetworkClient == null || !NetworkClient.IsConnected)
            {
                return;
            }

            // HP 变为 0 时记录假死状态
            if (value <= 0 && player.IsDead)
            {
                if (!DeathPatches.AllowRealDeath)
                {
                    DeathManagementService.NotifyFakeDeath(player);
                    Plugin.Logger?.LogDebug($"[DeathStateSync] Player {player.Id} HP changed to 0, fake death state tracked");
                }
            }

            // HP 恢复时记录复活状态
            if (value > 0 && DeathManagementService.Instance.IsFakeDead(player))
            {
                DeathManagementService.NotifyResurrection(player);
                Plugin.Logger?.LogDebug($"[DeathStateSync] Player {player.Id} HP restored to {value}, resurrection tracked");
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[DeathStateSync] Error in OnPlayerHpChanged: {ex.Message}");
        }
    }
}

/// <summary>
/// 游戏事件监听补丁 - 监听战斗相关事件
/// 在游戏开始或结束时清理死亡状态
/// </summary>
[HarmonyPatch]
public class GameEventDeathSyncPatch
{
    /// <summary>
    /// 战斗开始时清空死亡状态
    /// 确保每场战斗都从干净的状态开始
    /// </summary>
    [HarmonyPatch(typeof(BattleController), "Start")]
    [HarmonyPostfix]
    public static void OnBattleStart(BattleController __instance)
    {
        try
        {
            // 清空之前战斗的死亡记录
            DeathManagementService.Instance.ClearAllFakeDead();
            DeathPatches.AllowRealDeath = false;

            Plugin.Logger?.LogDebug("[GameEventDeathSync] Battle started, death state reset");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[GameEventDeathSync] Error in OnBattleStart: {ex.Message}");
        }
    }

    /// <summary>
    /// 战斗结束时的清理
    /// </summary>
    [HarmonyPatch(typeof(BattleController), "Destroy")]
    [HarmonyPostfix]
    public static void OnBattleEnd(BattleController __instance)
    {
        try
        {
            DeathManagementService.Instance.ClearAllFakeDead();
            Plugin.Logger?.LogDebug("[GameEventDeathSync] Battle ended, death state cleared");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[GameEventDeathSync] Error in OnBattleEnd: {ex.Message}");
        }
    }
}
