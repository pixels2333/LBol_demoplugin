using System;
using System.Collections.Generic;
using NetworkPlugin.Network.Event;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Configuration;

namespace NetworkPlugin.Core;

/// <summary>
/// 本地状态缓存管理器
/// 负责存储最近的游戏状态快照、验证事件时间戳、应用远程事件到缓存
/// </summary>
internal sealed class StateCacheManager
{
    /// <summary>本地状态缓存字典</summary>
    private readonly Dictionary<string, object> _stateCache = new(StringComparer.Ordinal);
    /// <summary>同步配置</summary>
    private readonly SyncConfiguration _config;

    /// <summary>
    /// 初始化状态缓存管理器
    /// </summary>
    /// <param name="config">同步配置</param>
    public StateCacheManager(SyncConfiguration config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>
    /// 更新本地状态缓存
    /// </summary>
    public void UpdateLocalState(GameEvent gameEvent)
    {
        if (gameEvent == null) return;

        string stateKey = $"{gameEvent.EventType}_{gameEvent.UserName}";
        _stateCache[stateKey] = gameEvent.Data;

        // 清理过期的旧状态缓存条目
        List<string> keysToRemove = [];
        foreach (var kvp in _stateCache)
        {
            if (kvp.Key.Contains("Old") || kvp.Key.Contains("Temp"))
                keysToRemove.Add(kvp.Key);
        }
        foreach (string key in keysToRemove)
            _stateCache.Remove(key);
    }

    /// <summary>
    /// 验证事件时间戳的有效性
    /// </summary>
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

    /// <summary>
    /// 将从网络接收的远程事件应用到本地状态缓存
    /// </summary>
    public void ApplyRemoteEvent(GameEvent gameEvent)
    {
        if (gameEvent == null) return;

        if (!ValidateEventTimestamp(new DateTime(gameEvent.Timestamp)))
        {
            Plugin.Logger?.LogWarning($"[StateCache] Remote event timestamp invalid: {gameEvent.EventType} ({gameEvent.Timestamp})");
            return;
        }

        // 控制类消息不写入缓存（缓存仅用于业务状态粗粒度对账）
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

    /// <summary>
    /// 获取状态缓存条目数
    /// </summary>
    public int CachedStateCount => _stateCache.Count;


}
