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
                var general = (FlowLayoutPanel)form.Controls.Find("pageGeneral", true).Single();
                Assert.True(general.Visible);
                Assert.Contains(general.Controls.OfType<Label>(), label => label.Text == "Global hotkey (show window)");
                var processKey = typeof(OptionsForm).GetMethod("ProcessCmdKey", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
                var message = Message.Create(form.Handle, 0x100, IntPtr.Zero, IntPtr.Zero);
                Assert.True((bool)processKey.Invoke(form, [message, Keys.Control | Keys.Tab])!);
                var mouse = (FlowLayoutPanel)form.Controls.Find("pageMouse", true).Single();
                Assert.True(mouse.Visible);
                Assert.True(mouse.AutoScroll);
                Assert.True((bool)processKey.Invoke(form, [message, Keys.Control | Keys.Shift | Keys.Tab])!);
                Assert.True(general.Visible);
                var keyboard = (FlowLayoutPanel)form.Controls.Find("pageKeyboard", true).Single();
                ((Button)form.Controls.Find("tabKeyboard", true).Single()).PerformClick();
                Assert.True(keyboard.Visible);
                Assert.False(general.Visible);
                var spaces = (CheckBox)form.Controls.Find("spacesEnabled", true).Single();
                var spacesMin = (NumericUpDown)form.Controls.Find("spacesMin", true).Single();
                spaces.Checked = false;
                Assert.False(spacesMin.Enabled);
                spaces.Checked = true;
                Assert.True(spacesMin.Enabled);
                ((Button)form.Controls.Find("tabAbout", true).Single()).PerformClick();
                var about = (FlowLayoutPanel)form.Controls.Find("pageAbout", true).Single();
                Assert.True(about.Visible);
                var version = (Label)about.Controls.Find("version", true).Single();
                Assert.Equal("1.7.0", version.Text);
                Assert.Equal(typeof(MainWindow).Assembly.GetName().Version!.ToString(3), version.Text);
                ((Button)form.Controls.Find("tabGeneral", true).Single()).PerformClick();
                hotkey.Text = "invalid";
                save.PerformClick();
                Assert.Null(form.SavedSettings);
                Assert.False(string.IsNullOrWhiteSpace(form.Controls.Find("error", true).Single().Text));
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
