using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
using HarmonyLib;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;

namespace NetworkPlugin.Patch.Actions;

/// <summary>
/// 卡牌使用动作同步补丁类
/// 使用Harmony框架拦截LBoL游戏的卡牌使用流程
/// 实现卡牌使用操作的网络同步，这是游戏联机功能的核心同步点之一
/// </summary>
/// <remarks>
/// 这个类负责同步玩家使用卡牌的完整过程，包括：
/// 1. 卡牌开始使用时的状态同步
/// 2. 卡牌使用完成时的结果同步
/// 3. 卡牌执行阶段的进度同步
///
/// 卡牌同步是LBoL联机MOD最重要的功能，因为打牌是游戏的核心操作，
/// 需要确保所有玩家看到的卡牌使用过程和结果完全一致
/// </remarks>
public class PlayCardAction_Patch
{
    /// <summary>
    /// 服务提供者实例，用于获取依赖注入的网络客户端服务
    /// </summary>
    private static IServiceProvider serviceProvider => ModService.ServiceProvider;

    /// <summary>
    /// 卡牌开始使用事件同步补丁
    /// 在PlayCardAction.CardPlaying方法执行完成后被调用
    /// 同步玩家开始使用卡牌时的游戏状态，包括法力消耗、目标选择等信息
    /// </summary>
    /// <param name="args">卡牌使用事件参数，包含正在使用的卡牌和相关上下文信息</param>
    /// <remarks>
    /// 这个补丁在卡牌实际生效前触发，用于同步卡牌使用的初始状态
    /// 同步的信息包括：卡牌基本信息、法力消耗、目标选择、玩家当前状态等
    ///
    /// 发送的事件类型：NetworkMessageTypes.OnCardPlayStart
    /// </remarks>
    [HarmonyPatch(typeof(PlayCardAction), "CardPlaying")]
    [HarmonyPostfix]
    public static void PlayCardAction_CardPlaying_Postfix(CardUsingEventArgs args)
    {
        try
        {
            // 参数和依赖验证 - 确保必要的服务和参数可用
            if (serviceProvider == null || args == null)
            {
                Plugin.Logger?.LogDebug("[PlayCardSync] Service provider or args is null in CardPlaying");
                return;
            }

            // 获取并验证网络客户端
            var networkClient = serviceProvider.GetService<INetworkClient>();
            if (networkClient == null || !networkClient.IsConnected)
            {
                Plugin.Logger?.LogDebug("[PlayCardSync] Network client not available in CardPlaying");
                return;
            }

            // 获取并验证卡牌对象
            var card = args.Card;
            if (card == null)
            {
                Plugin.Logger?.LogDebug("[PlayCardSync] Card is null in CardPlaying");
                return;
            }

            // 只同步玩家使用的卡牌 - 过滤敌人使用的卡牌
            if (card.Zone?.Owner == null || !(card.Zone.Owner is PlayerUnit))
            {
                return; // 敌人卡牌不进行同步
            }

            // 获取玩家和战斗信息
            var player = card.Zone.Owner as PlayerUnit;
            var battle = player.Battle;

            // 构建详细的卡牌使用同步数据
            var cardData = new
            {
                Timestamp = DateTime.Now.Ticks,               // 事件时间戳
                CardId = card.Id,                             // 卡牌唯一ID
                CardName = card.Name,                         // 卡牌名称
                CardType = card.CardType.ToString(),          // 卡牌类型
                TargetType = card.Config?.TargetType.ToString() ?? "Unknown", // 目标类型
                ManaCost = GetManaCost(card),                 // 法力消耗
                Selector = args.Selector?.ToString() ?? "Nobody", // 目标选择器
                PlayerId = GetCurrentPlayerId(player),        // 玩家ID
                PlayerState = new                             // 玩家当前状态
                {
                    Hp = player.Hp,                           // 当前HP
                    MaxHp = player.MaxHp,                     // 最大HP
                    Block = player.Block,                     // 当前格挡值
                    Shield = player.Shield,                   // 当前护盾值
                    Mana = battle?.BattleMana != null ? GetManaGroup(battle.BattleMana) : [0, 0, 0, 0], // 法力值
                    CardsInHand = player.HandZone?.Count ?? 0, // 手牌数量
                    CardsInDraw = battle?.DrawZone?.Count ?? 0, // 牌库数量
                    CardsInDiscard = battle?.DiscardZone?.Count ?? 0, // 弃牌堆数量
                    IsCardUpgraded = card.IsUpgraded         // 卡牌是否升级
                }
            };

            // 发送卡牌开始使用事件到网络服务器
            SendGameEvent(NetworkMessageTypes.OnCardPlayStart, cardData);

            // 记录详细的卡牌使用开始日志
            Plugin.Logger?.LogInfo(
                $"[PlayCardSync] Player started playing card: {card.Name} " +
                $"(Mana: {JsonSerializer.Serialize(GetManaCost(card))}, " +
                $"Target: {cardData.Selector}, " +
                $"Player: {cardData.PlayerId})");
        }
        catch (Exception ex)
        {
            // 捕获并记录异常，防止补丁错误影响游戏
            Plugin.Logger?.LogError($"[PlayCardSync] Error in CardPlaying_Postfix: {ex.Message}\n{ex.StackTrace}");
        }
    }

