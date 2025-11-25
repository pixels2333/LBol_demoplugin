using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.Stations;
using LBoL.Core.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using NetworkPlugin.Network.Client;
using Microsoft.Extensions.DependencyInjection;

namespace NetworkPlugin.Patch.Network;

/// <summary>
/// 商店同步补丁 - 同步商店购买操作
/// 参考杀戮尖塔Together in Spire商店同步机制
/// TODO: 需要测试联机环境下商店物品的同步表现
/// </summary>
public class ShopSyncPatch
{
    private static IServiceProvider serviceProvider => ModService.ServiceProvider;

    /// <summary>
    /// 商店初始化同步 - 当玩家进入商店时同步商店库存
    /// </summary>
    [HarmonyPatch(typeof(ShopStation), nameof(ShopStation.OnEnter))]
    [HarmonyPostfix]
    public static void ShopEnter_Postfix(ShopStation __instance)
    {
        try
        {
            if (serviceProvider == null) return;

            var networkClient = serviceProvider.GetService<INetworkClient>();
            if (networkClient == null || !networkClient.IsConnected)
                return;

            // TODO: 确认是否需要同步整个商店库存给其他玩家
            // 杀戮尖塔中通常只有当前玩家能看到自己的商店
            // 但在合作模式下，可能需要显示队友的商店状态

            var shopData = new
            {
                ShopId = __instance.GetHashCode().ToString(),
                LocationX = __instance.Location.X,
                LocationY = __instance.Location.Y,
                // TODO: 确定是否需要同步完整的商店物品列表
                // 这可能需要序列化卡牌和宝物的完整信息
                CardCount = __instance.ShopCards?.Count ?? 0,
                ExhibitCount = __instance.ShopExhibits?.Count ?? 0,
                HasDiscount = __instance.ShopCards?.Any(c => c.IsDiscounted) ?? false
            };

            var json = JsonSerializer.Serialize(shopData);
            networkClient.SendRequest("ShopEnter", json);

            Plugin.Logger?.LogInfo($"[ShopSync] Entered shop at ({__instance.Location.X}, {__instance.Location.Y})");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[ShopSync] Error in ShopEnter: {ex.Message}");
        }
    }

    /// <summary>
    /// 购买卡牌同步
    /// </summary>
    [HarmonyPatch(typeof(ShopStation), nameof(ShopStation.BuyCard))]
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

