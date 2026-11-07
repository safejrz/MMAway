using MMA.Input;

namespace MMAway.Tests;

public sealed class MouseActivityTrackerTests
{
    [Fact]
    public async Task RemoteEchoIsIgnoredButRealMovementIsNot()
    {
        var tracker = new MouseActivityTracker();
        var point = new Point(100, 100);
        tracker.RecordInjection(point);
        await Task.Delay(150);
        tracker.RecordMouseEvent(point, injected: false, movement: true);
        tracker.RecordMouseEvent(point, injected: true, movement: false);
        Assert.Equal(0u, tracker.Version);
        tracker.RecordMouseEvent(new Point(120, 120), injected: false, movement: true);
        Assert.Equal(1u, tracker.Version);
    }

    [Fact]
    public void RealClickAtInjectedPositionIsNotIgnored()
    {
        var tracker = new MouseActivityTracker();
        var point = new Point(100, 100);
        tracker.RecordInjection(point);
        tracker.RecordMouseEvent(point, injected: false, movement: false);
        Assert.Equal(1u, tracker.Version);
    }

    [Fact]
    public async Task OldInjectedPositionDoesNotHideUserMovement()
    {
        var tracker = new MouseActivityTracker();
        var point = new Point(100, 100);
        tracker.RecordInjection(point);
        await Task.Delay(2150);
        tracker.RecordMouseEvent(point, injected: false, movement: true);
        Assert.Equal(1u, tracker.Version);
    }
}
