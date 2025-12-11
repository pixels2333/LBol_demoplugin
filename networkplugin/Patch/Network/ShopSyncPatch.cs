using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.Stations;
using LBoL.Core.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;

namespace NetworkPlugin.Patch.Network;

/// <summary>
/// 商店同步补丁 - 同步商店购买操作
/// 参考杀戮尖塔Together in Spire商店同步机制
/// 重要性: ⭐⭐⭐⭐⭐ (核心资源管理)
/// </summary>
public class ShopSyncPatch
{
    private static IServiceProvider serviceProvider => ModService.ServiceProvider;

    /// <summary>
    /// 商店初始化同步 - 当玩家进入商店时同步商店库存
    /// </summary>
    [HarmonyPatch(typeof(ShopStation), "OnEnter")]
    [HarmonyPostfix]
    public static void ShopEnter_Postfix(ShopStation __instance)
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

            // 在合作模式下，广播商店进入事件
            var shopData = new
            {
                Timestamp = DateTime.Now.Ticks,
                EventType = NetworkMessageTypes.OnShopEnter,
                ShopId = __instance.GetHashCode().ToString(),
                Location = new { __instance.Location.X, __instance.Location.Y },
                ShopType = __instance.GetType().Name,
                Inventory = BuildShopInventoryData(__instance),
                PlayerId = GetCurrentPlayerId(),
                PlayerGold = __instance.GameRun?.Money ?? 0
            };

            SendGameEvent(NetworkMessageTypes.OnShopEnter, shopData);

