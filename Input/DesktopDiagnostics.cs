using System.Runtime.InteropServices;
using System.Text;

namespace MMA.Input;

internal static class DesktopDiagnostics
{
    private const int DesktopReadObjects = 0x0001;
    private const int UserObjectName = 2;

    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentThreadId();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr GetThreadDesktop(uint threadId);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr OpenInputDesktop(uint flags, bool inherit, int access);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool CloseDesktop(IntPtr desktop);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool GetUserObjectInformation(IntPtr handle, int index, StringBuilder name, int length, out int needed);

    public static string Capture()
    {
        var threadDesktop = GetThreadDesktop(GetCurrentThreadId());
        var threadName = threadDesktop == IntPtr.Zero
            ? $"unavailable (Windows error {Marshal.GetLastWin32Error()})"
            : GetName(threadDesktop);
        var inputDesktop = OpenInputDesktop(0, false, DesktopReadObjects);
        if (inputDesktop == IntPtr.Zero)
            return $"thread desktop={threadName}; input desktop unavailable (Windows error {Marshal.GetLastWin32Error()})";

        var inputName = GetName(inputDesktop);
        var closeResult = CloseDesktop(inputDesktop)
            ? "closed"
            : $"close failed (Windows error {Marshal.GetLastWin32Error()})";
        return $"thread desktop={threadName}; input desktop={inputName}; input desktop handle={closeResult}";
    }

    private static string GetName(IntPtr desktop)
    {
        var name = new StringBuilder(256);
        return GetUserObjectInformation(desktop, UserObjectName, name, name.Capacity * sizeof(char), out _)
            ? name.ToString()
            : $"name unavailable (Windows error {Marshal.GetLastWin32Error()})";
    }
}
