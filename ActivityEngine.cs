using MMA.Input;

namespace MMA;

public sealed class ActivityEngine
{
    private readonly IInputInjector input;
    private readonly Random random = new();
    private readonly Func<Rectangle?> getActivityBounds;
    private readonly Func<Point, Rectangle, bool> canClick;
    private readonly Action<string> appendText;
    private readonly Action clearText;
    private readonly Action<Exception> reportError;
    private const int EdgeMargin = 8;
    private CancellationTokenSource? cancellation;
    private Task? loop;
    private int pass;

    public ActivityEngine(IInputInjector input, Func<Rectangle?> getActivityBounds,
        Func<Point, Rectangle, bool> canClick, Action<string> appendText, Action clearText, Action<Exception> reportError)
    {
        this.input = input;
        this.getActivityBounds = getActivityBounds;
        this.canClick = canClick;
        this.appendText = appendText;
        this.clearText = clearText;
        this.reportError = reportError;
    }

    public bool IsRunning => loop is { IsCompleted: false };

    public void Start()
    {
        if (cancellation is not null)
            throw new InvalidOperationException("The previous activity session has not been stopped.");
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
            var delay = random.Next(10000, 15000);
            while (true)
            {
                await Task.Delay(delay, token).ConfigureAwait(false);
                Step(token);
                delay = random.Next(5000, 15000);
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
        WriteRandomText(token);
        if (pass == 0) MoveTo(Center(WaitForArea(token)), token);
        if (pass > 2 && random.Next(0, 3) <= pass)
        {
            RunPattern(token);
            WriteRandomText(token);
        }
        if (pass >= 4)
        {
            RandomMove(token);
            pass = 0;
            WaitForArea(token);
            clearText();
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
        while (true)
        {
            token.ThrowIfCancellationRequested();
            if (TryGetArea(out var area)) return area;
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

    private void WriteRandomText(CancellationToken token)
    {
        var repeat = random.Next(2, 7);
        for (var i = 0; i < repeat; i++)
        {
            WaitForArea(token);
            appendText(((char)random.Next(48, 122)).ToString());
            Pause(repeat * random.Next(20, 100), token);
        }
    }

    private void RunPattern(CancellationToken token)
    {
        if (random.Next(1, 9) >= 5) MoveInSquare(token);
        else MoveInCircles(token);
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

    private void MoveInSquare(CancellationToken token)
    {
        var area = WaitForArea(token);
        var size = Math.Max(1, Math.Min(area.Width, area.Height) * 3 / 4);
        var center = Center(area);
        MoveTo(new Point(center.X - size / 2, center.Y - size / 2), token);
        var variance = random.Next(1, 3);
        for (var i = 0; i < size; i++)
        {
            token.ThrowIfCancellationRequested();
            Move(1, -variance, token); i++; Move(1, variance, token);
            if (i % 7 == 0) Pause(1, token);
        }
        for (var i = 0; i < size; i++)
        {
            token.ThrowIfCancellationRequested();
            Move(-variance, 1, token); i++;
            if (i % 9 == 0) Pause(1, token);
            Move(variance, 1, token);
        }
        for (var i = 0; i < size; i++)
        {
            token.ThrowIfCancellationRequested();
            Move(-1, -variance, token); i++; Move(-1, variance, token);
            if (i % 8 == 0) Pause(1, token);
        }
        for (var i = 0; i < size; i++)
        {
            token.ThrowIfCancellationRequested();
            Move(-variance, -1, token); i++;
            if (i % 7 == 0) Pause(1, token);
            Move(variance, -1, token);
        }
    }

    private void MoveInCircles(CancellationToken token)
    {
        var area = WaitForArea(token);
        var size = Math.Max(1, Math.Min(area.Width, area.Height) * 3 / 4);
        var center = Center(area);
        MoveTo(new Point(center.X - size / 2, center.Y + size / 2), token);
        var variance = random.Next(1, 3);
        for (var i = 0; i < size; i++)
        {
            token.ThrowIfCancellationRequested();
            Move(1, -1, token);
            if (i % 7 == 0) Pause(1, token);
        }
        for (var i = 0; i < size; i++)
        {
            token.ThrowIfCancellationRequested();
            Move(-variance, 1, token); i++; Move(variance, 1, token);
            if (i % 11 == 0) Pause(1, token);
        }
        for (var i = 0; i < size; i++)
        {
            token.ThrowIfCancellationRequested();
            Move(-1, -1, token);
            if (i % 7 == 0) Pause(1, token);
        }
        for (var i = 0; i < size; i++)
        {
            token.ThrowIfCancellationRequested();
            Move(-variance, 1, token); i++;
            if (i % 9 == 0) Pause(1, token);
            Move(variance, 1, token);
        }
    }
}
