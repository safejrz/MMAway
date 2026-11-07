namespace MMA;

internal sealed class OptionsForm : Form
{
    private ActivitySettings original;
    private readonly NumericUpDown intervalMin = Number("intervalMin", 1, 300);
    private readonly NumericUpDown intervalMax = Number("intervalMax", 1, 300);
    private readonly NumericUpDown autoStop = Number("autoStop", 0, 1440);
    private readonly NumericUpDown userIdle = Number("userIdle", 1, 3600);
    private readonly CheckBox showSessionInfo = new() { Name = "showSessionInfo", Text = "Show session time and activity count", AutoSize = true };
    private readonly TextBox hotkey = new() { Name = "hotkey", Width = 160, BackColor = Color.FromArgb(37, 37, 38), ForeColor = Color.White };
    private readonly NumericUpDown typingMin = Number("typingMin", 1, 300);
    private readonly NumericUpDown typingMax = Number("typingMax", 1, 300);
    private readonly NumericUpDown burstMin = Number("burstMin", 1, 100);
    private readonly NumericUpDown burstMax = Number("burstMax", 1, 100);
    private readonly NumericUpDown spacesMin = Number("spacesMin", 0, 100);
    private readonly NumericUpDown spacesMax = Number("spacesMax", 0, 100);
    private readonly CheckBox typingEnabled = new() { Text = "Enable typing in the activity display", AutoSize = true, Name = "typingEnabled" };
    private readonly CheckBox spacesEnabled = new() { Text = "Include spaces", AutoSize = true, Name = "spacesEnabled" };
    private readonly CheckedListBox patterns = new() { Name = "patterns", Width = 320, Height = 190, CheckOnClick = true, BackColor = Color.FromArgb(37, 37, 38), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
    private readonly TrackBar speed = new() { Name = "speed", Minimum = 5, Maximum = 30, TickFrequency = 5, Width = 300, AutoSize = false, Height = 35 };
    private readonly Label speedValue = new() { AutoSize = true };
    private readonly Label error = new() { ForeColor = Color.Salmon, AutoSize = false, Height = 38, Dock = DockStyle.Bottom };

    public ActivitySettings? SavedSettings { get; private set; }

    public OptionsForm(ActivitySettings settings)
    {
        original = settings;
        Text = "MMAway Options";
        ClientSize = new Size(428, 600);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.FromArgb(30, 30, 30);
        ForeColor = Color.FromArgb(204, 204, 204);
        Font = new Font("Segoe UI", 9F);

        var tabs = new TabControl { Dock = DockStyle.Fill };
        var general = Page("General");
        var mouse = Page("Mouse");
        var keyboard = Page("Keyboard");
        tabs.TabPages.AddRange([general.Page, mouse.Page, keyboard.Page]);
        general.Content.Controls.Add(Heading("Activity interval (seconds)"));
        general.Content.Controls.Add(RangeRow(intervalMin, intervalMax));
        general.Content.Controls.Add(Note("Long intervals can allow Teams to become Away. Delays may also include time spent typing or moving."));
        general.Content.Controls.Add(Heading("Global hotkey (not active yet)"));
        general.Content.Controls.Add(hotkey);
        general.Content.Controls.Add(Heading("Auto-stop minutes (not active yet)"));
        general.Content.Controls.Add(autoStop);
        general.Content.Controls.Add(Note("These values are saved for a later feature. Neither hotkeys nor auto-stop work in this version."));
        general.Content.Controls.Add(showSessionInfo);

        mouse.Content.Controls.Add(Heading("Resume after mouse is idle (seconds)"));
        mouse.Content.Controls.Add(userIdle);
        mouse.Content.Controls.Add(Heading("Movement patterns"));
        foreach (var pattern in Enum.GetValues<MovementPattern>()) patterns.Items.Add(pattern);
        mouse.Content.Controls.Add(patterns);
        var all = new Button { Text = "Select all", AutoSize = true };
        all.Click += (_, _) => { for (var i = 0; i < patterns.Items.Count; i++) patterns.SetItemChecked(i, true); };
        mouse.Content.Controls.Add(all);
        mouse.Content.Controls.Add(Heading("Movement speed"));
        var speedRow = new FlowLayoutPanel { AutoSize = true, WrapContents = false };
        speedRow.Controls.Add(speed);
        speedRow.Controls.Add(speedValue);
        mouse.Content.Controls.Add(speedRow);
        mouse.Content.Controls.Add(Note("Scales the 2–5 second movement duration: 0.5x slower, 1.5x default, 3.0x faster."));
        speed.ValueChanged += (_, _) => speedValue.Text = $"{speed.Value / 10.0:0.0}x";

        keyboard.Content.Controls.Add(typingEnabled);
        keyboard.Content.Controls.Add(Note("Generated text is added only to MMAway's own display, not other apps."));
        keyboard.Content.Controls.Add(Heading("Delay between typing bursts (seconds)"));
        keyboard.Content.Controls.Add(RangeRow(typingMin, typingMax));
        keyboard.Content.Controls.Add(Heading("Characters per burst"));
        keyboard.Content.Controls.Add(RangeRow(burstMin, burstMax));
        keyboard.Content.Controls.Add(spacesEnabled);
        keyboard.Content.Controls.Add(Heading("Characters between spaces"));
        keyboard.Content.Controls.Add(RangeRow(spacesMin, spacesMax));
        spacesEnabled.CheckedChanged += (_, _) => { spacesMin.Enabled = spacesEnabled.Checked; spacesMax.Enabled = spacesEnabled.Checked; };

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 48, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(6) };
        var save = new Button { Text = "Save", AutoSize = true, Name = "save" };
        var cancel = new Button { Text = "Cancel", AutoSize = true, DialogResult = DialogResult.Cancel };
        var reset = new Button { Text = "Reset to defaults", AutoSize = true, Name = "reset" };
        save.Click += Save;
        reset.Click += (_, _) => SetValues(new ActivitySettings());
        buttons.Controls.AddRange([save, cancel, reset]);
        AcceptButton = save;
        CancelButton = cancel;
        Controls.Add(tabs);
        Controls.Add(error);
        Controls.Add(buttons);
        SetValues(settings);
    }

