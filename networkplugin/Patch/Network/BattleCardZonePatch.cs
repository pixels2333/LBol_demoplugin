using System;
using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Patch.Network;

/// <summary>
/// 战斗中玩家卡牌区域同步补丁：
/// 参考 Together in Spire 的 PlayerPatches.java（CardGroup 增删/移动同步）实现 LBoL 的 CardZone 变更同步。
/// 通过 Patch BattleController 的 Move/Add/Remove 入口，覆盖绝大多数卡牌区域变化（抽牌、弃牌、放逐、回牌库等）。
/// </summary>
public static class BattleCardZoneSyncPatch
{
    /// <summary>
    /// DI 容器引用，通过 <see cref="ModService"/> 获取，用于解析网络客户端等服务。
    /// </summary>
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider; // 游戏启动时初始化，整局对战内保持稳定

    /// <summary>
    /// 发送一条战斗相关的游戏事件到网络层。
    /// </summary>
    /// <param name="eventType">事件类型标识，通常来自 <see cref="NetworkMessageTypes"/>。</param>
    /// <param name="eventData">具体的事件数据负载，序列化后通过网络发送。</param>
    private static void SendGameEvent(string eventType, object eventData)
    {
        try
        {
            // 从 ServiceProvider 中解析出网络客户端
            var networkClient = ServiceProvider?.GetService<INetworkClient>();
            if (networkClient == null || !networkClient.IsConnected)
            {
                // 没有客户端或未连接时，不做任何同步
                return;
            }

            // 交给网络层实现实际的发送逻辑
            networkClient.SendGameEventData(eventType, eventData); // 序列化并通过底层传输协议发送
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[BattleCardZoneSync] Error sending game event {eventType}: {ex.Message}");
        }
    }

    /// <summary>
    /// 构造单张卡牌的快照对象，用于网络序列化传输。
    /// 只挑选对远端还原卡牌状态有用的字段。
    /// </summary>
    /// <param name="card">需要拍快照的卡牌实例。</param>
    /// <returns>匿名对象，包含卡牌标识、实例 ID、基础信息以及所在区域等。</returns>
    private static object BuildCardSnapshot(Card card)
        => new
        {
            CardId = card?.Id ?? "null",              // 卡牌配置 ID（静态配置表中的 Id）
            InstanceId = card?.InstanceId ?? -1,       // 实例 ID，用于区分同名不同实例
            CardName = card?.Name ?? "null",          // 当前显示名称（含语言与增幅等变更）
            CardType = card?.GetType().Name ?? "null",// 运行时派生类型，用于远端做类型映射
            IsUpgraded = card?.IsUpgraded ?? false,    // 是否为强化版
            Cost = card != null ? card.Cost : default, // 当前费用（已考虑各种 Buff/Debuff）
            Zone = card?.Zone.ToString() ?? "Unknown",// 当前所在区域（手牌/牌库/弃牌等）
            IsEthereal = card?.IsEthereal ?? false,    // 是否回合结束自动消失
            IsAutoExile = card?.IsAutoExile ?? false,  // 使用后是否自动放逐
        };

    /// <summary>
    /// 判断当前战斗是否需要进行网络同步。
    /// 只同步本地玩家控制的战斗，避免远端回放再次广播事件导致循环。
    /// </summary>
    /// <param name="battle">当前战斗控制器实例。</param>
    /// <returns>如果需要同步则为 true，否则为 false。</returns>
    private static bool ShouldSync(BattleController battle)
    {
        if (battle == null)
        {
            return false; // 没有战斗上下文时直接跳过
        }

        // 只同步本地玩家战斗（多人模式下远端回放由网络层驱动，不应再次发送）
        return battle.Player != null && battle.Player == GameStateUtils.GetCurrentPlayer(); // 确认是当前本地玩家的 BattleController
    }

    #region MoveCard 补丁

