using System;
using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Units;

namespace NetworkPlugin.Patch;

/// <summary>
/// 战斗结束自动复活补丁：在原版 LeaveBattle 判定失败前，
/// 对“非全员死亡但本地仍处于死亡状态”的玩家执行自动复活。
/// </summary>
[HarmonyPatch(typeof(GameRunController), nameof(GameRunController.LeaveBattle))]
public static class BattleEndAutoRevivePatch
{    /// <summary>
    /// LeaveBattle 前置补丁：检查本地玩家是否处于假死状态，若是则自动复活后再执行原逻辑。
    /// </summary>    [HarmonyPrefix]
    public static void Prefix(GameRunController __instance)
    {
        try
        {
            if (__instance?.Player is not PlayerUnit player)
            {
                return;
            }

            if (!DeathPatches.ShouldAutoReviveAfterBattle(player))
            {
                return;
            }

            int reviveHp = DeathPatches.CalculateConfiguredAutoReviveHp(player);
            DeathPatches.ResurrectPlayer(player, reviveHp);
            Plugin.Logger?.LogInfo($"[BattleEndAutoRevive] 战斗结束后自动复活本地玩家，目标生命值: {reviveHp}/{player.MaxHp}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[BattleEndAutoRevive] LeaveBattle 前自动复活失败: {ex.Message}");
        }
    }
}
