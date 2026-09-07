using System;
using System.Collections.Generic;
using NetworkPlugin.Network.Event;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Configuration;

namespace NetworkPlugin.Core;

internal sealed class StateCacheManager
{
        private readonly Dictionary<string, (object Data, DateTime UpdatedTime)> _stateCache = new(StringComparer.Ordinal);
        private readonly SyncConfiguration _config;

        public StateCacheManager(SyncConfiguration config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

        public void UpdateLocalState(GameEvent gameEvent)
    {
        if (gameEvent == null) return;

        string stateKey = $"{gameEvent.EventType}_{gameEvent.UserName}";
        DateTime now = DateTime.UtcNow;
        _stateCache[stateKey] = (gameEvent.Data, now);

        DateTime cutoffTime = now - _config.StateCacheExpiry;
        List<string> keysToRemove = [];
        foreach (var kvp in _stateCache)
        {
            if (kvp.Key.Contains("Old") || kvp.Key.Contains("Temp") || kvp.Value.UpdatedTime < cutoffTime)
                keysToRemove.Add(kvp.Key);
        }
        foreach (string key in keysToRemove)
            _stateCache.Remove(key);
    }

        public bool ValidateEventTimestamp(DateTime timestamp)
    {
        var now = DateTime.Now;
        var maxFutureTime = now.AddSeconds(5);
        var minValidTime = now.AddHours(-1);

        if (timestamp > maxFutureTime)
        {
            Plugin.Logger?.LogWarning($"[StateCache] 事件时间戳在未来: {timestamp}, 当前时间: {now}");
            return false;
        }
        if (timestamp < minValidTime)
        {
            Plugin.Logger?.LogWarning($"[StateCache] 事件时间戳太早: {timestamp}, 当前时间: {now}");
            return false;
        }
        if (timestamp == DateTime.MinValue || timestamp == DateTime.MaxValue)
        {
            Plugin.Logger?.LogWarning($"[StateCache] 事件时间戳异常: {timestamp}");
            return false;
        }
        return true;
    }

        public void ApplyRemoteEvent(GameEvent gameEvent)
    {
        if (gameEvent == null) return;

        if (!ValidateEventTimestamp(new DateTime(gameEvent.Timestamp)))
        {
            Plugin.Logger?.LogWarning($"[StateCache] Remote event timestamp invalid: {gameEvent.EventType} ({gameEvent.Timestamp})");
            return;
        }

        if (string.Equals(gameEvent.EventType, NetworkMessageTypes.FullStateSyncRequest, StringComparison.Ordinal) ||
            string.Equals(gameEvent.EventType, NetworkMessageTypes.FullStateSyncResponse, StringComparison.Ordinal) ||
            string.Equals(gameEvent.EventType, "DirectMessage", StringComparison.Ordinal))
        {
            gameEvent.IsProcessed = true;
            return;
        }

        UpdateLocalState(gameEvent);
        gameEvent.IsProcessed = true;
    }

        public int CachedStateCount => _stateCache.Count;

}
