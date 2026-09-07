using System;
using HarmonyLib;
using LBoL.Core.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network.Services;
using NetworkPlugin.Network.Client;

namespace NetworkPlugin.Patch.EnemyUnits;

[HarmonyPatch]
public class EnemyUnitPatches
{
    #region 私有字段

        private static IServiceProvider serviceProvider = ModService.ServiceProvider;

        private static INetworkManager networkManager => serviceProvider?.GetRequiredService<INetworkManager>();

    #endregion

    #region 血量调整补丁

        [HarmonyPatch(typeof(EnemyUnit), "SetMaxHpInBattle")]
    [HarmonyPrefix]
    public static void SetMaxHpInBattle(EnemyUnit __instance, ref int hp, ref int maxHp)
    {
        try
        {

            if (networkManager == null)
            {
                Plugin.Logger?.LogWarning("[EnemyUnitPatches] NetworkManager is null, skipping HP adjustment");
                return;
            }

            int playerCount = networkManager.GetPlayerCount();
            float multiplier = Plugin.ConfigManager?.GetEnemyHpMultiplier(playerCount) ?? playerCount;
            multiplier = Math.Max(1.0f, multiplier);

            int originalMaxHp = maxHp;
            hp = (int)Math.Round(hp * multiplier);
            maxHp = (int)Math.Round(maxHp * multiplier);
            if (hp < 1) hp = 1;
            if (maxHp < 1) maxHp = 1;

            Plugin.Logger?.LogInfo($"[EnemyUnitPatches] Adjusted enemy HP: {originalMaxHp} -> {maxHp} (x{multiplier:0.##} for {playerCount} players)");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[EnemyUnitPatches] Error adjusting enemy HP: {ex.Message}");

        }
    }

    #endregion
}