    private static NumericUpDown Number(string name, int min, int max) => new()
    {
        Name = name, Minimum = min, Maximum = max, Width = 85,
        BackColor = Color.FromArgb(37, 37, 38), ForeColor = Color.White
    };

    private static (TabPage Page, FlowLayoutPanel Content) Page(string title)
    {
        var page = new TabPage(title) { BackColor = Color.FromArgb(30, 30, 30), ForeColor = Color.FromArgb(204, 204, 204) };
        var content = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Padding = new Padding(12) };
        page.Controls.Add(content);
        return (page, content);
    }

    private static Label Heading(string text) => new() { Text = text, AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Margin = new Padding(3, 10, 3, 4) };
    private static Label Note(string text) => new() { Text = text, AutoSize = false, Width = 340, Height = 44, Margin = new Padding(3, 8, 3, 3) };

    private static FlowLayoutPanel RangeRow(NumericUpDown min, NumericUpDown max)
    {
        var row = new FlowLayoutPanel { AutoSize = true, WrapContents = false };
        row.Controls.Add(new Label { Text = "Minimum", AutoSize = true, Margin = new Padding(3, 5, 3, 0) });
        row.Controls.Add(min);
        row.Controls.Add(new Label { Text = "Maximum", AutoSize = true, Margin = new Padding(10, 5, 3, 0) });
        row.Controls.Add(max);
        return row;
    }

    private void SetValues(ActivitySettings settings)
    {
        original = settings;
        intervalMin.Value = settings.IntervalMinimumSeconds;
        intervalMax.Value = settings.IntervalMaximumSeconds;
        autoStop.Value = settings.AutoStopMinutes;
        userIdle.Value = settings.UserIdleSeconds;
        showSessionInfo.Checked = settings.ShowSessionInfo;
        hotkey.Text = settings.GlobalHotkey;
        typingMin.Value = settings.TypingIntervalMinimumSeconds;
        typingMax.Value = settings.TypingIntervalMaximumSeconds;
        burstMin.Value = settings.TypingBurstMinimumCharacters;
        burstMax.Value = settings.TypingBurstMaximumCharacters;
        spacesMin.Value = settings.SpaceMinimumCharacters;
        spacesMax.Value = settings.SpaceMaximumCharacters;
        typingEnabled.Checked = settings.TypingEnabled;
        spacesEnabled.Checked = settings.TypingSpacesEnabled;
        spacesMin.Enabled = spacesMax.Enabled = settings.TypingSpacesEnabled;
        speed.Value = Math.Clamp((int)Math.Round(settings.Aggressiveness * 10), speed.Minimum, speed.Maximum);
        speedValue.Text = $"{speed.Value / 10.0:0.0}x";
        for (var i = 0; i < patterns.Items.Count; i++)
            patterns.SetItemChecked(i, settings.SelectedPatterns.Contains(patterns.Items[i]!.ToString()!, StringComparer.Ordinal));
        error.Text = string.Empty;
    }

    private void Save(object? sender, EventArgs e)
    {
        var updated = original with
        {
            IntervalMinimumSeconds = (int)intervalMin.Value,
            IntervalMaximumSeconds = (int)intervalMax.Value,
            AutoStopMinutes = (int)autoStop.Value,
            UserIdleSeconds = (int)userIdle.Value,
            ShowSessionInfo = showSessionInfo.Checked,
            GlobalHotkey = hotkey.Text.Trim(),
            TypingIntervalMinimumSeconds = (int)typingMin.Value,
            TypingIntervalMaximumSeconds = (int)typingMax.Value,
            TypingBurstMinimumCharacters = (int)burstMin.Value,
            TypingBurstMaximumCharacters = (int)burstMax.Value,
            SpaceMinimumCharacters = (int)spacesMin.Value,
            SpaceMaximumCharacters = (int)spacesMax.Value,
            TypingEnabled = typingEnabled.Checked,
            TypingSpacesEnabled = spacesEnabled.Checked,
            Aggressiveness = speed.Value / 10.0,
            SelectedPatterns = patterns.CheckedItems.Cast<MovementPattern>().Select(p => p.ToString()).ToList()
        };
        try
        {
            updated.Validate();
            SavedSettings = updated;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (ArgumentException ex)
        {
            error.Text = ex.Message;
        }
    }
}
