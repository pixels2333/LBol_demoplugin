using System;
using HarmonyLib;
using LBoL.Base;
using LBoL.Core.Battle;
using LBoL.Core.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network.Services;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Patch.Network;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Patch.EnemyUnits;

[HarmonyPatch]
public static class SpawnedEnemyManager
{
    public static int EnemySpawnCount { get; private set; }

    private static IServiceProvider serviceProvider => ModService.ServiceProvider;

    private static INetworkClient networkClient => serviceProvider?.GetService<INetworkClient>();

    internal static int SuppressBroadcastDepth { get; private set; }

        internal readonly struct SuppressBroadcastScope : IDisposable
    {
                public void Dispose()
        {
            if (SuppressBroadcastDepth > 0)
            {
                SuppressBroadcastDepth--;
            }
        }
    }

        internal static IDisposable SuppressBroadcast()
    {
        SuppressBroadcastDepth++;
        return new SuppressBroadcastScope();
    }

    private static void TryRestoreEnemyBattleRng(BattleController battleController, in RngSwapState state)
    {
        if (!state.Swapped || battleController?.GameRun == null)
        {
            return;
        }

        try
        {
            Traverse.Create(battleController.GameRun).Field("<EnemyBattleRng>k__BackingField").SetValue(state.OriginalEnemyBattleRng);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[SpawnedEnemyManager] Failed to restore EnemyBattleRng: {ex.Message}");
        }
    }

    private static ulong HashStable(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return 0;
        }

        const ulong fnvOffset = 14695981039346656037UL;
        const ulong fnvPrime = 1099511628211UL;

        ulong hash = fnvOffset;
        foreach (char c in value)
        {
            hash ^= c;
            hash *= fnvPrime;
        }

        return hash;
    }

    private readonly struct RngSwapState
    {
        public RngSwapState(RandomGen originalEnemyBattleRng, bool swapped)
        {
            OriginalEnemyBattleRng = originalEnemyBattleRng;
            Swapped = swapped;
        }

        public RandomGen OriginalEnemyBattleRng { get; }
        public bool Swapped { get; }
    }

    [HarmonyPatch(typeof(BattleController), "Spawn", new[] { typeof(EnemyUnit), typeof(Type), typeof(int), typeof(bool) })]
    private static class BattleController_Spawn_Patch
    {
        [HarmonyPrefix]
        private static void Prefix(BattleController __instance, EnemyUnit spawner, Type type, int rootIndex, bool isServant, ref RngSwapState __state)
        {
            __state = default;

            try
            {
                var client = networkClient;
                if (client == null || !client.IsConnected)
                {
                    return;
                }

                var gameRun = __instance.GameRun;
                if (gameRun == null)
                {
                    return;
                }

                ulong seed = gameRun.RootSeed;
                int spawnerRootIndex = spawner?.RootIndex ?? 0;
                seed = unchecked(seed + (ulong)(rootIndex + 1));
                seed = unchecked(seed + (ulong)(EnemySpawnCount + 1));
                seed = unchecked(seed + (ulong)(spawner?.RootIndex ?? 0));
                seed = unchecked(seed + (ulong)((spawner?.RootIndex ?? 0) + 1));
                seed = unchecked(seed + HashStable(type?.FullName ?? string.Empty));
                seed = unchecked(seed + (isServant ? 1UL : 0UL));

                Traverse gameRunTraverse = Traverse.Create(gameRun);
                var originalEnemyBattleRng = gameRunTraverse.Field("<EnemyBattleRng>k__BackingField").GetValue<RandomGen>();
                __state = new RngSwapState(originalEnemyBattleRng, swapped: true);

                gameRunTraverse.Field("<EnemyBattleRng>k__BackingField").SetValue(new RandomGen(seed));
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[SpawnedEnemyManager] Failed to swap EnemyBattleRng: {ex.Message}");
                __state = default;
            }
        }

        [HarmonyPostfix]
        private static void Postfix(BattleController __instance, EnemyUnit spawner, Type type, int rootIndex, bool isServant, EnemyUnit __result, RngSwapState __state)
        {
            try
            {
                var client = networkClient;
                if (client == null || !client.IsConnected)
                {
                    return;
                }

                if (__instance?.GameRun == null || __result == null)
                {
                    return;
                }

                EnemySpawnCount++;
                string spawnId = SpawnedEnemySyncPatch.BuildSpawnId(__instance.EnemyGroup?.Id, EnemySpawnCount, __result.RootIndex, __result.Id);
                SpawnedEnemySyncPatch.BindSpawnId(__result, spawnId);

                var spawnEvent = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    BattleId = __instance.GetHashCode().ToString(),
                    SpawnIndex = EnemySpawnCount,
                    SpawnId = spawnId,
                    Spawner = spawner == null
                        ? null
                        : new
                        {
                            spawner.Id,
                            Type = spawner.GetType().Name,
                            spawner.RootIndex
                        },
                    Spawned = new
                    {
                        SpawnId = spawnId,
                        __result.Id,
                        Type = __result.GetType().Name,
                        __result.RootIndex,
                        __result.MaxHp,
                        CurrentHp = __result.Hp,
                        __result.Block,
                        __result.Shield,
                        __result.Status
                    },
                    Args = new
                    {
                        EnemyType = type?.Name ?? "unknown",
                        RootIndex = rootIndex,
                        IsServant = isServant
                    }
                };

                if (SuppressBroadcastDepth <= 0)
                {

                    client.SendRequest(NetworkMessageTypes.BattleEnemySpawned, JsonCompat.Serialize(spawnEvent));

                    client.SendRequest(NetworkMessageTypes.EnemySpawned, JsonCompat.Serialize(spawnEvent));
                    Plugin.Logger?.LogInfo($"[SpawnedEnemyManager] Enemy spawned: {__result.Name} (Type={__result.GetType().Name}, RootIndex={__result.RootIndex})");
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[SpawnedEnemyManager] Error in Spawn postfix: {ex.Message}");
            }
            finally
            {
                TryRestoreEnemyBattleRng(__instance, in __state);
            }
        }
    }
}
