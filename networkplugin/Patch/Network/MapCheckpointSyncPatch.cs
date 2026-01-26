using System;
using HarmonyLib;
using Microsoft.Extensions.DependencyInjection;
using LBoL.Core;
using LBoL.Core.Stations;
using LBoL.Presentation.UI.Panels;
using NetworkPlugin.Network;
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

    private static ReconnectionManager? TryGetReconnectionManager()
    {
        try
        {
            return ServiceProvider?.GetService<ReconnectionManager>();
        }
        catch
        {
            return null;
        }
    }

    private static bool IsHostConnected()
    {
        try
        {
            var client = ServiceProvider?.GetService<INetworkClient>();
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

    [HarmonyPatch(typeof(GameRunController), nameof(GameRunController.EnterNextStage))]
    private static class GameRunController_EnterNextStage_Checkpoint
    {
        [HarmonyPostfix]
        public static void Postfix(GameRunController __instance)
        {
            try
            {
                if (!IsHostConnected())
                {
                    return;
                }

                TryGetReconnectionManager()?.MarkMapCheckpoint("next_stage", TryBuildCurrentNodeKey(__instance));
            }
            catch
            {
                // ignored
            }
        }
    }

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

                if (!IsHostConnected())
                {
                    return;
                }

                TryGetReconnectionManager()?.MarkMapCheckpoint("station_finish", TryBuildCurrentNodeKey(__instance?.GameRun));
            }
            catch
            {
                // ignored
            }
        }
    }

    [HarmonyPatch(typeof(RewardPanel), "OnHided")]
    private static class RewardPanel_OnHided_Checkpoint
    {
        [HarmonyPostfix]
        public static void Postfix(RewardPanel __instance)
        {
            try
            {
                if (!IsHostConnected())
                {
                    return;
                }

                GameRunController? run = GameStateUtils.GetCurrentGameRun();
                TryGetReconnectionManager()?.MarkMapCheckpoint("reward_closed", TryBuildCurrentNodeKey(run));
            }
            catch
            {
                // ignored
            }
        }
    }

    [HarmonyPatch(typeof(ShopPanel), nameof(ShopPanel.SetShopAfterBuying))]
    private static class ShopPanel_SetShopAfterBuying_Checkpoint
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            try
            {
                if (!IsHostConnected())
                {
                    return;
                }

                GameRunController? run = GameStateUtils.GetCurrentGameRun();
                TryGetReconnectionManager()?.MarkMapCheckpoint("shop_after_buying", TryBuildCurrentNodeKey(run));
            }
            catch
            {
                // ignored
            }
        }
    }

    [HarmonyPatch(typeof(GapOptionsPanel), nameof(GapOptionsPanel.SelectedAndHide))]
    private static class GapOptionsPanel_SelectedAndHide_Checkpoint
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            try
            {
                if (!IsHostConnected())
                {
                    return;
                }

                GameRunController? run = GameStateUtils.GetCurrentGameRun();
                TryGetReconnectionManager()?.MarkMapCheckpoint("gap_option_selected", TryBuildCurrentNodeKey(run));
            }
            catch
            {
                // ignored
            }
        }
    }
}
