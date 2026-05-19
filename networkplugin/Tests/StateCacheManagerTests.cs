using NetworkPlugin.Configuration;
using NetworkPlugin.Core;
using Xunit;

namespace NetworkPlugin.Tests;

public class StateCacheManagerTests
{
    private static StateCacheManager CreateManager()
    {
        var config = new SyncConfiguration
        {
            StateCacheExpiry = TimeSpan.FromMinutes(5)
        };
        return new StateCacheManager(config);
    }

    [Fact]
    public void Constructor_DoesNotThrow()
    {
        var exception = Record.Exception(() => CreateManager());
        Assert.Null(exception);
    }

    [Fact]
    public void ValidateEventTimestamp_CurrentTime_ReturnsTrue()
    {
        var mgr = CreateManager();
        Assert.True(mgr.ValidateEventTimestamp(DateTime.Now));
    }

    [Fact]
    public void ValidateEventTimestamp_FutureTime_ReturnsFalse()
    {
        var mgr = CreateManager();
        Assert.False(mgr.ValidateEventTimestamp(DateTime.Now.AddDays(1)));
    }

    [Fact]
    public void ValidateEventTimestamp_MinValue_ReturnsFalse()
    {
        var mgr = CreateManager();
        Assert.False(mgr.ValidateEventTimestamp(DateTime.MinValue));
    }

    [Fact]
    public void CachedStateCount_InitiallyZero()
    {
        var mgr = CreateManager();
        Assert.Equal(0, mgr.CachedStateCount);
    }
}
