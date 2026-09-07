using System;
using HarmonyLib;
using LBoL.Core.Battle;
using LBoL.Core.Units;
using NetworkPlugin.Network.Services;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Patch.Network;
using NetworkPlugin.UI.State;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Patch;

[HarmonyPatch]
public class DeathStateSyncPatch
{
    #region 依赖注入

        private static INetworkClient NetworkClient
        => ModService.ServiceProvider?.GetService(typeof(INetworkClient)) as INetworkClient;

    #endregion

    #region 玩家状态变化监听

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

            if (NetworkClient == null || !NetworkClient.IsConnected)
            {
                return;
            }

            if (value == UnitStatus.Dead && player.IsDead)
            {
                DeathManagementService.NotifyFakeDeath(player);
                Plugin.Logger?.LogDebug($"[DeathStateSync] 玩家 {player.Id} 死亡状态已记录");
            }

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

    private static int _lastReportedHp = -1;
    private static int _lastReportedMaxHp = -1;

    public static void EnsureInitialHpSynced()
    {
        try
        {
            if (NetworkClient == null || !NetworkClient.IsConnected)
            {
                return;
            }

            var player = GameStateUtils.GetCurrentPlayer();
            if (player != null && (player.Hp != _lastReportedHp || player.MaxHp != _lastReportedMaxHp))
            {
                _lastReportedHp = player.Hp;
                _lastReportedMaxHp = player.MaxHp;
                PlayerStateSyncPatch.SendFullPlayerStateSnapshot(player, "InitialHpSync");
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogWarning($"[DeathStateSync] EnsureInitialHpSynced 异常: {ex.Message}");
        }
    }

    [HarmonyPatch(typeof(Unit), "Hp", MethodType.Setter)]
    [HarmonyPostfix]
    public static void OnPlayerHpChanged(Unit __instance, int value)
    {
        try
        {
            if (__instance is not PlayerUnit player)
            {
                return;
            }

            if (NetworkClient == null || !NetworkClient.IsConnected)
            {
                return;
            }

            if (value <= 0 && player.IsDead)
            {
                if (!DeathPatches.AllowRealDeath)
                {
                    DeathManagementService.NotifyFakeDeath(player);
                    Plugin.Logger?.LogDebug($"[DeathStateSync] 玩家 {player.Id} HP 变为 0，假死状态已记录");
                }
            }

            if (value > 0 && DeathManagementService.Instance.IsFakeDead(player))
            {
                DeathManagementService.NotifyResurrection(player);
                Plugin.Logger?.LogDebug($"[DeathStateSync] 玩家 {player.Id} HP 恢复为 {value}，复活状态已记录");
            }

            if (player == GameStateUtils.GetCurrentPlayer())
            {
                if (player.Hp != _lastReportedHp || player.MaxHp != _lastReportedMaxHp)
                {
                    _lastReportedHp = player.Hp;
                    _lastReportedMaxHp = player.MaxHp;
                    PlayerStateSyncPatch.SendFullPlayerStateSnapshot(player, "HpChanged");
                }
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[DeathStateSync] OnPlayerHpChanged 异常: {ex.Message}");
        }
    }

    #endregion
}

[HarmonyPatch]
public class GameEventDeathSyncPatch
{
    #region 战斗生命周期

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
