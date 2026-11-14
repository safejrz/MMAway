using System.Collections.Concurrent;
using System.ComponentModel;
using MMA;
using MMA.Input;

namespace MMAway.Tests;

public sealed class ActivityEngineTests
{
    [Fact]
    public async Task RepeatedOwnMovesDoNotPauseAndActivityIsSignaled()
    {
        using var input = new FakeInput();
        var engine = Create(input, () => false, out var errors);
        engine.Start();
        try
        {
            await WaitUntil(() => input.Signals > 0, TimeSpan.FromSeconds(8));
            Assert.Empty(errors);
            Assert.True(input.Moves > 30);
            Assert.Equal(0, input.Clicks);
            Assert.Equal(0, input.OutOfBounds);
            await WaitUntil(() => engine.ActivityCount >= 1, TimeSpan.FromSeconds(2));
        }
        finally { await engine.StopAsync(); }
    }

    [Theory]
    [InlineData(MovementPattern.Square)]
    [InlineData(MovementPattern.Circle)]
    [InlineData(MovementPattern.Triangle)]
    [InlineData(MovementPattern.VerticalHourglass)]
    [InlineData(MovementPattern.HorizontalHourglass)]
    [InlineData(MovementPattern.SpiralOut)]
    [InlineData(MovementPattern.SpiralIn)]
    [InlineData(MovementPattern.Oval)]
    [InlineData(MovementPattern.IceCreamCone)]
    [InlineData(MovementPattern.FigureEight)]
    public async Task EveryPatternStaysInsideBoundsAndSignalsActivity(MovementPattern pattern)
    {
        using var input = new FakeInput();
        var errors = new ConcurrentQueue<Exception>();
        var settings = new ActivitySettings { SelectedPatterns = [pattern.ToString()], TypingEnabled = false, Aggressiveness = 3 };
        var engine = new ActivityEngine(input, () => new Rectangle(100, 100, 200, 200), (_, _) => false,
            () => settings, _ => { }, () => { }, () => { }, errors.Enqueue);
        engine.Start();
        try
        {
            await WaitUntil(() => input.Signals > 0, TimeSpan.FromSeconds(8));
            Assert.Empty(errors);
            Assert.Equal(0, input.OutOfBounds);
        }
        finally { await engine.StopAsync(); }
    }

    [Fact]
    public async Task UserInputPausesAndResumesAfterIdle()
    {
        using var input = new FakeInput();
        var engine = Create(input, () => false, out var errors);
        engine.Start();
        try
        {
            await WaitUntil(() => input.Moves > 5, TimeSpan.FromSeconds(5));
            input.UserMoved();
            await Task.Delay(250);
            var pausedAt = input.Moves;
            await Task.Delay(400);
            Assert.Equal(pausedAt, input.Moves);
            await WaitUntil(() => input.Moves > pausedAt, TimeSpan.FromSeconds(3));
            Assert.Empty(errors);
        }
        finally { await engine.StopAsync(); }
    }

    [Fact]
    public async Task StopInterruptsUserIdleWait()
    {
        using var input = new FakeInput();
        var engine = Create(input, () => false, out _);
        engine.Start();
        await WaitUntil(() => input.Moves > 5, TimeSpan.FromSeconds(5));
        input.UserMoved();
        await Task.Delay(150);
        await engine.StopAsync().WaitAsync(TimeSpan.FromSeconds(2));
        Assert.False(engine.IsRunning);
    }

    [Fact]
    public async Task ClickOccursOnlyWhenAllowedAndPositionIsVerified()
    {
        using var input = new FakeInput();
        var engine = Create(input, () => true, out var errors);
        engine.Start();
        try
        {
            await WaitUntil(() => input.Clicks == 1, TimeSpan.FromSeconds(8));
            Assert.Empty(errors);
            Assert.Equal(new Point(200, 200), input.ClickPosition);
        }
        finally { await engine.StopAsync(); }
    }

