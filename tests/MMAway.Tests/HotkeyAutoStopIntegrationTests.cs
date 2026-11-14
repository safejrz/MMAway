using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using MMA;

namespace MMAway.Tests;

public sealed class HotkeyAutoStopIntegrationTests
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool PostMessage(IntPtr window, int message, IntPtr wParam, IntPtr lParam);

    [Fact]
    public void ShowHotkeyAndLocalToggleWorkWithAutoStop()
    {
        Exception? error = null;
        var thread = new Thread(() =>
        {
            var path = Path.Combine(Path.GetTempPath(), $"mmaway-integration-{Guid.NewGuid():N}.json");
            try
            {
                var store = new SettingsStore(path);
                store.SaveAsync(new ActivitySettings { AutoStopMinutes = 1, GlobalHotkey = "Ctrl+Alt+Shift+Win+F9" }).GetAwaiter().GetResult();
                using (var window = new TestMainWindow(store))
                using (var other = new Form())
                {
                    try
                    {
                        window.Show();
                        var button = (Button)window.Controls.Find("buttonOk", true).Single();
                        WaitUntil(() => button.Enabled, TimeSpan.FromSeconds(10));
                        var textBox = (TextBox)window.Controls.Find("textBox", true).Single();
                        var append = typeof(MainWindow).GetMethod("AppendGeneratedText", BindingFlags.Instance | BindingFlags.NonPublic)!;
                        textBox.Text = new string('a', 99);
                        append.Invoke(window, ["b"]);
                        Assert.Equal(100, textBox.TextLength);
                        textBox.Select(0, 1);
                        append.Invoke(window, ["c"]);
                        Assert.Equal(100, textBox.TextLength);
                        textBox.Select(textBox.TextLength, 0);
                        append.Invoke(window, ["d"]);
                        Assert.Equal(0, textBox.TextLength);
                        textBox.Text = new string('a', 101);
                        append.Invoke(window, ["b"]);
                        Assert.Equal(0, textBox.TextLength);
                        Assert.False(window.SendShortcut(Keys.Control | Keys.Shift | Keys.S));
                        window.WindowState = FormWindowState.Minimized;
                        other.Show();
                        other.Activate();
                        SendHotkey(window);
                        WaitUntil(() => window.WindowState == FormWindowState.Normal, TimeSpan.FromSeconds(5));
                        Assert.Equal("Start", button.Text);
                        Assert.True(window.SendShortcut(Keys.Control | Keys.S));
                        WaitUntil(() => button.Text == "Stop!", TimeSpan.FromSeconds(5));
                        SendHotkey(window);
                        PumpFor(TimeSpan.FromMilliseconds(100));
                        Assert.Equal("Stop!", button.Text);
                        var firstStart = Stopwatch.StartNew();
                        PumpFor(TimeSpan.FromSeconds(8));
                        Assert.True(window.SendShortcut(Keys.Control | Keys.S));
                        WaitUntil(() => button.Text == "Start" && button.Enabled, TimeSpan.FromSeconds(5));
                        Assert.True(window.SendShortcut(Keys.Control | Keys.S));
                        WaitUntil(() => button.Text == "Stop!", TimeSpan.FromSeconds(5));
                        while (firstStart.Elapsed < TimeSpan.FromSeconds(61)) PumpFor(TimeSpan.FromMilliseconds(100));
                        Assert.Equal("Stop!", button.Text);
                        WaitUntil(() => button.Text == "Start" && button.Enabled, TimeSpan.FromSeconds(15));
                    }
                    finally { window.Close(); }
                }

                store.SaveAsync(new ActivitySettings { AutoStopMinutes = 0, GlobalHotkey = "Ctrl+Alt+Shift+Win+F9" }).GetAwaiter().GetResult();
                using var disabledWindow = new TestMainWindow(store);
                try
                {
                    disabledWindow.Show();
                    var button = (Button)disabledWindow.Controls.Find("buttonOk", true).Single();
                    WaitUntil(() => button.Enabled, TimeSpan.FromSeconds(10));
                    Assert.True(disabledWindow.SendShortcut(Keys.Control | Keys.S));
                    WaitUntil(() => button.Text == "Stop!", TimeSpan.FromSeconds(5));
                    var timer = (System.Windows.Forms.Timer)typeof(MainWindow).GetField("autoStopTimer", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(disabledWindow)!;
                    Assert.False(timer.Enabled);
                    Assert.True(disabledWindow.SendShortcut(Keys.Control | Keys.S));
                    WaitUntil(() => button.Text == "Start" && button.Enabled, TimeSpan.FromSeconds(5));
                }
                finally { disabledWindow.Close(); }
            }
            catch (Exception ex) { error = ex; }
            finally { File.Delete(path); }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(100)));
        if (error is not null) throw new InvalidOperationException("Hotkey and auto-stop integration test failed.", error);
    }

    private sealed class TestMainWindow(SettingsStore store) : MainWindow(store)
    {
        public bool SendShortcut(Keys shortcut)
        {
            var message = Message.Create(Handle, 0x100, IntPtr.Zero, IntPtr.Zero);
            return ProcessCmdKey(ref message, shortcut);
        }
    }

    private static void SendHotkey(Form window) =>
        Assert.True(PostMessage(window.Handle, 0x0312, new IntPtr(1), IntPtr.Zero));

    private static void WaitUntil(Func<bool> condition, TimeSpan timeout)
    {
        var clock = Stopwatch.StartNew();
        while (!condition() && clock.Elapsed < timeout) PumpFor(TimeSpan.FromMilliseconds(20));
        Assert.True(condition(), "Timed out waiting for the application state.");
    }

    private static void PumpFor(TimeSpan duration)
    {
        var clock = Stopwatch.StartNew();
        while (clock.Elapsed < duration)
        {
            Application.DoEvents();
            Thread.Sleep(10);
        }
    }
}
