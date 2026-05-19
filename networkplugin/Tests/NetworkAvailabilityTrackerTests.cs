using NetworkPlugin.Core;
using Xunit;

namespace NetworkPlugin.Tests;

public class NetworkAvailabilityTrackerTests
{
    [Fact]
    public void Constructor_IsAvailableFalse()
    {
        var tracker = new NetworkAvailabilityTracker(null);
        Assert.False(tracker.IsAvailable);
    }

    [Fact]
    public void SetAvailable_And_Check()
    {
        var tracker = new NetworkAvailabilityTracker(null);
        tracker.SetAvailable();
        Assert.True(tracker.IsAvailable);
    }

    [Fact]
    public void SetUnavailable_And_Check()
    {
        var tracker = new NetworkAvailabilityTracker(null);
        tracker.SetAvailable();
        tracker.SetUnavailable();
        Assert.False(tracker.IsAvailable);
    }

    [Fact]
    public void CanRequestFullSync_FirstCall_ReturnsTrue()
    {
        var tracker = new NetworkAvailabilityTracker(null);
        Assert.True(tracker.CanRequestFullSync());
    }

    [Fact]
    public void MarkSyncCompleted_DoesNotThrow()
    {
        var tracker = new NetworkAvailabilityTracker(null);
        var exception = Record.Exception(() => tracker.MarkSyncCompleted());
        Assert.Null(exception);
    }
}