    /// <summary>
    /// 卡牌使用完成事件同步补丁
    /// 在PlayCardAction.CardPlayed方法执行完成后被调用
    /// 同步卡牌使用完成后的最终结果，包括是否被取消、卡牌去向等状态
    /// </summary>
    /// <param name="args">卡牌使用事件参数，包含已使用的卡牌和执行结果</param>
    /// <remarks>
    /// 这个补丁在卡牌完全执行完毕后触发，用于同步卡牌使用的最终结果
    /// 同步的信息包括：卡牌执行结果、取消原因、最终位置、玩家状态变化等
    ///
    /// 发送的事件类型：NetworkMessageTypes.OnCardPlayComplete
    /// </remarks>
    [HarmonyPatch(typeof(PlayCardAction), "CardPlayed")]
    [HarmonyPostfix]
    public static void PlayCardAction_CardPlayed_Postfix(CardUsingEventArgs args)
    {
        try
        {
            // 参数和依赖验证
            if (serviceProvider == null || args == null)
            {
                Plugin.Logger?.LogDebug("[PlayCardSync] Service provider or args is null in CardPlayed");
                return;
            }

            // 验证网络客户端
            var networkClient = serviceProvider.GetService<INetworkClient>();
            if (networkClient == null || !networkClient.IsConnected)
            {
                Plugin.Logger?.LogDebug("[PlayCardSync] Network client not available in CardPlayed");
                return;
            }

            // 获取并验证卡牌对象
            var card = args.Card;
            if (card == null)
            {
                Plugin.Logger?.LogDebug("[PlayCardSync] Card is null in CardPlayed");
                return;
            }

            // 只同步玩家使用的卡牌
            if (card.Zone?.Owner == null || !(card.Zone.Owner is PlayerUnit))
            {
                return; // 敌人卡牌不进行同步
            }

            // 获取玩家和战斗信息
            var player = card.Zone.Owner as PlayerUnit;
            var battle = player.Battle;

            // 构建卡牌使用完成的详细同步数据
            var cardData = new
            {
                Timestamp = DateTime.Now.Ticks,                   // 事件时间戳
                CardId = card.Id,                                 // 卡牌唯一ID
                CardName = card.Name,                             // 卡牌名称
                CardType = card.CardType.ToString(),              // 卡牌类型
                IsCanceled = args.IsCanceled,                     // 是否被取消
                CancelCause = args.CancelCause?.ToString() ?? "None", // 取消原因
                PlayerId = GetCurrentPlayerId(player),            // 玩家ID
                PlayerState = new                                 // 玩家使用后的状态
                {
                    Hp = player.Hp,                               // 当前HP
                    MaxHp = player.MaxHp,                         // 最大HP
                    Block = player.Block,                         // 当前格挡值
                    Shield = player.Shield,                       // 当前护盾值
                    Mana = battle?.BattleMana != null ? GetManaGroup(battle.BattleMana) : [0, 0, 0, 0], // 法力值
                    CardsInHand = player.HandZone?.Count ?? 0,   // 手牌数量
                    CardsInDraw = battle?.DrawZone?.Count ?? 0,   // 牌库数量
                    CardsInDiscard = battle?.DiscardZone?.Count ?? 0, // 弃牌堆数量
                    CardsInExhaust = battle?.ExhaustZone?.Count ?? 0 // 放逐堆数量
                },
                CardZone = card.Zone?.ToString() ?? "Unknown",    // 卡牌最终位置
                CardEffectsApplied = GetCardEffectsApplied(args)  // 卡牌效果应用情况
            };

            // 发送卡牌使用完成事件到网络服务器
            SendGameEvent(NetworkMessageTypes.OnCardPlayComplete, cardData);

            // 记录详细的卡牌使用完成日志
            Plugin.Logger?.LogInfo(
                $"[PlayCardSync] Card play completed: {card.Name} " +
                $"(Canceled: {args.IsCanceled}, " +
                $"Zone: {card.Zone}, " +
                $"Player: {cardData.PlayerId})");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[PlayCardSync] Error in CardPlayed_Postfix: {ex.Message}\n{ex.StackTrace}");
        }
    }

