using System;
using HarmonyLib;
using LBoL.Core;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.MidGameJoin;
using NetworkPlugin.Network.Snapshot;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Patch.MidGameJoin;

/// <summary>
/// Align joiner's initial stage index to host snapshot.
///
/// Why:
/// - Host may already be in stage N.
/// - Joiner starts a fresh run; by default _stageIndex is -1 and the first EnterNextStage enters stage 0.
/// - MapCatchUpOrchestrator waits for map seed alignment; entering the correct stage makes Stage.MapSeed match sooner.
///
/// How:
/// - Before the FIRST EnterNextStage call of a new run, set private _stageIndex = targetStageIndex - 1.
///   (EnterNextStage will then enter targetStageIndex.)
///
/// Guard:
/// - Only when connected and self is NOT host.
/// - Only when there is a pending FullSnapshot with GameState.StageIndex.
/// - Only when the current _stageIndex is still -1 (fresh run).
/// </summary>
[HarmonyPatch]
public static class JoinerStageIndexAlignPatch
{
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    private static INetworkClient TryGetClient()
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

    private static MapCatchUpOrchestrator TryGetCatchUp()
    {
        try
        {
            return ServiceProvider?.GetService<MapCatchUpOrchestrator>();
        }
        catch
        {
            return null;
        }
    }

    [HarmonyPatch(typeof(GameRunController), nameof(GameRunController.EnterNextStage))]
    [HarmonyPrefix]
    public static void GameRunController_EnterNextStage_Prefix(GameRunController __instance)
    {
        try
        {
            if (__instance == null)
            {
                return;
            }

            INetworkClient client = TryGetClient();
            if (client == null || !client.IsConnected)
            {
                return;
            }

            NetworkIdentityTracker.EnsureSubscribed(client);
            if (NetworkIdentityTracker.GetSelfIsHost())
            {
                return;
            }

            MapCatchUpOrchestrator catchUp = TryGetCatchUp();
            if (catchUp == null)
            {
                return;
            }

            if (!catchUp.TryGetPendingFullSnapshot(out FullStateSnapshot snapshot))
            {
                return;
            }

            int? targetStageIndex = snapshot?.GameState?.StageIndex;
            if (targetStageIndex == null)
            {
                return;
            }

            int idx = targetStageIndex.Value;
            if (idx < 0)
            {
                return;
            }

            // Only align fresh runs.
            int currentStageIndex = Traverse.Create(__instance).Field("_stageIndex").GetValue<int>();
            if (currentStageIndex != -1)
            {
                return;
            }

            // Clamp to available stages.
            int stageCount = 0;
            try
            {
                stageCount = __instance.Stages?.Count ?? 0;
            }
            catch
            {
                stageCount = 0;
            }

            if (stageCount > 0)
            {
                idx = Math.Max(0, Math.Min(idx, stageCount - 1));
            }

            int desired = idx - 1;
            if (desired < -1)
            {
                desired = -1;
            }

            Traverse.Create(__instance).Field("_stageIndex").SetValue(desired);
        }
        catch
        {
            // ignored
        }
    }
}
