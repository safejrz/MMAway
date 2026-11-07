using System.Diagnostics;
using MMA.Input;

namespace MMA;

public sealed class ActivityEngine
{
    private readonly IInputInjector input;
    private readonly Random random = new();
    private readonly Func<Rectangle?> getActivityBounds;
    private readonly Func<Point, Rectangle, bool> canClick;
    private readonly Func<ActivitySettings> getSettings;
    private readonly Action<string> appendText;
    private readonly Action clearText;
    private readonly Action<Exception> reportError;
    private readonly Action selectRandomText;
    private const int EdgeMargin = 8;
    private const int MinMoveMilliseconds = 2000;
    private const int MaxMoveMilliseconds = 5000;
    private readonly Stopwatch paceClock = new();
    private double paceIntervalMilliseconds;
    private long paceCount;
    private MovementPattern? lastPattern;
    private int typingCyclesSinceSelection;
    private int nextSelectionAfter = 3;
    private int charactersUntilNextSpace;
    private CancellationTokenSource? cancellation;
    private Task? loop;
    private int pass;

    public ActivityEngine(IInputInjector input, Func<Rectangle?> getActivityBounds,
        Func<Point, Rectangle, bool> canClick, Func<ActivitySettings> getSettings,
        Action<string> appendText, Action selectRandomText, Action clearText, Action<Exception> reportError)
    {
        this.input = input;
        this.getActivityBounds = getActivityBounds;
        this.canClick = canClick;
        this.getSettings = getSettings;
        this.appendText = appendText;
        this.selectRandomText = selectRandomText;
        this.clearText = clearText;
        this.reportError = reportError;
    }

    public bool IsRunning => loop is { IsCompleted: false };

    public void Start()
    {
        if (cancellation is not null)
            throw new InvalidOperationException("The previous activity session has not been stopped.");
        getSettings().Validate();
        pass = 0;
        charactersUntilNextSpace = 0;
        cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        loop = Task.Run(() => RunAsync(token));
    }

    public async Task StopAsync()
    {
        if (cancellation is null) return;
        cancellation.Cancel();
        try
        {
            if (loop is not null) await loop.ConfigureAwait(false);
        }
        finally
        {
            cancellation.Dispose();
            cancellation = null;
            loop = null;
        }
    }