    /// <summary>
    /// Patch <see cref="BattleController.MoveCard"/>，用于同步卡牌在不同 CardZone 之间的移动。
    /// </summary>
    [HarmonyPatch(typeof(BattleController), "MoveCard")]
    private static class MoveCardPatch
    {
        /// <summary>
        /// 用于在 Prefix / Postfix 之间传递一次 MoveCard 调用的状态。
        /// </summary>
        private struct MoveState
        {
            /// <summary>
            /// 卡牌移动前所在的区域。
            /// </summary>
            public CardZone FromZone; // 调用前 card.Zone

            /// <summary>
            /// 卡牌目标区域（Move 调用目的地）。
            /// </summary>
            public CardZone ToZone;   // 调用时传入的 dstZone

            /// <summary>
            /// 卡牌移动前的快照，用于和移动后的状态做差异。
            /// </summary>
            public object CardSnapshotBefore; // 便于远端进行状态对比/补偿
        }

        /// <summary>
        /// MoveCard 调用前记录卡牌的起始区域及快照。
        /// </summary>
        /// <param name="__instance">被补丁的 <see cref="BattleController"/> 实例。</param>
        /// <param name="card">被移动的卡牌。</param>
        /// <param name="dstZone">目标区域。</param>
        /// <param name="__state">在 Prefix 与 Postfix 之间传递的状态。</param>
        [HarmonyPrefix]
        private static void Prefix(BattleController __instance, Card card, CardZone dstZone, out MoveState __state)
        {
            __state = default; // 若后续 ShouldSync 为 false，Postfix 中 FromZone/快照可能为空
            try
            {
                if (!ShouldSync(__instance))
                {
                    return; // 非本地玩家控制的战斗，直接跳过记录
                }

                // 记录移动前的区域和快照
                __state = new MoveState
                {
                    FromZone = card.Zone,             // 源区域
                    ToZone = dstZone,                 // 目标区域
                    CardSnapshotBefore = BuildCardSnapshot(card) // 记录移动前状态
                };
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[BattleCardZoneSync] Error in MoveCard Prefix: {ex.Message}");
            }
        }

        /// <summary>
        /// MoveCard 调用后，根据结果发送区域变更事件。
        /// </summary>
        /// <param name="__instance">被补丁的 <see cref="BattleController"/> 实例。</param>
        /// <param name="card">被移动的卡牌。</param>
        /// <param name="dstZone">目标区域。</param>
        /// <param name="__result">MoveCard 的取消原因，成功为 <see cref="CancelCause.None"/>。</param>
        /// <param name="__state">在 Prefix 记录的状态。</param>
        [HarmonyPostfix]
        private static void Postfix(BattleController __instance, Card card, CardZone dstZone, CancelCause __result, MoveState __state)
        {
            try
            {
                // 只在需要同步且操作未被取消时发送网络事件
                if (!ShouldSync(__instance) || __result != CancelCause.None)
                {
                    return; // Move 失败/被取消不广播
                }

                var payload = new
                {
                    Timestamp = DateTime.Now.Ticks,                  // 用时间戳帮助远端按顺序处理
                    EventType = NetworkMessageTypes.CardStateChanged,
                    PlayerId = GameStateUtils.GetCurrentPlayerId(), // 绑定到当前玩家
                    ChangeType = "ZoneChanged",                     // 标记为区域改变事件
                    FromZone = __state.FromZone.ToString(),         // 源区域字符串，便于跨语言解析
                    ToZone = dstZone.ToString(),                    // 目标区域字符串
                    CancelCause = __result.ToString(),              // 一般为 None，仅用于调试
                    CardBefore = __state.CardSnapshotBefore,        // 移动前快照
                    CardAfter = BuildCardSnapshot(card)             // 移动后快照
                };

                SendGameEvent(NetworkMessageTypes.CardStateChanged, payload); // 通知远端同步一次移动
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[BattleCardZoneSync] Error in MoveCard Postfix: {ex.Message}");
            }
        }
    }

