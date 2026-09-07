using System;
using HarmonyLib;
using LBoL.Core;
using LBoL.Presentation.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Configuration;
using NetworkPlugin.Network.Services;
using NetworkPlugin.Network.Client;

namespace NetworkPlugin.Patch.UI;

[HarmonyPatch]
internal static class AiDefaultMimicLocalAnimationPatch
{
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    [ThreadStatic]
    private static bool _isMirroring;

    private static bool IsEnabled()
    {
        var cfg = ServiceProvider?.GetService<ConfigManager>();
        return cfg?.DebugVirtualPlayerAiDefault?.Value == true;
    }

    private static bool IsNetworkConnected()
        => ServiceProvider?.GetService<INetworkClient>()?.IsConnected == true;

    [HarmonyPatch(typeof(UnitView), "PlayAnimation", typeof(string))]
    [HarmonyPostfix]
    private static void UnitView_PlayAnimation_Postfix(UnitView __instance, string animationName)
    {
        if (_isMirroring)
        {
            return;
        }

        if (!IsEnabled())
        {
            return;
        }

        if (IsNetworkConnected())
        {
            return;
        }

        UnitView local = Singleton<GameDirector>.Instance?.PlayerUnitView;
        if (local == null || __instance == null)
        {
            return;
        }

        if (string.Equals(animationName, "hit", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (!ReferenceEquals(__instance, local))
        {
            return;
        }

        if (!OtherPlayersOverlayPatch.TryGetRemoteCharacterUnitView("aidefault", out UnitView remote))
        {
            return;
        }

        _isMirroring = true;
        try
        {
            remote.PlayAnimation(animationName);
        }
        finally
        {
            _isMirroring = false;
        }
    }
}
