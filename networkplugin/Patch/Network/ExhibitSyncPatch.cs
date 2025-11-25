using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Units;
using LBoL.EntityLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;
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
                ExhibitId = exhibit.Id,
                ExhibitName = exhibit.Name,
                ExhibitType = exhibit.GetType().Name,
                Rarity = exhibit.Config?.Rarity?.ToString() ?? "Unknown",
                HasCounter = exhibit.HasCounter,
                Counter = exhibit.HasCounter ? exhibit.Counter : 0,
                IsActive = exhibit.Active,
                IsBlackout = exhibit.Blackout,
                IconName = exhibit.IconName
            };

            var message = new
            {
                Timestamp = DateTime.Now.Ticks,
                EventType = NetworkMessageTypes.OnExhibitObtained,
                PlayerId = GetCurrentPlayerId(__instance),
                Action = "GainExhibit",
                ExhibitData = exhibitData,
                TotalExhibits = __instance.Exhibits?.Count ?? 0
            };

            SendGameEvent(NetworkMessageTypes.OnExhibitObtained, message);

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
                ExhibitId = exhibit.Id,
                ExhibitName = exhibit.Name,
                ExhibitType = exhibit.GetType().Name,
                HasCounter = exhibit.HasCounter,
                Counter = exhibit.HasCounter ? exhibit.Counter : 0,
                IsActive = exhibit.Active,
                IsBlackout = exhibit.Blackout
            };

            var message = new
            {
                Timestamp = DateTime.Now.Ticks,
                EventType = NetworkMessageTypes.OnExhibitRemoved,
                PlayerId = GetCurrentPlayerId(__instance),
                Action = "RemoveExhibit",
                ExhibitData = exhibitData,
                TotalExhibitsAfter = Math.Max(0, (__instance.Exhibits?.Count ?? 0) - 1)
            };

            SendGameEvent(NetworkMessageTypes.OnExhibitRemoved, message);

            Plugin.Logger?.LogInfo($"[ExhibitSync] Player lost exhibit: {exhibit.Name}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[ExhibitSync] Error in InternalRemoveExhibit: {ex.Message}");
        }
    }

    /// <summary>
    /// 宝物激活状态变化同步 - 使用反射监控Active属性变化
    /// </summary>
    [HarmonyPatch]
    public static class ExhibitActivationSync
    {
        private static readonly Dictionary<Exhibit, bool> _activationStates = new Dictionary<Exhibit, bool>();

        [HarmonyTargetMethods]
        static System.Collections.Generic.IEnumerable<System.Reflection.MethodBase> TargetMethods()
        {
            // 搜索可能修改Active属性的方法
            var methodNames = new[]
            {
                "Activate",
                "Deactivate",
                "SetActive",
                "ToggleActive",
                "NotifyActivationChange"
            };

            var methods = new List<System.Reflection.MethodBase>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (typeof(Exhibit).IsAssignableFrom(type))
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
                }
                catch (Exception)
                {
                    // 忽略无法访问的程序集
                }
            }

            return methods;
        }

        [HarmonyPrefix]
        public static void ActivationChange_Prefix(object __instance, out bool __state)
        {
            __state = false;
            try
            {
                if (__instance is Exhibit exhibit)
                {
                    __state = exhibit.Active;
                    _activationStates[exhibit] = exhibit.Active;
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[ExhibitSync] Error in ActivationChange_Prefix: {ex.Message}");
            }
        }

        [HarmonyPostfix]
        public static void ActivationChange_Postfix(object __instance, bool __state)
        {
            try
            {
                if (!(__instance is Exhibit exhibit)) return;
                if (exhibit.Active == __state) return; // 状态没有变化

                SendExhibitActivationEvent(exhibit, __state, exhibit.Active);

                // 更新缓存
                _activationStates[exhibit] = exhibit.Active;

                Plugin.Logger?.LogInfo($"[ExhibitSync] Exhibit {exhibit.Name} active state changed: {__state} -> {exhibit.Active}");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[ExhibitSync] Error in ActivationChange_Postfix: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 宝物计数器变化同步 - 监控Counter属性变化
    /// </summary>
    [HarmonyPatch]
    public static class ExhibitCounterSync
    {
        private static readonly Dictionary<Exhibit, int> _counterStates = new Dictionary<Exhibit, int>();

        [HarmonyTargetMethods]
        static System.Collections.Generic.IEnumerable<System.Reflection.MethodBase> TargetMethods()
        {
            // 搜索可能修改Counter的方法
            var methodNames = new[]
            {
                "SetCounter",
                "IncreaseCounter",
                "DecreaseCounter",
                "AddCounter",
                "RemoveCounter",
                "ModifyCounter"
            };

            var methods = new List<System.Reflection.MethodBase>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (typeof(Exhibit).IsAssignableFrom(type))
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
                }
                catch (Exception)
                {
                    // 忽略无法访问的程序集
                }
            }

            return methods;
        }

        [HarmonyPrefix]
        public static void CounterChange_Prefix(object __instance, out int __state)
        {
            __state = 0;
            try
            {
                if (__instance is Exhibit exhibit && exhibit.HasCounter)
                {
                    __state = exhibit.Counter;
                    _counterStates[exhibit] = exhibit.Counter;
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[ExhibitSync] Error in CounterChange_Prefix: {ex.Message}");
            }
        }

        [HarmonyPostfix]
        public static void CounterChange_Postfix(object __instance, int __state)
        {
            try
            {
                if (!(__instance is Exhibit exhibit)) return;
                if (!exhibit.HasCounter) return;
                if (exhibit.Counter == __state) return; // 计数器没有变化

                SendExhibitCounterEvent(exhibit, __state, exhibit.Counter);

                // 更新缓存
                _counterStates[exhibit] = exhibit.Counter;

                Plugin.Logger?.LogInfo($"[ExhibitSync] Exhibit {exhibit.Name} counter changed: {__state} -> {exhibit.Counter}");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[ExhibitSync] Error in CounterChange_Postfix: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 定期检查宝物状态变化（作为反射方法的补充）
    /// </summary>
    [HarmonyPatch(typeof(PlayerUnit), nameof(PlayerUnit.Update))]
    [HarmonyPostfix]
    public static void PlayerUnitUpdate_Postfix(PlayerUnit __instance)
    {
        try
        {
            if (__instance.Exhibits == null) return;
            if (serviceProvider == null) return;

            var networkClient = serviceProvider.GetService<INetworkClient>();
            if (networkClient == null || !networkClient.IsConnected)
                return;

            // 检查所有宝物的状态变化
            foreach (var exhibit in __instance.Exhibits)
            {
                // 检查激活状态
                if (ExhibitActivationSync._activationStates.TryGetValue(exhibit, out var lastActive))
                {
                    if (exhibit.Active != lastActive)
                    {
                        SendExhibitActivationEvent(exhibit, lastActive, exhibit.Active);
                        ExhibitActivationSync._activationStates[exhibit] = exhibit.Active;
                    }
                }
                else
                {
                    // 首次遇到，记录状态
                    ExhibitActivationSync._activationStates[exhibit] = exhibit.Active;
                }

                // 检查计数器
                if (exhibit.HasCounter)
                {
                    if (ExhibitCounterSync._counterStates.TryGetValue(exhibit, out var lastCounter))
                    {
                        if (exhibit.Counter != lastCounter)
                        {
                            SendExhibitCounterEvent(exhibit, lastCounter, exhibit.Counter);
                            ExhibitCounterSync._counterStates[exhibit] = exhibit.Counter;
                        }
                    }
                    else
                    {
                        // 首次遇到，记录计数器
                        ExhibitCounterSync._counterStates[exhibit] = exhibit.Counter;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[ExhibitSync] Error in PlayerUnitUpdate_Postfix: {ex.Message}");
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
            ExhibitId = exhibit.Id,
            ExhibitName = exhibit.Name,
            ExhibitType = exhibit.GetType().Name,
            Rarity = exhibit.Config?.Rarity?.ToString() ?? "Unknown",
            HasCounter = exhibit.HasCounter,
            Counter = exhibit.HasCounter ? exhibit.Counter : 0,
            IsActive = exhibit.Active,
            IsBlackout = exhibit.Blackout,
            IconName = exhibit.IconName
        }).ToList();

        return new
        {
            Exhibits = exhibits,
            Count = exhibits.Count,
            Timestamp = DateTime.Now.Ticks
        };
    }

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
            Plugin.Logger?.LogError($"[ExhibitSync] Error sending game event {eventType}: {ex.Message}");
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
            Plugin.Logger?.LogError($"[ExhibitSync] Error getting current player ID: {ex.Message}");
            return "unknown_player";
        }
    }

    private static void SendExhibitActivationEvent(Exhibit exhibit, bool oldState, bool newState)
    {
        try
        {
            if (serviceProvider == null) return;

            var message = new
            {
                Timestamp = DateTime.Now.Ticks,
                EventType = "ExhibitActivationChanged",
                PlayerId = exhibit.Owner != null ? GetCurrentPlayerId(exhibit.Owner) : "unknown",
                ExhibitId = exhibit.Id,
                ExhibitName = exhibit.Name,
                OldActiveState = oldState,
                NewActiveState = newState,
                StateChange = newState ? "Activated" : "Deactivated"
            };

            SendGameEvent("ExhibitActivationChanged", message);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[ExhibitSync] Error sending exhibit activation event: {ex.Message}");
        }
    }

    private static void SendExhibitCounterEvent(Exhibit exhibit, int oldValue, int newValue)
    {
        try
        {
            if (!exhibit.HasCounter) return;
            if (serviceProvider == null) return;

            var message = new
            {
                Timestamp = DateTime.Now.Ticks,
                EventType = "ExhibitCounterChanged",
                PlayerId = exhibit.Owner != null ? GetCurrentPlayerId(exhibit.Owner) : "unknown",
                ExhibitId = exhibit.Id,
                ExhibitName = exhibit.Name,
                OldValue = oldValue,
                NewValue = newValue,
                Difference = newValue - oldValue,
                ChangeType = newValue > oldValue ? "Increased" : newValue < oldValue ? "Decreased" : "Unchanged"
            };

            SendGameEvent("ExhibitCounterChanged", message);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[ExhibitSync] Error sending exhibit counter event: {ex.Message}");
        }
    }
}