    /// <summary>
    /// 卡牌执行阶段同步补丁
    /// 在PlayCardAction.GetPhases方法执行完成后被调用
    /// 同步卡牌即将执行的各个动作阶段，用于更细粒度的状态同步
    /// </summary>
    /// <param name="__instance">被补丁的PlayCardAction实例（Harmony自动注入）</param>
    /// <param name="__result">原始方法返回的阶段列表（Harmony自动注入）</param>
    /// <remarks>
    /// 这个补丁用于记录卡牌即将执行的具体动作阶段
    /// 不发送详细的同步数据，让具体的Action（如DamageAction）自己处理同步
    ///
    /// 发送的事件类型：CardPhasesStarted
    /// </remarks>
    [HarmonyPatch(typeof(PlayCardAction), "GetPhases")]
    [HarmonyPostfix]
    public static void PlayCardAction_GetPhases_Postfix(PlayCardAction __instance, ref IEnumerable<Phase> __result)
    {
        try
        {
            // 验证服务和参数
            if (serviceProvider == null || __instance?.Args?.Card == null)
            {
                Plugin.Logger?.LogDebug("[PlayCardSync] Service provider or instance args is null in GetPhases");
                return;
            }

            // 验证网络客户端
            var networkClient = serviceProvider.GetService<INetworkClient>();
            if (networkClient == null || !networkClient.IsConnected)
            {
                Plugin.Logger?.LogDebug("[PlayCardSync] Network client not available in GetPhases");
                return;
            }

            var card = __instance.Args.Card;
            if (card.Zone?.Owner == null || !(card.Zone.Owner is PlayerUnit))
            {
                return; // 只同步玩家卡牌
            }

            // 构建卡牌阶段开始的同步数据
            var phaseData = new
            {
                Timestamp = DateTime.Now.Ticks,                   // 事件时间戳
                EventType = "CardPhasesStarted",                 // 事件类型标识
                CardId = card.Id,                                 // 卡牌ID
                CardName = card.Name,                             // 卡牌名称
                PlayerId = GetCurrentPlayerId(card.Zone.Owner as PlayerUnit), // 玩家ID
                PhaseCount = __result?.Count() ?? 0              // 执行阶段数量
            };

            // 发送卡牌阶段开始事件
            SendGameEvent("CardPhasesStarted", phaseData);

            // 记录卡牌即将执行具体动作的调试信息
            // 这里不发送详细的同步数据，让具体的Action（如DamageAction）自己处理同步
            Plugin.Logger?.LogDebug(
                $"[PlayCardSync] GetPhases called for card: {card.Name} " +
                $"(Phases: {__result?.Count() ?? 0}, " +
                $"Player: {phaseData.PlayerId})");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[PlayCardSync] Error in GetPhases_Postfix: {ex.Message}\n{ex.StackTrace}");
        }
    }

    /*
    /// <summary>
    /// 抽牌动作同步补丁（已注释）
    /// 用于同步玩家的抽牌操作，确保牌库状态一致性
    /// </summary>
    /// <remarks>
    /// 当前版本中此功能被注释，因为LBoL的抽牌机制比较复杂
    /// 需要进一步研究游戏机制后再实现完整的抽牌同步
    ///
    /// 注意：DrawCardAction.Execute方法可能不存在或名称有变化
    /// </remarks>
    [HarmonyPatch(typeof(DrawCardAction), nameof(DrawCardAction.Execute))]
    [HarmonyPostfix]
    public static void DrawCardAction_Postfix(DrawCardAction __instance)
    {
        try
        {
            if (serviceProvider == null)
                return;

            var networkClient = serviceProvider.GetService<INetworkClient>();
            if (networkClient == null || !networkClient.IsConnected)
                return;

            var source = __instance.Source;
            var battle = source.Battle;
            if (battle == null || !(source is PlayerUnit))
                return;

            var drawData = new
            {
                DrawCount = __instance.Count,
                CardsInHand = battle.Player.HandZone.Count,
                CardsInDraw = battle.DrawZone.Count,
                CardsInDiscard = battle.DiscardZone.Count
            };

            var json = JsonSerializer.Serialize(drawData);
            networkClient.SendRequest("OnCardDraw", json);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[PlayCardSync] Error in DrawCardAction_Postfix: {ex.Message}");
        }
    }
    */