    [Fact]
    public async Task UserInputDuringClickCheckSuppressesClick()
    {
        using var input = new FakeInput();
        var checks = 0;
        var engine = Create(input, () =>
        {
            if (Interlocked.Increment(ref checks) == 1) input.UserMoved();
            return true;
        }, out var errors);
        engine.Start();
        try
        {
            await WaitUntil(() => input.Signals > 0, TimeSpan.FromSeconds(8));
            await Task.Delay(250);
            Assert.Equal(0, input.Clicks);
            Assert.Empty(errors);
        }
        finally { await engine.StopAsync(); }
    }

    [Fact]
    public async Task ClickIsSuppressedIfFocusIsLostBeforeClick()
    {
        using var input = new FakeInput();
        var checks = 0;
        var engine = Create(input, () => Interlocked.Increment(ref checks) == 1, out var errors);
        engine.Start();
        try
        {
            await WaitUntil(() => engine.ActivityCount > 0, TimeSpan.FromSeconds(8));
            Assert.Equal(0, input.Clicks);
            Assert.Empty(errors);
        }
        finally { await engine.StopAsync(); }
    }

    [Fact]
    public async Task ClickIsSuppressedIfCursorDriftsAfterTargetMove()
    {
        using var input = new FakeInput { DriftAfterSignal = true };
        var engine = Create(input, () => true, out var errors);
        engine.Start();
        try
        {
            await WaitUntil(() => engine.ActivityCount > 0, TimeSpan.FromSeconds(8));
            Assert.Equal(0, input.Clicks);
            Assert.Empty(errors);
        }
        finally { await engine.StopAsync(); }
    }

    [Fact]
    public async Task MissingBoundsPauseUntilRestoredAndCanBeCancelled()
    {
        using var input = new FakeInput();
        var hasBounds = 0;
        var errors = new ConcurrentQueue<Exception>();
        var engine = new ActivityEngine(input, () => Volatile.Read(ref hasBounds) == 0 ? null : new Rectangle(100, 100, 200, 200),
            (_, _) => false, () => new ActivitySettings { UserIdleSeconds = 1, TypingEnabled = false, Aggressiveness = 3 },
            _ => { }, () => { }, () => { }, errors.Enqueue);
        engine.Start();
        await Task.Delay(300);
        Assert.Equal(0, input.Moves);
        Volatile.Write(ref hasBounds, 1);
        await WaitUntil(() => input.Moves > 5, TimeSpan.FromSeconds(5));
        await engine.StopAsync().WaitAsync(TimeSpan.FromSeconds(2));
        Assert.Empty(errors);
        Volatile.Write(ref hasBounds, 0);
        engine.Start();
        await Task.Delay(200);
        await engine.StopAsync().WaitAsync(TimeSpan.FromSeconds(2));
        Assert.False(engine.IsRunning);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(170)]
    public async Task TransientCursorFailureAtStartupIsRetried(int errorCode)
    {
        using var input = new FakeInput { MoveFailuresRemaining = 2, MoveErrorCode = errorCode };
        var engine = Create(input, () => false, out var errors);
        engine.Start();
        try
        {
            await WaitUntil(() => input.Signals > 0, TimeSpan.FromSeconds(10));
            Assert.Empty(errors);
            Assert.True(input.MoveAttempts >= 3);
        }
        finally { await engine.StopAsync(); }
    }

    [Theory]
    [InlineData(5, 4)]
    [InlineData(170, 4)]
    [InlineData(87, 1)]
    public async Task PersistentCursorFailureIsReported(int errorCode, int expectedAttempts)
    {
        using var input = new FakeInput { MoveFailuresRemaining = 10, MoveErrorCode = errorCode };
        var engine = Create(input, () => false, out var errors);
        engine.Start();
        try
        {
            await WaitUntil(() => !errors.IsEmpty, TimeSpan.FromSeconds(5));
            Assert.Equal(errorCode, Assert.IsType<Win32Exception>(Assert.Single(errors)).NativeErrorCode);
            Assert.Equal(expectedAttempts, input.MoveAttempts);
        }
        finally { await engine.StopAsync(); }
    }

