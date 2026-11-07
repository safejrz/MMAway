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
    public void HotkeyTogglesAndAutoStopResetsForEachSession()
    {
        Exception? error = null;
        var thread = new Thread(() =>
        {
            var path = Path.Combine(Path.GetTempPath(), $"mmaway-integration-{Guid.NewGuid():N}.json");
            try
            {
                var store = new SettingsStore(path);
                store.SaveAsync(new ActivitySettings { AutoStopMinutes = 1, GlobalHotkey = "Ctrl+Alt+Shift+Win+F9" }).GetAwaiter().GetResult();
                using (var window = new MainWindow(store))
                using (var other = new Form())
                {
                    try
                    {
                        window.Show();
                        var button = (Button)window.Controls.Find("buttonOk", true).Single();
                        WaitUntil(() => button.Enabled, TimeSpan.FromSeconds(10));
                        other.Show();
                        other.Activate();
                        SendHotkey(window);
                        WaitUntil(() => button.Text == "Stop!", TimeSpan.FromSeconds(5));
                        var firstStart = Stopwatch.StartNew();
                        PumpFor(TimeSpan.FromSeconds(8));
                        SendHotkey(window);
                        WaitUntil(() => button.Text == "Start" && button.Enabled, TimeSpan.FromSeconds(5));
                        SendHotkey(window);
                        WaitUntil(() => button.Text == "Stop!", TimeSpan.FromSeconds(5));
                        while (firstStart.Elapsed < TimeSpan.FromSeconds(61)) PumpFor(TimeSpan.FromMilliseconds(100));
                        Assert.Equal("Stop!", button.Text);
                        WaitUntil(() => button.Text == "Start" && button.Enabled, TimeSpan.FromSeconds(15));
                    }
                    finally { window.Close(); }
                }

                store.SaveAsync(new ActivitySettings { AutoStopMinutes = 0, GlobalHotkey = "Ctrl+Alt+Shift+Win+F9" }).GetAwaiter().GetResult();
                using var disabledWindow = new MainWindow(store);
                try
                {
                    disabledWindow.Show();
                    var button = (Button)disabledWindow.Controls.Find("buttonOk", true).Single();
                    WaitUntil(() => button.Enabled, TimeSpan.FromSeconds(10));
                    SendHotkey(disabledWindow);
                    WaitUntil(() => button.Text == "Stop!", TimeSpan.FromSeconds(5));
                    var timer = (System.Windows.Forms.Timer)typeof(MainWindow).GetField("autoStopTimer", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(disabledWindow)!;
                    Assert.False(timer.Enabled);
                    SendHotkey(disabledWindow);
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
