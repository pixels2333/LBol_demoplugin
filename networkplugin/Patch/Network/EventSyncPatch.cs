using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Adventures;
using LBoL.Core.Dialogs;
using LBoL.Core.Stations;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Panels;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Utils;
using TMPro;
using UnityEngine;

namespace NetworkPlugin.Patch.Network;

/// <summary>
/// 事件/对话同步补丁。
/// </summary>
/// <remarks>
/// 用于同步跑图过程中的“事件节点/对话节点”的关键决策，避免多人联机时进度分叉。
/// 使用 Harmony 拦截点捕获事件/对话关键选择，并通过网络广播到其他客户端落地。
/// </remarks>
public class EventSyncPatch
{
    #region 依赖注入

    /// <summary>
    /// 依赖注入服务提供者。
    /// </summary>
    private static IServiceProvider serviceProvider => ModService.ServiceProvider;

    #endregion

    #region 运行时订阅与本地上下文

    private static readonly object SyncLock = new();
    private static bool _subscribed;
    private static INetworkClient _subscribedClient;

    private static string _activeEventId;
    private static string _activeEventName;
    private static string _activeSpeaker;

    private static readonly Dictionary<string, PendingSelection> _pendingSelectionByEventId = new(StringComparer.Ordinal);

    private static long _suppressOutgoingUntilTicks;

    private sealed class PendingSelection
    {
        public string EventId { get; set; } = string.Empty;
        public int OptionId { get; set; }
        public int OptionIndex { get; set; }
        public string FromPlayerId { get; set; } = string.Empty;
        public long TimestampTicks { get; set; }
    }

    private static bool IsOutgoingSuppressed
    {
        get
        {
            long until = Volatile.Read(ref _suppressOutgoingUntilTicks);
            return until > 0 && DateTime.Now.Ticks < until;
        }
    }

    private static void SuppressOutgoingFor(TimeSpan duration)
    {
        long until = DateTime.Now.Add(duration).Ticks;
        Volatile.Write(ref _suppressOutgoingUntilTicks, until);
    }

    private static void EnsureSubscribed(INetworkClient client)
    {
        if (client == null)
        {
            return;
        }

        lock (SyncLock)
        {
            if (_subscribed && ReferenceEquals(_subscribedClient, client))
            {
                return;
            }
        }

        try
        {
            if (_subscribedClient != null)
            {
                _subscribedClient.OnGameEventReceived -= OnGameEventReceived;
            }
        }
        catch
        {
            // ignored
        }

        try
        {
            client.OnGameEventReceived += OnGameEventReceived;

            lock (SyncLock)
            {
                _subscribedClient = client;
                _subscribed = true;
            }
        }
        catch
        {
            lock (SyncLock)
            {
                _subscribedClient = null;
                _subscribed = false;
            }
        }
    }

