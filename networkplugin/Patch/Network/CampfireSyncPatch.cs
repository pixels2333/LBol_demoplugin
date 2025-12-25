using System;
using System.Collections.Generic;
using System.Text.Json;
using HarmonyLib;
using LBoL.Core.GapOptions;
using LBoL.Core.Stations;
using LBoL.Core.Units;
using LBoL.Presentation.UI.Panels;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Patch.Network;

/// <summary>
/// GapStation同步补丁 - 同步喝茶、升级卡牌、移除卡牌等休息点操作
/// </summary>
public class CampfireSyncPatch
{
    private static IServiceProvider serviceProvider => ModService.ServiceProvider;

    /// <summary>
    /// GapStation.OnEnter同步补丁 - 进入休息点时同步可用选项
    /// </summary>
    [HarmonyPatch(typeof(GapStation), "OnEnter")]
    [HarmonyPostfix]
    public static void GapStation_OnEnter_Postfix(GapStation __instance)
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

            List<object> gapOptions = new List<object>();
            foreach (var option in __instance.GapOptions)
            {
                gapOptions.Add(new
                {
                    OptionType = option.GetType().Name,
                    OptionId = option.UniqueId,
                    DisplayName = option.Name // TODO: 确认GapOption有Name属性
                });
            }

            var stationData = new
            {
                Timestamp = DateTime.Now.Ticks,
                EventType = "GapStationEntered",
                AvailableOptions = gapOptions,
                StationType = "Gap",
                PlayerId = GetCurrentPlayerId()
            };

            string json = JsonSerializer.Serialize(stationData);
            networkClient.SendRequest("GapStationEntered", json);

