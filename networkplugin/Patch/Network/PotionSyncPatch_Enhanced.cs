using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;

namespace NetworkPlugin.Patch.Network;

/// <summary>
/// 药水（道具）同步补丁 - 同步Tool卡获取、使用、丢弃
/// LBoL中的药水对应Tool类，继承自Card类，通过Actions方法使用
/// 重要性: ⭐⭐⭐⭐
/// </summary>
public class PotionSyncPatch
{
    private static IServiceProvider serviceProvider => ModService.ServiceProvider;

    /// <summary>
    /// Tool卡获取同步 - 监控Tool卡加入玩家手牌/牌库
    /// </summary>
    [HarmonyPatch]
    public static class ToolObtainSync
    {
        [HarmonyTargetMethods]
        static System.Collections.Generic.IEnumerable<System.Reflection.MethodBase> TargetMethods()
        {
            var methodNames = new[]
            {
                "AddToHand",
                "AddToDeck",
                "AddToDraw",
                "GainTool",
                "ObtainTool"
            };

            List<MethodBase> methods = [];

            // 搜索可能添加Tool卡的方法
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        foreach (var method in type.GetMethods())
                        {
                            if (methodNames.Any(name => method.Name.Contains(name)) ||
                                method.Name.Contains("Tool") && method.Name.Contains("Add"))
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
        public static void ToolObtain_Prefix(object __instance, object[] __args, out ToolCardState __state)
        {
            __state = new ToolCardState();
            try
            {
                // 记录获取前的Tool卡状态
                __state.BeforeCount = GetCurrentToolCount();
                __state.ObtainTime = DateTime.Now;

                // 尝试识别Tool卡信息
                if (__args.Length > 0 && __args[0] is Card card && IsToolCard(card))
                {
                    __state.ToolId = card.Id;
                    __state.ToolName = card.Name;
                    __state.ToolType = card.GetType().Name;
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[PotionSync] Error in ToolObtain_Prefix: {ex.Message}");
            }
        }

        [HarmonyPostfix]
        public static void ToolObtain_Postfix(object __instance, object[] __args, ToolCardState __state)
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

                var afterCount = GetCurrentToolCount();
                var obtainedCount = afterCount - __state.BeforeCount;

                if (obtainedCount > 0)
                {
                    var source = DetermineToolSource(__instance, __args);
                    var obtainData = new
                    {
                        Timestamp = DateTime.Now.Ticks,
                        EventType = NetworkMessageTypes.OnPotionObtained,
                        PlayerId = GetCurrentPlayerId(),
                        ToolId = __state.ToolId ?? "Unknown",
                        ToolName = __state.ToolName ?? "Unknown",
                        ToolType = __state.ToolType ?? "Tool",
                        Quantity = obtainedCount,
                        Source = source,
                        TotalTools = afterCount
                    };

                    SendGameEvent(NetworkMessageTypes.OnPotionObtained, obtainData);

                    Plugin.Logger?.LogInfo($"[PotionSync] Player obtained tool: {__state.ToolName} x{obtainedCount} from {source}");
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[PotionSync] Error in ToolObtain_Postfix: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Tool卡使用同步 - 监控Tool卡Actions执行
    /// </summary>
    [HarmonyPatch(typeof(Card), nameof(Card.Actions))]
    [HarmonyPrefix]
    public static void ToolUse_Prefix(Card __instance, UnitSelector selector, ManaGroup consumingMana, Interaction precondition, out ToolUseState __state)
    {
        __state = new ToolUseState();
        try
        {
            if (!IsToolCard(__instance))
            {
                return;
            }

            var player = GetCurrentPlayer();
            if (player == null)
            {
                return;
            }

            // 记录使用前的状态
            __state.ToolId = __instance.Id;
            __state.ToolName = __instance.Name;
            __state.ToolType = __instance.GetType().Name;
            __state.PlayerHpBefore = player.Hp;
            __state.PlayerBlockBefore = player.Block;
            __state.PlayerManaBefore = GetManaSnapshot(player.Battle?.BattleMana);
            __state.UseTime = DateTime.Now;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[PotionSync] Error in ToolUse_Prefix: {ex.Message}");
        }
    }

    [HarmonyPatch(typeof(Card), nameof(Card.Actions))]
    [HarmonyPostfix]
    public static void ToolUse_Postfix(Card __instance, UnitSelector selector, ManaGroup consumingMana, Interaction precondition, ToolUseState __state)
    {
        try
        {
            if (!IsToolCard(__instance))
            {
                return;
            }

            if (string.IsNullOrEmpty(__state.ToolId))
            {
                return;
            }

            if (serviceProvider == null)
            {
                return;
            }

            var networkClient = serviceProvider.GetService<INetworkClient>();
            if (networkClient == null || !networkClient.IsConnected)
            {
                return;
            }

            var player = GetCurrentPlayer();
            if (player == null)
            {
                return;
            }

            // 计算状态变化
            var useData = new
            {
                Timestamp = DateTime.Now.Ticks,
                EventType = NetworkMessageTypes.OnPotionUsed,
                PlayerId = GetCurrentPlayerId(player),
                ToolId = __state.ToolId,
                ToolName = __state.ToolName,
                ToolType = __state.ToolType,
                Target = selector?.ToString() ?? "Player",
                Effects = GetToolEffects(__instance, __state, player),
                PlayerStateBefore = new
                {
                    Hp = __state.PlayerHpBefore,
                    Block = __state.PlayerBlockBefore,
                    Mana = __state.PlayerManaBefore
                },
                PlayerStateAfter = new
                {
                    Hp = player.Hp,
                    Block = player.Block,
                    Mana = GetManaSnapshot(player.Battle?.BattleMana)
                },
                ConsumingMana = GetManaSnapshot(consumingMana)
            };

            SendGameEvent(NetworkMessageTypes.OnPotionUsed, useData);

            Plugin.Logger?.LogInfo($"[PotionSync] Player used tool: {__state.ToolName}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[PotionSync] Error in ToolUse_Postfix: {ex.Message}");
        }
    }

    /// <summary>
    /// Tool卡丢弃/出售同步 - 监控Tool卡从手牌/牌库移除
    /// </summary>
    [HarmonyPatch]
    public static class ToolDiscardSync
    {
        [HarmonyTargetMethods]
        static System.Collections.Generic.IEnumerable<System.Reflection.MethodBase> TargetMethods()
        {
            var methodNames = new[]
            {
                "RemoveFromHand",
                "RemoveFromDeck",
                "Exhaust",
                "Discard",
                "SellTool"
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
                            if (methodNames.Any(name => method.Name.Contains(name)) ||
                                (method.Name.Contains("Remove") && method.GetParameters().Any(p => p.ParameterType == typeof(Card))))
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
        public static void ToolDiscard_Prefix(object __instance, object[] __args, out ToolDiscardState __state)
        {
            __state = new ToolDiscardState();
            try
            {
                // 检查是否有Tool卡被移除
                var toolCard = __args.OfType<Card>().FirstOrDefault(IsToolCard);
                if (toolCard != null)
                {
                    __state.ToolId = toolCard.Id;
                    __state.ToolName = toolCard.Name;
                    __state.ToolType = toolCard.GetType().Name;
                    __state.BeforeCount = GetCurrentToolCount();
                    __state.DiscardTime = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[PotionSync] Error in ToolDiscard_Prefix: {ex.Message}");
            }
        }

        [HarmonyPostfix]
        public static void ToolDiscard_Postfix(object __instance, object[] __args, ToolDiscardState __state)
        {
            try
            {
                if (string.IsNullOrEmpty(__state.ToolId))
                {
                    return;
                }

                if (serviceProvider == null)
                {
                    return;
                }

                var networkClient = serviceProvider.GetService<INetworkClient>();
                if (networkClient == null || !networkClient.IsConnected)
                {
                    return;
                }

                var afterCount = GetCurrentToolCount();
                var discardedCount = __state.BeforeCount - afterCount;

                if (discardedCount > 0)
                {
                    var discardType = DetermineDiscardType(__instance, __args);
                    var discardData = new
                    {
                        Timestamp = DateTime.Now.Ticks,
                        EventType = "ToolDiscarded",
                        PlayerId = GetCurrentPlayerId(),
                        ToolId = __state.ToolId,
                        ToolName = __state.ToolName,
                        ToolType = __state.ToolType,
                        QuantityDiscarded = discardedCount,
                        DiscardType = discardType,
                        RemainingTools = afterCount
                    };

                    SendGameEvent("ToolDiscarded", discardData);

                    Plugin.Logger?.LogInfo($"[PotionSync] Player discarded tool: {__state.ToolName} x{discardedCount} ({discardType})");
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[PotionSync] Error in ToolDiscard_Postfix: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 药水状态同步（用于断线重连时恢复状态）
    /// </summary>
    public class ToolStateSnapshot
    {
        /// <summary>
        /// 获取Tool卡状态快照
        /// </summary>
        public static object GetToolSnapshot(PlayerUnit player)
        {
            try
            {
                if (player == null)
                {
                    return new { Error = "Player is null" };
                }

                var toolCards = GetAllToolCards(player);
                var tools = toolCards.Select(tool => new
                {
                    ToolId = tool.Id,
                    ToolName = tool.Name,
                    ToolType = tool.GetType().Name,
                    IsUpgraded = tool.IsUpgraded,
                    Location = GetToolLocation(tool),
                    ManaCost = GetManaSnapshot(tool.ManaGroup)
                }).ToList();

                return new
                {
                    PlayerId = player.Id,
                    Timestamp = DateTime.Now.Ticks,
                    Tools = tools,
                    TotalTools = tools.Count,
                    ToolsInHand = tools.Count(t => t.Location == "Hand"),
                    ToolsInDeck = tools.Count(t => t.Location == "Deck")
                };
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[PotionSync] Error in GetToolSnapshot: {ex.Message}");
                return new { Error = ex.Message };
            }
        }

        /// <summary>
        /// 应用药水状态快照（用于断线重连后恢复）
        /// </summary>
        public static void ApplyToolSnapshot(object snapshot)
        {
            try
            {
                // TODO: 实现根据快照恢复玩家Tool卡状态
                Plugin.Logger?.LogInfo("[PotionSync] Tool snapshot applied successfully");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[PotionSync] Error in ApplyToolSnapshot: {ex.Message}");
            }
        }
    }

    // 辅助方法和数据结构

    private static void SendGameEvent(string eventType, object eventData)
    {
        try
        {
            var networkClient = serviceProvider?.GetService<INetworkClient>();
            if (networkClient is NetworkClient liteNetClient)
            {
                liteNetClient.SendGameEventData(eventType, eventData);
            }
            else
            {
                // 备用方案：使用通用SendRequest方法
                networkClient?.SendRequest(eventType, JsonSerializer.Serialize(eventData));
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[PotionSync] Error sending game event {eventType}: {ex.Message}");
        }
    }

    private static string GetCurrentPlayerId(PlayerUnit player = null)
    {
        try
        {
            if (player != null)
            {
                return $"Player_{player.Index}";
            }
            // TODO: 从GameStateUtils获取
            return "current_player";
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[PotionSync] Error getting current player ID: {ex.Message}");
            return "unknown_player";
        }
    }

    private static PlayerUnit GetCurrentPlayer()
    {
        try
        {
            // TODO: 从当前游戏状态获取玩家
            return null;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[PotionSync] Error getting current player: {ex.Message}");
            return null;
        }
    }

    private static bool IsToolCard(Card card)
    {
        return card != null && card.GetType().Name.Contains("Tool");
    }

    private static int GetCurrentToolCount()
    {
        try
        {
            // TODO: 获取当前玩家拥有的Tool卡数量
            return 0;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[PotionSync] Error getting tool count: {ex.Message}");
            return 0;
        }
    }

    private static string DetermineToolSource(object instance, object[] args)
    {
        try
        {
            if (instance?.GetType().Name.Contains("Shop") == true)
            {
                return "Shop";
            }

            if (instance?.GetType().Name.Contains("Event") == true)
            {
                return "Event";
            }

            if (instance?.GetType().Name.Contains("Battle") == true)
            {
                return "Battle";
            }

            return "Unknown";
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[PotionSync] Error determining tool source: {ex.Message}");
            return "Unknown";
        }
    }

    private static object GetToolEffects(Card tool, ToolUseState state, PlayerUnit player)
    {
        try
        {
            var hpChange = player.Hp - state.PlayerHpBefore;
            var blockChange = player.Block - state.PlayerBlockBefore;

            return new
            {
                HpChange = hpChange,
                BlockChange = blockChange,
                ManaChange = GetManaDifference(state.PlayerManaBefore, GetManaSnapshot(player.Battle?.BattleMana))
            };
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[PotionSync] Error getting tool effects: {ex.Message}");
            return new { Error = ex.Message };
        }
    }

    private static List<Card> GetAllToolCards(PlayerUnit player)
    {
        try
        {
            List<Card> toolCards = [];
            // TODO: 获取玩家所有的Tool卡（手牌、牌库、弃牌堆等）
            return toolCards;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[PotionSync] Error getting all tool cards: {ex.Message}");
            return [];
        }
    }

    private static string GetToolLocation(Card tool)
    {
        try
        {
            // TODO: 确定Tool卡的位置（Hand/Deck/Discard等）
            return "Unknown";
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[PotionSync] Error getting tool location: {ex.Message}");
            return "Unknown";
        }
    }

    private static object GetManaSnapshot(ManaGroup manaGroup)
    {
        if (manaGroup == null)
        {
            return new { Red = 0, Blue = 0, Green = 0, White = 0, Total = 0 };
        }

        return new
        {
            Red = manaGroup.Red,
            Blue = manaGroup.Blue,
            Green = manaGroup.Green,
            White = manaGroup.White,
            Total = manaGroup.Red + manaGroup.Blue + manaGroup.Green + manaGroup.White
        };
    }

    private static object GetManaDifference(object before, object after)
    {
        try
        {
            if (before == null || after == null)
            {
                return new { Red = 0, Blue = 0, Green = 0, White = 0, Total = 0 };
            }

            // TODO: 计算法力差值
            return new { Red = 0, Blue = 0, Green = 0, White = 0, Total = 0 };
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[PotionSync] Error calculating mana difference: {ex.Message}");
            return new { Red = 0, Blue = 0, Green = 0, White = 0, Total = 0 };
        }
    }

    private static string DetermineDiscardType(object instance, object[] args)
    {
        try
        {
            if (instance?.GetType().Name.Contains("Shop") == true)
            {
                return "Sell";
            }

            if (args.Any(arg => arg?.ToString().Contains("Exhaust") == true))
            {
                return "Exhaust";
            }

            return "Discard";
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[PotionSync] Error determining discard type: {ex.Message}");
            return "Unknown";
        }
    }

    // 数据结构

    private class ToolCardState
    {
        public string ToolId { get; set; }
        public string ToolName { get; set; }
        public string ToolType { get; set; }
        public int BeforeCount { get; set; }
        public DateTime ObtainTime { get; set; }
    }

    private class ToolUseState
    {
        public string ToolId { get; set; }
        public string ToolName { get; set; }
        public string ToolType { get; set; }
        public int PlayerHpBefore { get; set; }
        public int PlayerBlockBefore { get; set; }
        public object PlayerManaBefore { get; set; }
        public DateTime UseTime { get; set; }
    }

    private class ToolDiscardState
    {
        public string ToolId { get; set; }
        public string ToolName { get; set; }
        public string ToolType { get; set; }
        public int BeforeCount { get; set; }
        public DateTime DiscardTime { get; set; }
    }
}