    #endregion

    #region MoveCardToDrawZone 补丁

    /// <summary>
    /// Patch <see cref="BattleController.MoveCardToDrawZone"/>，用于同步卡牌进入抽牌堆（洗回牌库等）。
    /// </summary>
    [HarmonyPatch(typeof(BattleController), "MoveCardToDrawZone")]
    private static class MoveCardToDrawZonePatch
    {
        /// <summary>
        /// 在移动到抽牌堆前记录卡牌原区域、目标位置和快照。
        /// </summary>
        private struct MoveToDrawState
        {
            /// <summary>
            /// 卡牌原始区域。
            /// </summary>
            public CardZone FromZone; // 原始 Zone，例如 Hand/Discard

            /// <summary>
            /// 抽牌堆目标位置（顶/底等）。
            /// </summary>
            public DrawZoneTarget Target; // 放到牌库顶、底或随机位置

            /// <summary>
            /// 移动前的卡牌快照。
            /// </summary>
            public object CardSnapshotBefore; // 便于远端做顺序重建
        }

        /// <summary>
        /// MoveCardToDrawZone 调用前记录状态。
        /// </summary>
        [HarmonyPrefix]
        private static void Prefix(BattleController __instance, Card card, DrawZoneTarget target, out MoveToDrawState __state)
        {
            __state = default; // 默认清空，防止未同步时 Postfix 误读
            try
            {
                if (!ShouldSync(__instance))
                {
                    return; // 非本地战斗不做记录
                }

                // 记录原始区域和目标抽牌堆位置
                __state = new MoveToDrawState
                {
                    FromZone = card.Zone,
                    Target = target,
                    CardSnapshotBefore = BuildCardSnapshot(card)
                };
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[BattleCardZoneSync] Error in MoveCardToDrawZone Prefix: {ex.Message}");
            }
        }

