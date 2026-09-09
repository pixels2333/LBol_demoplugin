using System;
using System.Threading;

namespace NetworkPlugin.Utils;

/// <summary>
/// 追踪远端卡牌视效回放生命周期的轻量级标记器，用于协调视效打击与底层数据跳字的排他性。
/// </summary>
public static class RemoteCardPlaybackTracker
{
    private static int _activeVisualCount;
    private static long _playbackWindowUntilTick;

    /// <summary>
    /// 当前是否正处于远端卡牌视效打击回放视窗内。
    /// </summary>
    public static bool IsInCardPlaybackWindow
    {
        get
        {
            if (Volatile.Read(ref _activeVisualCount) > 0)
            {
                long until = Interlocked.Read(ref _playbackWindowUntilTick);
                return DateTime.UtcNow.Ticks < until;
            }
            return false;
        }
    }

    /// <summary>
    /// 开启卡牌视效打击回放视窗（默认 2.5 秒超时保底，防止极端异常下永久静默）。
    /// </summary>
    public static void NotifyCardVisualStarted(float timeoutSeconds = 2.5f)
    {
        Interlocked.Increment(ref _activeVisualCount);
        long targetTick = DateTime.UtcNow.AddSeconds(Math.Max(0.5f, timeoutSeconds)).Ticks;
        Interlocked.Exchange(ref _playbackWindowUntilTick, targetTick);
    }

    /// <summary>
    /// 结束卡牌视效打击回放视窗。
    /// </summary>
    public static void NotifyCardVisualEnded()
    {
        int count = Interlocked.Decrement(ref _activeVisualCount);
        if (count <= 0)
        {
            Interlocked.Exchange(ref _activeVisualCount, 0);
            Interlocked.Exchange(ref _playbackWindowUntilTick, 0);
        }
    }

    /// <summary>
    /// 供单元测试重置内部状态。
    /// </summary>
    public static void ResetForTest()
    {
        Interlocked.Exchange(ref _activeVisualCount, 0);
        Interlocked.Exchange(ref _playbackWindowUntilTick, 0);
    }
}
