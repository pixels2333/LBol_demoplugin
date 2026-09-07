using System;
using System.Linq;
using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Battle;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network.Services;
using NetworkPlugin.Network.Client;

namespace NetworkPlugin.Patch.Network;

public static class ExhibitSyncPatch
{
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    private static INetworkClient TryGetNetworkClient()
        => ServiceProvider?.GetService<INetworkClient>();

        [HarmonyPatch(typeof(Exhibit), nameof(Exhibit.NotifyActivating))]
    private static class Exhibit_NotifyActivating_Patch
    {
        [HarmonyPostfix]
        private static void Postfix(Exhibit __instance)
        {
            try
            {
                INetworkClient client = TryGetNetworkClient();
                BattleController battle = __instance?.Battle;
                if (__instance == null ||
                    client == null ||
                    !client.IsConnected ||
                    !string.Equals(__instance.Id, "Bianhua", StringComparison.Ordinal) ||
                    battle == null ||
                    battle.BattleShouldEnd ||
                    battle.EnemyGroup?.Alives == null ||
                    !battle.EnemyGroup.Alives.Any())
                {
                    return;
                }

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
