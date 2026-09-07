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
using NetworkPlugin.Network.Services;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Utils;
using TMPro;
using UnityEngine;

namespace NetworkPlugin.Patch.Network;

public class EventSyncPatch
{
    #region 依赖注入

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

    private static readonly Dictionary<string, List<DialogSync.DialogOptionData>> _cachedDialogOptionsByEventId = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, PendingSelection> _lastConfirmedSelectionByEventId = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, int> _appliedOptionIdByEventId = new(StringComparer.Ordinal);

    private static long _lastDialogOptionsBroadcastTicks;
    private static long _lastSnapshotBroadcastTicks;

    private static long _suppressOutgoingUntilTicks;

    private sealed class PendingSelection
    {
        public string EventId { get; set; } = string.Empty;
        public int OptionId { get; set; }
        public int OptionIndex { get; set; }
        public string OptionText { get; set; } = string.Empty;
        public string FromPlayerId { get; set; } = string.Empty;
        public long TimestampTicks { get; set; }
    }

    private static bool ShouldBroadcastAgain(ref long lastTicks, TimeSpan interval)
    {
        long now = DateTime.Now.Ticks;
        long last = Volatile.Read(ref lastTicks);
        if (last > 0 && now - last < interval.Ticks)
        {
            return false;
        }

        Volatile.Write(ref lastTicks, now);
        return true;
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
                _subscribedClient.OnGameEventReceived -= OnGameEventReceived;
        }
        catch
        {

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

    private static bool TryGetConnectedClient(out INetworkClient networkClient)
    {
        networkClient = null;
        if (serviceProvider == null)
        {
            return false;
        }

        networkClient = serviceProvider.GetService<INetworkClient>();
        return networkClient != null && networkClient.IsConnected;
    }

    private static bool TryGetConnectedSubscribedClient(out INetworkClient networkClient)
    {
        if (!TryGetConnectedClient(out networkClient))
        {
            return false;
        }

        EnsureSubscribed(networkClient);
        return true;
    }

    private static void OnGameEventReceived(string eventType, object payload)
    {

        if (string.IsNullOrWhiteSpace(eventType))
        {
            return;
        }

        if (string.Equals(eventType, NetworkMessageTypes.PlayerJoined, StringComparison.Ordinal) ||
            string.Equals(eventType, NetworkMessageTypes.Welcome, StringComparison.Ordinal) ||
            string.Equals(eventType, NetworkMessageTypes.PlayerListUpdate, StringComparison.Ordinal))
        {

            if (NetworkIdentityTracker.GetSelfIsHost() && ShouldBroadcastAgain(ref _lastSnapshotBroadcastTicks, TimeSpan.FromSeconds(1)))
            {
                TryBroadcastActiveDialogSnapshot();
            }

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

                return;
            }

            if (string.Equals(eventType, NetworkMessageTypes.OnDialogOptions, StringComparison.Ordinal))
            {
                string eventId = GetString(root, "EventId");
                if (string.IsNullOrWhiteSpace(eventId))
                {
                    return;
                }

                if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("Options", out JsonElement optionsElem) &&
                    optionsElem.ValueKind == JsonValueKind.Array)
                {
                    List<DialogSync.DialogOptionData> list = new List<DialogSync.DialogOptionData>();
                    foreach (JsonElement item in optionsElem.EnumerateArray())
                    {
                        if (item.ValueKind != JsonValueKind.Object)
                        {
                            continue;
                        }

                        list.Add(new DialogSync.DialogOptionData
                        {
                            Index = GetInt(item, "Index", -1),
                            OptionId = GetInt(item, "OptionId", -1),
                            Text = GetString(item, "Text"),
                            IsAvailable = GetInt(item, "IsAvailable", 0) == 1 ||
                                          (item.TryGetProperty("IsAvailable", out var av) && av.ValueKind == JsonValueKind.True),
                            Tooltip = GetString(item, "Tooltip"),
                        });
                    }

                    lock (SyncLock)
                    {
                        _cachedDialogOptionsByEventId[eventId] = list;
                    }
                }

                TryApplyPendingSelectionNow(eventId);
                return;
            }

            if (string.Equals(eventType, NetworkMessageTypes.OnEventSelection, StringComparison.Ordinal))
            {
                string eventId = GetString(root, "EventId");
                int optionId = GetInt(root, "OptionId", -1);
                int optionIndex = GetInt(root, "OptionIndex", -1);
                string fromPlayerId = GetString(root, "PlayerId");
                string optionText = GetString(root, "OptionText");

                if (string.IsNullOrWhiteSpace(eventId) || (optionId < 0 && optionIndex < 0))
                {
                    return;
                }

                PendingSelection pending = new PendingSelection
                {
                    EventId = eventId,
                    OptionId = optionId,
                    OptionIndex = optionIndex,
                    FromPlayerId = fromPlayerId,
                    OptionText = optionText,
                    TimestampTicks = DateTime.Now.Ticks,
                };

                lock (SyncLock)
                {
                    _pendingSelectionByEventId[eventId] = pending;
                    _lastConfirmedSelectionByEventId[eventId] = pending;
                }

                SuppressOutgoingFor(TimeSpan.FromSeconds(2));
                TryApplyPendingSelectionNow(eventId);
                return;
            }

            if (string.Equals(eventType, NetworkMessageTypes.OnEventVoteCast, StringComparison.Ordinal))
            {

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

                return;
            }
        }
        catch
        {

        }
    }

    private static void TryBroadcastActiveDialogSnapshot()
    {
        try
        {
            if (!TryGetConnectedClient(out INetworkClient client))
            {
                return;
            }

            if (!NetworkIdentityTracker.GetSelfIsHost())
            {
                return;
            }

            string eventId;
            string eventName;
            lock (SyncLock)
            {
                eventId = _activeEventId;
                eventName = _activeEventName;
            }

            if (string.IsNullOrWhiteSpace(eventId))
            {
                return;
            }

            client.BroadcastState(NetworkMessageTypes.OnEventStart, new
            {
                Timestamp = DateTime.Now.Ticks,
                EventId = eventId,
                EventName = eventName ?? string.Empty,
                EventType = "Snapshot",
                PlayerId = GetCurrentPlayerId(),
            });

            List<DialogSync.DialogOptionData> options;
            lock (SyncLock)
            {
                _cachedDialogOptionsByEventId.TryGetValue(eventId, out options);
            }

            if (options != null && options.Count > 0)
            {
                DialogSync.SyncDialogOptions(eventId, options);
            }

            PendingSelection last;
            lock (SyncLock)
            {
                _lastConfirmedSelectionByEventId.TryGetValue(eventId, out last);
            }

            if (last != null && (last.OptionId >= 0 || last.OptionIndex >= 0))
            {
                string text = last.OptionText;
                if (string.IsNullOrWhiteSpace(text) && options != null && last.OptionIndex >= 0)
                {
                    foreach (var opt in options)
                    {
                        if (opt.Index == last.OptionIndex)
                        {
                            text = opt.Text;
                            break;
                        }
                    }
                }

                EventSelectionSync.SyncEventSelection(eventId, last.OptionIndex, last.OptionId, text ?? string.Empty, "SnapshotResend");
            }
        }
        catch
        {

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

            DialogRunner runner = AccessTools.Field(typeof(VnPanel), "_dialogRunner")?.GetValue(vnPanel) as DialogRunner;
            DialogOptionsPhase phase = runner?.CurrentPhase as DialogOptionsPhase;
            if (phase == null)
            {
                return;
            }

            int resolvedOptionId = pending.OptionId;
            if (resolvedOptionId < 0 && pending.OptionIndex >= 0 && pending.OptionIndex < phase.Options.Length)
            {
                resolvedOptionId = phase.Options[pending.OptionIndex].Id;
            }

            if (resolvedOptionId < 0 && pending.OptionIndex >= 0)
            {
                List<DialogSync.DialogOptionData> cached;
                lock (SyncLock)
                {
                    _cachedDialogOptionsByEventId.TryGetValue(eventId, out cached);
                }

                if (cached != null)
                {
                    foreach (var opt in cached)
                    {
                        if (opt.Index == pending.OptionIndex && opt.OptionId >= 0)
                        {
                            resolvedOptionId = opt.OptionId;
                            break;
                        }
                    }
                }
            }

            if (resolvedOptionId < 0)
            {
                return;
            }

            lock (SyncLock)
            {
                if (_appliedOptionIdByEventId.TryGetValue(eventId, out int already) && already == resolvedOptionId)
                {
                    _pendingSelectionByEventId.Remove(eventId);
                    return;
                }
            }

            var selectedField = AccessTools.Field(typeof(VnPanel), "_selectedOptionId");
            if (selectedField == null)
            {
                return;
            }

            object value = selectedField.FieldType == typeof(int?) ? (int?)resolvedOptionId : resolvedOptionId;
            selectedField.SetValue(vnPanel, value);

            lock (SyncLock)
            {
                _appliedOptionIdByEventId[eventId] = resolvedOptionId;
                _pendingSelectionByEventId.Remove(eventId);
            }
        }
        catch
        {

        }
    }

    private static bool TryGetJsonElement(object payload, out JsonElement root)
        => NetworkEventHelper.TryGetJsonElement(payload, out root);

    private static string GetString(JsonElement root, string name)
        => NetworkEventHelper.GetString(root, name) ?? string.Empty;

    private static int GetInt(JsonElement root, string name, int fallback = 0)
    {
        if (NetworkEventHelper.TryGetInt(root, name, out int v))
            return v;
        return fallback;
    }

    #endregion

    #region 事件初始化同步

        public class EventInitSync
    {
                public static void SyncEventStart(Adventure adventure, string eventId, string eventName)
        {
            try
            {
                if (!TryGetConnectedSubscribedClient(out INetworkClient networkClient))
                {
                    return;
                }

                NetworkIdentityTracker.EnsureSubscribed(networkClient);

                var eventData = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    EventId = eventId,
                    EventName = eventName,
                    EventType = adventure.GetType().Name,
                    PlayerId = GetCurrentPlayerId(),
                };

                networkClient.BroadcastState(NetworkMessageTypes.OnEventStart, eventData);

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

        public class EventSelectionSync
    {
                public static void SyncEventSelection(string eventId, int optionIndex, int optionId, string optionText, string optionResult)
        {
            try
            {
                if (!TryGetConnectedSubscribedClient(out INetworkClient networkClient))
                {
                    return;
                }

                if (IsOutgoingSuppressed)
                {
                    return;
                }

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

                networkClient.BroadcastState(NetworkMessageTypes.OnEventSelection, selectionData);

                Plugin.Logger?.LogInfo($"[EventSync] 事件选项选择: {optionText} -> {optionResult}");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[EventSync] SyncEventSelection 异常: {ex.Message}");
            }
        }

                public static void SyncEventResult(string eventId, Dictionary<string, object> effects)
        {
            try
            {
                if (!TryGetConnectedSubscribedClient(out INetworkClient networkClient))
                {
                    return;
                }

                if (IsOutgoingSuppressed)
                {
                    return;
                }

                var resultData = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    EventId = eventId,
                    Effects = effects,
                };

                networkClient.BroadcastState(NetworkMessageTypes.OnEventResult, resultData);

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

        public class DialogSync
    {
                public static void SyncDialogText(string eventId, string speaker, string text, int dialogIndex)
        {
            try
            {
                if (!TryGetConnectedSubscribedClient(out INetworkClient networkClient))
                {
                    return;
                }

                if (IsOutgoingSuppressed)
                {
                    return;
                }

                var dialogData = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    EventId = eventId,
                    Speaker = speaker,
                    Text = text,
                    DialogIndex = dialogIndex,
                };

                networkClient.BroadcastState(NetworkMessageTypes.OnDialogText, dialogData);

                Plugin.Logger?.LogDebug($"[EventSync] 对话[{dialogIndex}] {speaker}: {text}");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[EventSync] SyncDialogText 异常: {ex.Message}");
            }
        }

                public static void SyncDialogOptions(string eventId, List<DialogOptionData> options)
        {
            try
            {
                if (!TryGetConnectedSubscribedClient(out INetworkClient networkClient))
                {
                    return;
                }

                if (IsOutgoingSuppressed)
                {
                    return;
                }

                var optionsData = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    EventId = eventId,
                    Options = options,
                };

                networkClient.BroadcastState(NetworkMessageTypes.OnDialogOptions, optionsData);

                Plugin.Logger?.LogInfo($"[EventSync] 对话选项已同步: {options.Count} 项");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[EventSync] SyncDialogOptions 异常: {ex.Message}");
            }
        }

                public class DialogOptionData
        {
                        public int Index { get; set; }

                        public int OptionId { get; set; }

                        public string Text { get; set; } = string.Empty;

                        public bool IsAvailable { get; set; }

                        public string Tooltip { get; set; } = string.Empty;
        }
    }

    #endregion

    #region 特殊事件同步

        public class SpecialEventSync
    {
                public static void SyncBossRewardSelection(string bossId, string rewardType, string rewardId)
        {
            try
            {
                if (!TryGetConnectedSubscribedClient(out INetworkClient networkClient))
                {
                    return;
                }

                if (IsOutgoingSuppressed)
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

                networkClient.BroadcastState(NetworkMessageTypes.OnBossRewardSelection, rewardData);

                Plugin.Logger?.LogInfo($"[EventSync] Boss 奖励选择: {rewardType} - {rewardId}");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[EventSync] SyncBossRewardSelection 异常: {ex.Message}");
            }
        }

                public static void SyncShopEvent(string shopId, string eventType)
        {
            try
            {
                if (!TryGetConnectedSubscribedClient(out INetworkClient networkClient))
                {
                    return;
                }

                if (IsOutgoingSuppressed)
                {
                    return;
                }
                var shopData = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    ShopId = shopId,
                    EventType = eventType,
                };

                networkClient.BroadcastState(NetworkMessageTypes.OnShopEvent, shopData);
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[EventSync] SyncShopEvent 异常: {ex.Message}");
            }
        }

                public static void SyncTreasureEvent(string treasureId, List<string> rewards)
        {
            try
            {
                if (!TryGetConnectedSubscribedClient(out INetworkClient networkClient))
                {
                    return;
                }

                if (IsOutgoingSuppressed)
                {
                    return;
                }
                var treasureData = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    TreasureId = treasureId,
                    Rewards = rewards,
                };

                networkClient.BroadcastState(NetworkMessageTypes.OnTreasureEvent, treasureData);
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[EventSync] SyncTreasureEvent 异常: {ex.Message}");
            }
        }
    }

    #endregion

    #region 事件投票（预留）

        public class EventVotingSystem
    {

        private static readonly Dictionary<string, Dictionary<string, int>> _playerVotes = new(StringComparer.Ordinal);

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

                public static bool IsVotingRequired(string eventId)
        {
            if (string.IsNullOrWhiteSpace(eventId))
            {
                return false;
            }

            if (NetworkIdentityTracker.GetPlayerIdsSnapshot().Count <= 1)
            {
                return false;
            }

            return _votingEventIds.Contains(eventId);
        }

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

            votesByPlayer[playerId] = optionIndex;

            if (AllPlayersVoted(eventId))
            {
                ResolveVoting(eventId);
            }
        }

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

            foreach (string pid in players)
            {
                if (!votesByPlayer.ContainsKey(pid))
                {
                    return false;
                }
            }

            return true;
        }

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

            BroadcastVotingResult(eventId, winningOption, votesByPlayer.Count);

            TryApplyVoteResultLocally(eventId, winningOption);

            TryBroadcastSelectionFromVoteResult(eventId, winningOption);

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

            }
        }

                private static void BroadcastVotingResult(string eventId, int winningOption, int totalVotes)
        {
            try
            {
                if (!TryGetConnectedSubscribedClient(out INetworkClient networkClient))
                {
                    return;
                }

                if (IsOutgoingSuppressed)
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

                networkClient.BroadcastState(NetworkMessageTypes.OnEventVotingResult, resultData);

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

        public static object BuildEventSnapshot()
    {

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

            if (!client.IsConnected)
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

        }
    }

    [HarmonyPatch(typeof(VnPanel), "CoRunDialog")]
    [HarmonyPrefix]
    public static void VnPanel_CoRunDialog_Prefix(string vnName, DialogStorage storage, Yarn.Library library,
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

            if (adventure != null && client != null && client.IsConnected && NetworkIdentityTracker.GetSelfIsHost())
            {
                EventInitSync.SyncEventStart(adventure, adventure.Id, adventure.Title);
            }
        }
        catch
        {

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

            string speaker = string.Empty;
            GameObject leftRoot = AccessTools.Field(typeof(VnPanel), "leftCharacterNameRoot")?.GetValue(__instance) as GameObject;
            GameObject rightRoot = AccessTools.Field(typeof(VnPanel), "rightCharacterNameRoot")?.GetValue(__instance) as GameObject;

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

        bool hasNext = false;
        try
        {
            hasNext = (original != null && original.MoveNext());
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[EventSync] WrapShowOptions MoveNext 异常: {ex.Message}");
        }

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

        if (panel != null && options != null && NetworkIdentityTracker.GetSelfIsHost())
        {
            ShouldBroadcastAgain(ref _lastDialogOptionsBroadcastTicks, TimeSpan.FromSeconds(2));
        }

        var client = serviceProvider?.GetService<INetworkClient>();
        if (panel != null && client != null && client.IsConnected && !NetworkIdentityTracker.GetSelfIsHost())
        {
            string eventId;
            lock (SyncLock)
            {
                eventId = _activeEventId;
            }

            if (!string.IsNullOrWhiteSpace(eventId) && !EventVotingSystem.IsVotingRequired(eventId))
            {
                try
                {
                    var widgets = AccessTools.Field(typeof(VnPanel), "optionWidgets")?.GetValue(panel) as LBoL.Presentation.UI.Widgets.OptionWidget[];
                    if (widgets != null)
                    {
                        for (int i = 0; i < widgets.Length; i++)
                        {
                            var widget = widgets[i];
                            if (widget != null && widget.gameObject.activeSelf)
                            {

                                var btn = widget.GetComponentInChildren<UnityEngine.UI.Button>();
                                if (btn != null)
                                {
                                    btn.interactable = false;
                                }
                                widget.enabled = false;

                                var tmp = widget.GetComponentInChildren<TextMeshProUGUI>();
                                if (tmp != null)
                                {
                                    string suffix = " (等待房主选择,此选项已禁用)";
                                    if (!tmp.text.Contains(suffix))
                                    {
                                        tmp.text += suffix;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Plugin.Logger?.LogError($"[EventSync] 禁用对话选项异常: {ex}");
                }
            }
        }

        if (hasNext)
        {
            yield return original.Current;

            while (original != null && original.MoveNext())
            {

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

                        if (NetworkIdentityTracker.GetSelfIsHost() && ShouldBroadcastAgain(ref _lastDialogOptionsBroadcastTicks, TimeSpan.FromSeconds(2)))
                        {
                            TrySendDialogOptions(panel, options);
                        }
                    }
                }
                catch
                {

                }

                yield return original.Current;
            }
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
                    OptionId = opt.Id,
                    Text = opt.GetLocalizedText(runner),
                    IsAvailable = opt.Available,
                    Tooltip = string.Empty,
                });
            }

            lock (SyncLock)
            {
                _cachedDialogOptionsByEventId[eventId] = list;
            }

            DialogSync.SyncDialogOptions(eventId, list);
        }
        catch
        {

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

            if (EventVotingSystem.IsVotingRequired(eventId))
            {
                return;
            }

            EventSelectionSync.SyncEventSelection(eventId, optionIndex, id, optionText, "DialogOptionConfirmed");
        }
        catch
        {

        }
    }

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

                client.BroadcastState(NetworkMessageTypes.OnEventVoteCast, new
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

            GameObject optionsRoot = AccessTools.Field(typeof(VnPanel), "optionsRoot")?.GetValue(__instance) as GameObject;
            if (optionsRoot == null || !optionsRoot.activeSelf)
            {
                return true;
            }

            if (EventVotingSystem.IsVotingRequired(eventId))
            {

                DialogOption[] currentOptions = AccessTools.Field(typeof(VnPanel), "_options")?.GetValue(__instance) as DialogOption[];
                if (currentOptions == null)
                {
                    __result = true;
                    return false;
                }

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

                client.BroadcastState(NetworkMessageTypes.OnEventVoteCast, new
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
