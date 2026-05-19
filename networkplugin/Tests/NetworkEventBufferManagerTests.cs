using NetworkPlugin.Core;
using Xunit;

namespace NetworkPlugin.Tests;

public class NetworkEventBufferManagerTests
{
    [Fact]
    public void EnqueueEvent_NullInput_DoesNotThrow()
    {
        var mgr = new NetworkEventBufferManager();
        var exception = Record.Exception(() => mgr.EnqueueEvent(null));
        Assert.Null(exception);
    }

    [Fact]
    public void EnqueueEvent_ValidEvent_ProcessesSuccessfully()
    {
        var mgr = new NetworkEventBufferManager();
        var eventData = new Dictionary<string, object>
        {
            ["EventType"] = "TestEvent",
            ["Payload"] = "test",
            ["Timestamp"] = DateTime.Now.Ticks
        };

        var exception = Record.Exception(() => mgr.EnqueueEvent(eventData));
        Assert.Null(exception);
    }

    [Fact]
    public void GetStatistics_ReturnsNonNull()
    {
        var mgr = new NetworkEventBufferManager();
        var stats = mgr.GetStatistics();
        Assert.NotNull(stats);
    }

    [Fact]
    public void TryNormalizeNetworkEvent_DictInput_ReturnsTrue()
    {
        var input = new Dictionary<string, object> { ["EventType"] = "Test" };
        bool result = NetworkEventBufferManager.TryNormalizeNetworkEvent(input, out var dict);
        Assert.True(result);
        Assert.NotNull(dict);
        Assert.Equal("Test", dict["EventType"]);
    }

    [Fact]
    public void TryNormalizeNetworkEvent_AnonymousObject_ReturnsTrue()
    {
        var input = new { EventType = "Test", Payload = "data" };
        bool result = NetworkEventBufferManager.TryNormalizeNetworkEvent(input, out var dict);
        Assert.True(result);
        Assert.NotNull(dict);
        Assert.Equal("Test", dict["EventType"]);
    }
}
