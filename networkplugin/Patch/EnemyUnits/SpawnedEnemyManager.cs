using System;
using System.Text.Json;
using HarmonyLib;
using LBoL.Base;
using LBoL.Core.Battle;
using LBoL.Core.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Network.Client;

namespace NetworkPlugin.Patch.EnemyUnits;

/// <summary>
/// 参照 Together in Spire 的 SpawnedMonsterManager：
/// - 为“战斗中生成的新敌人/随从”提供确定性的 RNG（避免各端生成物属性/随机行为不一致）
/// - 在生成完成后广播生成事件，便于其他客户端同步创建/追踪该单位
/// </summary>
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
                if (__instance == null)
                {
                    return;
                }

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

                // 仅在“生成敌人”这段窗口内，临时固定 EnemyBattleRng，尽量减少对其它随机行为的影响。
                // Seed 由 RootSeed + 关键参数 + 计数器组成：只要各端 RootSeed 与生成顺序一致即可确定性复现。
                ulong seed = gameRun.RootSeed;
                seed = unchecked(seed + (ulong)(rootIndex + 1));
                seed = unchecked(seed + (ulong)(EnemySpawnCount + 1));
                seed = unchecked(seed + (ulong)(spawner?.RootIndex ?? 0));
                seed = unchecked(seed + (ulong)((spawner?.RootIndex ?? 0) + 1));
                seed = unchecked(seed + HashStable(type?.FullName ?? string.Empty));
                seed = unchecked(seed + (isServant ? 1UL : 0UL));

                var gameRunTraverse = Traverse.Create(gameRun);
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

                var spawnEvent = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    BattleId = __instance.GetHashCode().ToString(),
                    SpawnIndex = EnemySpawnCount,
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
                    client.SendRequest(NetworkMessageTypes.EnemySpawned, JsonSerializer.Serialize(spawnEvent));
                    Plugin.Logger?.LogInfo($"[SpawnedEnemyManager] Enemy spawned: {__result.Name} (Type={__result.GetType().Name}, RootIndex={__result.RootIndex})");
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[SpawnedEnemyManager] Error in Spawn postfix: {ex.Message}");
            }
            finally
            {
                try
                {
                    if (__state.Swapped && __instance?.GameRun != null)
                    {
                        Traverse.Create(__instance.GameRun).Field("<EnemyBattleRng>k__BackingField").SetValue(__state.OriginalEnemyBattleRng);
                    }
                }
                catch (Exception ex)
                {
                    Plugin.Logger?.LogError($"[SpawnedEnemyManager] Failed to restore EnemyBattleRng: {ex.Message}");
                }
            }
        }
    }
}
