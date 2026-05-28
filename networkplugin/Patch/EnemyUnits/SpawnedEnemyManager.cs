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

    /// <summary>
    /// 抑制广播作用域：在 using 块内暂时禁止生成事件广播，避免各端重复生成
    /// </summary>
    internal readonly struct SuppressBroadcastScope : IDisposable
    {
        /// <summary>释放抑制广播作用域</summary>
        public void Dispose()
        {
            if (SuppressBroadcastDepth > 0)
            {
                SuppressBroadcastDepth--;
            }
        }
    }

    /// <summary>
    /// 在作用域内抑制敌人生成广播，避免回环
    /// </summary>
    /// <returns>释放时恢复广播的 IDisposable 作用域</returns>
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

                // 仅在“生成敌人”这段窗口内，临时固定 EnemyBattleRng，尽量减少对其它随机行为的影响。
                // Seed 由 RootSeed + 关键参数 + 计数器组成：只要各端 RootSeed 与生成顺序一致即可确定性复现。
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
                    // 主路径：BattleEnemySpawned
                    client.SendRequest(NetworkMessageTypes.BattleEnemySpawned, JsonCompat.Serialize(spawnEvent));

                    // 兼容桥（1 个迭代周期）：镜像发送旧事件名，避免旧端断链。
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
