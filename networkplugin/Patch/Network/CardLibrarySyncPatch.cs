using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using LBoL.Core.Cards;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;

namespace NetworkPlugin.Patch.Network;

/// <summary>
/// 手牌/牌库同步补丁 - 同步所有卡牌相关状态
/// 参考杀戮尖塔联机Mod的CardGroupSyncPatches
/// 重要性: ⭐⭐⭐⭐⭐ (核心游戏机制)
/// </summary>
public class CardLibrarySyncPatch
{
    private static IServiceProvider serviceProvider => ModService.ServiceProvider;

    /// <summary>
    /// 抽牌事件同步
    /// </summary>
    [HarmonyPatch(typeof(DrawCardAction), nameof(DrawCardAction.Execute))]
    [HarmonyPrefix]
    public static void DrawCard_Prefix(DrawCardAction __instance, out int __state)
    {
        __state = 0;
        try
        {
            // 记录抽牌前的手牌数量
            __state = __instance.Unit?.Hand?.Count ?? 0;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[CardLibrarySync] Error in DrawCard_Prefix: {ex.Message}");
        }
    }

    [HarmonyPatch(typeof(DrawCardAction), nameof(DrawCardAction.Execute))]
    [HarmonyPostfix]
    public static void DrawCard_Postfix(DrawCardAction __instance, int __state)
    {
        try
        {
            if (serviceProvider == null)
            {
                return;
            }

            var networkClient = serviceProvider.GetService<INetworkClient>();
            if (networkClient == null || !networkClient.IsConnected)
            {
                return;
            }

            var cardsDrawn = (__instance.Unit?.Hand?.Count ?? 0) - __state;
            if (cardsDrawn <= 0)
            {
                return;
            }

            var drawData = new
            {
                Timestamp = DateTime.Now.Ticks,
                EventType = NetworkMessageTypes.OnCardDraw,
                PlayerId = GetCurrentPlayerId(),
                CardsDrawn = cardsDrawn,
                TargetUnitId = GetUnitId(__instance.Unit),
                HandSizeAfter = __instance.Unit?.Hand?.Count ?? 0,
                DeckSizeBefore = GetDeckSize(__instance.Unit),
                DrawFrom = GetDrawSource(__instance),
                DrawnCards = GetDrawnCardInfos(__instance)
            };

            SendGameEvent(NetworkMessageTypes.OnCardDraw, drawData);

            Plugin.Logger?.LogInfo($"[CardLibrarySync] Drew {cardsDrawn} cards, hand size: {__instance.Unit?.Hand?.Count}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[CardLibrarySync] Error in DrawCard_Postfix: {ex.Message}");
        }
    }

    /// <summary>
    /// 弃牌事件同步
    /// </summary>
    [HarmonyPatch]
    public static class DiscardCardSync
    {
        // LBoL可能有多种弃牌方式，需要找到所有相关方法
        [HarmonyTargetMethods]
        static System.Collections.Generic.IEnumerable<System.Reflection.MethodBase> TargetMethods()
        {
            // 常见的弃牌方法名模式
            var methodNames = new[]
            {
                "Discard",
                "DiscardHand",
                "DiscardToBottom",
                "DiscardSpecific",
                "Exile"
            };

            List<MethodBase> methods = [];

            // 搜索LBoL中包含弃牌逻辑的方法
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        foreach (var method in type.GetMethods())
                        {
                            if (methodNames.Any(name =>
                                method.Name.Contains(name) &&
                                method.GetParameters().Any(p => p.ParameterType == typeof(Card) ||
                                                               p.ParameterType == typeof(List<Card>))))
                            {
                                methods.Add(method);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // 忽略无法访问的程序集
                }
            }

            return methods;
        }

