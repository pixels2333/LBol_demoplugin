using System;
using System.Collections.Generic;
using HarmonyLib;
using LBoL.Presentation.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network.Services;
using NetworkPlugin.Network.Client;

namespace NetworkPlugin.Patch.UI;

[HarmonyPatch]
public static class MoodSwitchRenderPatches
{
        public static UnitView RenderOn;

    private static INetworkClient NetworkClient => ModService.ServiceProvider?.GetService<INetworkClient>();

    private static readonly HashSet<string> MoodEffectNames = new(StringComparer.OrdinalIgnoreCase)
    {

        "ChaowoLoop",
        "BenwoLoop",
        "DunwuLoop",
    };

    private static bool ShouldRedirect(string effectName)
    {
        return !string.IsNullOrWhiteSpace(effectName) && MoodEffectNames.Contains(effectName);
    }

    private static bool IsConnected()
        => NetworkClient?.IsConnected == true;

    [HarmonyPatch(typeof(UnitView), nameof(UnitView.TryPlayEffectLoop))]
    [HarmonyPrefix]
    private static bool UnitView_TryPlayEffectLoop_Prefix(UnitView __instance, string effectName, ref bool __result)
    {
        if (!IsConnected() || RenderOn == null || ReferenceEquals(__instance, RenderOn) || !ShouldRedirect(effectName))
        {
            return true;
        }

        __result = RenderOn.TryPlayEffectLoop(effectName);
        return false;
    }

    [HarmonyPatch(typeof(UnitView), nameof(UnitView.SendEffectMessage))]
    [HarmonyPrefix]
    private static bool UnitView_SendEffectMessage_Prefix(UnitView __instance, string effectName, string message, object args)
    {
        if (!IsConnected() || RenderOn == null || ReferenceEquals(__instance, RenderOn) || !ShouldRedirect(effectName))
        {
            return true;
        }

        RenderOn.SendEffectMessage(effectName, message, args);
        return false;
    }

    [HarmonyPatch(typeof(UnitView), nameof(UnitView.EndEffectLoop))]
    [HarmonyPrefix]
    private static bool UnitView_EndEffectLoop_Prefix(UnitView __instance, string effectName, bool instant)
    {
        if (!IsConnected() || RenderOn == null || ReferenceEquals(__instance, RenderOn) || !ShouldRedirect(effectName))
        {
            return true;
        }

        RenderOn.EndEffectLoop(effectName, instant);
        return false;
    }
}
