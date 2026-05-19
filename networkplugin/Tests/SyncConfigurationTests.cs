using NetworkPlugin.Configuration;
using Xunit;

namespace NetworkPlugin.Tests;

public class SyncConfigurationTests
{
    [Fact]
    public void DefaultValues_AreSane()
    {
        var config = new SyncConfiguration();

        Assert.True(config.EnableCardSync);
        Assert.True(config.EnableManaSync);
        Assert.True(config.EnableBattleSync);
        Assert.True(config.EnableMapSync);
        Assert.True(config.EnableStatusEffectSync);
        Assert.True(config.EnableNatDetection);
        Assert.False(config.EnableUpnpExperimental);
    }

    [Fact]
    public void DefaultQueueSize_Is100()
    {
        var config = new SyncConfiguration();
        Assert.Equal(100, config.MaxQueueSize);
    }

    [Fact]
    public void DefaultCacheExpiry_Is5Minutes()
    {
        var config = new SyncConfiguration();
        Assert.Equal(5, config.StateCacheExpiry.TotalMinutes);
    }

    [Fact]
    public void CanSetAndReadProperties()
    {
        var config = new SyncConfiguration
        {
            EnableCardSync = false,
            EnableManaSync = false,
            MaxQueueSize = 50,
            StateCacheExpiry = TimeSpan.FromMinutes(10)
        };

        Assert.False(config.EnableCardSync);
        Assert.False(config.EnableManaSync);
        Assert.Equal(50, config.MaxQueueSize);
        Assert.Equal(10, config.StateCacheExpiry.TotalMinutes);
    }
}
