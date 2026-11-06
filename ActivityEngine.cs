using MMA.Input;

namespace MMA;

public sealed class ActivityEngine
{
    private readonly IInputInjector input;
    private readonly Random random = new();
    private readonly Point home;
    private readonly Func<Point> getWindowPosition;
    private readonly Action<string> appendText;
    private readonly Action clearText;
    private readonly Action<Exception> reportError;
    private CancellationTokenSource? cancellation;
    private Task? loop;
    private int pass;

    public ActivityEngine(IInputInjector input, Point home, Func<Point> getWindowPosition,
        Action<string> appendText, Action clearText, Action<Exception> reportError)
    {
        this.input = input;
        this.home = home;
        this.getWindowPosition = getWindowPosition;
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
            token.ThrowIfCancellationRequested();
            input.SetCursorPosition(home);
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
        if (pass == 0) input.SetCursorPosition(home);
        if (pass > 2 && random.Next(0, 3) <= pass)
        {
            RunPattern(token);
            WriteRandomText(token);
        }
        if (pass >= 4)
        {
            RandomMove(token);
            pass = 0;
            clearText();
        }
        pass++;
    }

    private void Move(int dx, int dy)
    {
        var position = input.GetCursorPosition();
        input.SetCursorPosition(new Point(position.X + dx, position.Y + dy));
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
            token.ThrowIfCancellationRequested();
            appendText(((char)random.Next(48, 122)).ToString());
            Pause(repeat * random.Next(20, 100), token);
        }
    }

    private void RunPattern(CancellationToken token)
    {
        if (random.Next(1, 9) >= 5) MoveInSquare(token);
        else MoveInCircles(token);
        token.ThrowIfCancellationRequested();
        var original = input.GetCursorPosition();
        var window = getWindowPosition();
        input.SetCursorPosition(new Point(window.X + 10, window.Y + 10));
        input.LeftClick();
        input.SetCursorPosition(original);
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
                Move(random.Next(-5, 5), -variance);
                i++;
                Move(random.Next(-5, 5), variance);
            }
            else
            {
                Move(variance, random.Next(-5, 5));
                i++;
                Move(variance, random.Next(-5, 5));
            }
            if (i % 3 == 0) Pause(3, token);
        }
    }

    private void MoveInSquare(CancellationToken token)
    {
        const int size = 350;
        var variance = random.Next(1, 3);
        for (var i = 0; i < size; i++)
        {
            token.ThrowIfCancellationRequested();
            Move(1, -variance); i++; Move(1, variance);
            if (i % 7 == 0) Pause(1, token);
        }
        for (var i = 0; i < size; i++)
        {
            token.ThrowIfCancellationRequested();
            Move(-variance, 1); i++;
            if (i % 9 == 0) Pause(1, token);
            Move(variance, 1);
        }
        for (var i = 0; i < size; i++)
        {
            token.ThrowIfCancellationRequested();
            Move(-1, -variance); i++; Move(-1, variance);
            if (i % 8 == 0) Pause(1, token);
        }
        for (var i = 0; i < size; i++)
        {
            token.ThrowIfCancellationRequested();
            Move(-variance, -1); i++;
            if (i % 7 == 0) Pause(1, token);
            Move(variance, -1);
        }
    }

    private void MoveInCircles(CancellationToken token)
    {
        const int size = 300;
        var variance = random.Next(1, 3);
        for (var i = 0; i < size; i++)
        {
            token.ThrowIfCancellationRequested();
            Move(1, -1);
            if (i % 7 == 0) Pause(1, token);
        }
        for (var i = 0; i < size; i++)
        {
            token.ThrowIfCancellationRequested();
            Move(-variance, 1); i++; Move(variance, 1);
            if (i % 11 == 0) Pause(1, token);
        }
        for (var i = 0; i < size; i++)
        {
            token.ThrowIfCancellationRequested();
            Move(-1, -1);
            if (i % 7 == 0) Pause(1, token);
        }
        for (var i = 0; i < size; i++)
        {
            token.ThrowIfCancellationRequested();
            Move(-variance, 1); i++;
            if (i % 9 == 0) Pause(1, token);
            Move(variance, 1);
        }
    }
}
