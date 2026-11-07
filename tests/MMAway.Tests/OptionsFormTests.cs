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
