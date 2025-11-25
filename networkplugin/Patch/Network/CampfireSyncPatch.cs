using HarmonyLib;
using LBoL.Core;
using LBoL.Core.GapOptions;
using LBoL.Core.Units;
using System;
using System.Collections.Generic;
using System.Text.Json;
using NetworkPlugin.Network.Client;
using Microsoft.Extensions.DependencyInjection;

namespace NetworkPlugin.Patch.Network;

/// <summary>
/// 篝火选项同步补丁 - 同步休息、升级、挖掘等操作
/// 参考杀戮尖塔CampfireOptionsPatches
/// 重要性: ⭐⭐⭐⭐⭐ (核心游戏循环)
/// TODO: 需要找到LBoL中对应的休息/篝火点逻辑
/// </summary>
public class CampfireSyncPatch
{
    private static IServiceProvider serviceProvider => ModService.ServiceProvider;

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
                if (serviceProvider == null) return;

                var networkClient = serviceProvider.GetService<INetworkClient>();
                if (networkClient == null || !networkClient.IsConnected)
                    return;

                var restData = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    RestOptionId = restOptionId,
                    RestOptionName = restOptionName,
                    PlayerId = GetCurrentPlayerId()
                };

                var json = JsonSerializer.Serialize(restData);
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
                if (serviceProvider == null) return;

                var networkClient = serviceProvider.GetService<INetworkClient>();
                if (networkClient == null || !networkClient.IsConnected)
                    return;

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

                var json = JsonSerializer.Serialize(resultData);
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
    /// 卡牌升级同步
    /// TODO: Patch LBoL中的卡牌升级逻辑
    /// 可能在GapStation的UpgradeCard选项中
    /// </summary>
    public class SmithOptionSync
    {
        // LBoL中的卡牌升级可能在：
        // 1. UpgradeCard（升级卡牌）选项
        // 2. 某些特殊事件中的升级
        // 3. 商店的升级服务

        // TODO: Patch UpgradeCard选项的选择
        // TODO: Patch卡牌升级的实际执行

        /// <summary>
        /// 同步卡牌升级选择
        /// </summary>
        public static void SyncUpgradeSelection(string cardId, string cardName)
        {
            try
            {
                if (serviceProvider == null) return;

                var networkClient = serviceProvider.GetService<INetworkClient>();
                if (networkClient == null || !networkClient.IsConnected)
                    return;

                var upgradeData = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    CardId = cardId,
                    CardName = cardName,
                    PlayerId = GetCurrentPlayerId()
                };

                var json = JsonSerializer.Serialize(upgradeData);
                networkClient.SendRequest("CampfireUpgradeSelected", json);

                Plugin.Logger?.LogInfo($"[CampfireSync] Player selected upgrade for card: {cardName}");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[CampfireSync] Error in SyncUpgradeSelection: {ex.Message}");
            }
        }

        /// <summary>
        /// 同步卡牌升级完成
        /// </summary>
        public static void SyncUpgradeComplete(string cardId, string cardName, int upgradeCount)
        {
            try
            {
                if (serviceProvider == null) return;

                var networkClient = serviceProvider.GetService<INetworkClient>();
                if (networkClient == null || !networkClient.IsConnected)
                    return;

                var resultData = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    CardId = cardId,
                    CardName = cardName,
                    UpgradeCount = upgradeCount
                };

                var json = JsonSerializer.Serialize(resultData);
                networkClient.SendRequest("CampfireUpgradeComplete", json);

                Plugin.Logger?.LogInfo($"[CampfireSync] Card upgraded: {cardName} ({upgradeCount} upgrades)");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[CampfireSync] Error in SyncUpgradeComplete: {ex.Message}");
            }
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
                if (serviceProvider == null) return;

                var networkClient = serviceProvider.GetService<INetworkClient>();
                if (networkClient == null || !networkClient.IsConnected)
                    return;

                var removeData = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    CardId = cardId,
                    CardName = cardName,
                    PlayerId = GetCurrentPlayerId()
                };

                var json = JsonSerializer.Serialize(removeData);
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
                if (serviceProvider == null) return;

                var networkClient = serviceProvider.GetService<INetworkClient>();
                if (networkClient == null || !networkClient.IsConnected)
                    return;

                var digData = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    PlayerId = player.Id,
                    TreasureType = treasureType,
                    TreasureName = treasureName
                };

                var json = JsonSerializer.Serialize(digData);
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
                if (serviceProvider == null) return;

                var networkClient = serviceProvider.GetService<INetworkClient>();
                if (networkClient == null || !networkClient.IsConnected)
                    return;

                var optionData = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    OptionId = optionId,
                    OptionName = optionName,
                    PlayerId = GetCurrentPlayerId(),
                    Effects = effects
                };

                var json = JsonSerializer.Serialize(optionData);
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
        private static Dictionary<string, string> _playerSelections = new();

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
            // 方案1: 多数投票
            // 方案2: 主机决定
            // 方案3: 随机选择

            return string.Empty;
        }

        /// <summary>
        /// 清空选择记录（离开火堆后调用）
        /// </summary>
        public static void ClearSelections()
        {
            _playerSelections.Clear();
        }
    }

    /// <summary>
    /// 获取当前玩家ID
    /// TODO: 实现获取当前玩家ID的方法
    /// </summary>
    private static string GetCurrentPlayerId()
    {
        // TODO: 从GameRun或NetworkClient获取
        return "current_player";
    }

    /// <summary>
    /// 构建完整的篝火状态快照
    /// 用于断线重连时恢复状态
    /// </summary>
    public static object BuildCampfireSnapshot()
    {
        // TODO: 实现快照构建
        // 包括：当前位置、可用选项、玩家选择状态等

        return new
        {
            Timestamp = DateTime.Now.Ticks,
            // TODO: 添加实际数据
        };
    }
}
