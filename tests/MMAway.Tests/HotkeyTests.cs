using System.ComponentModel;
using MMA;
using MMA.Input;

namespace MMAway.Tests;

public sealed class HotkeyTests
{
    [Theory]
    [InlineData("Ctrl+Alt+M", 3u, 0x4Du)]
    [InlineData("Ctrl+Alt+Shift+M", 7u, 0x4Du)]
    [InlineData("Control+Shift+F12", 6u, 0x7Bu)]
    [InlineData("Meta+Space", 8u, 0x20u)]
    [InlineData("Super+Enter", 8u, 0x0Du)]
    [InlineData("Alt+Esc", 1u, 0x1Bu)]
    [InlineData("Ctrl+Tab", 2u, 0x09u)]
    [InlineData("Ctrl+0", 2u, 0x30u)]
    public void ParsesSupportedKeys(string shortcut, uint modifiers, uint key) =>
        Assert.Equal(new HotkeyGesture(modifiers, key), HotkeyGesture.Parse(shortcut));

    [Theory]
    [InlineData("")]
    [InlineData("M")]
    [InlineData("Ctrl+Control+M")]
    [InlineData("Hyper+M")]
    [InlineData("Ctrl+F13")]
    [InlineData("Ctrl+?")]
    public void RejectsInvalidShortcuts(string shortcut) =>
        Assert.Throws<ArgumentException>(() => (new ActivitySettings { GlobalHotkey = shortcut }).Validate());

    [Fact]
    public void WindowsRegistrationPreservesPreviousBindingAfterConflict()
    {
        Exception? error = null;
        var thread = new Thread(() =>
        {
            try
            {
                using var first = new Form();
                using var second = new Form();
                using var hotkey = new WindowsGlobalHotkey(first.Handle);
                using var conflict = new WindowsGlobalHotkey(second.Handle);
                hotkey.Configure("Ctrl+Alt+Shift+Win+F10");
                hotkey.Configure("Control+Alt+Shift+Meta+F10");
                Assert.True(hotkey.IsMessage(new IntPtr(1)));
                conflict.Configure("Ctrl+Alt+Shift+Win+F11");
                Assert.Throws<Win32Exception>(() => hotkey.Configure("Ctrl+Alt+Shift+Win+F11"));
                Assert.True(hotkey.IsMessage(new IntPtr(1)));
                Assert.False(hotkey.IsMessage(new IntPtr(2)));
                conflict.Dispose();
                hotkey.Configure("Ctrl+Alt+Shift+Win+F11");
                Assert.False(hotkey.IsMessage(new IntPtr(1)));
                Assert.True(hotkey.IsMessage(new IntPtr(2)));
            }
            catch (Exception ex) { error = ex; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(10)));
        if (error is not null) throw new InvalidOperationException("Windows hotkey test failed.", error);
    }
}