    private async Task RunAsync(CancellationToken token)
    {
        try
        {
            var area = WaitForArea(token);
            MoveTo(Center(area), token);
            RunPattern(token);
            var clock = Stopwatch.StartNew();
            var nextActivity = clock.ElapsedMilliseconds + random.Next(10000, 15000);
            var nextTyping = clock.ElapsedMilliseconds + NextTypingInterval();
            while (true)
            {
                var settings = getSettings();
                var typingDue = settings.TypingEnabled && nextTyping <= nextActivity;
                var nextDue = typingDue ? nextTyping : nextActivity;
                var remaining = nextDue - clock.ElapsedMilliseconds;
                if (remaining > 0) await Task.Delay(TimeSpan.FromMilliseconds(remaining), token).ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                if (typingDue)
                {
                    WriteRandomText(token);
                    nextTyping = clock.ElapsedMilliseconds + NextTypingInterval();
                }
                else
                {
                    Step(token);
                    var current = getSettings();
                    nextActivity = clock.ElapsedMilliseconds + random.Next(current.IntervalMinimumSeconds, current.IntervalMaximumSeconds + 1) * 1000;
                }
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { }
        catch (Exception ex)
        {
            reportError(ex);
        }
    }

    private void Step(CancellationToken token)
    {
        RandomMove(token);
        if (pass == 0) MoveTo(Center(WaitForArea(token)), token);
        if (pass > 2 && random.Next(0, 3) <= pass)
        {
            RunPattern(token);
        }
        if (pass >= 4)
        {
            RandomMove(token);
            pass = 0;
            if (getSettings().TypingEnabled)
            {
                WaitForArea(token);
                clearText();
            }
        }
        pass++;
    }

    private bool TryGetArea(out Rectangle area)
    {
        var bounds = getActivityBounds();
        if (bounds is not { Width: > 0, Height: > 0 } rectangle)
        {
            area = Rectangle.Empty;
            return false;
        }
        var marginX = Math.Min(EdgeMargin, (rectangle.Width - 1) / 2);
        var marginY = Math.Min(EdgeMargin, (rectangle.Height - 1) / 2);
        area = Rectangle.FromLTRB(rectangle.Left + marginX, rectangle.Top + marginY,
            rectangle.Right - marginX, rectangle.Bottom - marginY);
        return true;
    }

    private Rectangle WaitForArea(CancellationToken token)
    {
        var waited = false;
        while (true)
        {
            token.ThrowIfCancellationRequested();
            if (TryGetArea(out var area))
            {
                if (waited && paceClock.IsRunning)
                {
                    paceClock.Restart();
                    paceCount = 0;
                }
                return area;
            }
            waited = true;
            Pause(100, token);
        }
    }

    private static Point Center(Rectangle area) =>
        new(area.Left + area.Width / 2, area.Top + area.Height / 2);

    private void MoveTo(Point target, CancellationToken token)
    {
        var area = WaitForArea(token);
        input.SetCursorPosition(new Point(Math.Clamp(target.X, area.Left, area.Right - 1),
            Math.Clamp(target.Y, area.Top, area.Bottom - 1)));
    }

    private void Move(int dx, int dy, CancellationToken token)
    {
        WaitForArea(token);
        var position = input.GetCursorPosition();
        MoveTo(new Point(position.X + dx, position.Y + dy), token);
    }

    private static void Pause(int milliseconds, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (token.WaitHandle.WaitOne(milliseconds)) token.ThrowIfCancellationRequested();
    }

    private int NextTypingInterval()
    {
        var settings = getSettings();
        return random.Next(settings.TypingIntervalMinimumSeconds, settings.TypingIntervalMaximumSeconds + 1) * 1000;
    }

    private void WriteRandomText(CancellationToken token)
    {
        var settings = getSettings();
        if (!settings.TypingEnabled) return;
        var repeat = random.Next(settings.TypingBurstMinimumCharacters, settings.TypingBurstMaximumCharacters + 1);
        for (var i = 0; i < repeat; i++)
        {
            WaitForArea(token);
            if (settings.TypingSpacesEnabled && charactersUntilNextSpace == 0)
            {
                appendText(" ");
                charactersUntilNextSpace = random.Next(settings.SpaceMinimumCharacters, settings.SpaceMaximumCharacters + 1);
            }
            else
            {
                appendText(((char)random.Next(48, 122)).ToString());
                if (settings.TypingSpacesEnabled) charactersUntilNextSpace--;
            }
            Pause(random.Next(40, 601), token);
        }
        typingCyclesSinceSelection++;
        if (typingCyclesSinceSelection >= nextSelectionAfter)
        {
            WaitForArea(token);
            selectRandomText();
            typingCyclesSinceSelection = 0;
            nextSelectionAfter = random.Next(3, 5);
        }
    }

    private void RunPattern(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var pattern = ChooseRandomPattern();
        lastPattern = pattern;
        MovePattern(pattern, token);
        var area = WaitForArea(token);
        input.SignalActivity();
        var target = Center(area);
        var original = input.GetCursorPosition();
        if (!canClick(target, area)) return;
        MoveTo(target, token);
        token.ThrowIfCancellationRequested();
        if (!TryGetArea(out var currentArea) || currentArea != area ||
            input.GetCursorPosition() != target || !canClick(target, area)) return;
        input.LeftClick();
        if (!token.IsCancellationRequested && TryGetArea(out currentArea) && currentArea == area &&
            input.GetCursorPosition() == target && canClick(target, area))
            MoveTo(original, token);
    }

    private void MovePattern(MovementPattern pattern, CancellationToken token)
    {
        var area = WaitForArea(token);
        var points = CreatePatternPoints(pattern, area);
        BeginPaced(points.Length);
        try
        {
            foreach (var point in points)
            {
                token.ThrowIfCancellationRequested();
                WaitForArea(token);
                MoveTo(point, token);
                Pace(token);
            }
        }
        finally
        {
            paceClock.Reset();
            paceIntervalMilliseconds = 0;
        }
    }

    private Point[] CreatePatternPoints(MovementPattern pattern, Rectangle area)
    {
        var center = Center(area);
        var radius = Math.Max(1, Math.Min(area.Width, area.Height) * 0.38);
        var halfWidth = Math.Max(1, area.Width * 0.42);
        var halfHeight = Math.Max(1, area.Height * 0.42);
        IReadOnlyList<PointF> path = pattern switch
        {
            MovementPattern.Square => MovementPatternLibrary.CreateSquare(),
            MovementPattern.Circle => MovementPatternLibrary.CreateCircle(),
            MovementPattern.Triangle => MovementPatternLibrary.CreateTriangle(),
            MovementPattern.VerticalHourglass => MovementPatternLibrary.CreateVerticalHourglass(),
            MovementPattern.HorizontalHourglass => MovementPatternLibrary.CreateHorizontalHourglass(),
            MovementPattern.SpiralOut => MovementPatternLibrary.CreateSpiralOut(),
            MovementPattern.SpiralIn => MovementPatternLibrary.CreateSpiralIn(),
            MovementPattern.Oval => MovementPatternLibrary.CreateOval(),
            MovementPattern.IceCreamCone => MovementPatternLibrary.CreateIceCreamCone(),
            MovementPattern.FigureEight => MovementPatternLibrary.CreateFigureEight(),
            _ => throw new ArgumentOutOfRangeException(nameof(pattern), pattern, "Unknown movement pattern.")
        };
        var preserveAspectRatio = pattern is MovementPattern.Circle or MovementPattern.SpiralOut
            or MovementPattern.SpiralIn or MovementPattern.HorizontalHourglass;
        var scaleX = preserveAspectRatio ? radius : halfWidth;
        var scaleY = preserveAspectRatio ? radius : halfHeight;
        return path.Select(point => new Point(
            center.X + (int)Math.Round(point.X * scaleX),
            center.Y + (int)Math.Round(point.Y * scaleY))).ToArray();
    }

    private MovementPattern ChooseRandomPattern()
    {
        var enabled = getSettings().SelectedPatterns.Select(Enum.Parse<MovementPattern>).ToArray();
        var candidates = enabled.Where(pattern => pattern != lastPattern).ToArray();
        if (candidates.Length == 0) candidates = enabled;
        return candidates[random.Next(candidates.Length)];
    }

    private void BeginPaced(int expectedMoves)
    {
        var duration = random.Next(MinMoveMilliseconds, MaxMoveMilliseconds + 1);
        paceIntervalMilliseconds = (double)duration / Math.Max(1, expectedMoves) / getSettings().Aggressiveness;
        paceCount = 0;
        paceClock.Restart();
    }

    private void Pace(CancellationToken token)
    {
        paceCount++;
        while (true)
        {
            token.ThrowIfCancellationRequested();
            var remaining = paceCount * paceIntervalMilliseconds - paceClock.Elapsed.TotalMilliseconds;
            if (remaining < 1) return;
            Pause((int)Math.Clamp(remaining, 1, 100), token);
        }
    }

    private void RandomMove(CancellationToken token)
    {
        var moves = random.Next(20, 55);
        var variance = random.Next(1, 3);
        var horizontal = random.Next(1, 6) >= 4;
        for (var i = 0; i < moves; i++)
        {
            token.ThrowIfCancellationRequested();
            if (horizontal)
            {
                Move(random.Next(-5, 5), -variance, token);
                i++;
                Move(random.Next(-5, 5), variance, token);
            }
            else
            {
                Move(variance, random.Next(-5, 5), token);
                i++;
                Move(variance, random.Next(-5, 5), token);
            }
            if (i % 3 == 0) Pause(3, token);
        }
        WaitForArea(token);
        input.SignalActivity();
    }
}
