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
using NetworkPlugin.Core;
using NetworkPlugin.Network.Event;
using NetworkPlugin.Network.NetworkPlayer;

namespace NetworkPlugin.Patch.Actions;

/// <summary>
/// 卡牌使用动作同步补丁类
/// 使用Harmony框架拦截LBoL游戏的卡牌使用流程
/// 实现卡牌使用操作的网络同步，这是游戏联机功能的核心同步点之一
/// </summary>
/// <remarks>
/// 这个类负责同步玩家使用卡牌的完整过程，包括：
/// 1. 卡牌开始使用时的状态同步（CardPlaying事件）
/// 2. 卡牌使用过程中的阶段同步（GetPhases中的各个阶段）
/// 3. 卡牌使用完成时的结果同步（CardPlayed事件）
///
/// 这是LBoL联机MOD最重要的功能，因为打牌是游戏的核心操作，
/// 需要确保所有玩家看到的卡牌使用过程和结果完全一致
/// </remarks>
public class PlayCardAction_Patch
{
    /// <summary>
    /// 服务提供者实例，用于获取依赖注入的网络客户端服务
    /// </summary>
    private static IServiceProvider serviceProvider => ModService.ServiceProvider;


    // ========================================
    // 构造函数补丁
    // ========================================

    /// <summary>
    /// PlayCardAction构造函数(1)同步补丁
    /// 拦截：PlayCardAction(Card card)
    /// </summary>
    [HarmonyPatch(typeof(PlayCardAction), MethodType.Constructor, typeof(Card))]
    [HarmonyPostfix]
    public static void Constructor1_Postfix(PlayCardAction __instance, Card card)
    {
        try
        {

            var syncManager = GetSyncManager();
            if (syncManager == null)
                return;
            var networkManager = GetNetworkManager();
            if (networkManager == null)
                return;

            INetworkPlayer player = networkManager.GetSelf();
            BattleController battle = card.Battle;

            // 构建卡牌开始使用的同步数据
            Dictionary<string, object> cardData = new()
            {
                ["Timestamp"] = DateTime.Now.Ticks,
                ["CardId"] = card.Id,
                ["CardName"] = card.Name,
                ["CardType"] = card.CardType.ToString(),
                ["TargetType"] = card.Config?.TargetType.ToString() ?? "Unknown",
                ["Selector"] = UnitSelector.Nobody.ToString(),
                ["UserName"] = player.userName,

                ["Hp"] = player.HP,
                ["MaxHp"] = player.maxHP,
                ["Block"] = player.block,
                ["Shield"] = player.shield,
                ["Mana"] = player.mana,
                ["CardsInHand"] = battle.HandZone?.Count ?? 0,
                ["CardsInDraw"] = battle.DrawZone?.Count ?? 0,
                ["CardsInDiscard"] = battle.DiscardZone?.Count ?? 0,
                ["IsCardUpgraded"] = card.IsUpgraded

            };

            var gameEvent = GameEventManager.CreateEvent(
                NetworkMessageTypes.OnCardPlayStart.ToString(),
                player.userName,
                cardData
            );

            syncManager.SendGameEvent(gameEvent);

            Plugin.Logger?.LogInfo($"[PlayCardSync] 卡牌开始: {card.Name} (玩家: {player.userName})");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[PlayCardSync] Constructor1_Postfix错误: {ex.Message}");
        }
    }