        /// <summary>
        /// MoveCardToDrawZone 调用后，发送卡牌进入抽牌堆的同步事件。
        /// </summary>
        [HarmonyPostfix]
        private static void Postfix(BattleController __instance, Card card, DrawZoneTarget target, CancelCause __result, MoveToDrawState __state)
        {
            try
            {
                if (!ShouldSync(__instance) || __result != CancelCause.None)
                {
                    return; // 被取消或不是本地玩家战斗不广播
                }

                var payload = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    EventType = NetworkMessageTypes.CardStateChanged,
                    PlayerId = GameStateUtils.GetCurrentPlayerId(),
                    ChangeType = "ZoneChanged",                    // 仍视为区域变更
                    FromZone = __state.FromZone.ToString(),        // 从哪个 Zone 送回牌库
                    ToZone = CardZone.Draw.ToString(),             // 目标固定为 Draw
                    DrawTarget = __state.Target.ToString(),        // 抽牌堆放置位置
                    CancelCause = __result.ToString(),
                    CardBefore = __state.CardSnapshotBefore,
                    CardAfter = BuildCardSnapshot(card)
                };

                SendGameEvent(NetworkMessageTypes.CardStateChanged, payload);
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[BattleCardZoneSync] Error in MoveCardToDrawZone Postfix: {ex.Message}");
            }
        }
    }

    #endregion

    #region AddCardToDrawZone 补丁

    /// <summary>
    /// Patch <see cref="BattleController.AddCardToDrawZone"/>，用于同步新卡牌被直接加入抽牌堆的情况。
    /// </summary>
    [HarmonyPatch(typeof(BattleController), "AddCardToDrawZone")]
    private static class AddCardToDrawZonePatch
    {
        /// <summary>
        /// 在卡牌加入抽牌堆之后，向远端广播 CardAdded 事件。
        /// </summary>
        [HarmonyPostfix]
        private static void Postfix(BattleController __instance, Card card, DrawZoneTarget target, CancelCause __result)
        {
            try
            {
                if (!ShouldSync(__instance) || __result != CancelCause.None)
                {
                    return; // 失败或非本地战斗都不广播
                }

                var payload = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    EventType = NetworkMessageTypes.CardStateChanged,
                    PlayerId = GameStateUtils.GetCurrentPlayerId(),
                    ChangeType = "CardAdded",                     // 表示有新卡进入某个区域
                    ToZone = CardZone.Draw.ToString(),             // 新卡被加到牌库
                    DrawTarget = target.ToString(),                // 放到牌库的具体位置
                    CancelCause = __result.ToString(),
                    CardAfter = BuildCardSnapshot(card)            // 新生成/加入的卡快照
                };

                SendGameEvent(NetworkMessageTypes.CardStateChanged, payload);
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[BattleCardZoneSync] Error in AddCardToDrawZone Postfix: {ex.Message}");
            }
        }
    }

    #endregion

    #region AddCardToHand 补丁

    /// <summary>
    /// Patch <see cref="BattleController.AddCardToHand"/>，同步卡牌进入手牌的事件（抽牌/生成卡牌等）。
    /// </summary>
    [HarmonyPatch(typeof(BattleController), "AddCardToHand")]
    private static class AddCardToHandPatch
    {
        /// <summary>
        /// 在卡牌成功加入手牌后发送 CardAdded 同步事件。
        /// </summary>
        [HarmonyPostfix]
        private static void Postfix(BattleController __instance, Card card, CancelCause __result)
        {
            try
            {
                if (!ShouldSync(__instance) || __result != CancelCause.None)
                {
                    return; // 抽牌被 Cancel 等情况不需要同步
                }

                var payload = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    EventType = NetworkMessageTypes.CardStateChanged,
                    PlayerId = GameStateUtils.GetCurrentPlayerId(),
                    ChangeType = "CardAdded",                     // 新卡进入手牌
                    ToZone = CardZone.Hand.ToString(),             // 目标区域为 Hand
                    CancelCause = __result.ToString(),
                    CardAfter = BuildCardSnapshot(card)
                };

                SendGameEvent(NetworkMessageTypes.CardStateChanged, payload);
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[BattleCardZoneSync] Error in AddCardToHand Postfix: {ex.Message}");
            }
        }
    }

    #endregion

    #region AddCardToDiscard 补丁

    /// <summary>
    /// Patch <see cref="BattleController.AddCardToDiscard"/>，同步卡牌进入弃牌堆的事件。
    /// </summary>
    [HarmonyPatch(typeof(BattleController), "AddCardToDiscard")]
    private static class AddCardToDiscardPatch
    {
        /// <summary>
        /// 在卡牌被加入弃牌堆后发送 CardAdded 事件。
        /// </summary>
        [HarmonyPostfix]
        private static void Postfix(BattleController __instance, Card card, CancelCause __result)
        {
            try
            {
                if (!ShouldSync(__instance) || __result != CancelCause.None)
                {
                    return; // 例如卡牌被抵消，没有真正进入弃牌堆
                }

                var payload = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    EventType = NetworkMessageTypes.CardStateChanged,
                    PlayerId = GameStateUtils.GetCurrentPlayerId(),
                    ChangeType = "CardAdded",                     // 有牌进入 Discard
                    ToZone = CardZone.Discard.ToString(),          // 目标区域为 Discard
                    CancelCause = __result.ToString(),
                    CardAfter = BuildCardSnapshot(card)
                };

                SendGameEvent(NetworkMessageTypes.CardStateChanged, payload);
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[BattleCardZoneSync] Error in AddCardToDiscard Postfix: {ex.Message}");
            }
        }
    }

    #endregion

    #region AddCardToExile 补丁

    /// <summary>
    /// Patch <see cref="BattleController.AddCardToExile"/>，同步卡牌被放逐的事件。
    /// </summary>
    [HarmonyPatch(typeof(BattleController), "AddCardToExile")]
    private static class AddCardToExilePatch
    {
        /// <summary>
        /// 在卡牌被加入放逐区后发送 CardAdded 事件（ToZone 为 Exile）。
        /// </summary>
        [HarmonyPostfix]
        private static void Postfix(BattleController __instance, Card card, CancelCause __result)
        {
            try
            {
                if (!ShouldSync(__instance) || __result != CancelCause.None)
                {
                    return; // 放逐被取消则不广播
                }

                var payload = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    EventType = NetworkMessageTypes.CardStateChanged,
                    PlayerId = GameStateUtils.GetCurrentPlayerId(),
                    ChangeType = "CardAdded",                     // 卡牌进入放逐区
                    ToZone = CardZone.Exile.ToString(),            // 目标区域为 Exile
                    CancelCause = __result.ToString(),
                    CardAfter = BuildCardSnapshot(card)
                };

                SendGameEvent(NetworkMessageTypes.CardStateChanged, payload);
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[BattleCardZoneSync] Error in AddCardToExile Postfix: {ex.Message}");
            }
        }
    }

    #endregion

    #region RemoveCard 补丁

    /// <summary>
    /// Patch <see cref="BattleController.RemoveCard"/>，用于同步卡牌彻底离开当前战斗（死亡/离场等）。
    /// </summary>
    [HarmonyPatch(typeof(BattleController), "RemoveCard")]
    private static class RemoveCardPatch
    {
        /// <summary>
        /// RemoveCard 调用前的状态：记录卡牌来源区域及快照。
        /// </summary>
        private struct RemoveState
        {
            /// <summary>
            /// 卡牌被移除前所在的区域。
            /// </summary>
            public CardZone FromZone; // 从哪个 Zone 被彻底移除

            /// <summary>
            /// 移除前的卡牌快照。
            /// </summary>
            public object CardSnapshotBefore; // 记录最后一次在场内的状态
        }

        /// <summary>
        /// RemoveCard 调用前记录当前卡牌区域和快照，便于 Postfix 构造事件。
        /// </summary>
        [HarmonyPrefix]
        private static void Prefix(BattleController __instance, Card card, out RemoveState __state)
        {
            __state = default; // 初始化，防止未同步时 Postfix 访问未定义数据
            try
            {
                if (!ShouldSync(__instance))
                {
                    return; // 非本地战斗不记录
                }

                // 记录卡牌从哪个区域被移除，以及移除前的状态
                __state = new RemoveState
                {
                    FromZone = card.Zone,
                    CardSnapshotBefore = BuildCardSnapshot(card)
                };
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[BattleCardZoneSync] Error in RemoveCard Prefix: {ex.Message}");
            }
        }

        /// <summary>
        /// RemoveCard 调用后，发送卡牌离开战斗的同步事件。
        /// </summary>
        [HarmonyPostfix]
        private static void Postfix(BattleController __instance, Card card, RemoveState __state)
        {
            try
            {
                if (!ShouldSync(__instance))
                {
                    return; // 远端回放调用时不应广播
                }

                var payload = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    EventType = NetworkMessageTypes.CardStateChanged,
                    PlayerId = GameStateUtils.GetCurrentPlayerId(),
                    ChangeType = "CardRemovedFromBattle",      // 标记为战斗中彻底移除
                    FromZone = __state.FromZone.ToString(),      // 最后所在区域
                    ToZone = CardZone.None.ToString(),           // None 表示不再属于任何战斗区域
                    CardBefore = __state.CardSnapshotBefore,     // 移除前快照
                    CardAfter = BuildCardSnapshot(card)          // 移除后快照（通常状态变化不大，仅作补充）
                };

                SendGameEvent(NetworkMessageTypes.CardStateChanged, payload);
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[BattleCardZoneSync] Error in RemoveCard Postfix: {ex.Message}");
            }
        }
    }

    #endregion
}
