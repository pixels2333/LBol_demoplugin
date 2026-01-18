using System;
using System.Collections.Generic;
using System.Text.Json;
using LBoL.Core.Adventures;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Patch.Network;

/// <summary>
/// 事件/对话同步补丁。
/// </summary>
/// <remarks>
/// 用于同步跑图过程中的“事件节点/对话节点”的关键决策，避免多人联机时进度分叉。
/// 目前主要以“工具类 + 事件发送”为主，具体 Harmony 拦截点仍有部分 TODO。
/// </remarks>
public class EventSyncPatch
{
    #region 依赖注入

    /// <summary>
    /// 依赖注入服务提供者。
    /// </summary>
    private static IServiceProvider serviceProvider => ModService.ServiceProvider;

    #endregion

    #region 事件初始化同步

    /// <summary>
    /// 事件初始化同步。
    /// </summary>
    /// <remarks>
    /// TODO：需要补齐 LBoL 事件开始时的稳定入口点。
    /// 当玩家进入事件节点或触发事件时调用。
    /// </remarks>
    public class EventInitSync
    {
        /// <summary>
        /// 同步事件开始。
        /// </summary>
        /// <param name="adventure">事件对象（Adventure 基类）。</param>
        /// <param name="eventId">事件标识。</param>
        /// <param name="eventName">事件名称。</param>
        public static void SyncEventStart(Adventure adventure, string eventId, string eventName)
        {
            try
            {
                // 服务未就绪时跳过。
                if (serviceProvider == null)
                {
                    return;
                }

                // 客户端未连接不发送。
                var networkClient = serviceProvider.GetService<INetworkClient>();
                if (networkClient == null || !networkClient.IsConnected)
                {
                    return;
                }

                // 打包事件启动数据。
                var eventData = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    EventId = eventId,
                    EventName = eventName,
                    EventType = adventure.GetType().Name,
                    PlayerId = GetCurrentPlayerId(),
                };

                // 发送到服务器。
                string json = JsonSerializer.Serialize(eventData);
                networkClient.SendRequest("EventStart", json);

                Plugin.Logger?.LogInfo($"[EventSync] 事件开始: {eventName} (ID: {eventId})");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[EventSync] SyncEventStart 异常: {ex.Message}");
            }
        }
    }

    #endregion

    #region 事件选择同步

    /// <summary>
    /// 事件选项选择同步。
    /// </summary>
    /// <remarks>
    /// TODO：需要补齐“事件选项被点击/确认”的拦截点。
    /// 当玩家在事件中做出选择时触发。
    /// </remarks>
    public class EventSelectionSync
    {
        /// <summary>
        /// 同步事件选项选择。
        /// </summary>
        /// <param name="eventId">事件标识。</param>
        /// <param name="optionIndex">选项下标。</param>
        /// <param name="optionText">选项文本。</param>
        /// <param name="optionResult">选项结果描述（用于日志/调试）。</param>
        public static void SyncEventSelection(string eventId, int optionIndex, string optionText, string optionResult)
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

                // 打包选项选择数据。
                var selectionData = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    EventId = eventId,
                    OptionIndex = optionIndex,
                    OptionText = optionText,
                    OptionResult = optionResult,
                    PlayerId = GetCurrentPlayerId(),
                };

                string json = JsonSerializer.Serialize(selectionData);
                networkClient.SendRequest("EventSelection", json);

                Plugin.Logger?.LogInfo($"[EventSync] 事件选项选择: {optionText} -> {optionResult}");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[EventSync] SyncEventSelection 异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 同步事件结果：用于在选择选项后广播“事件产生的效果”。
        /// </summary>
        /// <param name="eventId">事件标识。</param>
        /// <param name="effects">效果数据（键值对）。</param>
        public static void SyncEventResult(string eventId, Dictionary<string, object> effects)
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

                // 打包结果数据。
                var resultData = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    EventId = eventId,
                    Effects = effects,
                };

                string json = JsonSerializer.Serialize(resultData);
                networkClient.SendRequest("EventResult", json);

                Plugin.Logger?.LogInfo($"[EventSync] 事件结果已同步: {eventId}");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[EventSync] SyncEventResult 异常: {ex.Message}");
            }
        }
    }

    #endregion

    #region 对话同步

    /// <summary>
    /// 对话同步：同步对话文本/选项等。
    /// </summary>
    /// <remarks>
    /// TODO：补齐 DialogRunner / DialogPhase 的稳定拦截点，才能做到“跟随推进”。
    /// </remarks>
    public class DialogSync
    {
        /// <summary>
        /// 同步对话文本。
        /// </summary>
        /// <param name="eventId">事件标识。</param>
        /// <param name="speaker">说话人。</param>
        /// <param name="text">对话文本。</param>
        /// <param name="dialogIndex">对话序号（用于复现顺序）。</param>
        public static void SyncDialogText(string eventId, string speaker, string text, int dialogIndex)
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

                // 打包对话数据。
                var dialogData = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    EventId = eventId,
                    Speaker = speaker,
                    Text = text,
                    DialogIndex = dialogIndex,
                };

                string json = JsonSerializer.Serialize(dialogData);
                networkClient.SendRequest("DialogText", json);

                Plugin.Logger?.LogDebug($"[EventSync] 对话[{dialogIndex}] {speaker}: {text}");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[EventSync] SyncDialogText 异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 同步对话选项列表。
        /// </summary>
        /// <param name="eventId">事件标识。</param>
        /// <param name="options">选项数据列表。</param>
        public static void SyncDialogOptions(string eventId, List<DialogOptionData> options)
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

                // 打包选项数据。
                var optionsData = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    EventId = eventId,
                    Options = options,
                };

                string json = JsonSerializer.Serialize(optionsData);
                networkClient.SendRequest("DialogOptions", json);

                Plugin.Logger?.LogInfo($"[EventSync] 对话选项已同步: {options.Count} 项");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[EventSync] SyncDialogOptions 异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 对话选项数据。
        /// </summary>
        public class DialogOptionData
        {
            /// <summary>选项序号。</summary>
            public int Index { get; set; }

            /// <summary>选项文本。</summary>
            public string Text { get; set; } = string.Empty;

            /// <summary>该选项是否可选。</summary>
            public bool IsAvailable { get; set; }

            /// <summary>提示文本。</summary>
            public string Tooltip { get; set; } = string.Empty;
        }
    }

    #endregion

    #region 特殊事件同步

    /// <summary>
    /// 特殊事件同步：对 boss 奖励/商店/宝箱等进行显式同步。
    /// </summary>
    public class SpecialEventSync
    {
        /// <summary>
        /// Boss 战后奖励选择同步。
        /// </summary>
        /// <param name="bossId">Boss 标识。</param>
        /// <param name="rewardType">奖励类型（Exhibit/Card/Relic 等）。</param>
        /// <param name="rewardId">奖励标识。</param>
        public static void SyncBossRewardSelection(string bossId, string rewardType, string rewardId)
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

                var rewardData = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    BossId = bossId,
                    RewardType = rewardType,
                    RewardId = rewardId,
                    PlayerId = GetCurrentPlayerId(),
                };

                string json = JsonSerializer.Serialize(rewardData);
                networkClient.SendRequest("BossRewardSelection", json);

                Plugin.Logger?.LogInfo($"[EventSync] Boss 奖励选择: {rewardType} - {rewardId}");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[EventSync] SyncBossRewardSelection 异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 商店事件同步。
        /// </summary>
        /// <param name="shopId">商店标识。</param>
        /// <param name="eventType">事件类型。</param>
        public static void SyncShopEvent(string shopId, string eventType)
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
                    ShopId = shopId,
                    EventType = eventType,
                };

                string json = JsonSerializer.Serialize(shopData);
                networkClient.SendRequest("ShopEvent", json);
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[EventSync] SyncShopEvent 异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 宝箱/宝藏事件同步。
        /// </summary>
        /// <param name="treasureId">宝藏标识。</param>
        /// <param name="rewards">奖励列表。</param>
        public static void SyncTreasureEvent(string treasureId, List<string> rewards)
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

                var treasureData = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    TreasureId = treasureId,
                    Rewards = rewards,
                };

                string json = JsonSerializer.Serialize(treasureData);
                networkClient.SendRequest("TreasureEvent", json);
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[EventSync] SyncTreasureEvent 异常: {ex.Message}");
            }
        }
    }

    #endregion

    #region 事件投票（预留）

    /// <summary>
    /// 事件投票机制（预留）：对关键事件允许多人投票。
    /// </summary>
    public class EventVotingSystem
    {
        private static Dictionary<string, List<string>> _playerVotes = [];

        /// <summary>
        /// 判断事件是否需要投票。
        /// </summary>
        /// <param name="eventId">事件标识。</param>
        /// <returns>需要投票返回 true，否则 false。</returns>
        public static bool IsVotingRequired(string eventId)
        {
            // TODO：实现投票必要性判断。
            return false;
        }

        /// <summary>
        /// 记录玩家投票。
        /// </summary>
        /// <param name="playerId">玩家标识。</param>
        /// <param name="eventId">事件标识。</param>
        /// <param name="optionIndex">投票选项下标。</param>
        public static void RecordVote(string playerId, string eventId, int optionIndex)
        {
            if (!_playerVotes.ContainsKey(eventId))
            {
                _playerVotes[eventId] = [];
            }

            _playerVotes[eventId].Add($"{playerId}:{optionIndex}");

            // 若所有玩家已投票，则立即结算。
            if (AllPlayersVoted(eventId))
            {
                ResolveVoting(eventId);
            }
        }

        /// <summary>
        /// 检查是否所有玩家都已投票。
        /// </summary>
        /// <param name="eventId">事件标识。</param>
        /// <returns>全部投票完成返回 true，否则 false。</returns>
        private static bool AllPlayersVoted(string eventId)
        {
            // TODO：实现检查逻辑。
            return false;
        }

        /// <summary>
        /// 结算投票结果并广播。
        /// </summary>
        /// <param name="eventId">事件标识。</param>
        private static void ResolveVoting(string eventId)
        {
            if (!_playerVotes.ContainsKey(eventId))
            {
                return;
            }

            var votes = _playerVotes[eventId];
            Dictionary<int, int> optionCounts = [];

            // 统计各选项票数。
            foreach (string vote in votes)
            {
                string[] parts = vote.Split(':');
                if (parts.Length == 2 && int.TryParse(parts[1], out int optionIndex))
                {
                    optionCounts.TryAdd(optionIndex, 0);
                    optionCounts[optionIndex]++;
                }
            }

            // 多数决定。
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

            // 广播投票结果。
            BroadcastVotingResult(eventId, winningOption, votes.Count);

            // 清理投票记录。
            _playerVotes.Remove(eventId);
        }

        /// <summary>
        /// 广播投票结果。
        /// </summary>
        /// <param name="eventId">事件标识。</param>
        /// <param name="winningOption">获胜选项。</param>
        /// <param name="totalVotes">总票数。</param>
        private static void BroadcastVotingResult(string eventId, int winningOption, int totalVotes)
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
                    EventId = eventId,
                    WinningOption = winningOption,
                    TotalVotes = totalVotes,
                };

                string json = JsonSerializer.Serialize(resultData);
                networkClient.SendRequest("EventVotingResult", json);

                Plugin.Logger?.LogInfo($"[EventVoting] 投票已结算: Event {eventId}, Winning {winningOption}, Total {totalVotes}");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[EventVoting] 广播投票结果失败: {ex.Message}");
            }
        }
    }

    #endregion

    #region 快照与辅助方法

    /// <summary>
    /// 构建事件状态快照（用于断线重连/诊断）。
    /// </summary>
    /// <returns>最小可用快照对象。</returns>
    public static object BuildEventSnapshot()
    {
        // 最小可用快照：用于诊断/同步占位，不承诺覆盖全部事件内部状态。
        var player = GameStateUtils.GetCurrentPlayer();

        return new
        {
            Timestamp = DateTime.Now.Ticks,
            PlayerId = GetCurrentPlayerId(),
            IsHost = NetworkIdentityTracker.GetSelfIsHost(),
            PlayerModel = player?.ModelName,
            InBattle = player?.Battle != null,
        };
    }

    /// <summary>
    /// 获取当前玩家 ID（优先使用联机身份跟踪器，失败则回落到游戏状态工具）。
    /// </summary>
    /// <returns>玩家标识字符串。</returns>
    private static string GetCurrentPlayerId()
    {
        try
        {
            string id = NetworkIdentityTracker.GetSelfPlayerId();
            if (!string.IsNullOrWhiteSpace(id))
            {
                return id;
            }

            id = GameStateUtils.GetCurrentPlayerId();
            if (!string.IsNullOrWhiteSpace(id))
            {
                return id;
            }
        }
        catch
        {
            // 忽略：获取失败时返回占位。
        }

        return "unknown_player";
    }

    #endregion
}
