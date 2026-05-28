using System;
using System.Collections.Generic;
using System.Linq;
using NetworkPlugin.Network.Event;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Core;

/// <summary>
/// 远程事件缓冲区管理器
/// 负责存储、排序、处理来自网络的远程事件，确保事件按时间戳有序消费
/// </summary>
internal sealed class NetworkEventBufferManager
{
    // SortedList 不允许重复 key；网络突发时同一 tick 可能收到多条消息
    private readonly SortedList<long, NetworkEventBuffer> _remoteEventBuffer = [];
    private readonly object _remoteEventBufferLock = new();

    // 小于该阈值时直接取 Max，降低 key 冲突概率；推荐值：最近 5 秒的 tick 窗口
    private const long KeyWindowTicks = TimeSpan.TicksPerSecond * 5;

    private static readonly TimeSpan EventBufferTimeout = TimeSpan.FromSeconds(30);

    /// <summary>
    /// 接收来自网络的原始事件数据，标准化后加入缓冲区
    /// </summary>
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

        // 在锁内为 timestamp 找到可用的 key，保持总体顺序
        long key = timestamp;
        lock (_remoteEventBufferLock)
        {
            while (_remoteEventBuffer.ContainsKey(key))
            {
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

    /// <summary>
    /// 处理缓冲区中所有待处理的远程事件
    /// </summary>
    /// <param name="applyEventCallback">将 GameEvent 应用到本地状态的回调</param>
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

    /// <summary>
    /// 处理单个网络事件：解析事件数据、创建 GameEvent 并应用到本地状态
    /// </summary>
    /// <param name="eventBuffer">待处理的事件缓冲区条目</param>
    /// <param name="applyEventCallback">将 GameEvent 应用到本地状态的回调</param>
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

    /// <summary>
    /// 从网络事件数据创建 GameEvent 对象
    /// </summary>
    /// <param name="eventType">事件类型</param>
    /// <param name="payload">事件载荷</param>
    /// <param name="timestamp">事件时间戳</param>
    /// <returns>创建的 GameEvent 实例</returns>
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

    /// <summary>
    /// 从事件载荷中解析玩家名称
    /// </summary>
    /// <param name="payload">事件载荷</param>
    /// <returns>解析出的玩家名称，失败时返回 "remote"</returns>
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

    /// <summary>
    /// 尝试从字典中获取非空字符串值
    /// </summary>
    /// <param name="dict">源字典</param>
    /// <param name="key">键名</param>
    /// <param name="value">获取到的字符串值</param>
    /// <returns>成功获取且非空时返回 true</returns>
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

    /// <summary>
    /// 清理缓冲区中超时的待处理事件
    /// </summary>
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

    /// <summary>
    /// 获取缓冲区统计信息
    /// </summary>
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

    /// <summary>
    /// 将原始网络事件数据标准化为 Dictionary 格式
    /// </summary>
    /// <param name="eventData">原始事件数据</param>
    /// <param name="eventDict">标准化后的字典</param>
    /// <returns>标准化成功时返回 true</returns>
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

    /// <summary>
    /// 描述事件载荷的前 200 个字符（用于日志调试）
    /// </summary>
    /// <param name="maybeEvent">事件对象</param>
    /// <returns>载荷的前 200 字符描述</returns>
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
