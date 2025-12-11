using System.Collections.Generic;
using System.Threading;

namespace NetworkPlugin.Network.Event;

/// <summary>
/// 游戏事件管理器 - 用于创建和管理游戏事件
/// </summary>
public static class GameEventManager
{
    private static long _nextEventIndex = 0;

    /// <summary>
    /// 创建新事件并分配索引
    /// </summary>
    public static GameEvent CreateEvent(string eventType, string userName, object data)
    {
        GameEvent gameEvent = new(eventType, userName, data)
        {
            EventIndex = Interlocked.Increment(ref _nextEventIndex)
        };
        return gameEvent;
    }

    /// <summary>
    /// 获取当前事件索引
    /// </summary>
    public static long GetCurrentEventIndex()
    {
        return _nextEventIndex;
    }

    /// <summary>
    /// 重置事件索引计数器
    /// </summary>
    public static void ResetEventIndex()
    {
        _nextEventIndex = 0;
    }

    /// <summary>
    /// 从特定索引开始
    /// </summary>
    public static void SetEventIndex(long index)
    {
        _nextEventIndex = index;
    }
}