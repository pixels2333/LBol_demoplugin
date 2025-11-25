using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Adventures;
using System;
using System.Collections.Generic;
using System.Text.Json;
using NetworkPlugin.Network.Client;
using Microsoft.Extensions.DependencyInjection;

namespace NetworkPlugin.Patch.Network;

/// <summary>
/// 事件/对话同步补丁 - 同步事件选择、对话选项、事件结果
/// 参考杀戮尖塔NeowBlessingPatches, TalkPatches
/// 重要性: ⭐⭐⭐⭐⭐ (进度推进)
/// </summary>
public class EventSyncPatch
{
    private static IServiceProvider serviceProvider => ModService.ServiceProvider;

    /// <summary>
    /// 事件初始化同步
    /// TODO: 找到LBoL事件开始时的入口点
    /// 当玩家进入事件节点或触发事件时调用
    /// </summary>
    public class EventInitSync
    {
        // LBoL中的事件系统：
        // - Adventure类是事件基类
        // - 在地图节点触发事件
        // - 有对话和选项系统

        /// <summary>
        /// 同步事件开始
        /// </summary>
        public static void SyncEventStart(Adventure adventure, string eventId, string eventName)
        {
            try
            {
                if (serviceProvider == null) return;

                var networkClient = serviceProvider.GetService<INetworkClient>();
                if (networkClient == null || !networkClient.IsConnected)
                    return;

                var eventData = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    EventId = eventId,
                    EventName = eventName,
                    EventType = adventure.GetType().Name,
                    PlayerId = GetCurrentPlayerId()
                };

                var json = JsonSerializer.Serialize(eventData);
                networkClient.SendRequest("EventStart", json);

                Plugin.Logger?.LogInfo($"[EventSync] Event started: {eventName} (ID: {eventId})");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[EventSync] Error in SyncEventStart: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 事件选择同步
    /// TODO: Patch事件选项选择逻辑
    /// 当玩家在事件中做出选择时触发
    /// </summary>
    public class EventSelectionSync
    {
        // LBoL中的事件选择：
        // - 通过DialogOptionsPhase处理选项
        // - 玩家选择特定选项触发不同结果

        /// <summary>
        /// 同步事件选项选择
        /// </summary>
        public static void SyncEventSelection(string eventId, int optionIndex, string optionText, string optionResult)
        {
            try
            {
                if (serviceProvider == null) return;

                var networkClient = serviceProvider.GetService<INetworkClient>();
                if (networkClient == null || !networkClient.IsConnected)
                    return;

                var selectionData = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    EventId = eventId,
                    OptionIndex = optionIndex,
                    OptionText = optionText,
                    OptionResult = optionResult,
                    PlayerId = GetCurrentPlayerId()
                };

                var json = JsonSerializer.Serialize(selectionData);
                networkClient.SendRequest("EventSelection", json);

                Plugin.Logger?.LogInfo($"[EventSync] Event option selected: {optionText} -> {optionResult}");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[EventSync] Error in SyncEventSelection: {ex.Message}");
            }
        }

        /// <summary>
        /// 同步事件结果
        /// 在选择选项后，事件发生的结果
        /// </summary>
        public static void SyncEventResult(string eventId, Dictionary<string, object> effects)
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
                    EventId = eventId,
                    Effects = effects
                };

                var json = JsonSerializer.Serialize(resultData);
                networkClient.SendRequest("EventResult", json);

                Plugin.Logger?.LogInfo($"[EventSync] Event result applied: {eventId}");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[EventSync] Error in SyncEventResult: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 对话同步
    /// TODO: 同步对话文本和进度
    /// 当事件中的对话推进时触发
    /// </summary>
    public class DialogSync
    {
        // LBoL中的对话系统：
        // - DialogRunner管理对话流程
        // - DialogPhase表示对话阶段
        // - 有文本、选项、分支等

        /// <summary>
        /// 同步对话文本
        /// </summary>
        public static void SyncDialogText(string eventId, string speaker, string text, int dialogIndex)
        {
            try
            {
                if (serviceProvider == null) return;

                var networkClient = serviceProvider.GetService<INetworkClient>();
                if (networkClient == null || !networkClient.IsConnected)
                    return;

                var dialogData = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    EventId = eventId,
                    Speaker = speaker,
                    Text = text,
                    DialogIndex = dialogIndex
                };

                var json = JsonSerializer.Serialize(dialogData);
                networkClient.SendRequest("DialogText", json);

                Plugin.Logger?.LogDebug($"[EventSync] Dialog [{dialogIndex}] {speaker}: {text}");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[EventSync] Error in SyncDialogText: {ex.Message}");
            }
        }

        /// <summary>
        /// 同步对话选项
        /// </summary>
        public static void SyncDialogOptions(string eventId, List<DialogOptionData> options)
        {
            try
            {
                if (serviceProvider == null) return;

                var networkClient = serviceProvider.GetService<INetworkClient>();
                if (networkClient == null || !networkClient.IsConnected)
                    return;

                var optionsData = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    EventId = eventId,
                    Options = options
                };

                var json = JsonSerializer.Serialize(optionsData);
                networkClient.SendRequest("DialogOptions", json);

                Plugin.Logger?.LogInfo($"[EventSync] Dialog options synced: {options.Count} options");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[EventSync] Error in SyncDialogOptions: {ex.Message}");
            }
        }

