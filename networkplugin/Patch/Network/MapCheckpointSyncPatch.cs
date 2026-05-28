using System;
using HarmonyLib;
using Microsoft.Extensions.DependencyInjection;
using LBoL.Core;
using LBoL.Core.Stations;
using LBoL.Presentation.UI.Panels;
using NetworkPlugin.Network.Services;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Reconnection;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Patch.Network;

/// <summary>
/// Map checkpoint hooks (host-only): mark key progress points for mid-game join/reconnection.
/// </summary>
[HarmonyPatch]
public static class MapCheckpointSyncPatch
{
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    private static INetworkClient TryGetNetworkClient()
        => ServiceProvider?.GetService<INetworkClient>();

    private static ReconnectionManager? TryGetReconnectionManager()
        => ServiceProvider?.GetService<ReconnectionManager>();

    private static bool IsHostConnected()
    {
        try
        {
            INetworkClient client = TryGetNetworkClient();
            if (client == null || !client.IsConnected)
            {
                return false;
            }

            NetworkIdentityTracker.EnsureSubscribed(client);
            return NetworkIdentityTracker.GetSelfIsHost();
        }
        catch
        {
            return false;
        }
    }

    private static void TryMarkCheckpoint(string reason, GameRunController? run)
    {
        if (!IsHostConnected())
        {
            return;
        }

        TryGetReconnectionManager()?.MarkMapCheckpoint(reason, TryBuildCurrentNodeKey(run));
    }

    private static void TryMarkCurrentRunCheckpoint(string reason)
    {
        GameRunController? run = GameStateUtils.GetCurrentGameRun();
        TryMarkCheckpoint(reason, run);
    }

    private static string? TryBuildCurrentNodeKey(GameRunController? run)
    {
        try
        {
            MapNode? node = run?.CurrentMap?.VisitingNode;
            if (node == null)
            {
                return null;
            }

            return $"{node.Act}:{node.X}:{node.Y}:{node.StationType}";
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 进入下一阶段时标记检查点
    /// </summary>
    [HarmonyPatch(typeof(GameRunController), nameof(GameRunController.EnterNextStage))]
    private static class GameRunController_EnterNextStage_Checkpoint
    {
        [HarmonyPostfix]
        public static void Postfix(GameRunController __instance)
        {
            try
            {
                TryMarkCheckpoint("next_stage", __instance);
            }
            catch
            {
                // ignored
            }
        }
    }

    /// <summary>
    /// 节点结束时标记检查点（非战斗节点）
    /// </summary>
    [HarmonyPatch(typeof(Station), nameof(Station.Finish))]
    private static class Station_Finish_Checkpoint
    {
        [HarmonyPostfix]
        public static void Postfix(Station __instance)
        {
            try
            {
                if (__instance is BattleStation)
                {
                    // battle_end has its own explicit checkpoint hook.
                    return;
                }

                TryMarkCheckpoint("station_finish", __instance?.GameRun);
            }
            catch
            {
                // ignored
            }
        }
    }

    /// <summary>
    /// 奖励面板关闭时标记检查点
    /// </summary>
    [HarmonyPatch(typeof(RewardPanel), "OnHided")]
    private static class RewardPanel_OnHided_Checkpoint
    {
        [HarmonyPostfix]
        public static void Postfix(RewardPanel __instance)
        {
            try
            {
                TryMarkCurrentRunCheckpoint("reward_closed");
            }
            catch
            {
                // ignored
            }
        }
    }

    /// <summary>
    /// 商店购买后标记检查点
    /// </summary>
    [HarmonyPatch(typeof(ShopPanel), nameof(ShopPanel.SetShopAfterBuying))]
    private static class ShopPanel_SetShopAfterBuying_Checkpoint
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            try
            {
                TryMarkCurrentRunCheckpoint("shop_after_buying");
            }
            catch
            {
                // ignored
            }
        }
    }

    /// <summary>
    /// GapOptions 选择后标记检查点
    /// </summary>
    [HarmonyPatch(typeof(GapOptionsPanel), nameof(GapOptionsPanel.SelectedAndHide))]
    private static class GapOptionsPanel_SelectedAndHide_Checkpoint
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            try
            {
                TryMarkCurrentRunCheckpoint("gap_option_selected");
            }
            catch
            {
                // ignored
            }
        }
    }
}
