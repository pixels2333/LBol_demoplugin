using System;
using System.Collections.Generic;
using System.Linq;
using NetworkPlugin.Network.Event;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Core;

internal sealed class NetworkEventBufferManager
{

    private readonly SortedList<long, NetworkEventBuffer> _remoteEventBuffer = [];
    private readonly object _remoteEventBufferLock = new();

    private static readonly TimeSpan EventBufferTimeout = TimeSpan.FromSeconds(30);

        public void EnqueueEvent(object eventData)
    {
        if (!TryNormalizeNetworkEvent(eventData, out Dictionary<string, object> eventDict))
        {
            Plugin.Logger?.LogWarning($"[EventBufferManager] 无效的网络事件数据格式: type={eventData?.GetType().FullName}, head200={DescribePayloadHead200(eventData)}");
            return;
        }

        long timestamp = eventDict.ContainsKey("Timestamp")
            ? Convert.ToInt64(eventDict["Timestamp"])
            : DateTime.Now.Ticks;

        long key = timestamp;
        lock (_remoteEventBufferLock)
        {
            int attempts = 0;
            const int maxAttempts = 1000;
            while (_remoteEventBuffer.ContainsKey(key))
            {
                if (++attempts >= maxAttempts)
                {
                    Plugin.Logger?.LogError($"[EventBufferManager] 解决 key 冲突达到最大尝试次数 ({maxAttempts})，丢弃事件");
                    return;
                }

                if (key == long.MaxValue)
                {
                    key = DateTime.Now.Ticks;
                    continue;
                }
                key++;
            }

            var eventBuffer = new NetworkEventBuffer(key, eventDict);
            _remoteEventBuffer.Add(key, eventBuffer);
        }

        string eventType = eventDict["EventType"].ToString();
        Plugin.Logger?.LogDebug($"[EventBufferManager] 接收到网络事件: {eventType}, 时间戳: {key}");
    }

        public void ProcessBufferedEvents(Action<GameEvent> applyEventCallback)
    {
        CleanupTimeoutEvents();

        List<long> timestampsToRemove = [];

        List<KeyValuePair<long, NetworkEventBuffer>> snapshot;
        lock (_remoteEventBufferLock)
        {
            snapshot = _remoteEventBuffer.ToList();
        }

        foreach (var kvp in snapshot)
        {
            long timestamp = kvp.Key;
            NetworkEventBuffer eventBuffer = kvp.Value;

            if (eventBuffer.Status != NetworkEventBuffer.ProcessingStatus.Pending)
                continue;

            if (eventBuffer.IsTimeout(EventBufferTimeout))
            {
                Plugin.Logger?.LogWarning($"[EventBufferManager] 事件超时，丢弃: {eventBuffer.OriginalData["EventType"]}, 时间戳: {timestamp}");
                eventBuffer.Status = NetworkEventBuffer.ProcessingStatus.Discarded;
                timestampsToRemove.Add(timestamp);
                continue;
            }

            ProcessSingleNetworkEvent(eventBuffer, applyEventCallback);
            eventBuffer.Status = NetworkEventBuffer.ProcessingStatus.Completed;

            string eventType = eventBuffer.OriginalData["EventType"].ToString();
            Plugin.Logger?.LogDebug($"[EventBufferManager] 事件处理成功: {eventType}, 时间戳: {timestamp}");
            timestampsToRemove.Add(timestamp);
        }

        lock (_remoteEventBufferLock)
        {
            foreach (long timestamp in timestampsToRemove)
            {
                _remoteEventBuffer.Remove(timestamp);
            }
        }
    }

        private void ProcessSingleNetworkEvent(NetworkEventBuffer eventBuffer, Action<GameEvent> applyEventCallback)
    {
        var eventDict = eventBuffer.OriginalData;
        string eventType = eventDict["EventType"].ToString();
        object payload = eventDict.ContainsKey("Payload") ? eventDict["Payload"] : string.Empty;
        var timestamp = new DateTime(eventBuffer.Timestamp);

        var gameEvent = CreateGameEventFromNetworkData(eventType, payload, timestamp);
        if (gameEvent == null)
        {
            Plugin.Logger?.LogWarning($"[EventBufferManager] 无法创建游戏事件: {eventType}");
            return;
        }

        applyEventCallback(gameEvent);

        Plugin.Logger?.LogDebug($"[EventBufferManager] 单个事件应用成功: {gameEvent.EventType} (时间戳: {timestamp})");
    }

        private static GameEvent CreateGameEventFromNetworkData(string eventType, object payload, DateTime timestamp)
    {
        if (string.IsNullOrWhiteSpace(eventType)) eventType = "Unknown";

        string playerName = ResolvePlayerName(payload);

        return new GameEvent
        {
            EventType = eventType,
            Data = payload ?? string.Empty,
            Timestamp = timestamp.Ticks,
            UserName = playerName,
            Source = "Network",
            IsProcessed = false,
        };
    }

