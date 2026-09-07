using System;
using System.Collections.Generic;
using HarmonyLib;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Core;
using NetworkPlugin.Network.Services;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Event;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Network.NetworkPlayer;
using NetworkPlugin.Patch.Network;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Patch.Actions;

public class PlayCardAction_Patch
{
    #region 依赖注入

        private static IServiceProvider serviceProvider => ModService.ServiceProvider;

        private static ISynchronizationManager GetSyncManager()
    {
        try
        {
            Plugin.LogSynchronizationManagerResolveFromPatch(nameof(PlayCardAction_Patch), serviceProvider);
            return serviceProvider?.GetService<ISynchronizationManager>();
        }
        catch
        {
            return null;
        }
    }

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

    #region 构造函数补丁（出牌开始）

        [HarmonyPatch(typeof(PlayCardAction), MethodType.Constructor, typeof(Card))]
    [HarmonyPostfix]
    public static void Constructor1_Postfix(PlayCardAction __instance, Card card)
    {
        try
        {
            if (RemoteCardUsePatch.IsInRemoteCardPipeline)
            {
                return;
            }

            var syncManager = GetSyncManager();
            if (syncManager == null)
            {
                return;
            }

            var networkManager = GetNetworkManager();
            if (networkManager == null)
            {
                return;
            }

            INetworkPlayer player = networkManager.GetSelf();
            BattleController battle = card.Battle;

            if (player == null || battle == null)
            {
                return;
            }

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
                ["Mana"] = player.GetManaArraySafe(),

                ["CardsInHand"] = battle.HandZone?.Count ?? 0,
                ["CardsInDraw"] = battle.DrawZone?.Count ?? 0,
                ["CardsInDiscard"] = battle.DiscardZone?.Count ?? 0,

                ["IsCardUpgraded"] = card.IsUpgraded,
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
            Plugin.Logger?.LogError($"[PlayCardSync] Constructor1_Postfix 错误: {ex.Message}");
        }
    }

        [HarmonyPatch(typeof(PlayCardAction), MethodType.Constructor, typeof(Card), typeof(UnitSelector))]
    [HarmonyPostfix]
    public static void Constructor2_Postfix(PlayCardAction __instance, Card card, UnitSelector selector)
    {
        try
        {
            if (RemoteCardUsePatch.IsInRemoteCardPipeline)
            {
                return;
            }

            var syncManager = GetSyncManager();
            if (syncManager == null)
            {
                return;
            }

            var networkManager = GetNetworkManager();
            if (networkManager == null)
            {
                return;
            }

            INetworkPlayer player = networkManager.GetSelf();
            BattleController battle = card.Battle;

            if (player == null || battle == null)
            {
                return;
            }

            Dictionary<string, object> cardData = new Dictionary<string, object>
            {
                ["Timestamp"] = DateTime.Now.Ticks,
                ["CardId"] = card.Id,
                ["CardName"] = card.Name,
                ["CardType"] = card.CardType.ToString(),
                ["TargetType"] = card.Config?.TargetType.ToString() ?? "Unknown",
                ["Selector"] = selector?.ToString() ?? "Nobody",
                ["UserName"] = player.userName,

                ["PlayerState"] = new Dictionary<string, object>
                {
                    ["Hp"] = player.HP,
                    ["MaxHp"] = player.maxHP,
                    ["Block"] = player.block,
                    ["Shield"] = player.shield,
                    ["Mana"] = player.GetManaArraySafe(),
                    ["CardsInHand"] = battle.HandZone?.Count ?? 0,
                    ["CardsInDraw"] = battle.DrawZone?.Count ?? 0,
                    ["CardsInDiscard"] = battle.DiscardZone?.Count ?? 0,
                    ["IsCardUpgraded"] = card.IsUpgraded,
                },
            };

            var gameEvent = GameEventManager.CreateEvent(
                NetworkMessageTypes.OnCardPlayStart.ToString(),
                player.userName,
                cardData
            );

            syncManager.SendGameEvent(gameEvent);

            Plugin.Logger?.LogInfo($"[PlayCardSync] 卡牌开始: {card.Name} (玩家: {player.userName}, 目标: {selector})");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[PlayCardSync] Constructor2_Postfix 错误: {ex.Message}");
        }
    }

        [HarmonyPatch(typeof(PlayCardAction), MethodType.Constructor, typeof(Card), typeof(UnitSelector), typeof(ManaGroup))]
    [HarmonyPostfix]
    public static void Constructor3_Postfix(PlayCardAction __instance, Card card, UnitSelector selector, ManaGroup consumingMana)
    {
        try
        {
            if (RemoteCardUsePatch.IsInRemoteCardPipeline)
            {
                return;
            }

            var syncManager = GetSyncManager();
            if (syncManager == null)
            {
                return;
            }

            var networkManager = GetNetworkManager();
            if (networkManager == null)
            {
                return;
            }

            INetworkPlayer player = networkManager.GetSelf();
            BattleController battle = card.Battle;

            if (player == null || battle == null)
            {
                return;
            }

            Dictionary<string, object> cardData = new Dictionary<string, object>
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
                    ["Mana"] = player.GetManaArraySafe(),
                    ["CardsInHand"] = battle.HandZone?.Count ?? 0,
                    ["CardsInDraw"] = battle.DrawZone?.Count ?? 0,
                    ["CardsInDiscard"] = battle.DiscardZone?.Count ?? 0,
                    ["IsCardUpgraded"] = card.IsUpgraded,
                },
            };

            var gameEvent = GameEventManager.CreateEvent(
                NetworkMessageTypes.OnCardPlayStart.ToString(),
                player.userName,
                cardData
            );

            syncManager.SendGameEvent(gameEvent);

            Plugin.Logger?.LogInfo(
                $"[PlayCardSync] 卡牌开始: {card.Name} (玩家: {player.userName}, 目标: {selector}, 消耗法力: {consumingMana})");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[PlayCardSync] Constructor3_Postfix 错误: {ex.Message}");
        }
    }

    #endregion
}
