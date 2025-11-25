using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using NetworkPlugin.Network.Client;
using Microsoft.Extensions.DependencyInjection;

namespace NetworkPlugin.Patch.Network;

/// <summary>
/// 宝物同步补丁 - 同步宝物获取、移除、激活状态等
/// 参考杀戮尖塔Together in Spire的宝物/遗物同步机制
/// TODO: 需要测试宝物计数器同步的效果
/// </summary>
public class ExhibitSyncPatch
{
    private static IServiceProvider serviceProvider => ModService.ServiceProvider;

    /// <summary>
    /// 宝物获取同步 - 当玩家获得宝物时触发
    /// </summary>
    [HarmonyPatch(typeof(PlayerUnit), nameof(PlayerUnit.UnsafeAddExhibit))]
    [HarmonyPostfix]
    public static void UnsafeAddExhibit_Postfix(PlayerUnit __instance, Exhibit exhibit)
    {
        try
        {
            if (serviceProvider == null) return;

            var networkClient = serviceProvider.GetService<INetworkClient>();
            if (networkClient == null || !networkClient.IsConnected)
                return;

            // 构建宝物详细信息
            var exhibitData = new
            {
                exhibit.Id,
                exhibit.Name,
                Type = exhibit.GetType().Name,
                exhibit.Config.Rarity,
                exhibit.HasCounter,
                Counter = exhibit.HasCounter ? exhibit.Counter : 0,
                exhibit.Active,
                exhibit.Blackout,
                exhibit.IconName
            };

            var message = new
            {
                PlayerId = __instance.Id,
                Timestamp = DateTime.Now.Ticks,
                ActionType = "GainExhibit",
                Exhibit = exhibitData,
                TotalExhibits = __instance.Exhibits?.Count ?? 0
            };

            var json = JsonSerializer.Serialize(message);
            networkClient.SendRequest("ExhibitUpdate", json);

            Plugin.Logger?.LogInfo($"[ExhibitSync] Player gained exhibit: {exhibit.Name} (Total: {__instance.Exhibits?.Count ?? 0})");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[ExhibitSync] Error in UnsafeAddExhibit: {ex.Message}");
        }
    }

    /// <summary>
    /// 宝物移除同步 - 当玩家失去宝物时触发
    /// </summary>
    [HarmonyPatch(typeof(PlayerUnit), nameof(PlayerUnit.InternalRemoveExhibit))]
    [HarmonyPrefix]
    public static void InternalRemoveExhibit_Prefix(PlayerUnit __instance, Exhibit exhibit)
    {
        try
        {
            if (serviceProvider == null) return;

            var networkClient = serviceProvider.GetService<INetworkClient>();
            if (networkClient == null || !networkClient.IsConnected)
                return;

            var exhibitData = new
            {
                exhibit.Id,
                exhibit.Name,
                Type = exhibit.GetType().Name,
                exhibit.HasCounter,
                Counter = exhibit.HasCounter ? exhibit.Counter : 0
            };

            var message = new
            {
                PlayerId = __instance.Id,
                Timestamp = DateTime.Now.Ticks,
                ActionType = "RemoveExhibit",
                Exhibit = exhibitData,
                TotalExhibitsAfter = Math.Max(0, (__instance.Exhibits?.Count ?? 0) - 1)
            };

            var json = JsonSerializer.Serialize(message);
            networkClient.SendRequest("ExhibitUpdate", json);

            Plugin.Logger?.LogInfo($"[ExhibitSync] Player lost exhibit: {exhibit.Name}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[ExhibitSync] Error in InternalRemoveExhibit: {ex.Message}");
        }
    }

    /// <summary>
    /// 宝物激活状态变化同步
    /// TODO: 需要找到宝物Active属性变化时的触发点
    /// </summary>
    public static class ExhibitActivationSync
    {
        // 注：Exhibit.Active属性有setter，但需要通过反射或寻找调用点来Patch
        // TODO: 查找Exhibit.Active = true/false的调用位置

