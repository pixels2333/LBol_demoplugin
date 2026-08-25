using NetworkPlugin.Configuration;
using NetworkPlugin.Core;
using NetworkPlugin.Network.Event;
using Xunit;

namespace NetworkPlugin.Tests;

public class StateCacheManagerAdvancedTests
{
    private static StateCacheManager CreateManager()
    {
        return new StateCacheManager(new SyncConfiguration
        {
            StateCacheExpiry = TimeSpan.FromMinutes(5)
        });
    }

    [Fact]
    public void UpdateLocalState_AddsEntry()
    {
        var mgr = CreateManager();
        var evt = new GameEvent("TestEvent", "player1", "data123");

        mgr.UpdateLocalState(evt);
        Assert.Equal(1, mgr.CachedStateCount);
    }

    [Fact]
    public void UpdateLocalState_MultipleEvents_IncrementsCount()
    {
        var mgr = CreateManager();
        mgr.UpdateLocalState(new GameEvent("A", "p1", "x"));
        mgr.UpdateLocalState(new GameEvent("B", "p2", "y"));

        Assert.Equal(2, mgr.CachedStateCount);
    }

    [Fact]
    public void UpdateLocalState_NullEvent_DoesNotThrow()
    {
        var mgr = CreateManager();
        var exception = Record.Exception(() => mgr.UpdateLocalState(null));
        Assert.Null(exception);
    }

    [Fact]
    public void ValidateEventTimestamp_MaxValue_ReturnsFalse()
    {
        var mgr = CreateManager();
        Assert.False(mgr.ValidateEventTimestamp(DateTime.MaxValue));
    }

    [Fact]
    public void ValidateEventTimestamp_PastWithinRange_ReturnsTrue()
    {
        var mgr = CreateManager();
        // 5 minutes ago should be valid (max is 1 hour ago)
        Assert.True(mgr.ValidateEventTimestamp(DateTime.Now.AddMinutes(-5)));
    }

    [Fact]
    public void ValidateEventTimestamp_PastMoreThanHour_ReturnsFalse()
    {
        var mgr = CreateManager();
        Assert.False(mgr.ValidateEventTimestamp(DateTime.Now.AddHours(-2)));
    }

    [Fact]
    public void ApplyRemoteEvent_ControlMessage_DoesNotCache()
    {
        var mgr = CreateManager();
        var evt = new GameEvent("FullStateSyncRequest", "test", "");

        mgr.ApplyRemoteEvent(evt);
        Assert.True(evt.IsProcessed);
        Assert.Equal(0, mgr.CachedStateCount);
    }

    [Fact]
    public void ApplyRemoteEvent_Null_DoesNotThrow()
    {
        var mgr = CreateManager();
        var exception = Record.Exception(() => mgr.ApplyRemoteEvent(null));
        Assert.Null(exception);
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateLocalState_ExpiredCache_IsRemoved()
    {
        var mgr = new StateCacheManager(new SyncConfiguration
        {
            StateCacheExpiry = TimeSpan.FromMilliseconds(50)
        });

        mgr.UpdateLocalState(new GameEvent("EventA", "p1", "data1"));
        Assert.Equal(1, mgr.CachedStateCount);

        await System.Threading.Tasks.Task.Delay(100);

        mgr.UpdateLocalState(new GameEvent("EventB", "p1", "data2"));
        // EventA > 50ms 缓存应被清理，只保留 EventB
        Assert.Equal(1, mgr.CachedStateCount);
    }
}
