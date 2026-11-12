using MMA;

namespace MMAway.Tests;

public sealed class OptionsFormTests
{
    [Fact]
    public void SafetySettingsCanBeEditedAndSaved()
    {
        Exception? error = null;
        var thread = new Thread(() =>
        {
            try
            {
                using var form = new OptionsForm(new ActivitySettings());
                form.Show();
                Application.DoEvents();
                var idle = (NumericUpDown)form.Controls.Find("userIdle", true).Single();
                var session = (CheckBox)form.Controls.Find("showSessionInfo", true).Single();
                var save = (Button)form.Controls.Find("save", true).Single();
                var reset = (Button)form.Controls.Find("reset", true).Single();
                var hotkey = (TextBox)form.Controls.Find("hotkey", true).Single();
                var tabs = form.Controls.OfType<TabControl>().Single();
                var general = tabs.TabPages.Cast<TabPage>().Single(page => page.Text == "General");
                Assert.Contains(general.Controls.OfType<FlowLayoutPanel>().Single().Controls.OfType<Label>(),
                    label => label.Text == "Global hotkey (Run: start/stop)");
                var about = tabs.TabPages.Cast<TabPage>().Single(page => page.Text == "About");
                var version = (Label)about.Controls.Find("version", true).Single();
                Assert.Equal("1.6.0", version.Text);
                Assert.Equal(typeof(MainWindow).Assembly.GetName().Version!.ToString(3), version.Text);
                hotkey.Text = "Ctrl+F10";
                reset.PerformClick();
                Assert.Equal("Ctrl+Alt+Shift+M", hotkey.Text);
                Assert.Equal(10, idle.Value);
                Assert.False(session.Checked);
                idle.Value = 25;
                session.Checked = true;
                save.PerformClick();
                Assert.Equal(DialogResult.OK, form.DialogResult);
                Assert.NotNull(form.SavedSettings);
                Assert.Equal(25, form.SavedSettings.UserIdleSeconds);
                Assert.True(form.SavedSettings.ShowSessionInfo);
            }
            catch (Exception ex) { error = ex; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(10)));
        if (error is not null) throw new InvalidOperationException("Options form test failed.", error);
    }
}
