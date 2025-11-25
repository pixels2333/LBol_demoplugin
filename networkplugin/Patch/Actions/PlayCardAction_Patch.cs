using HarmonyLib;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using LBoL.Core;
using LBoL.Base;
using System;
using System.Text.Json;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace NetworkPlugin.Patch.Actions;

/// <summary>
/// PlayCardAction同步补丁 - 同步卡牌使用操作
/// 这是最重要的同步点之一,因为打牌是游戏的核心操作
/// </summary>
public class PlayCardAction_Patch
{
    private static IServiceProvider serviceProvider => ModService.ServiceProvider;

    /*
    /// <summary>
    /// PlayCardAction执行前的前缀补丁
    /// 用于记录卡牌使用的初始状态
    /// </summary>
    // [HarmonyPatch(typeof(PlayCardAction), nameof(PlayCardAction.Execute))] // Execute method doesn't exist
    // [HarmonyPrefix]
    // public static void PlayCardAction_Execute_Prefix(PlayCardAction __instance)
    {
        try
        {
            if (serviceProvider == null)
                return;

            var networkClient = serviceProvider.GetService<INetworkClient>();
            if (networkClient == null || !networkClient.IsConnected)
                return;

            // 获取卡牌和目标信息
            var card = __instance.Card;
            var source = __instance.Source;
            var target = __instance.Target;
            var battle = source.Battle;

            if (card == null || source == null || battle == null)
                return;

            // 只同步玩家出的牌
            if (!(source is PlayerUnit))
                return;

            var cardData = new
            {
                CardId = card.Id,
                CardName = card.Name,
                CardType = card.GetType().Name,
                ManaCost = GetManaCost(card),
                SourceId = source.Id,
                SourceType = source.GetType().Name,
                TargetId = target?.Id ?? "",
                TargetType = target?.GetType().Name ?? "",
                BeforeState = new
                {
                    Hp = source.Hp,
                    Block = source.Block,
                    Shield = source.Shield,
                    Mana = GetManaGroup(battle.BattleMana)
                }
            };

            Plugin.Logger?.LogInfo($"[PlayCardAction_Patch] Player playing card: {cardData.CardName} (ID: {cardData.CardId})");

            // 发送卡牌使用请求
            var json = JsonSerializer.Serialize(cardData);
            networkClient.SendRequest("OnCardPlayStart", json);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[PlayCardAction_Patch] Error in PlayCardAction_Execute_Prefix: {ex.Message}");
        }
    }
    */

    /*
    /// <summary>
    /// PlayCardAction执行后的后缀补丁
    /// 用于同步卡牌使用后的状态变化
    /// </summary>
    // [HarmonyPatch(typeof(PlayCardAction), nameof(PlayCardAction.Execute))] // Execute method doesn't exist
    // [HarmonyPostfix]
    // public static void PlayCardAction_Execute_Postfix(PlayCardAction __instance)
    {
        try
        {
            if (serviceProvider == null)
                return;

            var networkClient = serviceProvider.GetService<INetworkClient>();
            if (networkClient == null || !networkClient.IsConnected)
                return;

            var card = __instance.Card;
            var source = __instance.Source;
            var battle = source.Battle;

            if (card == null || source == null || battle == null)
                return;

            if (!(source is PlayerUnit))
                return;

            var cardData = new
            {
                CardId = card.Id,
                CardName = card.Name,
                AfterState = new
                {
                    Hp = source.Hp,
                    Block = source.Block,
                    Shield = source.Shield,
                    Mana = GetManaGroup(battle.BattleMana),
                    CardsInHand = battle.Player.HandZone.Count,
                    CardsInDraw = battle.DrawZone.Count
                }
            };

            var json = JsonSerializer.Serialize(cardData);
            networkClient.SendRequest("OnCardPlayComplete", json);

            Plugin.Logger?.LogInfo($"[PlayCardAction_Patch] Card play completed: {card.Name}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[PlayCardAction_Patch] Error in PlayCardAction_Execute_Postfix: {ex.Message}");
        }
    }
    */

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

    private static int[] GetManaGroup(ManaGroup manaGroup)
    {
        if (manaGroup == null)
            return new int[4] { 0, 0, 0, 0 };

        return new int[4]
        {
            manaGroup.Red,
            manaGroup.Blue,
            manaGroup.Green,
            manaGroup.White
        };
    }

    private static int[] GetManaCost(Card card)
    {
        if (card == null || card.ManaGroup == null)
            return new int[4] { 0, 0, 0, 0 };

        return GetManaGroup(card.ManaGroup);
    }
}
