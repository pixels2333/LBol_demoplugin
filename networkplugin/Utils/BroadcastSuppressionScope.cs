using System;
using System.Threading;

namespace NetworkPlugin.Utils;

public sealed class BroadcastSuppressionScope : IDisposable
{
    private static int _depth;

    public static bool IsSuppressed => _depth > 0;

    public BroadcastSuppressionScope() => Interlocked.Increment(ref _depth);

    public void Dispose() => Interlocked.Decrement(ref _depth);
}
