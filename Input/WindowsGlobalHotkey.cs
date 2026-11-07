using System.ComponentModel;
using System.Runtime.InteropServices;

namespace MMA.Input;

internal sealed class WindowsGlobalHotkey : IDisposable
{
    private const uint NoRepeat = 0x4000;
    private readonly IntPtr window;
    private int registeredId;
    private HotkeyGesture? registeredGesture;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr window, int id, uint modifiers, uint key);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr window, int id);

    public WindowsGlobalHotkey(IntPtr window) => this.window = window;

    public bool IsMessage(IntPtr id) => registeredId != 0 && id == new IntPtr(registeredId);

    public void Configure(string shortcut)
    {
        var gesture = HotkeyGesture.Parse(shortcut);
        if (gesture == registeredGesture) return;
        var nextId = registeredId == 1 ? 2 : 1;
        if (!RegisterHotKey(window, nextId, gesture.Modifiers | NoRepeat, gesture.Key))
            throw new Win32Exception(Marshal.GetLastWin32Error(), "The global hotkey could not be registered (it may already be in use).");
        if (registeredId != 0 && !UnregisterHotKey(window, registeredId))
        {
            var error = Marshal.GetLastWin32Error();
            UnregisterHotKey(window, nextId);
            throw new Win32Exception(error, "Could not release the previous global hotkey.");
        }
        registeredId = nextId;
        registeredGesture = gesture;
    }

    public void Dispose()
    {
        if (registeredId == 0) return;
        if (!UnregisterHotKey(window, registeredId))
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not release the global hotkey.");
        registeredId = 0;
        registeredGesture = null;
    }
}