    [HarmonyPatch(typeof(ShopStation), nameof(ShopStation.BuyCard))]
    [HarmonyPostfix]
    public static void BuyCard_Postfix(ShopStation __instance, ShopItem<Card> cardItem, ShopItem<Card> __state)
    {
        try
        {
            if (serviceProvider == null) return;

            var networkClient = serviceProvider.GetService<INetworkClient>();
            if (networkClient == null || !networkClient.IsConnected)
                return;

            // 检查是否真的购买了（状态从未售出变为已售出）
            if (__state?.IsSoldOut == false && cardItem.IsSoldOut)
            {
                var purchaseData = new
                {
                    ShopId = __instance.GetHashCode().ToString(),
                    PurchaseType = "BuyCard",
                    ItemId = cardItem.Content.Id,
                    ItemName = cardItem.Content.Name,
                    Price = __state.Price,
                    WasDiscounted = __state.IsDiscounted,
                    PlayerGoldBefore = __instance.GameRun.Money + __state.Price, // 购买前的金币
                    PlayerGoldAfter = __instance.GameRun.Money
                };

                var json = JsonSerializer.Serialize(purchaseData);
                networkClient.SendRequest("ShopPurchase", json);

                Plugin.Logger?.LogInfo($"[ShopSync] Bought card: {cardItem.Content.Name} for {__state.Price} gold");
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[ShopSync] Error in BuyCard_Postfix: {ex.Message}");
        }
    }

    /// <summary>
    /// 购买宝物同步
    /// TODO: 宝物购买有视觉表现，可能需要特殊处理
    /// </summary>
    [HarmonyPatch(typeof(ShopStation), nameof(ShopStation.BuyExhibitRunner))]
    public static class BuyExhibitSync
    {
        // 注意：BuyExhibitRunner是IEnumerator方法，需要使用不同的Patch方式
        // TODO: 研究如何正确Patch协程方法
        // 临时方案：在协程开始时记录状态

        public static void Prefix(ShopStation __instance, ShopItem<Exhibit> exhibitItem, out ShopItem<Exhibit> __state)
        {
            __state = null;
            try
            {
                __state = new ShopItem<Exhibit>(
                    __instance.GameRun,
                    exhibitItem.Content,
                    exhibitItem.Price,
                    exhibitItem.IsSoldOut,
                    exhibitItem.IsDiscounted
                );
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[ShopSync] Error in BuyExhibit_Prefix: {ex.Message}");
            }
        }

        // TODO: 需要实现在协程完成后的同步逻辑
        // 这可能需要使用Transpiler或等待协程完成
    }

    /// <summary>
    /// 升级卡组卡牌同步
    /// TODO: 确认升级操作是否需要同步
    /// 这可能只影响当前玩家，不需要同步
    /// </summary>
    [HarmonyPatch(typeof(ShopStation), nameof(ShopStation.UpgradeDeckCard))]
    [HarmonyPostfix]
    public static void UpgradeDeckCard_Postfix(ShopStation __instance, Card card)
    {
        try
        {
            if (serviceProvider == null) return;

            var networkClient = serviceProvider.GetService<INetworkClient>();
            if (networkClient == null || !networkClient.IsConnected)
                return;

            // TODO: 确定升级操作是否需要广播给其他玩家
            // 在杀戮尖塔Together in Spire中，这类个人操作通常不同步
            // 但在合作模式下可能需要显示提示信息

            var upgradeData = new
            {
                ShopId = __instance.GetHashCode().ToString(),
                ActionType = "UpgradeDeckCard",
                CardId = card.Id,
                CardName = card.Name,
                UpgradePrice = __instance.UpgradeDeckCardPrice
            };

            var json = JsonSerializer.Serialize(upgradeData);
            // 发送但不一定是广播，可以是仅提示
            networkClient.SendRequest("ShopServiceUsed", json);

            Plugin.Logger?.LogInfo($"[ShopSync] Upgraded card: {card.Name}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[ShopSync] Error in UpgradeDeckCard: {ex.Message}");
        }
    }

    /// <summary>
    /// 移除卡组卡牌同步
    /// TODO: 确认移除操作是否需要同步
    /// </summary>
    [HarmonyPatch(typeof(ShopStation), nameof(ShopStation.RemoveDeckCard))]
    [HarmonyPostfix]
    public static void RemoveDeckCard_Postfix(ShopStation __instance, Card card)
    {
        try
        {
            if (serviceProvider == null) return;

            var networkClient = serviceProvider.GetService<INetworkClient>();
            if (networkClient == null || !networkClient.IsConnected)
                return;

            var removeData = new
            {
                ShopId = __instance.GetHashCode().ToString(),
                ActionType = "RemoveDeckCard",
                CardId = card.Id,
                CardName = card.Name,
                RemovePrice = __instance.RemoveDeckCardPrice
            };

            var json = JsonSerializer.Serialize(removeData);
            networkClient.SendRequest("ShopServiceUsed", json);

            Plugin.Logger?.LogInfo($"[ShopSync] Removed card: {card.Name}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[ShopSync] Error in RemoveDeckCard: {ex.Message}");
        }
    }

    /// <summary>
    /// 构建商店库存数据
    /// TODO: 实现完整的商店物品序列化
    /// 需要处理卡牌和宝物的详细信息
    /// </summary>
    private static object BuildShopInventoryData(ShopStation shop)
    {
        try
        {
            var cards = shop.ShopCards?.Select(item => new
            {
                item.Content.Id,
                item.Content.Name,
                item.Content.GetType().Name,
                item.Price,
                item.IsSoldOut,
                item.IsDiscounted
            }).ToList() ?? new List<object>();

            var exhibits = shop.ShopExhibits?.Select(item => new
            {
                item.Content.Id,
                item.Content.Name,
                item.Content.GetType().Name,
                item.Price,
                item.IsSoldOut,
                item.IsDiscounted
            }).ToList() ?? new List<object>();

            return new
            {
                cards,
                exhibits,
                TotalCards = cards.Count,
                TotalExhibits = exhibits.Count
            };
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[ShopSync] Error building shop inventory: {ex.Message}");
            return new { Error = ex.Message };
        }
    }
}
