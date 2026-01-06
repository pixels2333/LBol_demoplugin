using System;
using System.Linq;
using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Battle;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin;
using NetworkPlugin.Network.Client;

namespace NetworkPlugin.Patch.Network;

/// <summary>
/// Exhibit(=Relic) 相关的联网一致性补丁。
/// 参照 Together in Spire 的 RelicPatches.java：
/// - GremlinHorn：怪物死亡触发回能时，避免回合被错误结束（需要“撤销结束回合/恢复输入”）。
/// 
/// 说明：
/// - LBoL 中 Exhibit 的触发通常通过 <see cref="Exhibit.NotifyActivating"/> 标记；
/// - 这里仅处理“敌人死亡触发、战斗未结束时需要继续等待玩家输入”的场景。
/// </summary>
public static class ExhibitSyncPatch
{
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    private static INetworkClient TryGetNetworkClient()
    {
        try
        {
            return ServiceProvider?.GetService<INetworkClient>();
        }
        catch
        {
            return null;
        }
    }

    private static bool IsConnected()
    {
        var client = TryGetNetworkClient();
        return client != null && client.IsConnected;
    }

    private static bool HasAliveEnemies(BattleController battle)
        => battle?.EnemyGroup != null && battle.EnemyGroup.Alives != null && battle.EnemyGroup.Alives.Any();

    /// <summary>
    /// GremlinHornPatch 对应：在怪物死亡触发遗物后，如果战斗未结束则“撤销结束回合”。
    /// 
    /// LBoL 对应做法：若仍在玩家回合且战斗未结束，则强制恢复 <see cref="BattleController.IsWaitingPlayerInput"/>，
    /// 避免多人联机时本地客户端因时序问题卡在“不等待输入”的状态。
    /// </summary>
    [HarmonyPatch(typeof(Exhibit), nameof(Exhibit.NotifyActivating))]
    private static class Exhibit_NotifyActivating_Patch
    {
        [HarmonyPostfix]
        private static void Postfix(Exhibit __instance)
        {
            try
            {
                if (__instance == null || !IsConnected())
                {
                    return;
                }

                // 当前仅对“变化(Bianhua)”做定向修复：它在 EnemyDied 时触发 GainManaAction，
                // 行为上最接近 Together in Spire 中 GremlinHorn 的“击杀回能”。
                if (!string.Equals(__instance.Id, "Bianhua", StringComparison.Ordinal))
                {
                    return;
                }

                var battle = __instance.Battle;
                if (battle == null || battle.BattleShouldEnd || battle.PlayerTurnShouldEnd || !HasAliveEnemies(battle))
                {
                    return;
                }

                // 若未处于等待输入状态，则恢复等待输入（类似 UnEndTurn）。
                if (!battle.IsWaitingPlayerInput)
                {
                    Traverse.Create(battle).Property(nameof(BattleController.IsWaitingPlayerInput)).SetValue(true);
                    Plugin.Logger?.LogDebug("[ExhibitSync] Restored IsWaitingPlayerInput for Bianhua trigger.");
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[ExhibitSync] Error in Exhibit.NotifyActivating Postfix: {ex.Message}");
            }
        }
    }
}