        private static string ResolvePlayerName(object payload)
    {
        if (payload is Dictionary<string, object> dict)
        {
            if (TryGetNonEmptyString(dict, "PlayerName", out string playerName))
                return playerName;
            if (TryGetNonEmptyString(dict, "UserName", out string legacyUserName))
                return legacyUserName;
            if (TryGetNonEmptyString(dict, "username", out string legacyUserNameLower))
                return legacyUserNameLower;
        }
        return "remote";
    }

        private static bool TryGetNonEmptyString(Dictionary<string, object> dict, string key, out string value)
    {
        value = null;
        if (!dict.TryGetValue(key, out object raw) || raw == null) return false;
        if (raw is string s && !string.IsNullOrWhiteSpace(s))
        {
            value = s;
            return true;
        }
        return false;
    }

        private void CleanupTimeoutEvents()
    {
        List<long> timestampsToRemove = [];

        List<KeyValuePair<long, NetworkEventBuffer>> snapshot;
        lock (_remoteEventBufferLock)
        {
            snapshot = _remoteEventBuffer.ToList();
        }

        foreach (var kvp in snapshot)
        {
            if (kvp.Value.Status == NetworkEventBuffer.ProcessingStatus.Pending &&
                kvp.Value.IsTimeout(EventBufferTimeout))
            {
                Plugin.Logger?.LogWarning($"[EventBufferManager] 清理超时事件: {kvp.Value.OriginalData["EventType"]}, 时间戳: {kvp.Key}");
                timestampsToRemove.Add(kvp.Key);
            }
        }

        lock (_remoteEventBufferLock)
        {
            foreach (long timestamp in timestampsToRemove)
                _remoteEventBuffer.Remove(timestamp);
        }
    }

        public object GetStatistics()
    {
        lock (_remoteEventBufferLock)
        {
            var statusCounts = new Dictionary<NetworkEventBuffer.ProcessingStatus, int>();
            foreach (NetworkEventBuffer.ProcessingStatus status in Enum.GetValues(typeof(NetworkEventBuffer.ProcessingStatus)))
                statusCounts[status] = 0;

            foreach (var kvp in _remoteEventBuffer)
                statusCounts[kvp.Value.Status]++;

            long? oldest = _remoteEventBuffer.Count > 0 ? _remoteEventBuffer.Keys[0] : null;
            long? newest = _remoteEventBuffer.Count > 0 ? _remoteEventBuffer.Keys[^1] : null;

            return new
            {
                TotalEvents = _remoteEventBuffer.Count,
                BufferCapacity = _remoteEventBuffer.Capacity,
                StatusDistribution = statusCounts.ToDictionary(k => k.Key.ToString(), k => k.Value),
                OldestTimestamp = oldest,
                NewestTimestamp = newest,
                TimeRange = oldest.HasValue && newest.HasValue
                    ? TimeSpan.FromTicks(newest.Value - oldest.Value) : (TimeSpan?)null,
                TimeoutThreshold = EventBufferTimeout.TotalSeconds,
            };
        }
    }

        public static bool TryNormalizeNetworkEvent(object eventData, out Dictionary<string, object> eventDict)
    {
        eventDict = null;
        if (eventData == null) return false;

        if (eventData is Dictionary<string, object> dict)
        {
            if (dict.ContainsKey("EventType"))
            {
                eventDict = dict;
                return true;
            }
            return false;
        }

        var t = eventData.GetType();
        var pEventType = t.GetProperty("EventType");
        if (pEventType == null) return false;

        object et = pEventType.GetValue(eventData, null);
        if (et == null) return false;

        var d = new Dictionary<string, object>(StringComparer.Ordinal) { ["EventType"] = et };

        var pPayload = t.GetProperty("Payload");
        if (pPayload != null) d["Payload"] = pPayload.GetValue(eventData, null);

        var pTimestamp = t.GetProperty("Timestamp");
        if (pTimestamp != null) d["Timestamp"] = pTimestamp.GetValue(eventData, null);

        eventDict = d;
        return true;
    }

        internal static string DescribePayloadHead200(object maybeEvent)
    {
        if (maybeEvent == null) return string.Empty;
        try
        {
            var t = maybeEvent.GetType();
            var pPayload = t.GetProperty("Payload");
            object payload = pPayload != null ? pPayload.GetValue(maybeEvent, null) : maybeEvent;

            string s = payload switch
            {
                null => string.Empty,
                string str => str,
                _ => JsonCompat.Serialize(payload)
            };

            s = (s ?? string.Empty).Replace("\r", "").Replace("\n", " ");
            return s.Length > 200 ? s[..200] : s;
        }
        catch
        {
            string fallback = (maybeEvent.ToString() ?? string.Empty).Replace("\r", "").Replace("\n", " ");
            return fallback.Length > 200 ? fallback[..200] : fallback;
        }
    }
}
