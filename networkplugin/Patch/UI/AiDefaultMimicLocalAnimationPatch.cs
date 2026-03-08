using System;
using HarmonyLib;
using LBoL.Core;
using LBoL.Presentation;
using LBoL.Presentation.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Configuration;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;

namespace NetworkPlugin.Patch.UI;

/// <summary>
/// Debug-only: when enabled, mirrors the local player's battle animations to the virtual remote players
/// (`aidefault` / `aidefault2`) to validate the remote render path without a real network peer.
/// </summary>
[HarmonyPatch]
internal static class AiDefaultMimicLocalAnimationPatch
{
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    [ThreadStatic]
    private static bool _isMirroring;

    private static bool IsEnabled()
    {
        try
        {
            var cfg = ServiceProvider?.GetService<ConfigManager>();
            return cfg?.DebugVirtualPlayerAiDefault?.Value == true;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsNetworkConnected()
    {
        try
        {
            var client = ServiceProvider?.GetService<INetworkClient>();
            return client != null && client.IsConnected;
        }
        catch
        {
            return false;
        }
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

            UnitView local = null;
            try
            {
                local = Singleton<GameDirector>.Instance?.PlayerUnitView;
            }
            catch
            {
                local = null;
            }

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

            _isMirroring = true;
            foreach (var debugPlayer in OtherPlayersOverlayPatch.EnumerateVirtualAiDebugPlayers())
            {
                if (!OtherPlayersOverlayPatch.TryGetRemoteCharacterUnitView(debugPlayer.PlayerId, out UnitView remote) || remote == null)
                {
                    continue;
                }

                remote.PlayAnimation(animationName);
            }
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
