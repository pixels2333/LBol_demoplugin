using System;
using System.Collections.Generic;
using System.Text.Json;
using HarmonyLib;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;

namespace NetworkPlugin.Patch.Actions;

/// <summary>
/// PlayCardAction同步补丁 - 同步卡牌使用操作
/// 这是最重要的同步点之一,因为打牌是游戏的核心操作
/// </summary>
public class PlayCardAction_Patch
{
    private static IServiceProvider serviceProvider => ModService.ServiceProvider;

    /// <summary>
    /// PlayCardAction - CardPlaying事件同步补丁
    /// 在卡牌开始使用时触发
    /// </summary>
    [HarmonyPatch(typeof(PlayCardAction), "CardPlaying")]
    [HarmonyPostfix]
    public static void PlayCardAction_CardPlaying_Postfix(CardUsingEventArgs args)
    {
        try
        {
            if (serviceProvider == null || args == null)
            {
                return;
            }

            var networkClient = serviceProvider.GetService<INetworkClient>();
            if (networkClient == null || !networkClient.IsConnected)
            {
                return;
            }

            var card = args.Card;
            if (card == null)
            {
                return;
            }

            // 只同步玩家使用的卡牌
            if (card.Zone?.Owner == null || !(card.Zone.Owner is PlayerUnit))
            {
                return;
            }

            var player = card.Zone.Owner as PlayerUnit;
            var battle = player.Battle;

            var cardData = new
            {
                Timestamp = DateTime.Now.Ticks,
                CardId = card.Id,
                CardName = card.Name,
                CardType = card.CardType.ToString(),
                TargetType = card.Config?.TargetType.ToString() ?? "Unknown",
                ManaCost = GetManaCost(card),
                Selector = args.Selector?.ToString() ?? "Nobody",
                PlayerId = GetCurrentPlayerId(player),
                PlayerState = new
                {
                    Hp = player.Hp,
                    MaxHp = player.MaxHp,
                    Block = player.Block,
                    Shield = player.Shield,
                    Mana = battle?.BattleMana != null ? GetManaGroup(battle.BattleMana) : [0, 0, 0, 0],
                    CardsInHand = player.HandZone?.Count ?? 0,
                    CardsInDraw = battle?.DrawZone?.Count ?? 0,
                    CardsInDiscard = battle?.DiscardZone?.Count ?? 0,
                    IsCardUpgraded = card.IsUpgraded
                }
            };

            SendGameEvent(NetworkMessageTypes.OnCardPlayStart, cardData);

            Plugin.Logger?.LogInfo($"[PlayCardAction_Patch] Player started playing card: {card.Name} (Mana: {JsonSerializer.Serialize(GetManaCost(card))})");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[PlayCardAction_Patch] Error in CardPlaying_Prefix: {ex.Message}");
        }
    }

    /// <summary>
    /// PlayCardAction - CardPlayed事件同步补丁
    /// 在卡牌使用完成后触发
    /// </summary>
    [HarmonyPatch(typeof(PlayCardAction), "CardPlayed")]
    [HarmonyPostfix]
    public static void PlayCardAction_CardPlayed_Postfix(CardUsingEventArgs args)
    {
        try
        {
            if (serviceProvider == null || args == null)
            {
                return;
            }

            var networkClient = serviceProvider.GetService<INetworkClient>();
            if (networkClient == null || !networkClient.IsConnected)
            {
                return;
            }

            var card = args.Card;
            if (card == null)
            {
                return;
            }

            // 只同步玩家使用的卡牌
            if (card.Zone?.Owner == null || !(card.Zone.Owner is PlayerUnit))
            {
                return;
            }

            var player = card.Zone.Owner as PlayerUnit;
            var battle = player.Battle;

            var cardData = new
            {
                Timestamp = DateTime.Now.Ticks,
                CardId = card.Id,
                CardName = card.Name,
                CardType = card.CardType.ToString(),
                IsCanceled = args.IsCanceled,
                CancelCause = args.CancelCause?.ToString() ?? "None",
                PlayerId = GetCurrentPlayerId(player),
                PlayerState = new
                {
                    Hp = player.Hp,
                    MaxHp = player.MaxHp,
                    Block = player.Block,
                    Shield = player.Shield,
                    Mana = battle?.BattleMana != null ? GetManaGroup(battle.BattleMana) : [0, 0, 0, 0],
                    CardsInHand = player.HandZone?.Count ?? 0,
                    CardsInDraw = battle?.DrawZone?.Count ?? 0,
                    CardsInDiscard = battle?.DiscardZone?.Count ?? 0,
                    CardsInExhaust = battle?.ExhaustZone?.Count ?? 0
                },
                CardZone = card.Zone?.ToString() ?? "Unknown",
                CardEffectsApplied = GetCardEffectsApplied(args)
            };

            SendGameEvent(NetworkMessageTypes.OnCardPlayComplete, cardData);

            Plugin.Logger?.LogInfo($"[PlayCardAction_Patch] Card play completed: {card.Name} (Canceled: {args.IsCanceled}, Zone: {card.Zone})");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[PlayCardAction_Patch] Error in CardPlayed_Postfix: {ex.Message}");
        }
    }

