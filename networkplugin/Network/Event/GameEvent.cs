using System;
using System.Collections.Generic;

namespace NetworkPlugin.Network.Event;

/// <summary>
/// 游戏事件基类 - 用于断线重连和中途加入的事件回放
/// </summary>
public class GameEvent
{
    /// <summary>
    /// 事件唯一ID
    /// </summary>
    public string EventId { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// 事件类型
    /// </summary>
    public string EventType { get; set; } = "Unknown";

    /// <summary>
    /// 事件时间戳（用于排序和追赶）
    /// </summary>
    public long Timestamp { get; set; } = DateTime.Now.Ticks;

    /// <summary>
    /// 事件索引（递增，用于确定事件顺序）
    /// </summary>
    public long EventIndex { get; set; } = 0;

    /// <summary>
    /// 触发事件的玩家ID
    /// </summary>
    public string PlayerId { get; set; } = "unknown";

    /// <summary>
    /// 影响的目标ID
    /// </summary>
    public string? TargetId { get; set; }

    /// <summary>
    /// 事件数据
    /// </summary>
    public Dictionary<string, object> Data { get; set; } = [];

    /// <summary>
    /// 事件是否已处理
    /// </summary>
    public bool IsProcessed { get; set; } = false;

    /// <summary>
    /// 事件来源（Client/Server/System）
    /// </summary>
    public string Source { get; set; } = "Unknown";

    /// <summary>
    /// 构造函数
    /// </summary>
    public GameEvent() { }

    /// <summary>
    /// 构造函数
    /// </summary>
    public GameEvent(string eventType, string playerId, Dictionary<string, object> data)
    {
        EventType = eventType;
        PlayerId = playerId;
        Data = data;
        Timestamp = DateTime.Now.Ticks;
    }


}