        [HarmonyPrefix]
        public static void Discard_Prefix(object __instance, out List<Card> __state, params object[] __args)
        {
            __state = [];
            try
            {
                // 从参数中提取要弃置的卡牌
                foreach (var arg in __args)
                {
                    if (arg is Card card)
                    {
                        __state.Add(card);
                    }
                    else if (arg is List<Card> cards)
                    {
                        __state.AddRange(cards);
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[CardLibrarySync] Error in Discard_Prefix: {ex.Message}");
            }
        }

        [HarmonyPostfix]
        public static void Discard_Postfix(object __instance, List<Card> __state, params object[] __args)
        {
            try
            {
                if (serviceProvider == null || __state.Count == 0)
                {
                    return;
                }

                var networkClient = serviceProvider.GetService<INetworkClient>();
                if (networkClient == null || !networkClient.IsConnected)
                {
                    return;
                }

                var discardData = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    EventType = NetworkMessageTypes.OnCardDiscard,
                    PlayerId = GetCurrentPlayerId(),
                    DiscardedCards = __state.Select(c => new
                    {
                        CardId = c.Id,
                        CardName = c.Name,
                        CardType = c.GetType().Name,
                        IsUpgraded = c.IsUpgraded,
                        Cost = c.GetManaCost()
                    }).ToList(),
                    DiscardType = "Unknown", // TODO: 根据具体方法确定弃牌类型
                    HandSizeAfter = GetCurrentHandSize(),
                    DiscardPileSizeAfter = GetDiscardPileSize()
                };

                SendGameEvent(NetworkMessageTypes.OnCardDiscard, discardData);

                Plugin.Logger?.LogInfo($"[CardLibrarySync] Discarded {__state.Count} cards");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[CardLibrarySync] Error in Discard_Postfix: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 卡组操作同步（抽牌、洗牌、搜索等）
    /// </summary>
    [HarmonyPatch]
    public static class DeckOperationsSync
    {
        [HarmonyTargetMethods]
        static System.Collections.Generic.IEnumerable<System.Reflection.MethodBase> TargetMethods()
        {
            var methodNames = new[]
            {
                "Shuffle",
                "TopDeck",
                "BottomDeck",
                "RandomCard",
                "SearchDeck",
                "ReorderDeck"
            };

            List<MethodBase> methods = [];

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        foreach (var method in type.GetMethods())
                        {
                            if (methodNames.Any(name => method.Name.Contains(name)) &&
                                method.DeclaringType?.Name.Contains("Deck") == true)
                            {
                                methods.Add(method);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // 忽略无法访问的程序集
                }
            }

            return methods;
        }

        [HarmonyPostfix]
        public static void DeckOperation_Postfix(object __instance, string __originalMethod)
        {
            try
            {
                if (serviceProvider == null)
                {
                    return;
                }

                var networkClient = serviceProvider.GetService<INetworkClient>();
                if (networkClient == null || !networkClient.IsConnected)
                {
                    return;
                }

                var operationType = DetermineOperationType(__originalMethod);
                var operationData = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    EventType = "DeckOperation",
                    PlayerId = GetCurrentPlayerId(),
                    OperationType = operationType,
                    DeckSize = GetDeckSize(),
                    ShuffleCount = GetShuffleCount(),
                    TopCard = GetTopCardInfo(),
                    BottomCard = GetBottomCardInfo()
                };

                SendGameEvent("DeckOperation", operationData);

                Plugin.Logger?.LogInfo($"[CardLibrarySync] Deck operation: {operationType}");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[CardLibrarySync] Error in DeckOperation_Postfix: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 卡牌状态变更同步（升级、变换等）
    /// </summary>
    [HarmonyPatch]
    public static class CardStateSync
    {
        [HarmonyTargetMethods]
        static System.Collections.Generic.IEnumerable<System.Reflection.MethodBase> TargetMethods()
        {
            var methodNames = new[]
            {
                "Upgrade",
                "Transform",
                "ChangeCost",
                "MakeTempCard",
                "MakeEthereal"
            };

            List<MethodBase> methods = [];

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        foreach (var method in type.GetMethods())
                        {
                            if (methodNames.Any(name => method.Name.Contains(name)))
                            {
                                methods.Add(method);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // 忽略无法访问的程序集
                }
            }

            return methods;
        }

        [HarmonyPrefix]
        public static void CardState_Prefix(Card __instance, out CardState __state, params object[] __args)
        {
            __state = new CardState
            {
                CardId = __instance.Id,
                CardName = __instance.Name,
                IsUpgraded = __instance.IsUpgraded,
                Cost = __instance.GetManaCost(),
                IsEthereal = __instance.IsEthereal,
                IsTemporary = __instance.IsTemporary
            };
        }

        [HarmonyPostfix]
        public static void CardState_Postfix(Card __instance, CardState __state, params object[] __args)
        {
            try
            {
                if (serviceProvider == null)
                {
                    return;
                }

                var networkClient = serviceProvider.GetService<INetworkClient>();
                if (networkClient == null || !networkClient.IsConnected)
                {
                    return;
                }

                var changeType = DetermineChangeType(__instance, __state);
                var stateChangeData = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    EventType = "CardStateChanged",
                    PlayerId = GetCurrentPlayerId(),
                    CardId = __instance.Id,
                    CardName = __instance.Name,
                    ChangeType = changeType,
                    PreviousState = __state,
                    CurrentState = new
                    {
                        CardId = __instance.Id,
                        CardName = __instance.Name,
                        IsUpgraded = __instance.IsUpgraded,
                        Cost = __instance.GetManaCost(),
                        IsEthereal = __instance.IsEthereal,
                        IsTemporary = __instance.IsTemporary
                    }
                };

                SendGameEvent("CardStateChanged", stateChangeData);

                Plugin.Logger?.LogInfo($"[CardLibrarySync] Card state changed: {changeType}");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[CardLibrarySync] Error in CardState_Postfix: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 完整牌库状态同步请求
    /// </summary>
    public static void RequestFullDeckSync()
    {
        try
        {
            if (serviceProvider == null)
            {
                return;
            }

            var networkClient = serviceProvider.GetService<INetworkClient>();
            if (networkClient == null || !networkClient.IsConnected)
            {
                return;
            }

            var syncRequest = new
            {
                Timestamp = DateTime.Now.Ticks,
                EventType = NetworkMessageTypes.StateSyncRequest,
                RequestType = "FullDeckSync",
                PlayerId = GetCurrentPlayerId(),
                RequestReason = "ManualRequest"
            };

            SendGameEvent(NetworkMessageTypes.StateSyncRequest, syncRequest);

            Plugin.Logger?.LogInfo("[CardLibrarySync] Full deck sync requested");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[CardLibrarySync] Error requesting full deck sync: {ex.Message}");
        }
    }

    /// <summary>
    /// 响应完整牌库状态同步
    /// </summary>
    public static void RespondToDeckSyncRequest(string requesterId)
    {
        try
        {
            var deckState = BuildFullDeckState();
            if (deckState == null || serviceProvider == null)
            {
                return;
            }

            var networkClient = serviceProvider.GetService<INetworkClient>();
            if (networkClient == null || !networkClient.IsConnected)
            {
                return;
            }

            var syncResponse = new
            {
                Timestamp = DateTime.Now.Ticks,
                EventType = NetworkMessageTypes.StateSyncResponse,
                RequesterId = requesterId,
                SyncType = "FullDeckSync",
                PlayerId = GetCurrentPlayerId(),
                DeckState = deckState
            };

            SendGameEvent(NetworkMessageTypes.StateSyncResponse, syncResponse);

            Plugin.Logger?.LogInfo($"[CardLibrarySync] Responded to deck sync request from {requesterId}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[CardLibrarySync] Error responding to deck sync request: {ex.Message}");
        }
    }

    // 辅助方法

    private static void SendGameEvent(string eventType, object eventData)
    {
        try
        {
            var networkClient = serviceProvider?.GetService<INetworkClient>();
            if (networkClient != null)
            {
                networkClient.SendGameEvent(eventType, eventData);
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[CardLibrarySync] Error sending game event {eventType}: {ex.Message}");
        }
    }

    private static string GetCurrentPlayerId()
    {
        try
        {
            // TODO: 从GameStateUtils获取
            return "current_player";
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[CardLibrarySync] Error getting current player ID: {ex.Message}");
            return "unknown_player";
        }
    }

    private static string GetUnitId(object unit)
    {
        try
        {
            if (unit == null)
            {
                return "null_unit";
            }
            // TODO: 获取单位的唯一标识符
            return unit.GetType().Name;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[CardLibrarySync] Error getting unit ID: {ex.Message}");
            return "unknown_unit";
        }
    }

    private static int GetDeckSize(object unit = null)
    {
        try
        {
            // TODO: 获取牌组大小
            return 0;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[CardLibrarySync] Error getting deck size: {ex.Message}");
            return 0;
        }
    }

    private static int GetCurrentHandSize()
    {
        try
        {
            // TODO: 获取当前手牌数量
            return 0;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[CardLibrarySync] Error getting hand size: {ex.Message}");
            return 0;
        }
    }

    private static int GetDiscardPileSize()
    {
        try
        {
            // TODO: 获取弃牌堆大小
            return 0;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[CardLibrarySync] Error getting discard pile size: {ex.Message}");
            return 0;
        }
    }

    private static string GetDrawSource(DrawCardAction drawAction)
    {
        try
        {
            // TODO: 确定抽牌来源（牌库、弃牌堆等）
            return "Deck";
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[CardLibrarySync] Error getting draw source: {ex.Message}");
            return "Unknown";
        }
    }

    private static object GetDrawnCardInfos(DrawCardAction drawAction)
    {
        try
        {
            // TODO: 获取抽到的卡牌信息
            return new List<object>();
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[CardLibrarySync] Error getting drawn card infos: {ex.Message}");
            return new List<object>();
        }
    }

    private static string DetermineOperationType(string methodName)
    {
        if (methodName.Contains("Shuffle"))
        {
            return "Shuffle";
        }

        if (methodName.Contains("TopDeck"))
        {
            return "TopDeck";
        }

        if (methodName.Contains("BottomDeck"))
        {
            return "BottomDeck";
        }

        if (methodName.Contains("Random"))
        {
            return "Random";
        }

        if (methodName.Contains("Search"))
        {
            return "Search";
        }

        if (methodName.Contains("Reorder"))
        {
            return "Reorder";
        }

        return "Unknown";
    }

    private static int GetShuffleCount()
    {
        try
        {
            // TODO: 获取洗牌次数
            return 0;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[CardLibrarySync] Error getting shuffle count: {ex.Message}");
            return 0;
        }
    }

    private static object GetTopCardInfo()
    {
        try
        {
            // TODO: 获取牌组顶卡牌信息
            return null;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[CardLibrarySync] Error getting top card info: {ex.Message}");
            return null;
        }
    }

    private static object GetBottomCardInfo()
    {
        try
        {
            // TODO: 获取牌组底卡牌信息
            return null;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[CardLibrarySync] Error getting bottom card info: {ex.Message}");
            return null;
        }
    }

    private static string DetermineChangeType(Card current, CardState previous)
    {
        if (current.IsUpgraded != previous.IsUpgraded)
        {
            return "Upgrade";
        }

        if (current.Id != previous.CardId)
        {
            return "Transform";
        }

        if (current.IsEthereal != previous.IsEthereal)
        {
            return "EtherealChange";
        }

        if (current.IsTemporary != previous.IsTemporary)
        {
            return "TemporaryChange";
        }

        return "Unknown";
    }

    private static object BuildFullDeckState()
    {
        try
        {
            // TODO: 构建完整的牌库状态数据
            return new
            {
                Hand = new List<object>(),
                Deck = new List<object>(),
                DiscardPile = new List<object>(),
                ExhaustPile = new List<object>(),
                HandSize = GetCurrentHandSize(),
                DeckSize = GetDeckSize(),
                DiscardPileSize = GetDiscardPileSize()
            };
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[CardLibrarySync] Error building full deck state: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 卡牌状态数据结构
    /// </summary>
    private class CardState
    {
        public string CardId { get; set; }
        public string CardName { get; set; }
        public bool IsUpgraded { get; set; }
        public object Cost { get; set; }
        public bool IsEthereal { get; set; }
        public bool IsTemporary { get; set; }
    }
}