    /// <summary>
    /// Patch "CardPlay/CardAction" 阶段来同步具体的卡牌动作执行
    /// </summary>
    [HarmonyPatch(typeof(PlayCardAction), "GetPhases")]
    [HarmonyPostfix]
    public static void PlayCardAction_GetPhases_Postfix(PlayCardAction __instance, ref IEnumerable<Phase> __result)
    {
        try
        {
            if (serviceProvider == null || __instance?.Args?.Card == null)
            {
                return;
            }

            var networkClient = serviceProvider.GetService<INetworkClient>();
            if (networkClient == null || !networkClient.IsConnected)
            {
                return;
            }

            var card = __instance.Args.Card;
            if (card.Zone?.Owner == null || !(card.Zone.Owner is PlayerUnit))
            {
                return;
            }

            // 发送卡牌阶段开始事件
            var phaseData = new
            {
                Timestamp = DateTime.Now.Ticks,
                EventType = "CardPhasesStarted",
                CardId = card.Id,
                CardName = card.Name,
                PlayerId = GetCurrentPlayerId(card.Zone.Owner as PlayerUnit),
                PhaseCount = __result?.Count() ?? 0
            };

            SendGameEvent("CardPhasesStarted", phaseData);

            // 这里我们记录卡片即将执行具体动作
            // 但不发送同步数据，让具体的Action (如DamageAction) 自己处理同步
            Plugin.Logger?.LogDebug($"[PlayCardAction_Patch] GetPhases called for card: {card.Name} (Phases: {__result?.Count() ?? 0})");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[PlayCardAction_Patch] Error in GetPhases_Postfix: {ex.Message}");
        }
    }

    /*
    /// <summary>
    /// DrawCardAction同步 - 抽牌操作
    /// </summary>
    // [HarmonyPatch(typeof(DrawCardAction), nameof(DrawCardAction.Execute))] // Execute method doesn't exist
    // [HarmonyPostfix]
    // public static void DrawCardAction_Postfix(DrawCardAction __instance)
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
            Plugin.Logger?.LogError($"[PlayCardAction_Patch] Error in DrawCardAction_Postfix: {ex.Message}");
        }
    }
    */

    // 辅助方法

    private static void SendGameEvent(string eventType, object eventData)
    {
        try
        {
            var networkClient = serviceProvider?.GetService<INetworkClient>();
            if (networkClient is NetworkClient liteNetClient)
            {
                liteNetClient.SendGameEvent(eventType, eventData);
            }
            else
            {
                // 备用方案：使用通用SendRequest方法
                networkClient?.SendRequest(eventType, JsonSerializer.Serialize(eventData));
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[PlayCardAction_Patch] Error sending game event {eventType}: {ex.Message}");
        }
    }

    private static string GetCurrentPlayerId(PlayerUnit player)
    {
        try
        {
            // TODO: 从GameStateUtils获取或使用player.Id
            return $"Player_{player.Index}";
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[PlayCardAction_Patch] Error getting current player ID: {ex.Message}");
            return "unknown_player";
        }
    }

    private static int[] GetManaGroup(ManaGroup manaGroup)
    {
        if (manaGroup == null)
        {
            return [0, 0, 0, 0];
        }

        return
        [
            manaGroup.Red,
            manaGroup.Blue,
            manaGroup.Green,
            manaGroup.White
        ];
    }

    private static int[] GetManaCost(Card card)
    {
        if (card == null || card.ManaGroup == null)
        {
            return [0, 0, 0, 0];
        }

        return GetManaGroup(card.ManaGroup);
    }

    private static object GetCardEffectsApplied(CardUsingEventArgs args)
    {
        try
        {
            // TODO: 根据需要提取卡牌应用的效果信息
            return new
            {
                HasDamage = false, // TODO: 检查是否包含伤害
                HasBuff = false,   // TODO: 检查是否包含buff
                HasHeal = false,   // TODO: 检查是否包含治疗
                HasDraw = false    // TODO: 检查是否包含抽牌
            };
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[PlayCardAction_Patch] Error getting card effects: {ex.Message}");
            return new { Error = ex.Message };
        }
    }
}
