namespace MMA.Input;

internal sealed class MouseActivityTracker
{
    private readonly object gate = new();
    private readonly Queue<(Point Position, long Time)> injections = new();
    private int version;

    public uint Version => unchecked((uint)Volatile.Read(ref version));

    public void RecordInjection(Point position)
    {
        lock (gate)
        {
            RemoveExpiredInjections();
            injections.Enqueue((position, Environment.TickCount64));
        }
    }

    public void RecordMouseEvent(Point position, bool injected, bool movement)
    {
        if (injected) return;
        if (movement)
        {
            lock (gate)
            {
                RemoveExpiredInjections();
                // Remote desktops can echo a warp as an unmarked mouse move.
                if (injections.Any(injection => injection.Position == position)) return;
            }
        }
        Interlocked.Increment(ref version);
    }

    private void RemoveExpiredInjections()
    {
        var cutoff = Environment.TickCount64 - 2000;
        while (injections.TryPeek(out var injection) && injection.Time < cutoff)
            injections.Dequeue();
    }
}
