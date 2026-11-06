using System.ComponentModel;
using System.Runtime.InteropServices;

namespace MMA.Input;

internal sealed class WindowsInput : IInputInjector
{
    private const uint MouseLeftDown = 0x0002;
    private const uint MouseLeftUp = 0x0004;

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint
    {
        public int X;
        public int Y;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool GetCursorPos(out NativePoint point);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    private static extern void mouse_event(uint flags, uint dx, uint dy, uint data, UIntPtr extraInfo);

    public Point GetCursorPosition()
    {
        if (!GetCursorPos(out var point))
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not read the cursor position.");
        return new Point(point.X, point.Y);
    }

    public void SetCursorPosition(Point position)
    {
        if (!SetCursorPos(position.X, position.Y))
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not move the cursor.");
    }

    public void LeftClick() => mouse_event(MouseLeftDown | MouseLeftUp, 0, 0, 0, UIntPtr.Zero);

    public void Dispose() { }
}

internal sealed class WindowsKeepAwake : IKeepAwake
{
    [Flags]
    private enum ExecutionState : uint
    {
        Continuous = 0x80000000,
        SystemRequired = 0x00000001,
        DisplayRequired = 0x00000002
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern uint SetThreadExecutionState(ExecutionState flags);

    // Execution state belongs to the calling thread; use the UI thread for both calls.
    public void Enable() => SetState(ExecutionState.Continuous | ExecutionState.SystemRequired | ExecutionState.DisplayRequired);
    public void Disable() => SetState(ExecutionState.Continuous);
    public void Dispose() => Disable();

    private static void SetState(ExecutionState state)
    {
        if (SetThreadExecutionState(state) == 0)
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not change the system sleep prevention state.");
    }
}