    /// <summary>
    /// PlayCardAction构造函数(2)同步补丁
    /// 拦截：PlayCardAction(Card card, UnitSelector selector)
    /// </summary>
    [HarmonyPatch(typeof(PlayCardAction), MethodType.Constructor, typeof(Card), typeof(UnitSelector))]
    [HarmonyPostfix]
    public static void Constructor2_Postfix(PlayCardAction __instance, Card card, UnitSelector selector)
    {
        try
        {

            var syncManager = GetSyncManager();
            if (syncManager == null)
                return;
            var networkManager = GetNetworkManager();
            if (networkManager == null)
                return;

            INetworkPlayer player = networkManager.GetSelf();
            BattleController battle = card.Battle;

            if (player == null || battle == null)
                return;

            // 构建卡牌开始使用的同步数据
            var cardData = new Dictionary<string, object>
            {
                ["Timestamp"] = DateTime.Now.Ticks,
                ["CardId"] = card.Id,
                ["CardName"] = card.Name,
                ["CardType"] = card.CardType.ToString(),
                ["TargetType"] = card.Config?.TargetType.ToString() ?? "Unknown",
                // ["ManaCost"] = GetManaCost(card),
                ["Selector"] = selector?.ToString() ?? "Nobody",
                ["UserName"] = player.userName,
                ["PlayerState"] = new Dictionary<string, object>
                {
                    ["Hp"] = player.HP,
                    ["MaxHp"] = player.maxHP,
                    ["Block"] = player.block,
                    ["Shield"] = player.shield,
                    ["Mana"] = player.mana,
                    ["CardsInHand"] = battle.HandZone?.Count ?? 0,
                    ["CardsInDraw"] = battle.DrawZone?.Count ?? 0,
                    ["CardsInDiscard"] = battle.DiscardZone?.Count ?? 0,
                    ["IsCardUpgraded"] = card.IsUpgraded
                }
            };

            var gameEvent = GameEventManager.CreateEvent(
                NetworkMessageTypes.OnCardPlayStart.ToString(), player.userName, cardData
            );

            syncManager.SendGameEvent(gameEvent);

            Plugin.Logger?.LogInfo($"[PlayCardSync] 卡牌开始: {card.Name} (玩家: {player.userName}, 目标: {selector})");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[PlayCardSync] Constructor2_Postfix错误: {ex.Message}");
        }
    }

    /// <summary>
    /// PlayCardAction构造函数(3)同步补丁
    /// 拦截：PlayCardAction(Card card, UnitSelector selector, ManaGroup consumingMana)
    /// </summary>
    [HarmonyPatch(typeof(PlayCardAction), MethodType.Constructor, typeof(Card), typeof(UnitSelector), typeof(ManaGroup))]
    [HarmonyPostfix]
    public static void Constructor3_Postfix(PlayCardAction __instance, Card card, UnitSelector selector, ManaGroup consumingMana)
    {
        try
        {
            var syncManager = GetSyncManager();
            if (syncManager == null)
                return;

            var networkManager = GetNetworkManager();
            if (networkManager == null)
                return;

            INetworkPlayer player = networkManager.GetSelf();
            BattleController battle = card.Battle;

            if (player == null || battle == null)
                return;

            // 构建卡牌开始使用的同步数据
            var cardData = new Dictionary<string, object>
            {
                ["Timestamp"] = DateTime.Now.Ticks,
                ["CardId"] = card.Id,
                ["CardName"] = card.Name,
                ["CardType"] = card.CardType.ToString(),
                ["TargetType"] = card.Config?.TargetType.ToString() ?? "Unknown",
                ["ConsumingMana"] = consumingMana.ToString() ?? "None",
                ["Selector"] = selector?.ToString() ?? "Nobody",
                ["UserName"] = player.userName,
                ["PlayerState"] = new Dictionary<string, object>
                {
                    ["Hp"] = player.HP,
                    ["MaxHp"] = player.maxHP,
                    ["Block"] = player.block,
                    ["Shield"] = player.shield,
                    ["Mana"] = player.mana,
                    ["CardsInHand"] = battle.HandZone?.Count ?? 0,
                    ["CardsInDraw"] = battle.DrawZone?.Count ?? 0,
                    ["CardsInDiscard"] = battle.DiscardZone?.Count ?? 0,
                    ["IsCardUpgraded"] = card.IsUpgraded
                }
            };

            var gameEvent = GameEventManager.CreateEvent(
                NetworkMessageTypes.OnCardPlayStart.ToString(),
                player.userName,
                cardData
            );

            syncManager.SendGameEvent(gameEvent);

            Plugin.Logger?.LogInfo($"[PlayCardSync] 卡牌开始: {card.Name} (玩家: {player.userName}, 目标: {selector}, 消耗法力: {consumingMana})");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[PlayCardSync] Constructor3_Postfix错误: {ex.Message}");
        }

    }




    #region 依赖注入管理

    /// <summary>
    /// 获取同步管理器
    /// </summary>
    private static ISynchronizationManager GetSyncManager()
    {
        try
        {
            return serviceProvider?.GetService<ISynchronizationManager>();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 获取网络管理器
    /// </summary>
    private static INetworkManager GetNetworkManager()
    {
        try
        {
            return serviceProvider?.GetService<INetworkManager>();
        }
        catch
        {
            return null;
        }
    }

    #endregion

}