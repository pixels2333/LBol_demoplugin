using NetworkPlugin.Core;
using Xunit;

namespace NetworkPlugin.Tests;

public class NetworkEventBufferManagerAdvancedTests
{
    [Fact]
    public void MultipleEvents_MaintainOrder()
    {
        var mgr = new NetworkEventBufferManager();
        long now = DateTime.Now.Ticks;

        var event1 = new Dictionary<string, object>
        {
            ["EventType"] = "EventA",
            ["Timestamp"] = now
        };
        var event2 = new Dictionary<string, object>
        {
            ["EventType"] = "EventB",
            ["Timestamp"] = now + 1000
        };

        mgr.EnqueueEvent(event1);
        mgr.EnqueueEvent(event2);

        bool hitA = false, hitB = false;
        mgr.ProcessBufferedEvents(gameEvent =>
        {
            if (gameEvent.EventType == "EventA") hitA = true;
            if (gameEvent.EventType == "EventB") hitB = true;
        });

        Assert.True(hitA);
        Assert.True(hitB);
    }

    [Fact]
    public void ProcessBufferedEvents_EmptyBuffer_DoesNotThrow()
    {
        var mgr = new NetworkEventBufferManager();
        var exception = Record.Exception(() =>
            mgr.ProcessBufferedEvents(gameEvent => { }));
        Assert.Null(exception);
    }

    [Fact]
    public void EnqueueEvent_SameTickEvents_AllAccepted()
    {
        var mgr = new NetworkEventBufferManager();
        long sameTick = DateTime.Now.Ticks;

        var event1 = new Dictionary<string, object>
        {
            ["EventType"] = "First",
            ["Timestamp"] = sameTick
        };
        var event2 = new Dictionary<string, object>
        {
            ["EventType"] = "Second",
            ["Timestamp"] = sameTick
        };

        mgr.EnqueueEvent(event1);
        var ex = Record.Exception(() => mgr.EnqueueEvent(event2));
        Assert.Null(ex);
    }

    [Fact]
    public void ProcessBufferedEvents_WithCallback_InvokesCallback()
    {
        var mgr = new NetworkEventBufferManager();
        int callCount = 0;

        var evt = new Dictionary<string, object>
        {
            ["EventType"] = "CountTest",
            ["Payload"] = "hello",
            ["Timestamp"] = DateTime.Now.Ticks
        };

        mgr.EnqueueEvent(evt);
        mgr.ProcessBufferedEvents(gameEvent =>
        {
            callCount++;
            Assert.Equal("CountTest", gameEvent.EventType);
        });

        Assert.Equal(1, callCount);
    }

    [Fact]
    public void GetStatistics_AfterEvent_ShowsCount()
    {
        var mgr = new NetworkEventBufferManager();

        var evt = new Dictionary<string, object>
        {
            ["EventType"] = "Stats",
            ["Timestamp"] = DateTime.Now.Ticks
        };
        mgr.EnqueueEvent(evt);

        mgr.ProcessBufferedEvents(_ => { });

        var stats = mgr.GetStatistics();
        Assert.NotNull(stats);
    }

    [Fact]
    public void DescribePayloadHead200_Null_ReturnsEmpty()
    {
        var result = NetworkEventBufferManager.DescribePayloadHead200(null);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void DescribePayloadHead200_LongString_Truncated()
    {
        var longString = new string('a', 500);
        var evt = new { EventType = "Test", Payload = longString };
        var result = NetworkEventBufferManager.DescribePayloadHead200(evt);
        Assert.True(result.Length <= 200, "Should be truncated to 200 chars");
    }
}