            Plugin.Logger?.LogInfo($"[ShopSync] Entered shop at ({__instance.Location.X}, {__instance.Location.Y}) with {(__instance.ShopCards?.Count ?? 0)} cards and {(__instance.ShopExhibits?.Count ?? 0)} exhibits");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[ShopSync] Error in ShopEnter: {ex.Message}");
        }
    }

    /// <summary>
    /// 商店离开同步
    /// </summary>
    [HarmonyPatch(typeof(ShopStation), "OnExit")]
    [HarmonyPostfix]
    public static void ShopExit_Postfix(ShopStation __instance)
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

            var shopData = new
            {
                Timestamp = DateTime.Now.Ticks,
                EventType = NetworkMessageTypes.OnShopExit,
                ShopId = __instance.GetHashCode().ToString(),
                PlayerId = GetCurrentPlayerId(),
                FinalGold = __instance.GameRun?.Money ?? 0,
                TotalPurchases = CalculateTotalPurchases(__instance)
            };

            SendGameEvent(NetworkMessageTypes.OnShopExit, shopData);

            Plugin.Logger?.LogInfo($"[ShopSync] Exited shop");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[ShopSync] Error in ShopExit: {ex.Message}");
        }
    }

    /// <summary>
    /// 购买卡牌同步
    /// </summary>
    [HarmonyPatch(typeof(ShopStation), "BuyCard")]
    [HarmonyPrefix]
    public static void BuyCard_Prefix(ShopStation __instance, ShopItem<Card> cardItem, out ShopItem<Card> __state)
    {
        __state = null;
        try
        {
            // 记录购买前的状态
            __state = new ShopItem<Card>(
                __instance.GameRun,
                cardItem.Content,
                cardItem.Price,
                cardItem.IsSoldOut,
                cardItem.IsDiscounted
            );
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[ShopSync] Error in BuyCard_Prefix: {ex.Message}");
        }
    }

    [HarmonyPatch(typeof(ShopStation), "BuyCard")]
    [HarmonyPostfix]
    public static void BuyCard_Postfix(ShopStation __instance, ShopItem<Card> cardItem, ShopItem<Card> __state)
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

            // 检查是否真的购买了（状态从未售出变为已售出）
            if (__state?.IsSoldOut == false && cardItem.IsSoldOut)
            {
                var purchaseData = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    EventType = NetworkMessageTypes.OnShopPurchase,
                    ShopId = __instance.GetHashCode().ToString(),
                    ItemType = "Card",
                    ItemId = cardItem.Content.Id,
                    ItemName = cardItem.Name,
                    CardType = cardItem.Content.GetType().Name,
                    Rarity = cardItem.Content.Config?.Rarity?.ToString() ?? "Unknown",
                    Price = __state.Price,
                    WasDiscounted = __state.IsDiscounted,
                    PlayerGoldBefore = __instance.GameRun.Money + __state.Price,
                    PlayerGoldAfter = __instance.GameRun.Money,
                    CardCountBefore = GetCardCount(__instance.GameRun),
                    CardCountAfter = GetCardCount(__instance.GameRun)
                };

                SendGameEvent(NetworkMessageTypes.OnShopPurchase, purchaseData);

                Plugin.Logger?.LogInfo($"[ShopSync] Player bought card: {cardItem.Name} for {__state.Price} gold (Discount: {__state.IsDiscounted})");
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[ShopSync] Error in BuyCard_Postfix: {ex.Message}");
        }
    }

    /// <summary>
    /// 购买宝物同步
    /// </summary>
    [HarmonyPatch(typeof(GameRunController), "BuyExhibitRunner")]
    public static class BuyExhibitSync
    {
        // BuyExhibitRunner是IEnumerator方法，需要使用不同的Patch方式

        /// <summary>
        /// 购买宝物前记录状态
        /// </summary>
        [HarmonyPatch(typeof(GameRunController), "BuyExhibitRunner")]
        [HarmonyPrefix]
        public static void BuyExhibit_Prefix(GameRunController __instance, ShopItem<Exhibit> exhibitItem, out int __state)
        {
            __state = 0;
            try
            {
                // 记录购买前的金币数量
                __state = __instance.Money;

                // 验证购买权限
                var authorityManager = serviceProvider?.GetService<NetworkPlugin.Authority.HostAuthorityManager>();
                if (authorityManager != null)
                {
                    // TODO: 调用权威管理器验证购买权限
                    // if (!authorityManager.ValidateShopPurchase(__instance.Player, exhibitItem.Price))
                    // {
                    //     // 阻止购买
                    // }
                }

                // 发送购买开始事件
                var startData = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    EventType = "ShopPurchaseStarted",
                    ItemId = exhibitItem.Content.Id,
                    ItemName = exhibitItem.Content.Name,
                    Price = exhibitItem.Price,
                    PlayerGold = __instance.Money
                };

                var networkClient = serviceProvider?.GetService<INetworkClient>();
                networkClient?.SendGameEventData("ShopPurchaseStarted", startData);
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[ShopSync] Error in BuyExhibit_Prefix: {ex.Message}");
            }
        }

        /// <summary>
        /// 购买宝物后发送完成事件
        /// </summary>
        [HarmonyPatch(typeof(GameRunController), "BuyExhibitRunner")]
        [HarmonyPostfix]
        public static void BuyExhibit_Postfix(GameRunController __instance, ShopItem<Exhibit> exhibitItem, int __state)
        {
            try
            {
                if (serviceProvider == null)
                {
                    return;
                }

                var networkClient = serviceProvider?.GetService<INetworkClient>();
                if (networkClient == null || !networkClient.IsConnected)
                {
                    return;
                }

                // 检查是否真的购买了（金币减少了）
                bool purchaseSuccess = __instance.Money < __state;

                if (purchaseSuccess)
                {
                    var purchaseData = new
                    {
                        Timestamp = DateTime.Now.Ticks,
                        EventType = NetworkMessageTypes.OnShopPurchase,
                        ItemType = "Exhibit",
                        ItemId = exhibitItem.Content.Id,
                        ItemName = exhibitItem.Content.Name,
                        ExhibitType = exhibitItem.Content.GetType().Name,
                        Rarity = exhibitItem.Content.Config?.Rarity?.ToString() ?? "Unknown",
                        Price = __state - __instance.Money,
                        PlayerGoldBefore = __state,
                        PlayerGoldAfter = __instance.Money,
                        ExhibitCountBefore = GetExhibitCount(__instance.Player),
                        ExhibitCountAfter = GetExhibitCount(__instance.Player)
                    };

                    SendGameEvent(NetworkMessageTypes.OnShopPurchase, purchaseData);

                    Plugin.Logger?.LogInfo($"[ShopSync] Player bought exhibit: {exhibitItem.Name} for {__state - __instance.Money} gold");
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[ShopSync] Error in BuyExhibit_Postfix: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 升级卡组卡牌同步
    /// </summary>
    [HarmonyPatch(typeof(ShopStation), "UpgradeDeckCard")]
    [HarmonyPostfix]
    public static void UpgradeDeckCard_Postfix(ShopStation __instance, Card card)
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

            var upgradeData = new
            {
                Timestamp = DateTime.Now.Ticks,
                EventType = "ShopUpgradeCard",
                ShopId = __instance.GetHashCode().ToString(),
                CardId = card.Id,
                CardName = card.Name,
                CardType = card.GetType().Name,
                UpgradePrice = __instance.UpgradeDeckCardPrice,
                IsUpgraded = card.IsUpgraded,
                PlayerGold = __instance.GameRun?.Money ?? 0
            };

            // 升级操作可能不需要强制同步，但发送通知
            SendGameEvent("ShopUpgradeCard", upgradeData);

            Plugin.Logger?.LogInfo($"[ShopSync] Player upgraded card: {card.Name} for {__instance.UpgradeDeckCardPrice} gold");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[ShopSync] Error in UpgradeDeckCard: {ex.Message}");
        }
    }

    /// <summary>
    /// 移除卡组卡牌同步
    /// </summary>
    [HarmonyPatch(typeof(ShopStation), "RemoveDeckCard")]
    [HarmonyPostfix]
    public static void RemoveDeckCard_Postfix(ShopStation __instance, Card card)
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

            var removeData = new
            {
                Timestamp = DateTime.Now.Ticks,
                EventType = "ShopRemoveCard",
                ShopId = __instance.GetHashCode().ToString(),
                CardId = card.Id,
                CardName = card.Name,
                CardType = card.GetType().Name,
                RemovePrice = __instance.RemoveDeckCardPrice,
                PlayerGold = __instance.GameRun?.Money ?? 0
            };

            SendGameEvent("ShopRemoveCard", removeData);

            Plugin.Logger?.LogInfo($"[ShopSync] Player removed card: {card.Name} for {__instance.RemoveDeckCardPrice} gold");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[ShopSync] Error in RemoveDeckCard: {ex.Message}");
        }
    }

    /// <summary>
    /// 商店状态变化同步（如物品状态变更）
    /// </summary>
    [HarmonyPatch(typeof(ShopStation), "UpdateShopState")]
    [HarmonyPostfix]
    public static void UpdateShopState_Postfix(ShopStation __instance)
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

            var stateData = new
            {
                Timestamp = DateTime.Now.Ticks,
                EventType = "ShopStateUpdate",
                ShopId = __instance.GetHashCode().ToString(),
                PlayerGold = __instance.GameRun?.Money ?? 0,
                AvailableItems = BuildShopInventoryData(__instance)
            };

            SendGameEvent("ShopStateUpdate", stateData);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[ShopSync] Error in UpdateShopState: {ex.Message}");
        }
    }

    /// <summary>
    /// 商店库存同步请求
    /// 请求其他玩家的商店状态（用于显示队友商店）
    /// </summary>
    public static void RequestShopInventory(string targetPlayerId = null)
    {
        try
        {
            if (serviceProvider == null)
            {
                return;
            }

            var networkClient = serviceProvider?.GetService<INetworkClient>();
            if (networkClient == null || !networkClient.IsConnected)
            {
                return;
            }

            var requestData = new
            {
                Timestamp = DateTime.Now.Ticks,
                EventType = "ShopInventoryRequest",
                TargetPlayerId = targetPlayerId,
                RequesterId = GetCurrentPlayerId()
            };

            SendGameEvent("ShopInventoryRequest", requestData);

            Plugin.Logger?.LogInfo($"[ShopSync] Requested shop inventory from {targetPlayerId ?? "all players"}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[ShopSync] Error requesting shop inventory: {ex.Message}");
        }
    }

    /// <summary>
    /// 响应商店库存请求
    /// </summary>
    public static void RespondToShopInventoryRequest(string requesterId)
    {
        try
        {
            var shopStation = GetCurrentShopStation();
            if (shopStation == null || serviceProvider == null)
            {
                return;
            }

            var networkClient = serviceProvider?.GetService<INetworkClient>();
            if (networkClient == null || !networkClient.IsConnected)
            {
                return;
            }

            var inventoryData = new
            {
                Timestamp = DateTime.Now.Ticks,
                EventType = "ShopInventoryResponse",
                RequesterId = requesterId,
                ShopId = shopStation.GetHashCode().ToString(),
                Inventory = BuildShopInventoryData(shopStation),
                PlayerGold = shopStation.GameRun?.Money ?? 0,
                PlayerId = GetCurrentPlayerId()
            };

            SendGameEvent("ShopInventoryResponse", inventoryData);

            Plugin.Logger?.LogInfo($"[ShopSync] Responded to shop inventory request from {requesterId}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[ShopSync] Error responding to shop inventory request: {ex.Message}");
        }
    }

    // 辅助方法

    /// <summary>
    /// 发送游戏事件
    /// </summary>
    private static void SendGameEvent(string eventType, object eventData)
    {
        try
        {
            var networkClient = serviceProvider?.GetService<INetworkClient>();
            if (networkClient != null)
            {
                networkClient.SendGameEventData(eventType, eventData);
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[ShopSync] Error sending game event {eventType}: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取当前玩家ID
    /// </summary>
    private static string GetCurrentPlayerId()
    {
        try
        {
            // 使用GameStateUtils获取
            return GameStateUtils.GetCurrentPlayerId();
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[ShopSync] Error getting current player ID: {ex.Message}");
            return "unknown_player";
        }
    }

    /// <summary>
    /// 获取当前商店实例
    /// </summary>
    private static ShopStation GetCurrentShopStation()
    {
        try
        {
            var gameRun = GameStateUtils.GetCurrentGameRun();
            if (gameRun != null)
            {
                // 尝试从游戏运行状态获取当前商店
                var currentStationProperty = gameRun.GetType().GetProperty("CurrentStation");
                if (currentStationProperty != null)
                {
                    var station = currentStationProperty.GetValue(gameRun);
                    if (station is ShopStation shopStation)
                    {
                        return shopStation;
                    }
                }

                // 备用方案：检查是否有商店相关的属性
                var shopProperty = gameRun.GetType().GetProperty("Shop");
                if (shopProperty != null)
                {
                    return shopProperty.GetValue(gameRun) as ShopStation;
                }
            }
            return null;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[ShopSync] Error getting current shop: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 获取玩家卡牌数量
    /// </summary>
    private static int GetCardCount(GameRunController gameRun)
    {
        try
        {
            if (gameRun?.Player == null)
            {
                return 0;
            }

            // 尝试获取玩家的所有卡牌数量
            var player = gameRun.Player;
            var count = 0;

            // 手牌
            var handZoneProperty = player.GetType().GetProperty("HandZone");
            if (handZoneProperty != null)
            {
                var handZone = handZoneProperty.GetValue(player);
                if (handZone != null)
                {
                    var countProperty = handZone.GetType().GetProperty("Count");
                    if (countProperty != null)
                    {
                        count += (int?)(countProperty.GetValue(handZone)) ?? 0;
                    }
                }
            }

            // 牌组
            var deckZoneProperty = gameRun.GetType().GetProperty("DeckZone");
            if (deckZoneProperty != null)
            {
                var deckZone = deckZoneProperty.GetValue(gameRun);
                if (deckZone != null)
                {
                    var countProperty = deckZone.GetType().GetProperty("Count");
                    if (countProperty != null)
                    {
                        count += (int?)(countProperty.GetValue(deckZone)) ?? 0;
                    }
                }
            }

            // 抽牌堆
            var drawZoneProperty = gameRun.GetType().GetProperty("DrawZone");
            if (drawZoneProperty != null)
            {
                var drawZone = drawZoneProperty.GetValue(gameRun);
                if (drawZone != null)
                {
                    var countProperty = drawZone.GetType().GetProperty("Count");
                    if (countProperty != null)
                    {
                        count += (int?)(countProperty.GetValue(drawZone)) ?? 0;
                    }
                }
            }

            // 弃牌堆
            var discardZoneProperty = gameRun.GetType().GetProperty("DiscardZone");
            if (discardZoneProperty != null)
            {
                var discardZone = discardZoneProperty.GetValue(gameRun);
                if (discardZone != null)
                {
                    var countProperty = discardZone.GetType().GetProperty("Count");
                    if (countProperty != null)
                    {
                        count += (int?)(countProperty.GetValue(discardZone)) ?? 0;
                    }
                }
            }

            return count;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[ShopSync] Error getting card count: {ex.Message}");
            return 0;
        }
    }

    /// <summary>
    /// 获取玩家宝物数量
    /// </summary>
    private static int GetExhibitCount(PlayerUnit player)
    {
        try
        {
            return player?.Exhibits?.Count ?? 0;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[ShopSync] Error getting exhibit count: {ex.Message}");
            return 0;
        }
    }

    /// <summary>
    /// 计算总购买花费
    /// </summary>
    private static int CalculateTotalPurchases(ShopStation shop)
    {
        try
        {
            if (shop == null)
            {
                return 0;
            }

            var totalSpent = 0;

            // 统计已购买卡牌的花费
            if (shop.ShopCards != null)
            {
                foreach (var cardItem in shop.ShopCards)
                {
                    if (cardItem?.IsSoldOut == true)
                    {
                        totalSpent += cardItem.Price;
                    }
                }
            }

            // 统计已购买宝物的花费
            if (shop.ShopExhibits != null)
            {
                foreach (var exhibitItem in shop.ShopExhibits)
                {
                    if (exhibitItem?.IsSoldOut == true)
                    {
                        totalSpent += exhibitItem.Price;
                    }
                }
            }

            return totalSpent;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[ShopSync] Error calculating total purchases: {ex.Message}");
            return 0;
        }
    }

    /// <summary>
    /// 构建商店库存数据
    /// </summary>
    private static object BuildShopInventoryData(ShopStation shop)
    {
        try
        {
            var cards = shop.ShopCards?.Select(item => new
            {
                ItemId = item.Content.Id,
                ItemName = item.Content.Name,
                ItemType = "Card",
                CardType = item.Content.GetType().Name,
                Rarity = item.Content.Config?.Rarity?.ToString() ?? "Unknown",
                Price = item.Price,
                IsSoldOut = item.IsSoldOut,
                IsDiscounted = item.IsDiscounted,
                IsPurgeable = item.Content.Config?.Purgeable ?? false
            }).ToList() ?? new List<object>();

            var exhibits = shop.ShopExhibits?.Select(item => new
            {
                ItemId = item.Content.Id,
                ItemName = item.Content.Name,
                ItemType = "Exhibit",
                ExhibitType = item.Content.GetType().Name,
                Rarity = item.Content.Config?.Rarity?.ToString() ?? "Unknown",
                Price = item.Price,
                IsSoldOut = item.IsSoldOut,
                IsDiscounted = item.IsDiscounted
            }).ToList() ?? new List<object>();

            return new
            {
                Cards = cards,
                Exhibits = exhibits,
                TotalCards = cards.Count,
                TotalExhibits = exhibits.Count,
                HasDiscounts = cards.Any(c => (bool)c.IsDiscounted) || exhibits.Any(e => (bool)e.IsDiscounted),
                Timestamp = DateTime.Now.Ticks
            };
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[ShopSync] Error building shop inventory: {ex.Message}");
            return new { Error = ex.Message, Timestamp = DateTime.Now.Ticks };
        }
    }
}