        /// <summary>
        /// TODO: 实现宝物激活状态同步
        /// 需要找到修改Active属性的方法并Patch
        /// </summary>
        public static void SyncExhibitActivation(Exhibit exhibit, bool newActiveState)
        {
            try
            {
                if (serviceProvider == null) return;

                var networkClient = serviceProvider.GetService<INetworkClient>();
                if (networkClient == null || !networkClient.IsConnected)
                    return;

                var message = new
                {
                    PlayerId = exhibit.Owner?.Id ?? "unknown",
                    Timestamp = DateTime.Now.Ticks,
                    ActionType = "ExhibitActivationChanged",
                    ExhibitId = exhibit.Id,
                    ExhibitName = exhibit.Name,
                    NewActiveState = newActiveState,
                    OldActiveState = !newActiveState
                };

                var json = JsonSerializer.Serialize(message);
                networkClient.SendRequest("ExhibitUpdate", json);

                Plugin.Logger?.LogInfo($"[ExhibitSync] Exhibit {exhibit.Name} active state changed: {newActiveState}");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[ExhibitSync] Error in SyncExhibitActivation: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 宝物计数器变化同步
    /// TODO: 需要监控Exhibit.Counter属性的变化
    /// </summary>
    public static class ExhibitCounterSync
    {
        // 注：Exhibit.Counter有setter，但需要在实际游戏逻辑中查找修改点

        /// <summary>
        /// TODO: 实现宝物计数器同步
        /// 寻找调用exhibit.Counter = value的位置并Patch
        /// </summary>
        public static void SyncExhibitCounter(Exhibit exhibit, int oldValue, int newValue)
        {
            try
            {
                if (!exhibit.HasCounter) return;
                if (serviceProvider == null) return;

                var networkClient = serviceProvider.GetService<INetworkClient>();
                if (networkClient == null || !networkClient.IsConnected)
                    return;

                var message = new
                {
                    PlayerId = exhibit.Owner?.Id ?? "unknown",
                    Timestamp = DateTime.Now.Ticks,
                    ActionType = "ExhibitCounterChanged",
                    ExhibitId = exhibit.Id,
                    ExhibitName = exhibit.Name,
                    OldValue = oldValue,
                    NewValue = newValue,
                    Difference = newValue - oldValue
                };

                var json = JsonSerializer.Serialize(message);
                networkClient.SendRequest("ExhibitUpdate", json);

                Plugin.Logger?.LogInfo($"[ExhibitSync] Exhibit {exhibit.Name} counter changed: {oldValue} -> {newValue}");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[ExhibitSync] Error in SyncExhibitCounter: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 清除所有宝物同步（特殊事件，如LOSE_ALL_EXHIBITS）
    /// </summary>
    [HarmonyPatch(typeof(PlayerUnit), nameof(PlayerUnit.ClearExhibits))]
    [HarmonyPostfix]
    public static void ClearExhibits_Postfix(PlayerUnit __instance)
    {
        try
        {
            if (serviceProvider == null) return;

            var networkClient = serviceProvider.GetService<INetworkClient>();
            if (networkClient == null || !networkClient.IsConnected)
                return;

            var message = new
            {
                PlayerId = __instance.Id,
                Timestamp = DateTime.Now.Ticks,
                ActionType = "ClearAllExhibits",
                TotalExhibitsCleared = __instance.Exhibits?.Count ?? 0
            };

            var json = JsonSerializer.Serialize(message);
            networkClient.SendRequest("ExhibitUpdate", json);

            Plugin.Logger?.LogInfo($"[ExhibitSync] All exhibits cleared (Total: {__instance.Exhibits?.Count ?? 0})");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[ExhibitSync] Error in ClearExhibits: {ex.Message}");
        }
    }

    /// <summary>
    /// 宝物商店购买同步（BuyExhibit）
    /// TODO: 宝物购买有视觉表现，可能需要特殊处理
    /// </summary>
    [HarmonyPatch(typeof(GameRunController), "BuyExhibitRunner")]
    public static class BuyExhibitSync
    {
        // BuyExhibitRunner是IEnumerator方法
        // TODO: 研究协程Patch的最佳实践

        /// <summary>
        /// 在购买开始时记录状态
        /// </summary>
        public static void Prefix(GameRunController __instance, ShopItem<Exhibit> exhibitItem, out int __state)
        {
            __state = __instance.Money;
        }

        /// <summary>
        /// TODO: 在协程完成后发送购买消息
        /// 需要在协程完成时触发，可能需要使用Transpiler
        /// </summary>
        public static void Postfix(GameRunController __instance, ShopItem<Exhibit> exhibitItem, int __state)
        {
            try
            {
                if (serviceProvider == null) return;

                var networkClient = serviceProvider.GetService<INetworkClient>();
                if (networkClient == null || !networkClient.IsConnected)
                    return;

                // TODO: 验证购买是否成功
                bool purchaseSuccess = exhibitItem.IsSoldOut && __instance.Money < __state;

                if (purchaseSuccess)
                {
                    var purchaseData = new
                    {
                        ShopId = "shop_instance", // TODO: 获取实际的商店ID
                        ItemId = exhibitItem.Content.Id,
                        ItemName = exhibitItem.Content.Name,
                        Price = __state - __instance.Money,
                        PlayerGoldBefore = __state,
                        PlayerGoldAfter = __instance.Money
                    };

                    var json = JsonSerializer.Serialize(purchaseData);
                    networkClient.SendRequest("ShopPurchase", json);

                    Plugin.Logger?.LogInfo($"[ExhibitSync] Bought exhibit: {exhibitItem.Content.Name}");
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[ExhibitSync] Error in BuyExhibit: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Boss宝物选择同步
    /// TODO: 实现Boss战后宝物选择的同步
    /// </summary>
    public static class BossExhibitSync
    {
        // TODO: 找到Boss战后选择宝物的逻辑并Patch
        // 这通常是玩家主动选择的，需要同步选择结果
    }

    /// <summary>
    /// 构建完整的宝物列表信息
    /// 用于完整同步或断线重连后的状态恢复
    /// </summary>
    public static object BuildFullExhibitInfo(PlayerUnit player)
    {
        if (player?.Exhibits == null)
            return new { Exhibits = new List<object>(), Count = 0 };

        var exhibits = player.Exhibits.Select(exhibit => new
        {
            exhibit.Id,
            exhibit.Name,
            Type = exhibit.GetType().Name,
            exhibit.Config.Rarity,
            exhibit.HasCounter,
            Counter = exhibit.HasCounter ? exhibit.Counter : 0,
            exhibit.Active,
            exhibit.Blackout,
            exhibit.IconName
        }).ToList();

        return new
        {
            Exhibits = exhibits,
            Count = exhibits.Count
        };
    }
}
