using System.ComponentModel;
using System.Runtime.InteropServices;

namespace MMA.Input;

internal sealed class WindowsInput : IInputInjector
{
    private const uint MouseMove = 0x0001;
    private const uint MouseLeftDown = 0x0002;
    private const uint MouseLeftUp = 0x0004;
    private const int MouseHook = 14;
    private const int MouseMoveMessage = 0x0200;
    private const uint InjectedFlag = 0x0001;
    private readonly MouseActivityTracker activity = new();
    private readonly HookCallback hookCallback;
    private IntPtr hook;

    private delegate IntPtr HookCallback(int code, IntPtr message, IntPtr data);

    [StructLayout(LayoutKind.Sequential)]
    private struct MouseInput
    {
        public int X;
        public int Y;
        public uint MouseData;
        public uint Flags;
        public uint Time;
        public UIntPtr ExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct InputEvent
    {
        public uint Type;
        public MouseInput Mouse;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MouseHookData
    {
        public NativePoint Position;
        public uint MouseData;
        public uint Flags;
        public uint Time;
        public UIntPtr ExtraInfo;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int type, HookCallback callback, IntPtr module, uint thread);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnhookWindowsHookEx(IntPtr hook);

    [DllImport("user32.dll")]
    private static extern IntPtr CallNextHookEx(IntPtr hook, int code, IntPtr message, IntPtr data);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool GetCursorPos(out NativePoint point);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    private static extern void mouse_event(uint flags, uint dx, uint dy, uint data, UIntPtr extraInfo);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint count, InputEvent[] inputs, int size);

    public WindowsInput()
    {
        hookCallback = OnMouseInput;
        hook = SetWindowsHookEx(MouseHook, hookCallback, IntPtr.Zero, 0);
        if (hook == IntPtr.Zero)
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not monitor user mouse activity.");
    }

    public uint? UserInputVersion => activity.Version;

    private IntPtr OnMouseInput(int code, IntPtr message, IntPtr data)
    {
        if (code >= 0)
        {
            var mouse = Marshal.PtrToStructure<MouseHookData>(data);
            activity.RecordMouseEvent(new Point(mouse.Position.X, mouse.Position.Y),
                (mouse.Flags & InjectedFlag) != 0, message == new IntPtr(MouseMoveMessage));
        }
        return CallNextHookEx(hook, code, message, data);
    }

    public Point GetCursorPosition()
    {
        if (!GetCursorPos(out var point))
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not read the cursor position.");
        return new Point(point.X, point.Y);
    }

    public void SetCursorPosition(Point position)
    {
        activity.RecordInjection(position);
        if (!SetCursorPos(position.X, position.Y))
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not move the cursor.");
        var actual = GetCursorPosition();
        if (actual != position) activity.RecordInjection(actual);
    }

    public void SignalActivity()
    {
        // SetCursorPos does not reset Windows' last-input time; a zero-distance input does.
        var events = new[] { new InputEvent { Mouse = new MouseInput { Flags = MouseMove } } };
        if (SendInput(1, events, Marshal.SizeOf<InputEvent>()) != 1)
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not signal mouse activity.");
    }

    public void LeftClick() => mouse_event(MouseLeftDown | MouseLeftUp, 0, 0, 0, UIntPtr.Zero);

    public void Dispose()
    {
        if (hook == IntPtr.Zero) return;
        if (!UnhookWindowsHookEx(hook))
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not stop monitoring user mouse activity.");
        hook = IntPtr.Zero;
    }
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
