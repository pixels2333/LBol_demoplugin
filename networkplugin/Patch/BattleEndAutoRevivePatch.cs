using System;
using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Units;

namespace NetworkPlugin.Patch;

[HarmonyPatch(typeof(GameRunController), nameof(GameRunController.LeaveBattle))]
public static class BattleEndAutoRevivePatch
{        public static void Prefix(GameRunController __instance)
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