    private static void OnGameEventReceived(string eventType, object payload)
    {
        // 只处理本补丁关心的消息。
        if (string.IsNullOrWhiteSpace(eventType))
        {
            return;
        }

        if (!string.Equals(eventType, NetworkMessageTypes.OnEventStart, StringComparison.Ordinal) &&
            !string.Equals(eventType, NetworkMessageTypes.OnEventSelection, StringComparison.Ordinal) &&
            !string.Equals(eventType, NetworkMessageTypes.OnDialogOptions, StringComparison.Ordinal) &&
            !string.Equals(eventType, NetworkMessageTypes.OnDialogText, StringComparison.Ordinal) &&
            !string.Equals(eventType, NetworkMessageTypes.OnEventVoteCast, StringComparison.Ordinal) &&
            !string.Equals(eventType, NetworkMessageTypes.OnEventVotingResult, StringComparison.Ordinal))
        {
            return;
        }

        if (!TryGetJsonElement(payload, out JsonElement root))
        {
            return;
        }

        try
        {
            if (string.Equals(eventType, NetworkMessageTypes.OnEventStart, StringComparison.Ordinal))
            {
                string eventId = GetString(root, "EventId");
                string eventName = GetString(root, "EventName");
                lock (SyncLock)
                {
                    _activeEventId = eventId;
                    _activeEventName = eventName;
                }

                return;
            }

            if (string.Equals(eventType, NetworkMessageTypes.OnDialogText, StringComparison.Ordinal))
            {
                // 目前仅用于诊断日志；真正推进由选项/权威端决定。
                return;
            }

            if (string.Equals(eventType, NetworkMessageTypes.OnDialogOptions, StringComparison.Ordinal))
            {
                // 目前仅用于诊断/断线重连占位。
                return;
            }

            if (string.Equals(eventType, NetworkMessageTypes.OnEventSelection, StringComparison.Ordinal))
            {
                string eventId = GetString(root, "EventId");
                int optionId = GetInt(root, "OptionId", -1);
                int optionIndex = GetInt(root, "OptionIndex", -1);
                string fromPlayerId = GetString(root, "PlayerId");

                if (string.IsNullOrWhiteSpace(eventId) || optionId < 0)
                {
                    return;
                }

                var pending = new PendingSelection
                {
                    EventId = eventId,
                    OptionId = optionId,
                    OptionIndex = optionIndex,
                    FromPlayerId = fromPlayerId,
                    TimestampTicks = DateTime.Now.Ticks,
                };

                lock (SyncLock)
                {
                    _pendingSelectionByEventId[eventId] = pending;
                }

                // 远端落地：设置一个短暂的 outgoing 抑制窗口，避免自动 SelectOption 触发回环发送。
                SuppressOutgoingFor(TimeSpan.FromSeconds(2));
                TryApplyPendingSelectionNow(eventId);
                return;
            }

            if (string.Equals(eventType, NetworkMessageTypes.OnEventVoteCast, StringComparison.Ordinal))
            {
                // 只有 Host 需要收集投票。
                if (!NetworkIdentityTracker.GetSelfIsHost())
                {
                    return;
                }

                string eventId = GetString(root, "EventId");
                string playerId = GetString(root, "PlayerId");
                int optionIndex = GetInt(root, "OptionIndex", -1);

                if (string.IsNullOrWhiteSpace(eventId) || string.IsNullOrWhiteSpace(playerId) || optionIndex < 0)
                {
                    return;
                }

                EventVotingSystem.RecordVote(playerId, eventId, optionIndex);
                return;
            }

            if (string.Equals(eventType, NetworkMessageTypes.OnEventVotingResult, StringComparison.Ordinal))
            {
                // 非 Host 收到结算后，等待随后的 OnEventSelection 推进；这里仅留作日志。
                return;
            }
        }
        catch
        {
            // ignored
        }
    }

    private static void TryApplyPendingSelectionNow(string eventId)
    {
        PendingSelection pending;
        lock (SyncLock)
        {
            if (!_pendingSelectionByEventId.TryGetValue(eventId, out pending))
            {
                return;
            }
        }

        try
        {
            VnPanel vnPanel = UiManager.GetPanel<VnPanel>();
            if (vnPanel == null || !vnPanel.IsRunning)
            {
                return;
            }

            // 尝试直接写入 _selectedOptionId，让 ShowOptions 协程继续。
            var selectedField = AccessTools.Field(typeof(VnPanel), "_selectedOptionId");
            if (selectedField == null)
            {
                return;
            }

            object cur = selectedField.GetValue(vnPanel);
            if (cur is int?)
            {
                selectedField.SetValue(vnPanel, (int?)pending.OptionId);
            }
        }
        catch
        {
            // ignored
        }
    }

    private static bool TryGetJsonElement(object payload, out JsonElement root)
    {
        if (payload is JsonElement elem)
        {
            root = elem;
            return true;
        }

        if (payload is string s)
        {
            try
            {
                root = JsonSerializer.Deserialize<JsonElement>(s);
                return true;
            }
            catch
            {
                // ignored
            }
        }

        root = default;
        return false;
    }

    private static string GetString(JsonElement root, string name)
    {
        if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty(name, out JsonElement prop) &&
            prop.ValueKind == JsonValueKind.String)
        {
            return prop.GetString() ?? string.Empty;
        }