        /// <summary>
        /// 对话选项数据
        /// </summary>
        public class DialogOptionData
        {
            public int Index { get; set; }
            public string Text { get; set; } = string.Empty;
            public bool IsAvailable { get; set; }
            public string Tooltip { get; set; } = string.Empty;
        }
    }

    /// <summary>
    /// 特殊事件同步
    /// 处理特殊类型的游戏事件
    /// </summary>
    public class SpecialEventSync
    {
        /// <summary>
        /// Boss战后奖励选择同步
        /// </summary>
        public static void SyncBossRewardSelection(string bossId, string rewardType, string rewardId)
        {
            try
            {
                if (serviceProvider == null) return;

                var networkClient = serviceProvider.GetService<INetworkClient>();
                if (networkClient == null || !networkClient.IsConnected)
                    return;

                var rewardData = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    BossId = bossId,
                    RewardType = rewardType,  // Exhibit/Card/Relic
                    RewardId = rewardId,
                    PlayerId = GetCurrentPlayerId()
                };

                var json = JsonSerializer.Serialize(rewardData);
                networkClient.SendRequest("BossRewardSelection", json);

                Plugin.Logger?.LogInfo($"[EventSync] Boss reward selected: {rewardType} - {rewardId}");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[EventSync] Error in SyncBossRewardSelection: {ex.Message}");
            }
        }

        /// <summary>
        /// 商店事件同步
        /// </summary>
        public static void SyncShopEvent(string shopId, string eventType)
        {
            try
            {
                if (serviceProvider == null) return;

                var networkClient = serviceProvider.GetService<INetworkClient>();
                if (networkClient == null || !networkClient.IsConnected)
                    return;

                var shopData = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    ShopId = shopId,
                    EventType = eventType
                };

                var json = JsonSerializer.Serialize(shopData);
                networkClient.SendRequest("ShopEvent", json);
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[EventSync] Error in SyncShopEvent: {ex.Message}");
            }
        }

        /// <summary>
        /// 宝箱/宝藏事件同步
        /// </summary>
        public static void SyncTreasureEvent(string treasureId, List<string> rewards)
        {
            try
            {
                if (serviceProvider == null) return;

                var networkClient = serviceProvider.GetService<INetworkClient>();
                if (networkClient == null || !networkClient.IsConnected)
                    return;

                var treasureData = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    TreasureId = treasureId,
                    Rewards = rewards
                };

                var json = JsonSerializer.Serialize(treasureData);
                networkClient.SendRequest("TreasureEvent", json);
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[EventSync] Error in SyncTreasureEvent: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 事件投票机制
    /// 对于重要事件，允许多人投票决定
    /// </summary>
    public class EventVotingSystem
    {
        private static Dictionary<string, List<string>> _playerVotes = new();

        /// <summary>
        /// 判断事件是否需要投票
        /// </summary>
        public static bool IsVotingRequired(string eventId)
        {
            // TODO: 实现投票必要性判断
            // 根据事件类型、重要性等决定

            // 例如：Boss战后的关键选择、重要剧情分支等

            return false;
        }

        /// <summary>
        /// 记录玩家投票
        /// </summary>
        public static void RecordVote(string playerId, string eventId, int optionIndex)
        {
            if (!_playerVotes.ContainsKey(eventId))
                _playerVotes[eventId] = new List<string>();

            _playerVotes[eventId].Add($"{playerId}:{optionIndex}");

            // Check if all players have voted
            if (AllPlayersVoted(eventId))
            {
                ResolveVoting(eventId);
            }
        }

        /// <summary>
        /// 检查是否所有玩家都已投票
        /// </summary>
        private static bool AllPlayersVoted(string eventId)
        {
            // TODO: 实现检查逻辑
            // 获取当前游戏的所有玩家
            // 检查投票记录

            return false;
        }

        /// <summary>
        /// 解决投票
        /// </summary>
        private static void ResolveVoting(string eventId)
        {
            if (!_playerVotes.ContainsKey(eventId))
                return;

            var votes = _playerVotes[eventId];
            var optionCounts = new Dictionary<int, int>();

            // 统计投票
            foreach (var vote in votes)
            {
                var parts = vote.Split(':');
                if (parts.Length == 2 && int.TryParse(parts[1], out var optionIndex))
                {
                    optionCounts.TryAdd(optionIndex, 0);
                    optionCounts[optionIndex]++;
                }
            }

            // 多数决定
            int winningOption = -1;
            int maxVotes = 0;
            foreach (var kvp in optionCounts)
            {
                if (kvp.Value > maxVotes)
                {
                    winningOption = kvp.Key;
                    maxVotes = kvp.Value;
                }
            }

            // TODO: 广播投票结果给所有玩家
            BroadcastVotingResult(eventId, winningOption, votes.Count);

            // 清理投票记录
            _playerVotes.Remove(eventId);
        }

        /// <summary>
        /// 广播投票结果
        /// </summary>
        private static void BroadcastVotingResult(string eventId, int winningOption, int totalVotes)
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
                    EventId = eventId,
                    WinningOption = winningOption,
                    TotalVotes = totalVotes
                };

                var json = JsonSerializer.Serialize(resultData);
                networkClient.SendRequest("EventVotingResult", json);

                Plugin.Logger?.LogInfo($"[EventVoting] Voting resolved: Event {eventId}, Winning option: {winningOption}, Total votes: {totalVotes}");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[EventVoting] Error broadcasting result: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 构建完整的事件状态快照
    /// 用于断线重连时恢复事件进度
    /// </summary>
    public static object BuildEventSnapshot()
    {
        // TODO: 实现事件快照
        // 包括：当前事件、选择状态、对话进度等

        return new
        {
            Timestamp = DateTime.Now.Ticks
            // TODO: 添加实际数据
        };
    }

    /// <summary>
    /// 获取当前玩家ID
    /// TODO: 实现获取当前玩家ID
    /// </summary>
    private static string GetCurrentPlayerId()
    {
        return "current_player";
    }
}
