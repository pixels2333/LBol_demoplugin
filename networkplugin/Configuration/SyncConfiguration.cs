using System;

namespace NetworkPlugin.Configuration;

public sealed class SyncConfiguration
{
    public bool EnableCardSync { get; set; } = true;
    public bool EnableManaSync { get; set; } = true;
    public bool EnableBattleSync { get; set; } = true;
    public bool EnableMapSync { get; set; } = true;
    public int MaxQueueSize { get; set; } = 100;
    public TimeSpan StateCacheExpiry { get; set; } = TimeSpan.FromMinutes(5);
}
