using System;
using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Patch.Network;

/// <summary>
/// 玩家基础状态同步补丁（参考 Together in Spire 的 CreatureSyncPatches 思路）。
/// </summary>
/// <remarks>
/// 目标：补齐那些“不经过伤害/治疗流程”的状态变化同步，例如：
/// - 本地玩家格挡/护盾变化（Cast/Lose BlockShield）
/// - 本地玩家金钱变化（Gain/Consume/Lose Money）
/// - 本地玩家最大生命变化（set_MaxHp）
/// </remarks>
[HarmonyPatch]
public static class PlayerStateSyncPatch
{
    #region 依赖注入与发送

    /// <summary>
    /// 依赖注入服务提供者。
    /// </summary>
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    /// <summary>
    /// 尝试解析网络客户端。
    /// </summary>
    /// <returns>解析成功返回客户端，否则返回 null。</returns>
    private static INetworkClient TryGetClient()
    {
        try
        {
            return ServiceProvider?.GetService<INetworkClient>();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 判断当前是否允许发送同步事件。
    /// </summary>
    /// <returns>允许发送返回 true，否则返回 false。</returns>
    private static bool ShouldSend()
    {
        // 获取客户端并确认已连接。
        var client = TryGetClient();
        if (client == null || !client.IsConnected)
        {
            return false;
        }

        // 确保已订阅/具备自身玩家标识。
        NetworkIdentityTracker.EnsureSubscribed(client);
        return !string.IsNullOrWhiteSpace(NetworkIdentityTracker.GetSelfPlayerId());
    }

    /// <summary>
    /// 发送一条游戏事件（带容错）。
    /// </summary>
    /// <param name="eventType">事件类型。</param>
    /// <param name="payload">事件负载（可序列化对象）。</param>
    private static void Send(string eventType, object payload)
    {
        try
        {
            // 再次确认连接状态，避免在发送时因断线抛异常。
            var client = TryGetClient();
            if (client == null || !client.IsConnected)
            {
                return;
            }

            // 确保订阅状态与身份信息就绪。
            NetworkIdentityTracker.EnsureSubscribed(client);
            client.SendGameEventData(eventType, payload);
        }
        catch (Exception ex)
        {
            // 记录错误但不打断游戏流程。
            Plugin.Logger?.LogError($"[PlayerStateSync] 发送事件 {eventType} 失败: {ex.Message}");
        }
    }

    #endregion

    #region 本地战斗判断与快照

    /// <summary>
    /// 判断是否应当同步“本地玩家所在战斗”。
    /// </summary>
    /// <param name="battle">战斗控制器。</param>
    /// <returns>本地玩家正在控制该战斗返回 true，否则返回 false。</returns>
    private static bool ShouldSyncLocalBattle(BattleController battle)
        => battle != null && battle.Player != null && battle.Player == GameStateUtils.GetCurrentPlayer();

    /// <summary>
    /// 构建玩家单位快照，用于日志/调试与回放辅助。
    /// </summary>
    /// <param name="player">玩家单位。</param>
    /// <returns>匿名对象快照。</returns>
    private static object SnapshotPlayer(PlayerUnit player)
        => new
        {
            PlayerUnitId = player?.Id?.ToString(),
            player?.Hp,
            player?.MaxHp,
            player?.Block,
            player?.Shield,
            player?.IsAlive,
            Status = player?.Status.ToString(),
        };

    #endregion

    #region 格挡/护盾变化同步

    [HarmonyPatch(typeof(BattleController), "CastBlockShield")]
    private static class BattleController_CastBlockShield_Sync
    {
        /// <summary>
        /// 前置：记录施加格挡/护盾前的状态，用于后置判断是否发生变化。
        /// </summary>
        /// <param name="__instance">战斗控制器实例。</param>
        /// <param name="target">施加目标。</param>
        /// <param name="__state">用于保存“变化前”的 (block, shield)。</param>
        [HarmonyPrefix]
        public static void Prefix(BattleController __instance, Unit target, ref (int block, int shield) __state)
        {
            // 使用哨兵值标记“未采集到前置状态”。
            __state = (int.MinValue, int.MinValue);
            try
            {
                // 不满足发送条件 / 远程出牌流程 / 非本地战斗时跳过。
                if (!ShouldSend() || RemoteCardUsePatch.IsInRemoteCardPipeline || !ShouldSyncLocalBattle(__instance))
                {
                    return;
                }

                // 只关注本地玩家单位。
                if (target is not PlayerUnit player || player != __instance.Player)
                {
                    return;
                }

                // 记录变化前的格挡/护盾。
                __state = (player.Block, player.Shield);
            }
            catch
            {
                // 忽略：容错处理，不影响主流程。
            }
        }

        /// <summary>
        /// 后置：若格挡/护盾发生变化，则发送一次状态更新。
        /// </summary>
        /// <param name="__instance">战斗控制器实例。</param>
        /// <param name="target">施加目标。</param>
        /// <param name="block">请求施加的格挡值（float）。</param>
        /// <param name="shield">请求施加的护盾值（float）。</param>
        /// <param name="__state">前置采集到的 (block, shield)。</param>
        [HarmonyPostfix]
        public static void Postfix(BattleController __instance, Unit target, float block, float shield, (int block, int shield) __state)
        {
            try
            {
                // 不满足发送条件 / 远程出牌流程 / 非本地战斗时跳过。
                if (!ShouldSend() || RemoteCardUsePatch.IsInRemoteCardPipeline || !ShouldSyncLocalBattle(__instance))
                {
                    return;
                }

                // 只关注本地玩家单位。
                if (target is not PlayerUnit player || player != __instance.Player)
                {
                    return;
                }

                // 前置未采集到状态则不发送。
                if (__state.block == int.MinValue)
                {
                    return;
                }

                // 没有变化则不发送。
                if (player.Block == __state.block && player.Shield == __state.shield)
                {
                    return;
                }

                // 发送状态变更事件。
                Send(NetworkMessageTypes.OnPlayerStateUpdate, new
                {
                    Timestamp = DateTime.Now.Ticks,
                    UpdateType = "BlockShieldGained",
                    PlayerId = NetworkIdentityTracker.GetSelfPlayerId(),
                    BattleId = __instance.GetHashCode().ToString(),
                    Round = __instance.RoundCounter,
                    Requested = new { Block = block, Shield = shield },
                    Before = new { Block = __state.block, Shield = __state.shield },
                    After = new { Block = player.Block, Shield = player.Shield },
                    Player = SnapshotPlayer(player),
                });
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[PlayerStateSync] CastBlockShield 后置同步失败: {ex.Message}");
            }
        }
    }

    [HarmonyPatch(typeof(BattleController), "LoseBlockShield")]
    private static class BattleController_LoseBlockShield_Sync
    {
        /// <summary>
        /// 前置：记录移除格挡/护盾前的状态，用于后置判断是否发生变化。
        /// </summary>
        /// <param name="__instance">战斗控制器实例。</param>
        /// <param name="target">移除目标。</param>
        /// <param name="__state">用于保存“变化前”的 (block, shield)。</param>
        [HarmonyPrefix]
        public static void Prefix(BattleController __instance, Unit target, ref (int block, int shield) __state)
        {
            // 使用哨兵值标记“未采集到前置状态”。
            __state = (int.MinValue, int.MinValue);
            try
            {
                // 不满足发送条件 / 远程出牌流程 / 非本地战斗时跳过。
                if (!ShouldSend() || RemoteCardUsePatch.IsInRemoteCardPipeline || !ShouldSyncLocalBattle(__instance))
                {
                    return;
                }

                // 只关注本地玩家单位。
                if (target is not PlayerUnit player || player != __instance.Player)
                {
                    return;
                }

                // 记录变化前的格挡/护盾。
                __state = (player.Block, player.Shield);
            }
            catch
            {
                // 忽略：容错处理，不影响主流程。
            }
        }

        /// <summary>
        /// 后置：若格挡/护盾发生变化，则发送一次状态更新。
        /// </summary>
        /// <param name="__instance">战斗控制器实例。</param>
        /// <param name="target">移除目标。</param>
        /// <param name="block">请求移除的格挡值（float）。</param>
        /// <param name="shield">请求移除的护盾值（float）。</param>
        /// <param name="__state">前置采集到的 (block, shield)。</param>
        [HarmonyPostfix]
        public static void Postfix(BattleController __instance, Unit target, float block, float shield, (int block, int shield) __state)
        {
            try
            {
                // 不满足发送条件 / 远程出牌流程 / 非本地战斗时跳过。
                if (!ShouldSend() || RemoteCardUsePatch.IsInRemoteCardPipeline || !ShouldSyncLocalBattle(__instance))
                {
                    return;
                }

                // 只关注本地玩家单位。
                if (target is not PlayerUnit player || player != __instance.Player)
                {
                    return;
                }

                // 前置未采集到状态则不发送。
                if (__state.block == int.MinValue)
                {
                    return;
                }

                // 没有变化则不发送。
                if (player.Block == __state.block && player.Shield == __state.shield)
                {
                    return;
                }

                // 发送状态变更事件。
                Send(NetworkMessageTypes.OnPlayerStateUpdate, new
                {
                    Timestamp = DateTime.Now.Ticks,
                    UpdateType = "BlockShieldLost",
                    PlayerId = NetworkIdentityTracker.GetSelfPlayerId(),
                    BattleId = __instance.GetHashCode().ToString(),
                    Round = __instance.RoundCounter,
                    Requested = new { Block = block, Shield = shield },
                    Before = new { Block = __state.block, Shield = __state.shield },
                    After = new { Block = player.Block, Shield = player.Shield },
                    Player = SnapshotPlayer(player),
                });
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[PlayerStateSync] LoseBlockShield 后置同步失败: {ex.Message}");
            }
        }
    }

    #endregion

    #region 最大生命变化同步

    [HarmonyPatch(typeof(Unit), "set_MaxHp")]
    private static class Unit_SetMaxHp_Sync
    {
        /// <summary>
        /// 前置：记录变更前的 MaxHp。
        /// </summary>
        /// <param name="__instance">被设置 MaxHp 的单位。</param>
        /// <param name="__state">用于保存变更前的 MaxHp。</param>
        [HarmonyPrefix]
        public static void Prefix(Unit __instance, ref int __state)
        {
            __state = 0;
            try
            {
                // 不满足发送条件或处于远程出牌流程时跳过。
                if (!ShouldSend() || RemoteCardUsePatch.IsInRemoteCardPipeline)
                {
                    return;
                }

                // 只同步本地玩家。
                if (__instance is not PlayerUnit player || player != GameStateUtils.GetCurrentPlayer())
                {
                    return;
                }

                __state = player.MaxHp;
            }
            catch
            {
                // 忽略：容错处理，不影响主流程。
            }
        }

        /// <summary>
        /// 后置：若 MaxHp 发生变化，则发送一次状态更新。
        /// </summary>
        /// <param name="__instance">被设置 MaxHp 的单位。</param>
        /// <param name="__state">前置保存的变更前 MaxHp。</param>
        [HarmonyPostfix]
        public static void Postfix(Unit __instance, int __state)
        {
            try
            {
                // 不满足发送条件或处于远程出牌流程时跳过。
                if (!ShouldSend() || RemoteCardUsePatch.IsInRemoteCardPipeline)
                {
                    return;
                }

                // 只同步本地玩家。
                if (__instance is not PlayerUnit player || player != GameStateUtils.GetCurrentPlayer())
                {
                    return;
                }

                // 0 表示前置未采集到，或者未发生变化。
                if (__state == 0 || __state == player.MaxHp)
                {
                    return;
                }

                // 发送最大生命变更事件。
                Send(NetworkMessageTypes.OnPlayerStateUpdate, new
                {
                    Timestamp = DateTime.Now.Ticks,
                    UpdateType = "MaxHpChanged",
                    PlayerId = NetworkIdentityTracker.GetSelfPlayerId(),
                    Before = new { MaxHp = __state },
                    After = new { MaxHp = player.MaxHp },
                    Player = SnapshotPlayer(player),
                });
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[PlayerStateSync] set_MaxHp 后置同步失败: {ex.Message}");
            }
        }
    }

    #endregion

    #region 金钱变化同步

    [HarmonyPatch(typeof(GameRunController), nameof(GameRunController.GainMoney))]
    private static class GameRun_GainMoney_Sync
    {
        /// <summary>
        /// 前置：记录变更前的金钱。
        /// </summary>
        /// <param name="__instance">游戏流程控制器。</param>
        /// <param name="money">请求增加的金钱数量。</param>
        /// <param name="__state">用于保存变更前的金钱。</param>
        [HarmonyPrefix]
        public static void Prefix(GameRunController __instance, int money, out int __state)
        {
            __state = 0;
            try
            {
                if (!ShouldSend() || RemoteCardUsePatch.IsInRemoteCardPipeline)
                {
                    return;
                }

                __state = __instance.Money;
            }
            catch
            {
                // 忽略：容错处理，不影响主流程。
            }
        }

        /// <summary>
        /// 后置：若金钱发生变化，则发送一次状态更新。
        /// </summary>
        /// <param name="__instance">游戏流程控制器。</param>
        /// <param name="money">请求增加的金钱数量。</param>
        /// <param name="__state">前置保存的变更前金钱。</param>
        [HarmonyPostfix]
        public static void Postfix(GameRunController __instance, int money, int __state)
        {
            try
            {
                if (!ShouldSend() || RemoteCardUsePatch.IsInRemoteCardPipeline)
                {
                    return;
                }

                // 没有变化则不发送。
                if (__instance.Money == __state)
                {
                    return;
                }

                Send(NetworkMessageTypes.OnPlayerStateUpdate, new
                {
                    Timestamp = DateTime.Now.Ticks,
                    UpdateType = "MoneyGained",
                    PlayerId = NetworkIdentityTracker.GetSelfPlayerId(),
                    Before = new { Money = __state },
                    After = new { Money = __instance.Money },
                    Delta = __instance.Money - __state,
                    Requested = money,
                    PlayerUnitId = GameStateUtils.GetCurrentPlayerId(),
                });
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[PlayerStateSync] GainMoney 后置同步失败: {ex.Message}");
            }
        }
    }

    [HarmonyPatch(typeof(GameRunController), nameof(GameRunController.ConsumeMoney))]
    private static class GameRun_ConsumeMoney_Sync
    {
        /// <summary>
        /// 前置：记录变更前的金钱。
        /// </summary>
        /// <param name="__instance">游戏流程控制器。</param>
        /// <param name="cost">请求消耗的金钱数量。</param>
        /// <param name="__state">用于保存变更前的金钱。</param>
        [HarmonyPrefix]
        public static void Prefix(GameRunController __instance, int cost, out int __state)
        {
            __state = 0;
            try
            {
                if (!ShouldSend() || RemoteCardUsePatch.IsInRemoteCardPipeline)
                {
                    return;
                }

                __state = __instance.Money;
            }
            catch
            {
                // 忽略：容错处理，不影响主流程。
            }
        }

        /// <summary>
        /// 后置：若金钱发生变化，则发送一次状态更新。
        /// </summary>
        /// <param name="__instance">游戏流程控制器。</param>
        /// <param name="cost">请求消耗的金钱数量。</param>
        /// <param name="__state">前置保存的变更前金钱。</param>
        [HarmonyPostfix]
        public static void Postfix(GameRunController __instance, int cost, int __state)
        {
            try
            {
                if (!ShouldSend() || RemoteCardUsePatch.IsInRemoteCardPipeline)
                {
                    return;
                }

                // 没有变化则不发送。
                if (__instance.Money == __state)
                {
                    return;
                }

                Send(NetworkMessageTypes.OnPlayerStateUpdate, new
                {
                    Timestamp = DateTime.Now.Ticks,
                    UpdateType = "MoneyConsumed",
                    PlayerId = NetworkIdentityTracker.GetSelfPlayerId(),
                    Before = new { Money = __state },
                    After = new { Money = __instance.Money },
                    Delta = __instance.Money - __state,
                    Requested = cost,
                    PlayerUnitId = GameStateUtils.GetCurrentPlayerId(),
                });
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[PlayerStateSync] ConsumeMoney 后置同步失败: {ex.Message}");
            }
        }
    }

    [HarmonyPatch(typeof(GameRunController), nameof(GameRunController.LoseMoney))]
    private static class GameRun_LoseMoney_Sync
    {
        /// <summary>
        /// 前置：记录变更前的金钱。
        /// </summary>
        /// <param name="__instance">游戏流程控制器。</param>
        /// <param name="money">请求丢失的金钱数量。</param>
        /// <param name="__state">用于保存变更前的金钱。</param>
        [HarmonyPrefix]
        public static void Prefix(GameRunController __instance, int money, out int __state)
        {
            __state = 0;
            try
            {
                if (!ShouldSend() || RemoteCardUsePatch.IsInRemoteCardPipeline)
                {
                    return;
                }

                __state = __instance.Money;
            }
            catch
            {
                // 忽略：容错处理，不影响主流程。
            }
        }

        /// <summary>
        /// 后置：若金钱发生变化，则发送一次状态更新。
        /// </summary>
        /// <param name="__instance">游戏流程控制器。</param>
        /// <param name="money">请求丢失的金钱数量。</param>
        /// <param name="__state">前置保存的变更前金钱。</param>
        [HarmonyPostfix]
        public static void Postfix(GameRunController __instance, int money, int __state)
        {
            try
            {
                if (!ShouldSend() || RemoteCardUsePatch.IsInRemoteCardPipeline)
                {
                    return;
                }

                // 没有变化则不发送。
                if (__instance.Money == __state)
                {
                    return;
                }

                Send(NetworkMessageTypes.OnPlayerStateUpdate, new
                {
                    Timestamp = DateTime.Now.Ticks,
                    UpdateType = "MoneyLost",
                    PlayerId = NetworkIdentityTracker.GetSelfPlayerId(),
                    Before = new { Money = __state },
                    After = new { Money = __instance.Money },
                    Delta = __instance.Money - __state,
                    Requested = money,
                    PlayerUnitId = GameStateUtils.GetCurrentPlayerId(),
                });
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[PlayerStateSync] LoseMoney 后置同步失败: {ex.Message}");
            }
        }
    }

    #endregion
}