            Plugin.Logger?.LogInfo($"[CampfireSync] GapStation entered with {__instance.GapOptions.Count} options");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[CampfireSync] Error in GapStation_OnEnter_Postfix: {ex.Message}");
        }
    }

    /// <summary>
    /// 喝茶（DrinkTea）同步补丁
    /// </summary>
    [HarmonyPatch(typeof(GapStation), "DrinkTea")]
    [HarmonyPrefix]
    public static void GapStation_DrinkTea_Prefix(GapStation __instance, DrinkTea drinkTea)
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

            var gameRun = __instance.GameRun;
            var player = gameRun.Player;

            var teaData = new
            {
                Timestamp = DateTime.Now.Ticks,
                EventType = "DrinkTeaStarted",
                HealValue = drinkTea.Value + drinkTea.AdditionalHeal,
                AdditionalPower = drinkTea.AdditionalPower,
                AdditionalCardReward = drinkTea.AdditionalCardReward,
                PlayerState = new
                {
                    Hp = player.Hp,
                    MaxHp = player.MaxHp,
                    Power = gameRun.Power // TODO: 确认GameRun有Power属性
                },
                PlayerId = GetCurrentPlayerId()
            };

            string json = JsonSerializer.Serialize(teaData);
            networkClient.SendRequest("DrinkTeaStarted", json);

            Plugin.Logger?.LogInfo($"[CampfireSync] DrinkTea started: Heal +{teaData.HealValue}, Power +{teaData.AdditionalPower}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[CampfireSync] Error in GapStation_DrinkTea_Prefix: {ex.Message}");
        }
    }

    [HarmonyPatch(typeof(GapStation), "DrinkTea")]
    [HarmonyPostfix]
    public static void GapStation_DrinkTea_Postfix(GapStation __instance, DrinkTea drinkTea)
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

            var gameRun = __instance.GameRun;
            var player = gameRun.Player;

            var teaData = new
            {
                Timestamp = DateTime.Now.Ticks,
                EventType = "DrinkTeaCompleted",
                HealValue = drinkTea.Value + drinkTea.AdditionalHeal,
                AdditionalPower = drinkTea.AdditionalPower,
                AdditionalCardReward = drinkTea.AdditionalCardReward,
                PlayerState = new
                {
                    Hp = player.Hp,
                    MaxHp = player.MaxHp,
                    Power = gameRun.Power
                },
                PlayerId = GetCurrentPlayerId()
            };

            string json = JsonSerializer.Serialize(teaData);
            networkClient.SendRequest("DrinkTeaCompleted", json);

            Plugin.Logger?.LogInfo($"[CampfireSync] DrinkTea completed");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[CampfireSync] Error in GapStation_DrinkTea_Postfix: {ex.Message}");
        }
    }

    /// <summary>
    /// TODO: 找到LBoL中的休息节点/篝火点
    /// 在杀戮尖塔中是CampfireUI，在LBoL中可能是GapStation或其他休息节点
    /// </summary>
    public class RestOptionSync
    {
        // LBoL中的休息可能对应以下几种情况：
        // 1. GapStation中的休息选项
        // 2. 特殊的休息事件
        // 3. SupplyStation（补给站）

        // TODO: Patch休息选项的选择
        // 需要找到玩家选择休息的逻辑

        // TODO: Patch休息效果的实际应用（HP回复等）

        /// <summary>
        /// 同步休息选项选择
        /// </summary>
        public static void SyncRestSelection(string restOptionId, string restOptionName)
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

                var restData = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    RestOptionId = restOptionId,
                    RestOptionName = restOptionName,
                    PlayerId = GetCurrentPlayerId()
                };

                string json = JsonSerializer.Serialize(restData);
                networkClient.SendRequest("CampfireRestSelected", json);

                Plugin.Logger?.LogInfo($"[CampfireSync] Player selected rest option: {restOptionName}");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[CampfireSync] Error in SyncRestSelection: {ex.Message}");
            }
        }

        /// <summary>
        /// 同步休息结果（HP回复等）
        /// </summary>
        public static void SyncRestResult(int hpGained, int maxHpGained, PlayerUnit player)
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

                var resultData = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    PlayerId = player.Id,
                    HpBefore = player.Hp - hpGained,
                    HpAfter = player.Hp,
                    HpGained = hpGained,
                    MaxHpBefore = player.MaxHp - maxHpGained,
                    MaxHpAfter = player.MaxHp,
                    MaxHpGained = maxHpGained
                };

                string json = JsonSerializer.Serialize(resultData);
                networkClient.SendRequest("CampfireRestResult", json);

                Plugin.Logger?.LogInfo($"[CampfireSync] Rest result: HP +{hpGained}, MaxHP +{maxHpGained}");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[CampfireSync] Error in SyncRestResult: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 卡牌升级同步补丁 - 完成UpgradeCard选项的实际补丁
    /// </summary>
    [HarmonyPatch(typeof(UpgradeCard), "RunAction")]
    [HarmonyPrefix]
    public static void UpgradeCard_RunAction_Prefix(UpgradeCard __instance)
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

            var gameRun = __instance.GameRun;
            var player = gameRun?.Player;
            if (player == null)
            {
                return;
            }

            var upgradeData = new
            {
                Timestamp = DateTime.Now.Ticks,
                EventType = "UpgradeCardStarted",
                PlayerId = GetCurrentPlayerId(),
                PlayerState = new
                {
                    Hp = player.Hp,
                    MaxHp = player.MaxHp,
                    Power = gameRun?.Power ?? 0,
                    CardsInDeck = gameRun?.DeckZone?.Count ?? 0
                },
                UpgradeOptionType = "UpgradeCard"
            };

            SendGameEvent("UpgradeCardStarted", upgradeData);

            Plugin.Logger?.LogInfo("[CampfireSync] UpgradeCard action started");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[CampfireSync] Error in UpgradeCard_RunAction_Prefix: {ex.Message}");
        }
    }

    [HarmonyPatch(typeof(UpgradeCard), "RunAction")]
    [HarmonyPostfix]
    public static void UpgradeCard_RunAction_Postfix(UpgradeCard __instance)
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

            var gameRun = __instance.GameRun;
            var player = gameRun?.Player;
            if (player == null)
            {
                return;
            }

            var upgradeData = new
            {
                Timestamp = DateTime.Now.Ticks,
                EventType = "UpgradeCardCompleted",
                PlayerId = GetCurrentPlayerId(),
                PlayerState = new
                {
                    Hp = player.Hp,
                    MaxHp = player.MaxHp,
                    Power = gameRun?.Power ?? 0,
                    CardsInDeck = gameRun?.DeckZone?.Count ?? 0
                },
                UpgradeOptionType = "UpgradeCard"
            };

            SendGameEvent("UpgradeCardCompleted", upgradeData);

            Plugin.Logger?.LogInfo("[CampfireSync] UpgradeCard action completed");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[CampfireSync] Error in UpgradeCard_RunAction_Postfix: {ex.Message}");
        }
    }

    /// <summary>
    /// 卡牌移除同步补丁 - 完成RemoveCard选项的实际补丁
    /// </summary>
    [HarmonyPatch(typeof(RemoveCard), "RunAction")]
    [HarmonyPrefix]
    public static void RemoveCard_RunAction_Prefix(RemoveCard __instance)
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

            var gameRun = __instance.GameRun;
            var player = gameRun?.Player;
            if (player == null)
            {
                return;
            }

            var removeData = new
            {
                Timestamp = DateTime.Now.Ticks,
                EventType = "RemoveCardStarted",
                PlayerId = GetCurrentPlayerId(),
                PlayerState = new
                {
                    Hp = player.Hp,
                    MaxHp = player.MaxHp,
                    Power = gameRun?.Power ?? 0,
                    CardsInDeck = gameRun?.DeckZone?.Count ?? 0
                },
                RemoveOptionType = "RemoveCard"
            };

            SendGameEvent("RemoveCardStarted", removeData);

            Plugin.Logger?.LogInfo("[CampfireSync] RemoveCard action started");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[CampfireSync] Error in RemoveCard_RunAction_Prefix: {ex.Message}");
        }
    }

    [HarmonyPatch(typeof(RemoveCard), "RunAction")]
    [HarmonyPostfix]
    public static void RemoveCard_RunAction_Postfix(RemoveCard __instance)
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

            var gameRun = __instance.GameRun;
            var player = gameRun?.Player;
            if (player == null)
            {
                return;
            }

            var removeData = new
            {
                Timestamp = DateTime.Now.Ticks,
                EventType = "RemoveCardCompleted",
                PlayerId = GetCurrentPlayerId(),
                PlayerState = new
                {
                    Hp = player.Hp,
                    MaxHp = player.MaxHp,
                    Power = gameRun?.Power ?? 0,
                    CardsInDeck = gameRun?.DeckZone?.Count ?? 0
                },
                RemoveOptionType = "RemoveCard"
            };

            SendGameEvent("RemoveCardCompleted", removeData);

            Plugin.Logger?.LogInfo("[CampfireSync] RemoveCard action completed");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[CampfireSync] Error in RemoveCard_RunAction_Postfix: {ex.Message}");
        }
    }

    /// <summary>
    /// 寻找宝物同步补丁 - 完成FindExhibit选项的实际补丁
    /// </summary>
    [HarmonyPatch(typeof(FindExhibit), "RunAction")]
    [HarmonyPrefix]
    public static void FindExhibit_RunAction_Prefix(FindExhibit __instance)
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

            var gameRun = __instance.GameRun;
            var player = gameRun?.Player;
            if (player == null)
            {
                return;
            }

            var exhibitData = new
            {
                Timestamp = DateTime.Now.Ticks,
                EventType = "FindExhibitStarted",
                PlayerId = GetCurrentPlayerId(),
                PlayerState = new
                {
                    Hp = player.Hp,
                    MaxHp = player.MaxHp,
                    Power = gameRun?.Power ?? 0,
                    ExhibitsCount = player?.Exhibits?.Count ?? 0
                },
                ExhibitOptionType = "FindExhibit"
            };

            SendGameEvent("FindExhibitStarted", exhibitData);

            Plugin.Logger?.LogInfo("[CampfireSync] FindExhibit action started");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[CampfireSync] Error in FindExhibit_RunAction_Prefix: {ex.Message}");
        }
    }

    [HarmonyPatch(typeof(FindExhibit), "RunAction")]
    [HarmonyPostfix]
    public static void FindExhibit_RunAction_Postfix(FindExhibit __instance)
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

            var gameRun = __instance.GameRun;
            var player = gameRun?.Player;
            if (player == null)
            {
                return;
            }

            var exhibitData = new
            {
                Timestamp = DateTime.Now.Ticks,
                EventType = "FindExhibitCompleted",
                PlayerId = GetCurrentPlayerId(),
                PlayerState = new
                {
                    Hp = player.Hp,
                    MaxHp = player.MaxHp,
                    Power = gameRun?.Power ?? 0,
                    ExhibitsCount = player?.Exhibits?.Count ?? 0
                },
                ExhibitOptionType = "FindExhibit"
            };

            SendGameEvent("FindExhibitCompleted", exhibitData);

            Plugin.Logger?.LogInfo("[CampfireSync] FindExhibit action completed");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[CampfireSync] Error in FindExhibit_RunAction_Postfix: {ex.Message}");
        }
    }

    /// <summary>
    /// 卡牌移除同步
    /// TODO: Patch LBoL中的卡牌移除逻辑
    /// 可能在GapStation的RemoveCard选项中
    /// </summary>
    public class PurgeOptionSync
{
    // LBoL中的卡牌移除可能在：
    // 1. RemoveCard（移除卡牌）选项
    // 2. 商店的移除服务
    // 3. 某些事件的移除选项