    [Fact]
    public async Task CursorFailureDuringLaterActivityIsReported()
    {
        using var input = new FakeInput { MoveErrorCode = 5 };
        var engine = Create(input, () => false, out var errors);
        engine.Start();
        try
        {
            await WaitUntil(() => engine.ActivityCount > 0, TimeSpan.FromSeconds(8));
            input.MoveFailuresRemaining = 10;
            await WaitUntil(() => !errors.IsEmpty, TimeSpan.FromSeconds(23));
            Assert.Equal(5, Assert.IsType<Win32Exception>(Assert.Single(errors)).NativeErrorCode);
        }
        finally { await engine.StopAsync(); }
    }

    [Fact]
    public async Task StopInterruptsCursorRetry()
    {
        using var input = new FakeInput { MoveFailuresRemaining = 10, MoveErrorCode = 5 };
        var engine = Create(input, () => false, out var errors);
        engine.Start();
        await WaitUntil(() => input.MoveAttempts > 0, TimeSpan.FromSeconds(5));
        await engine.StopAsync().WaitAsync(TimeSpan.FromSeconds(2));
        Assert.Empty(errors);
        Assert.False(engine.IsRunning);
    }

    private static ActivityEngine Create(FakeInput input, Func<bool> canClick, out ConcurrentQueue<Exception> errors)
    {
        var captured = new ConcurrentQueue<Exception>();
        errors = captured;
        return new ActivityEngine(input, () => new Rectangle(100, 100, 200, 200), (_, _) => canClick(),
            () => new ActivitySettings { UserIdleSeconds = 1, TypingEnabled = false, Aggressiveness = 3 },
            _ => { }, () => { }, () => { }, captured.Enqueue);
    }

    private static async Task WaitUntil(Func<bool> condition, TimeSpan timeout)
    {
        var clock = System.Diagnostics.Stopwatch.StartNew();
        while (!condition())
        {
            if (clock.Elapsed > timeout) throw new TimeoutException("Timed out waiting for activity.");
            await Task.Delay(20);
        }
    }

    private sealed class FakeInput : IInputInjector
    {
        private int version;
        private int moves;
        private int signals;
        private int clicks;
        private int outOfBounds;
        private int moveAttempts;
        private int moveFailuresRemaining;
        private Point position = new(150, 150);
        public bool DriftAfterSignal { get; init; }
        public int MoveFailuresRemaining { get => Volatile.Read(ref moveFailuresRemaining); set => Volatile.Write(ref moveFailuresRemaining, value); }
        public int MoveErrorCode { get; init; }
        public uint? UserInputVersion => unchecked((uint)Volatile.Read(ref version));
        public int MoveAttempts => Volatile.Read(ref moveAttempts);
        public int Moves => Volatile.Read(ref moves);
        public int Signals => Volatile.Read(ref signals);
        public int Clicks => Volatile.Read(ref clicks);
        public int OutOfBounds => Volatile.Read(ref outOfBounds);
        public Point ClickPosition { get; private set; }
        public Point GetCursorPosition() => position;
        public void SetCursorPosition(Point value)
        {
            Interlocked.Increment(ref moveAttempts);
            if (MoveFailuresRemaining > 0)
            {
                Interlocked.Decrement(ref moveFailuresRemaining);
                throw new Win32Exception(MoveErrorCode);
            }
            position = DriftAfterSignal && Signals > 0 ? new Point(value.X + 1, value.Y) : value;
            if (!new Rectangle(100, 100, 200, 200).Contains(value)) Interlocked.Increment(ref outOfBounds);
            Interlocked.Increment(ref moves);
        }
        public void SignalActivity() => Interlocked.Increment(ref signals);
        public void LeftClick() { ClickPosition = position; Interlocked.Increment(ref clicks); }
        public void UserMoved() => Interlocked.Increment(ref version);
        public void Dispose() { }
    }
}