        return string.Empty;
    }

    private static int GetInt(JsonElement root, string name, int fallback = 0)
    {
        if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty(name, out JsonElement prop))
        {
            if (prop.ValueKind == JsonValueKind.Number && prop.TryGetInt32(out int v))
            {
                return v;
            }

            if (prop.ValueKind == JsonValueKind.String && int.TryParse(prop.GetString(), out int vs))
            {
                return vs;
            }
        }

        return fallback;
    }

    #endregion

    #region 事件初始化同步

    /// <summary>
    /// 事件初始化同步。
    /// </summary>
    /// <remarks>
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

                EnsureSubscribed(networkClient);
                NetworkIdentityTracker.EnsureSubscribed(networkClient);

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
                networkClient.SendGameEventData(NetworkMessageTypes.OnEventStart, eventData);

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
        public static void SyncEventSelection(string eventId, int optionIndex, int optionId, string optionText, string optionResult)
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

                if (IsOutgoingSuppressed)
                {
                    return;
                }

                EnsureSubscribed(networkClient);

                // 打包选项选择数据。
                var selectionData = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    EventId = eventId,
                    OptionIndex = optionIndex,
                    OptionId = optionId,
                    OptionText = optionText,
                    OptionResult = optionResult,
                    PlayerId = GetCurrentPlayerId(),
                };

                networkClient.SendGameEventData(NetworkMessageTypes.OnEventSelection, selectionData);

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

                if (IsOutgoingSuppressed)
                {
                    return;
                }

                EnsureSubscribed(networkClient);

                // 打包结果数据。
                var resultData = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    EventId = eventId,
                    Effects = effects,
                };

                networkClient.SendGameEventData(NetworkMessageTypes.OnEventResult, resultData);

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
    /// 由 VnPanel/DialogRunner 等拦截点补齐发送时机与上下文。
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

                if (IsOutgoingSuppressed)
                {
                    return;
                }

                EnsureSubscribed(networkClient);

                // 打包对话数据。
                var dialogData = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    EventId = eventId,
                    Speaker = speaker,
                    Text = text,
                    DialogIndex = dialogIndex,
                };

                networkClient.SendGameEventData(NetworkMessageTypes.OnDialogText, dialogData);

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

                if (IsOutgoingSuppressed)
                {
                    return;
                }

                EnsureSubscribed(networkClient);

                // 打包选项数据。
                var optionsData = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    EventId = eventId,
                    Options = options,
                };

                networkClient.SendGameEventData(NetworkMessageTypes.OnDialogOptions, optionsData);

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

                if (IsOutgoingSuppressed)
                {
                    return;
                }

                EnsureSubscribed(networkClient);

                var rewardData = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    BossId = bossId,
                    RewardType = rewardType,
                    RewardId = rewardId,
                    PlayerId = GetCurrentPlayerId(),
                };

                networkClient.SendGameEventData(NetworkMessageTypes.OnBossRewardSelection, rewardData);

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

                if (IsOutgoingSuppressed)
                {
                    return;
                }

                EnsureSubscribed(networkClient);

                var shopData = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    ShopId = shopId,
                    EventType = eventType,
                };

                networkClient.SendGameEventData(NetworkMessageTypes.OnShopEvent, shopData);
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

                if (IsOutgoingSuppressed)
                {
                    return;
                }

                EnsureSubscribed(networkClient);

                var treasureData = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    TreasureId = treasureId,
                    Rewards = rewards,
                };

                networkClient.SendGameEventData(NetworkMessageTypes.OnTreasureEvent, treasureData);
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
        // eventId -> (playerId -> optionIndex)
        private static readonly Dictionary<string, Dictionary<string, int>> _playerVotes = new(StringComparer.Ordinal);

        // 默认不强制投票；由外部按需注册关键事件 Id。
        private static readonly HashSet<string> _votingEventIds = new(StringComparer.Ordinal);

        public static void RegisterVotingEvent(string eventId)
        {
            if (string.IsNullOrWhiteSpace(eventId))
            {
                return;
            }

            _votingEventIds.Add(eventId);
        }

        public static void ClearVotingEvents() => _votingEventIds.Clear();

        /// <summary>
        /// 判断事件是否需要投票。
        /// </summary>
        /// <param name="eventId">事件标识。</param>
        /// <returns>需要投票返回 true，否则 false。</returns>
        public static bool IsVotingRequired(string eventId)
        {
            if (string.IsNullOrWhiteSpace(eventId))
            {
                return false;
            }

            // 只有多人房间才有投票意义。
            if (NetworkIdentityTracker.GetPlayerIdsSnapshot().Count <= 1)
            {
                return false;
            }

            // 默认策略：仅对显式注册的事件启用投票，避免误伤所有事件。
            return _votingEventIds.Contains(eventId);
        }

        /// <summary>
        /// 记录玩家投票。
        /// </summary>
        /// <param name="playerId">玩家标识。</param>
        /// <param name="eventId">事件标识。</param>
        /// <param name="optionIndex">投票选项下标。</param>
        public static void RecordVote(string playerId, string eventId, int optionIndex)
        {
            if (string.IsNullOrWhiteSpace(playerId) || string.IsNullOrWhiteSpace(eventId) || optionIndex < 0)
            {
                return;
            }

            if (!_playerVotes.TryGetValue(eventId, out var votesByPlayer))
            {
                votesByPlayer = new Dictionary<string, int>(StringComparer.Ordinal);
                _playerVotes[eventId] = votesByPlayer;
            }

            // 同一玩家重复投票：以最后一次为准。
            votesByPlayer[playerId] = optionIndex;

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
            if (string.IsNullOrWhiteSpace(eventId))
            {
                return false;
            }

            if (!_playerVotes.TryGetValue(eventId, out var votesByPlayer))
            {
                return false;
            }

            var players = NetworkIdentityTracker.GetPlayerIdsSnapshot();
            if (players.Count <= 0)
            {
                return false;
            }

            // 允许玩家列表存在延迟：只要已知玩家都投了即可。
            foreach (string pid in players)
            {
                if (!votesByPlayer.ContainsKey(pid))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 结算投票结果并广播。
        /// </summary>
        /// <param name="eventId">事件标识。</param>
        private static void ResolveVoting(string eventId)
        {
            if (!_playerVotes.TryGetValue(eventId, out var votesByPlayer))
            {
                return;
            }

            Dictionary<int, int> optionCounts = new();
            foreach (var kvp in votesByPlayer)
            {
                int optionIndex = kvp.Value;
                optionCounts.TryAdd(optionIndex, 0);
                optionCounts[optionIndex]++;
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
            BroadcastVotingResult(eventId, winningOption, votesByPlayer.Count);

            // Host 本地落地：根据 winningOption 反查 optionId，并推进协程。
            TryApplyVoteResultLocally(eventId, winningOption);

            // 同步最终选项（用于推进其他客户端）。
            TryBroadcastSelectionFromVoteResult(eventId, winningOption);

            // 清理投票记录。
            _playerVotes.Remove(eventId);
        }

        private static void TryApplyVoteResultLocally(string eventId, int winningOptionIndex)
        {
            try
            {
                if (winningOptionIndex < 0)
                {
                    return;
                }

                VnPanel vnPanel = UiManager.GetPanel<VnPanel>();
                if (vnPanel == null || !vnPanel.IsRunning)
                {
                    return;
                }

                // 需要确保协程仍在等待选择。
                DialogOptionsPhase phase = AccessTools.Field(typeof(VnPanel), "_dialogRunner")?.GetValue(vnPanel) is DialogRunner runner
                    ? runner.CurrentPhase as DialogOptionsPhase
                    : null;

                if (phase == null || winningOptionIndex >= phase.Options.Length)
                {
                    return;
                }

                int optionId = phase.Options[winningOptionIndex].Id;

                SuppressOutgoingFor(TimeSpan.FromSeconds(2));
                AccessTools.Field(typeof(VnPanel), "_selectedOptionId")?.SetValue(vnPanel, (int?)optionId);
            }
            catch
            {
                // ignored
            }
        }

        private static void TryBroadcastSelectionFromVoteResult(string eventId, int winningOptionIndex)
        {
            try
            {
                if (serviceProvider == null)
                {
                    return;
                }

                var client = serviceProvider.GetService<INetworkClient>();
                if (client == null || !client.IsConnected)
                {
                    return;
                }

                if (winningOptionIndex < 0)
                {
                    return;
                }

                VnPanel vnPanel = UiManager.GetPanel<VnPanel>();
                DialogRunner runner = AccessTools.Field(typeof(VnPanel), "_dialogRunner")?.GetValue(vnPanel) as DialogRunner;
                DialogOptionsPhase phase = runner?.CurrentPhase as DialogOptionsPhase;
                if (phase == null || winningOptionIndex >= phase.Options.Length)
                {
                    return;
                }

                DialogOption opt = phase.Options[winningOptionIndex];
                string text = opt.GetLocalizedText(runner);
                EventSelectionSync.SyncEventSelection(eventId, winningOptionIndex, opt.Id, text, "VotingResolved");
            }
            catch
            {
                // ignored
            }
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

                if (IsOutgoingSuppressed)
                {
                    return;
                }

                EnsureSubscribed(networkClient);

                var resultData = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    EventId = eventId,
                    WinningOption = winningOption,
                    TotalVotes = totalVotes,
                };

                networkClient.SendGameEventData(NetworkMessageTypes.OnEventVotingResult, resultData);

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

    #region Harmony 拦截点

    [HarmonyPatch(typeof(AdventureStation), "OnEnter")]
    [HarmonyPostfix]
    public static void AdventureStation_OnEnter_Postfix(AdventureStation __instance)
    {
        try
        {
            if (__instance?.Adventure == null)
            {
                return;
            }

            var client = serviceProvider?.GetService<INetworkClient>();
            if (client == null)
            {
                return;
            }

            EnsureSubscribed(client);
            NetworkIdentityTracker.EnsureSubscribed(client);

            // 只由 Host 广播事件开始，避免多端重复。
            if (!client.IsConnected || !NetworkIdentityTracker.GetSelfIsHost())
            {
                return;
            }

            lock (SyncLock)
            {
                _activeEventId = __instance.Adventure.Id;
                _activeEventName = __instance.Adventure.Title;
            }

            EventInitSync.SyncEventStart(__instance.Adventure, __instance.Adventure.Id, __instance.Adventure.Title);
        }
        catch
        {
            // ignored
        }
    }

    [HarmonyPatch(typeof(VnPanel), "CoRunDialog")]
    [HarmonyPrefix]
    public static void VnPanel_CoRunDialog_Prefix(string vnName, DialogStorage storage, global::Yarn.Library library,
        RuntimeCommandHandler extraCommandHandler, string startNode, Adventure adventure)
    {
        try
        {
            var client = serviceProvider?.GetService<INetworkClient>();
            if (client != null)
            {
                EnsureSubscribed(client);
                NetworkIdentityTracker.EnsureSubscribed(client);
            }

            lock (SyncLock)
            {
                _activeEventId = !string.IsNullOrWhiteSpace(adventure?.Id) ? adventure.Id : (vnName ?? string.Empty);
                _activeEventName = !string.IsNullOrWhiteSpace(adventure?.Title) ? adventure.Title : (vnName ?? string.Empty);
            }

            // adventure 可能来自 RestoreAdventure/RunDialog；补一层“Host 才广播”。
            if (adventure != null && client != null && client.IsConnected && NetworkIdentityTracker.GetSelfIsHost())
            {
                EventInitSync.SyncEventStart(adventure, adventure.Id, adventure.Title);
            }
        }
        catch
        {
            // ignored
        }
    }

    [HarmonyPatch(typeof(VnPanel), "End")]
    [HarmonyPostfix]
    public static void VnPanel_End_Postfix()
    {
        lock (SyncLock)
        {
            _activeEventId = null;
            _activeEventName = null;
            _activeSpeaker = null;
        }
    }

    [HarmonyPatch(typeof(VnPanel), "SetCharacterName")]
    [HarmonyPostfix]
    public static void VnPanel_SetCharacterName_Postfix(VnPanel __instance)
    {
        try
        {
            // 通过 UI 文本提取 speaker（若无则为空）。
            string speaker = string.Empty;
            var leftRoot = AccessTools.Field(typeof(VnPanel), "leftCharacterNameRoot")?.GetValue(__instance) as GameObject;
            var rightRoot = AccessTools.Field(typeof(VnPanel), "rightCharacterNameRoot")?.GetValue(__instance) as GameObject;

            if (leftRoot != null && leftRoot.activeSelf)
            {
                speaker = (AccessTools.Field(typeof(VnPanel), "leftCharacterNameText")?.GetValue(__instance) as TextMeshProUGUI)?.text ?? string.Empty;
            }
            else if (rightRoot != null && rightRoot.activeSelf)
            {
                speaker = (AccessTools.Field(typeof(VnPanel), "rightCharacterNameText")?.GetValue(__instance) as TextMeshProUGUI)?.text ?? string.Empty;
            }

            lock (SyncLock)
            {
                _activeSpeaker = speaker;
            }
        }
        catch
        {
            // ignored
        }
    }

    [HarmonyPatch(typeof(VnPanel), "ShowOptions")]
    [HarmonyPostfix]
    public static void VnPanel_ShowOptions_Postfix(VnPanel __instance, DialogOption[] options, ref IEnumerator __result)
    {
        __result = WrapShowOptions(__instance, options, __result);
    }

    private static IEnumerator WrapShowOptions(VnPanel panel, DialogOption[] options, IEnumerator original)
    {
        // 进入 ShowOptions 时，如果收到了远端选择但本地还没进入等待，可在此处自动落地。
        if (panel != null && options != null)
        {
            TrySendDialogOptions(panel, options);

            string eventId;
            lock (SyncLock)
            {
                eventId = _activeEventId;
            }

            if (!string.IsNullOrWhiteSpace(eventId))
            {
                TryApplyPendingSelectionNow(eventId);
            }
        }

        while (original != null && original.MoveNext())
        {
            // 每帧推进时尝试落地 pending selection（直到成功）。
            try
            {
                string eventId;
                lock (SyncLock)
                {
                    eventId = _activeEventId;
                }

                if (!string.IsNullOrWhiteSpace(eventId))
                {
                    TryApplyPendingSelectionNow(eventId);
                }
            }
            catch
            {
                // ignored
            }

            yield return original.Current;
        }
    }

    private static void TrySendDialogOptions(VnPanel panel, DialogOption[] options)
    {
        try
        {
            var client = serviceProvider?.GetService<INetworkClient>();
            if (client == null || !client.IsConnected)
            {
                return;
            }

            if (!NetworkIdentityTracker.GetSelfIsHost() || IsOutgoingSuppressed)
            {
                return;
            }

            DialogRunner runner = AccessTools.Field(typeof(VnPanel), "_dialogRunner")?.GetValue(panel) as DialogRunner;
            if (runner == null)
            {
                return;
            }

            string eventId;
            lock (SyncLock)
            {
                eventId = _activeEventId;
            }

            if (string.IsNullOrWhiteSpace(eventId))
            {
                return;
            }

            List<DialogSync.DialogOptionData> list = new(options.Length);
            for (int i = 0; i < options.Length; i++)
            {
                DialogOption opt = options[i];
                list.Add(new DialogSync.DialogOptionData
                {
                    Index = i,
                    Text = opt.GetLocalizedText(runner),
                    IsAvailable = opt.Available,
                    Tooltip = string.Empty,
                });
            }

            DialogSync.SyncDialogOptions(eventId, list);
        }
        catch
        {
            // ignored
        }
    }

    private static readonly ConditionalWeakTable<DialogLinePhase, object> _sentDialogLines = new();
    private static int _dialogIndex;

    [HarmonyPatch(typeof(DialogLinePhase), nameof(DialogLinePhase.GetLocalizedText))]
    [HarmonyPostfix]
    public static void DialogLinePhase_GetLocalizedText_Postfix(DialogLinePhase __instance, DialogRunner runner, ref string __result)
    {
        try
        {
            if (__instance == null || runner == null)
            {
                return;
            }

            var client = serviceProvider?.GetService<INetworkClient>();
            if (client == null || !client.IsConnected)
            {
                return;
            }

            if (!NetworkIdentityTracker.GetSelfIsHost() || IsOutgoingSuppressed)
            {
                return;
            }

            // 同一 phase 的本地化文本可能被多次获取：只发送一次。
            lock (_sentDialogLines)
            {
                if (_sentDialogLines.TryGetValue(__instance, out _))
                {
                    return;
                }

                _sentDialogLines.Add(__instance, new object());
            }

            string eventId;
            string speaker;
            lock (SyncLock)
            {
                eventId = _activeEventId;
                speaker = _activeSpeaker;
            }

            if (string.IsNullOrWhiteSpace(eventId))
            {
                return;
            }

            int idx = Interlocked.Increment(ref _dialogIndex);
            DialogSync.SyncDialogText(eventId, speaker ?? string.Empty, __result ?? string.Empty, idx);
        }
        catch
        {
            // ignored
        }
    }

    [HarmonyPatch(typeof(DialogRunner), nameof(DialogRunner.SelectOption))]
    [HarmonyPrefix]
    public static void DialogRunner_SelectOption_Prefix(DialogRunner __instance, int id)
    {
        try
        {
            var client = serviceProvider?.GetService<INetworkClient>();
            if (client == null || !client.IsConnected)
            {
                return;
            }

            if (!NetworkIdentityTracker.GetSelfIsHost() || IsOutgoingSuppressed)
            {
                return;
            }

            string eventId;
            lock (SyncLock)
            {
                eventId = _activeEventId;
            }

            if (string.IsNullOrWhiteSpace(eventId))
            {
                return;
            }

            // 尝试在当前 options phase 中反查 index/text。
            DialogOptionsPhase phase = __instance.CurrentPhase as DialogOptionsPhase;
            if (phase == null)
            {
                return;
            }

            int optionIndex = -1;
            string optionText = string.Empty;
            for (int i = 0; i < phase.Options.Length; i++)
            {
                if (phase.Options[i].Id == id)
                {
                    optionIndex = i;
                    optionText = phase.Options[i].GetLocalizedText(__instance);
                    break;
                }
            }

            if (optionIndex < 0)
            {
                return;
            }

            // 若该事件启用投票，则最终选项由投票结算广播，这里不重复广播。
            if (EventVotingSystem.IsVotingRequired(eventId))
            {
                return;
            }

            EventSelectionSync.SyncEventSelection(eventId, optionIndex, id, optionText, "DialogOptionConfirmed");
        }
        catch
        {
            // ignored
        }
    }

    // 投票模式：点击/按键选择仅提交投票，不立即推进；Host 收齐后广播最终选择。
    [HarmonyPatch(typeof(VnPanel), "OnClickOption")]
    [HarmonyPrefix]
    public static bool VnPanel_OnClickOption_Prefix(VnPanel __instance, int i)
    {
        try
        {
            var client = serviceProvider?.GetService<INetworkClient>();
            if (client == null || !client.IsConnected)
            {
                return true;
            }

            EnsureSubscribed(client);
            NetworkIdentityTracker.EnsureSubscribed(client);

            string eventId;
            lock (SyncLock)
            {
                eventId = _activeEventId;
            }

            if (string.IsNullOrWhiteSpace(eventId))
            {
                return true;
            }

            // 投票模式：所有玩家点击都只提交投票，不推进；Host 收齐后广播最终选择。
            if (EventVotingSystem.IsVotingRequired(eventId))
            {
                DialogRunner runner = AccessTools.Field(typeof(VnPanel), "_dialogRunner")?.GetValue(__instance) as DialogRunner;
                DialogOptionsPhase phase = runner?.CurrentPhase as DialogOptionsPhase;
                if (phase == null || i < 0 || i >= phase.Options.Length)
                {
                    return false;
                }

                string playerId = GetCurrentPlayerId();
                int optionIndex = i;
                string optionText = phase.Options[i].GetLocalizedText(runner);

                // 本地先记录（Host 自己的投票也计入）。
                if (NetworkIdentityTracker.GetSelfIsHost())
                {
                    EventVotingSystem.RecordVote(playerId, eventId, optionIndex);
                }

                client.SendGameEventData(NetworkMessageTypes.OnEventVoteCast, new
                {
                    Timestamp = DateTime.Now.Ticks,
                    EventId = eventId,
                    PlayerId = playerId,
                    OptionIndex = optionIndex,
                    OptionId = phase.Options[i].Id,
                    OptionText = optionText,
                });

                return false;
            }

            // 非投票模式：非 Host 不允许本地确认，避免分叉。
            if (!NetworkIdentityTracker.GetSelfIsHost())
            {
                return false;
            }

            return true;
        }
        catch
        {
            return true;
        }
    }

    [HarmonyPatch(typeof(VnPanel), nameof(VnPanel.HandleSelectionFromKey))]
    [HarmonyPrefix]
    public static bool VnPanel_HandleSelectionFromKey_Prefix(VnPanel __instance, int i, ref bool __result)
    {
        try
        {
            var client = serviceProvider?.GetService<INetworkClient>();
            if (client == null || !client.IsConnected)
            {
                return true;
            }

            EnsureSubscribed(client);
            NetworkIdentityTracker.EnsureSubscribed(client);

            string eventId;
            lock (SyncLock)
            {
                eventId = _activeEventId;
            }

            if (string.IsNullOrWhiteSpace(eventId))
            {
                return true;
            }

            // 只有在 optionsRoot 激活时才拦截。
            var optionsRoot = AccessTools.Field(typeof(VnPanel), "optionsRoot")?.GetValue(__instance) as GameObject;
            if (optionsRoot == null || !optionsRoot.activeSelf)
            {
                return true;
            }

            if (EventVotingSystem.IsVotingRequired(eventId))
            {
                // 将 key selection 也视为投票。
                DialogOption[] currentOptions = AccessTools.Field(typeof(VnPanel), "_options")?.GetValue(__instance) as DialogOption[];
                if (currentOptions == null)
                {
                    __result = true;
                    return false;
                }

                // i 是“可用选项列表”的索引，需要映射回原 options 的索引。
                List<int> availableIndices = new();
                for (int idx = 0; idx < currentOptions.Length; idx++)
                {
                    if (currentOptions[idx]?.Available == true)
                    {
                        availableIndices.Add(idx);
                    }
                }

                if (i < 0 || i >= availableIndices.Count)
                {
                    __result = true;
                    return false;
                }

                int optionIndex = availableIndices[i];
                DialogRunner runner = AccessTools.Field(typeof(VnPanel), "_dialogRunner")?.GetValue(__instance) as DialogRunner;
                string optionText = currentOptions[optionIndex]?.GetLocalizedText(runner) ?? string.Empty;

                string playerId = GetCurrentPlayerId();
                if (NetworkIdentityTracker.GetSelfIsHost())
                {
                    EventVotingSystem.RecordVote(playerId, eventId, optionIndex);
                }

                client.SendGameEventData(NetworkMessageTypes.OnEventVoteCast, new
                {
                    Timestamp = DateTime.Now.Ticks,
                    EventId = eventId,
                    PlayerId = playerId,
                    OptionIndex = optionIndex,
                    OptionId = currentOptions[optionIndex]?.Id ?? -1,
                    OptionText = optionText,
                });

                __result = true;
                return false;
            }

            if (!NetworkIdentityTracker.GetSelfIsHost())
            {
                __result = true;
                return false;
            }

            return true;
        }
        catch
        {
            return true;
        }
    }

    #endregion
}