    /// <summary>
    /// 同步卡牌移除
    /// </summary>
    public static void SyncRemoveCard(string cardId, string cardName)
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
                CardId = cardId,
                CardName = cardName,
                PlayerId = GetCurrentPlayerId()
            };

                string json = JsonSerializer.Serialize(removeData);
            networkClient.SendRequest("CampfireRemoveCard", json);

            Plugin.Logger?.LogInfo($"[CampfireSync] Player removed card: {cardName}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[CampfireSync] Error in SyncRemoveCard: {ex.Message}");
        }
    }
}

/// <summary>
/// 挖掘同步（获取随机宝物）
/// TODO: Patch LBoL中的挖掘逻辑（如果存在）
/// 可能在某些特殊节点或事件中
/// </summary>
public class DigOptionSync
{
    /// <summary>
    /// 同步挖掘结果
    /// </summary>
    public static void SyncDigResult(string treasureType, string treasureName, PlayerUnit player)
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

            var digData = new
            {
                Timestamp = DateTime.Now.Ticks,
                PlayerId = player.Id,
                TreasureType = treasureType,
                TreasureName = treasureName
            };

                string json = JsonSerializer.Serialize(digData);
            networkClient.SendRequest("CampfireDigResult", json);

            Plugin.Logger?.LogInfo($"[CampfireSync] Dig result: {treasureName} ({treasureType})");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[CampfireSync] Error in SyncDigResult: {ex.Message}");
        }
    }
}

