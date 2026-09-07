using System;
using HarmonyLib;
using LBoL.Core;
using LBoL.Presentation;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network.Services;
using NetworkPlugin.Network.Client;

namespace NetworkPlugin.Patch.Network;

[HarmonyPatch]
public static class UnlockEverythingForMPPatch
{
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    private static INetworkClient TryGetNetworkClient()
        => ServiceProvider?.GetService<INetworkClient>();

    private static bool IsMultiplayerConnected()
        => TryGetNetworkClient()?.IsConnected == true;

    private static bool TryUseMaxUnlockLevel(ref int result)
    {
        if (!IsMultiplayerConnected())
        {
            return true;
        }

        result = ExpHelper.MaxLevel;
        return false;
    }

        [HarmonyPatch(typeof(GameMaster), nameof(GameMaster.CurrentProfileLevel), MethodType.Getter)]
    private static class GameMaster_CurrentProfileLevel_Getter_Patch
    {
        [HarmonyPrefix]
        private static bool Prefix(ref int __result)
        {
            return TryUseMaxUnlockLevel(ref __result);
        }
    }

        [HarmonyPatch(typeof(GameRunStartupParameters), nameof(GameRunStartupParameters.UnlockLevel), MethodType.Getter)]
    private static class GameRunStartupParameters_UnlockLevel_Getter_Patch
    {
        [HarmonyPrefix]
        private static bool Prefix(ref int __result)
        {
            return TryUseMaxUnlockLevel(ref __result);
        }
    }
}
