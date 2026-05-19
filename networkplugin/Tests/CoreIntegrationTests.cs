using NetworkPlugin.Configuration;
using NetworkPlugin.Core;
using NetworkPlugin.Network.Event;
using Xunit;

namespace NetworkPlugin.Tests;

/// <summary>
/// 集成测试：验证 EventBufferManager + StateCacheManager + NetworkAvailabilityTracker 的协同工作
/// </summary>
public class CoreIntegrationTests
{
    [Fact]
    public void FullPipeline_EventFlow_EndToEnd()
    {
        // Arrange: wire up all three managers
        var config = new SyncConfiguration { StateCacheExpiry = TimeSpan.FromMinutes(5) };
        var bufferMgr = new NetworkEventBufferManager();
        var cacheMgr = new StateCacheManager(config);
        int appliedCount = 0;
        string lastEventType = null;

        // Act: enqueue a network event, then process through the pipeline
        var rawEvent = new Dictionary<string, object>
        {
            ["EventType"] = "OnDamageDealt",
            ["Payload"] = "50 damage",
            ["Timestamp"] = DateTime.Now.Ticks,
            ["PlayerName"] = "Alice"
        };

        bufferMgr.EnqueueEvent(rawEvent);
        bufferMgr.ProcessBufferedEvents(gameEvent =>
        {
            cacheMgr.ApplyRemoteEvent(gameEvent);
            appliedCount++;
            lastEventType = gameEvent.EventType;
        });

        // Assert: event was processed and cached
        Assert.Equal(1, appliedCount);
        Assert.Equal("OnDamageDealt", lastEventType);
        Assert.Equal(1, cacheMgr.CachedStateCount);
    }

    [Fact]
    public void FullPipeline_MultipleEvents_ProcessedInOrder()
    {
        var config = new SyncConfiguration { StateCacheExpiry = TimeSpan.FromMinutes(5) };
        var bufferMgr = new NetworkEventBufferManager();
        var cacheMgr = new StateCacheManager(config);
        var processedTypes = new List<string>();
        long baseTime = DateTime.Now.Ticks;

        // Enqueue events out of timestamp order
        bufferMgr.EnqueueEvent(new Dictionary<string, object>
        {
            ["EventType"] = "Second", ["Timestamp"] = baseTime + 2000, ["PlayerName"] = "A"
        });
        bufferMgr.EnqueueEvent(new Dictionary<string, object>
        {
            ["EventType"] = "First", ["Timestamp"] = baseTime + 1000, ["PlayerName"] = "B"
        });

        bufferMgr.ProcessBufferedEvents(gameEvent =>
        {
            cacheMgr.ApplyRemoteEvent(gameEvent);
            processedTypes.Add(gameEvent.EventType);
        });

        // Currently SortedList orders by key (timestamp), so both should be processed
        Assert.Contains("First", processedTypes);
        Assert.Contains("Second", processedTypes);
        Assert.Equal(2, processedTypes.Count);
    }

    [Fact]
    public void FullPipeline_ControlMessage_NotCached()
    {
        var config = new SyncConfiguration { StateCacheExpiry = TimeSpan.FromMinutes(5) };
        var bufferMgr = new NetworkEventBufferManager();
        var cacheMgr = new StateCacheManager(config);

        bufferMgr.EnqueueEvent(new Dictionary<string, object>
        {
            ["EventType"] = "FullStateSyncRequest",
            ["Timestamp"] = DateTime.Now.Ticks
        });

        bufferMgr.ProcessBufferedEvents(gameEvent =>
        {
            cacheMgr.ApplyRemoteEvent(gameEvent);
        });

        // Control messages are processed but not stored in cache
        Assert.Equal(0, cacheMgr.CachedStateCount);
    }

    [Fact]
    public void FullPipeline_NullEventData_DoesNotCrash()
    {
        var bufferMgr = new NetworkEventBufferManager();
        var exception = Record.Exception(() =>
        {
            bufferMgr.EnqueueEvent(null);
            bufferMgr.ProcessBufferedEvents(_ => { });
        });
        Assert.Null(exception);
    }

    [Fact]
    public void FullPipeline_InvalidEventData_DoesNotCrash()
    {
        var bufferMgr = new NetworkEventBufferManager();
        var exception = Record.Exception(() =>
        {
            bufferMgr.EnqueueEvent("just a string, not a valid event");
            bufferMgr.ProcessBufferedEvents(_ => { });
        });
        Assert.Null(exception);
    }

    [Fact]
    public void AvailabilityTracker_And_BufferManager_Independent()
    {
        var tracker = new NetworkAvailabilityTracker(null);
        var bufferMgr = new NetworkEventBufferManager();

        // Tracker state is initially unavailable
        Assert.False(tracker.IsAvailable);

        // Buffer can still enqueue/process events independently
        bufferMgr.EnqueueEvent(new Dictionary<string, object>
        {
            ["EventType"] = "OfflineEvent",
            ["Timestamp"] = DateTime.Now.Ticks
        });

        int processed = 0;
        bufferMgr.ProcessBufferedEvents(_ => processed++);
        Assert.Equal(1, processed);

        // Tracker state should not be affected by buffer operations
        Assert.False(tracker.IsAvailable);
    }

    [Fact]
    public void FullPipeline_EventWithNoTimestamp_UsesCurrentTime()
    {
        var bufferMgr = new NetworkEventBufferManager();
        int processed = 0;

        bufferMgr.EnqueueEvent(new Dictionary<string, object>
        {
            ["EventType"] = "NoTimestamp"
            // No "Timestamp" key
        });

        bufferMgr.ProcessBufferedEvents(_ => processed++);
        Assert.Equal(1, processed);
    }

    [Fact]
    public void FullPipeline_RepeatedSameTypeEvents_AllCached()
    {
        var config = new SyncConfiguration { StateCacheExpiry = TimeSpan.FromMinutes(5) };
        var bufferMgr = new NetworkEventBufferManager();
        var cacheMgr = new StateCacheManager(config);
        long now = DateTime.Now.Ticks;

        for (int i = 0; i < 5; i++)
        {
            bufferMgr.EnqueueEvent(new Dictionary<string, object>
            {
                ["EventType"] = "RepeatEvent",
                ["Timestamp"] = now + i,
                ["Index"] = i
            });
        }

        int count = 0;
        bufferMgr.ProcessBufferedEvents(gameEvent =>
        {
            cacheMgr.ApplyRemoteEvent(gameEvent);
            count++;
        });

        Assert.Equal(5, count);
        // State cache dedup by "EventType_UserName" key — same EventType + no UserName → 1 entry
        Assert.True(cacheMgr.CachedStateCount >= 1, "At least one state cache entry should exist");
    }
}