/// <summary>
/// 特殊选项同步
/// TODO: Patch其他特殊选项（如祈祷、回忆等，如果存在）
/// </summary>
public class SpecialOptionSync
{
    /// <summary>
    /// 同步特殊选项选择
    /// </summary>
    public static void SyncSpecialOption(string optionId, string optionName, Dictionary<string, object> effects)
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

            var optionData = new
            {
                Timestamp = DateTime.Now.Ticks,
                OptionId = optionId,
                OptionName = optionName,
                PlayerId = GetCurrentPlayerId(),
                Effects = effects
            };

                string json = JsonSerializer.Serialize(optionData);
            networkClient.SendRequest("CampfireSpecialOption", json);

            Plugin.Logger?.LogInfo($"[CampfireSync] Special option selected: {optionName}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[CampfireSync] Error in SyncSpecialOption: {ex.Message}");
        }
    }
}

/// <summary>
/// 处理多人选择冲突
/// TODO: 实现冲突解决机制
/// 当多个玩家同时选择不同选项时的处理
/// </summary>
public class CampfireConflictResolver
{
    /// <summary>
    /// 篝火选项的选择状态
    /// </summary>
    private static Dictionary<string, string> _playerSelections = new Dictionary<string, string>();

    /// <summary>
    /// 记录玩家选择
    /// </summary>
    public static void RecordPlayerSelection(string playerId, string optionId)
    {
        _playerSelections[playerId] = optionId;

        // TODO: 检查是否所有玩家都已选择
        // 如果是，则决定是否应用或进行投票
    }

    /// <summary>
    /// 检查是否所有在火堆旁的玩员都已选择
    /// </summary>
    public static bool AllPlayersSelected()
    {
        // TODO: 实现逻辑检查
        // 获取当前位置的所有玩家
        // 检查是否都有选择记录

        return false;
    }

    /// <summary>
    /// 解决冲突（投票或主机决定）
    /// </summary>
    public static string ResolveConflict()
    {
        // TODO: 实现冲突解决
        return string.Empty;
    }

    /// <summary>
    /// 清空选择记录（离开火堆后调用）
    /// </summary>
    public static void ClearSelections()
    {
        _playerSelections.Clear();
    }

    /// <summary>
    /// 辅助方法：获取当前玩家ID
    /// </summary>
    private static string GetCurrentPlayerId()
    {
        try
        {
            // 使用GameStateUtils获取玩家ID
            return GameStateUtils.GetCurrentPlayerId();
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[CampfireSync] Error getting current player ID: {ex.Message}");
            return "unknown_player";
        }
    }

    /// <summary>
    /// 发送游戏事件
    /// </summary>
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
            Plugin.Logger?.LogError($"[CampfireSync] Error sending game event {eventType}: {ex.Message}");
        }
    }
