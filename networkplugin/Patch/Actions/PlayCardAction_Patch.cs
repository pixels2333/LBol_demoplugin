using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using HarmonyLib;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Core;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Event;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Network.NetworkPlayer;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Patch.Actions;

/// <summary>
/// 卡牌使用动作同步补丁。
/// </summary>
/// <remarks>
/// 本补丁拦截 <see cref="PlayCardAction"/> 的构造流程，用于同步“开始出牌”的关键上下文，
/// 让其他客户端能按相同顺序复现出牌管线。
/// </remarks>
public class PlayCardAction_Patch
{
    #region 依赖注入

    /// <summary>
    /// 依赖注入服务提供者（用于解析网络相关服务）。
    /// </summary>
    private static IServiceProvider serviceProvider => ModService.ServiceProvider;

    /// <summary>
    /// 获取同步管理器。
    /// </summary>
    /// <returns>解析成功返回实例，否则返回 null。</returns>
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
    /// 获取网络管理器。
    /// </summary>
    /// <returns>解析成功返回实例，否则返回 null。</returns>
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

    /// <summary>
    /// 构造函数补丁(1)：<see cref="PlayCardAction"/> (Card card)。
    /// </summary>
    /// <param name="__instance">动作实例（Harmony 注入）。</param>
    /// <param name="card">即将使用的卡牌。</param>
    [HarmonyPatch(typeof(PlayCardAction), MethodType.Constructor, typeof(Card))]
    [HarmonyPostfix]
    public static void Constructor1_Postfix(PlayCardAction __instance, Card card)
    {
        try
        {
            // 解析同步管理器。
            var syncManager = GetSyncManager();
            if (syncManager == null)
            {
                return;
            }

            // 解析网络管理器。
            var networkManager = GetNetworkManager();
            if (networkManager == null)
            {
                return;
            }

            // 获取本地玩家与战斗上下文。
            INetworkPlayer player = networkManager.GetSelf();
            BattleController battle = card.Battle;

            // 构建“出牌开始”同步数据（尽量包含可复现所需的上下文）。
            Dictionary<string, object> cardData = new()
            {
                ["Timestamp"] = DateTime.Now.Ticks,
                ["CardId"] = card.Id,
                ["CardName"] = card.Name,
                ["CardType"] = card.CardType.ToString(),
                ["TargetType"] = card.Config?.TargetType.ToString() ?? "Unknown",
                ["Selector"] = UnitSelector.Nobody.ToString(),
                ["UserName"] = player.userName,

                // 玩家关键战斗状态（供远端对齐 UI/资源）。
                ["Hp"] = player.HP,
                ["MaxHp"] = player.maxHP,
                ["Block"] = player.block,
                ["Shield"] = player.shield,
                ["Mana"] = player.GetManaArraySafe(),

                // 牌库区域计数（便于远端校验一致性）。
                ["CardsInHand"] = battle.HandZone?.Count ?? 0,
                ["CardsInDraw"] = battle.DrawZone?.Count ?? 0,
                ["CardsInDiscard"] = battle.DiscardZone?.Count ?? 0,

                // 卡牌升级状态。
                ["IsCardUpgraded"] = card.IsUpgraded,
            };

            // 组装事件并发送。
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

    /// <summary>
    /// 构造函数补丁(2)：<see cref="PlayCardAction"/> (Card card, UnitSelector selector)。
    /// </summary>
    /// <param name="__instance">动作实例（Harmony 注入）。</param>
    /// <param name="card">即将使用的卡牌。</param>
    /// <param name="selector">选择器/目标信息。</param>
    [HarmonyPatch(typeof(PlayCardAction), MethodType.Constructor, typeof(Card), typeof(UnitSelector))]
    [HarmonyPostfix]
    public static void Constructor2_Postfix(PlayCardAction __instance, Card card, UnitSelector selector)
    {
        try
        {
            // 解析同步管理器。
            var syncManager = GetSyncManager();
            if (syncManager == null)
            {
                return;
            }

            // 解析网络管理器。
            var networkManager = GetNetworkManager();
            if (networkManager == null)
            {
                return;
            }

            // 获取本地玩家与战斗上下文。
            INetworkPlayer player = networkManager.GetSelf();
            BattleController battle = card.Battle;

            if (player == null || battle == null)
            {
                return;
            }

            // 构建“出牌开始”同步数据。
            var cardData = new Dictionary<string, object>
            {
                ["Timestamp"] = DateTime.Now.Ticks,
                ["CardId"] = card.Id,
                ["CardName"] = card.Name,
                ["CardType"] = card.CardType.ToString(),
                ["TargetType"] = card.Config?.TargetType.ToString() ?? "Unknown",
                ["Selector"] = selector?.ToString() ?? "Nobody",
                ["UserName"] = player.userName,

                // 将玩家状态归到子对象，结构更清晰。
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

            // 组装事件并发送。
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

    /// <summary>
    /// 构造函数补丁(3)：<see cref="PlayCardAction"/> (Card card, UnitSelector selector, ManaGroup consumingMana)。
    /// </summary>
    /// <param name="__instance">动作实例（Harmony 注入）。</param>
    /// <param name="card">即将使用的卡牌。</param>
    /// <param name="selector">选择器/目标信息。</param>
    /// <param name="consumingMana">本次消耗的法力。</param>
    [HarmonyPatch(typeof(PlayCardAction), MethodType.Constructor, typeof(Card), typeof(UnitSelector), typeof(ManaGroup))]
    [HarmonyPostfix]
    public static void Constructor3_Postfix(PlayCardAction __instance, Card card, UnitSelector selector, ManaGroup consumingMana)
    {
        try
        {
            // 解析同步管理器。
            var syncManager = GetSyncManager();
            if (syncManager == null)
            {
                return;
            }

            // 解析网络管理器。
            var networkManager = GetNetworkManager();
            if (networkManager == null)
            {
                return;
            }

            // 获取本地玩家与战斗上下文。
            INetworkPlayer player = networkManager.GetSelf();
            BattleController battle = card.Battle;

            if (player == null || battle == null)
            {
                return;
            }

            // 构建“出牌开始”同步数据（包含消耗法力）。
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
                    ["Mana"] = player.GetManaArraySafe(),
                    ["CardsInHand"] = battle.HandZone?.Count ?? 0,
                    ["CardsInDraw"] = battle.DrawZone?.Count ?? 0,
                    ["CardsInDiscard"] = battle.DiscardZone?.Count ?? 0,
                    ["IsCardUpgraded"] = card.IsUpgraded,
                },
            };

            // 组装事件并发送。
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
