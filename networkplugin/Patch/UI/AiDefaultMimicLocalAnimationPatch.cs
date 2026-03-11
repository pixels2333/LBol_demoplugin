using System;
using HarmonyLib;
using LBoL.Core;
using LBoL.Presentation.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Configuration;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;

namespace NetworkPlugin.Patch.UI;

/// <summary>
/// Debug-only: when enabled, mirrors the local player's battle animations to the virtual remote player (PlayerId=aidefault).
/// This helps validate the "remote player" render path in battle without a real network peer.
/// </summary>
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
    {
        var client = ServiceProvider?.GetService<INetworkClient>();
        return client != null && client.IsConnected;
    }

    [HarmonyPatch(typeof(UnitView), "PlayAnimation", typeof(string))]
    [HarmonyPostfix]
    private static void UnitView_PlayAnimation_Postfix(UnitView __instance, string animationName)
    {
        try
        {
            if (_isMirroring)
            {
                return;
            }

            if (!IsEnabled())
            {
                return;
            }

            // Avoid affecting real multiplayer sessions.
            if (IsNetworkConnected())
            {
                return;
            }

            var local = Singleton<GameDirector>.Instance?.PlayerUnitView;

            if (local == null || __instance == null)
            {
                return;
            }

            // Keep the debug avatar from looking like it is taking double damage.
            if (string.Equals(animationName, "hit", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // Only mirror from the local player's UnitView.
            if (!ReferenceEquals(__instance, local))
            {
                return;
            }

            if (!OtherPlayersOverlayPatch.TryGetRemoteCharacterUnitView("aidefault", out UnitView remote) || remote == null)
            {
                return;
            }

            _isMirroring = true;
            remote.PlayAnimation(animationName);
        }
        catch
        {
            // ignored
        }
        finally
        {
            _isMirroring = false;
        }
    }
}