    // ========================================
    // 辅助方法 - 用于数据转换和事件发送
    // ========================================

    /// <summary>
    /// 发送游戏事件到网络服务器
    /// 支持NetworkClient的专用SendGameEvent方法和通用SendRequest方法
    /// </summary>
    /// <param name="eventType">事件类型标识符</param>
    /// <param name="eventData">事件数据对象</param>
    private static void SendGameEvent(string eventType, object eventData)
    {
        try
        {
            var networkClient = serviceProvider?.GetService<INetworkClient>();
            if (networkClient is NetworkClient liteNetClient)
            {
                // 优先使用专用的游戏事件发送方法
                liteNetClient.SendGameEvent(eventType, eventData);
            }
            else
            {
                // 备用方案：使用通用请求方法
                networkClient?.SendRequest(eventType, JsonSerializer.Serialize(eventData));
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[PlayCardSync] Error sending game event {eventType}: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取当前玩家的唯一标识符
    /// 用于网络同步中的玩家身份识别
    /// </summary>
    /// <param name="player">玩家单位实例</param>
    /// <returns>玩家ID字符串</returns>
    private static string GetCurrentPlayerId(PlayerUnit player)
    {
        try
        {
            // TODO: 从GameStateUtils获取统一的玩家ID或使用player.Id
            // 当前使用玩家索引作为临时标识
            return $"Player_{player.Index}";
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[PlayCardSync] Error getting current player ID: {ex.Message}");
            return "unknown_player"; // 错误时返回默认标识
        }
    }

    /// <summary>
    /// 将ManaGroup对象转换为整数数组
    /// 用于网络传输中的法力值序列化
    /// </summary>
    /// <param name="manaGroup">法力组对象</param>
    /// <returns>包含四种颜色法力值的数组 [红, 蓝, 绿, 白]</returns>
    private static int[] GetManaGroup(ManaGroup manaGroup)
    {
        if (manaGroup == null)
        {
            return [0, 0, 0, 0]; // 默认值：所有颜色法力为0
        }

        return
        [
            manaGroup.Red,    // 红色法力（火）
            manaGroup.Blue,   // 蓝色法力（水）
            manaGroup.Green,  // 绿色法力（木）
            manaGroup.White   // 白色法力（光）
        ];
    }

    /// <summary>
    /// 获取卡牌的法力消耗
    /// 调用GetManaGroup方法进行法力值转换
    /// </summary>
    /// <param name="card">卡牌对象</param>
    /// <returns>法力消耗数组</returns>
    private static int[] GetManaCost(Card card)
    {
        if (card == null || card.ManaGroup == null)
        {
            return [0, 0, 0, 0]; // 默认值：无消耗
        }

        return GetManaGroup(card.ManaGroup);
    }

    /// <summary>
    /// 分析卡牌应用的效果类型
    /// 用于同步卡牌执行后的效果摘要信息
    /// </summary>
    /// <param name="args">卡牌使用事件参数</param>
    /// <returns>包含效果类型信息的对象</returns>
    private static object GetCardEffectsApplied(CardUsingEventArgs args)
    {
        try
        {
            // TODO: 根据需要提取和分析卡牌应用的具体效果信息
            // 当前返回基础的效果类型框架，需要在后续版本中完善
            return new
            {
                HasDamage = false, // TODO: 检查卡牌是否包含伤害效果
                HasBuff = false,   // TODO: 检查卡牌是否包含增益效果
                HasHeal = false,   // TODO: 检查卡牌是否包含治疗效果
                HasDraw = false,   // TODO: 检查卡牌是否包含抽牌效果
                HasMana = false,   // TODO: 检查卡牌是否包含法力效果
                HasStatusEffect = false // TODO: 检查卡牌是否包含状态效果
            };
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[PlayCardSync] Error getting card effects: {ex.Message}");
            return new { Error = ex.Message }; // 错误时返回错误信息
        }
    }
}