namespace MMA;

internal sealed class OptionsForm : Form
{
    private static readonly Color Background = Color.FromArgb(30, 30, 30);
    private static readonly Color Surface = Color.FromArgb(45, 45, 48);
    private static readonly Color Accent = Color.FromArgb(0, 122, 204);
    private readonly List<(Button Button, FlowLayoutPanel Page)> pages = [];
    private int selectedPage;
    private ActivitySettings original;
    private readonly NumericUpDown intervalMin = Number("intervalMin", 1, 300);
    private readonly NumericUpDown intervalMax = Number("intervalMax", 1, 300);
    private readonly NumericUpDown autoStop = Number("autoStop", 0, 1440);
    private readonly NumericUpDown userIdle = Number("userIdle", 1, 3600);
    private readonly CheckBox showSessionInfo = Check("showSessionInfo", "Show session time and activity count");
    private readonly TextBox hotkey = new() { Name = "hotkey", Width = 180, BackColor = Surface, ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
    private readonly NumericUpDown typingMin = Number("typingMin", 1, 300);
    private readonly NumericUpDown typingMax = Number("typingMax", 1, 300);
    private readonly NumericUpDown burstMin = Number("burstMin", 1, 100);
    private readonly NumericUpDown burstMax = Number("burstMax", 1, 100);
    private readonly NumericUpDown spacesMin = Number("spacesMin", 0, 100);
    private readonly NumericUpDown spacesMax = Number("spacesMax", 0, 100);
    private readonly CheckBox typingEnabled = Check("typingEnabled", "Enable typing in the activity display");
    private readonly CheckBox spacesEnabled = Check("spacesEnabled", "Include spaces");
    private readonly CheckedListBox patterns = new() { Name = "patterns", Width = 380, Height = 190, CheckOnClick = true, BackColor = Surface, ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
    private readonly TrackBar speed = new() { Name = "speed", Minimum = 5, Maximum = 30, TickFrequency = 5, Width = 300, AutoSize = false, Height = 35 };
    private readonly Label speedValue = new() { AutoSize = true };
    private readonly Label error = new() { Name = "error", ForeColor = Color.Salmon, AutoSize = false, Height = 38, Dock = DockStyle.Bottom };

    public ActivitySettings? SavedSettings { get; private set; }

    public OptionsForm(ActivitySettings settings)
    {
        original = settings;
        Text = "MMAway Options";
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(520, 600);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Background;
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 9F);
        Icon = Properties.Resources.MainIcon;

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new Padding(24, 18, 24, 18) };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var navigation = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, Margin = Padding.Empty };
        var pageHost = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
        var general = Page("General");
        var mouse = Page("Mouse");
        var keyboard = Page("Keyboard");
        var about = Page("About");
        foreach (var page in new[] { general, mouse, keyboard, about })
        {
            var button = NavigationButton(page.Name);
            var index = pages.Count;
            button.Click += (_, _) => SelectPage(index);
            pages.Add((button, page.Content));
            navigation.Controls.Add(button);
            pageHost.Controls.Add(page.Content);
        }
        SelectPage(0);
        about.Content.Controls.Add(Heading("MMAway version"));
        about.Content.Controls.Add(new Label { Name = "version", AutoSize = true, Text = typeof(MainWindow).Assembly.GetName().Version!.ToString(3) });
        general.Content.Controls.Add(Heading("Activity interval (seconds)"));
        general.Content.Controls.Add(RangeRow("Activity interval", intervalMin, intervalMax));
        general.Content.Controls.Add(Note("Long intervals can allow Teams to become Away. Delays may also include time spent typing or moving."));
        general.Content.Controls.Add(Heading("Global hotkey (show window)"));
        general.Content.Controls.Add(hotkey);
        general.Content.Controls.Add(Note("Use Ctrl, Alt, Shift, or Win with A-Z, 0-9, F1-F12, Space, Enter, Esc, or Tab. Ctrl+S starts/stops activity only in MMAway."));
        general.Content.Controls.Add(Heading("Auto-stop minutes (0 disables)"));
        general.Content.Controls.Add(autoStop);
        general.Content.Controls.Add(showSessionInfo);

        mouse.Content.Controls.Add(Heading("Resume after mouse is idle (seconds)"));
        mouse.Content.Controls.Add(userIdle);
        mouse.Content.Controls.Add(Heading("Movement patterns"));
        foreach (var pattern in Enum.GetValues<MovementPattern>()) patterns.Items.Add(pattern);
        mouse.Content.Controls.Add(patterns);
        var all = ActionButton("Select all");
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
        keyboard.Content.Controls.Add(RangeRow("Typing interval", typingMin, typingMax));
        keyboard.Content.Controls.Add(Heading("Characters per burst"));
        keyboard.Content.Controls.Add(RangeRow("Characters per burst", burstMin, burstMax));
        keyboard.Content.Controls.Add(spacesEnabled);
        keyboard.Content.Controls.Add(Heading("Characters between spaces"));
        keyboard.Content.Controls.Add(RangeRow("Characters between spaces", spacesMin, spacesMax));
        spacesEnabled.CheckedChanged += (_, _) => { spacesMin.Enabled = spacesEnabled.Checked; spacesMax.Enabled = spacesEnabled.Checked; };

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, Margin = new Padding(0, 12, 0, 0) };
        var save = ActionButton("Save");
        save.Name = "save";
        var cancel = ActionButton("Cancel");
        cancel.DialogResult = DialogResult.Cancel;
        var reset = ActionButton("Reset to defaults");
        reset.Name = "reset";
        save.Click += Save;
        reset.Click += (_, _) => SetValues(new ActivitySettings());
        buttons.Controls.AddRange([save, cancel, reset]);
        AcceptButton = save;
        CancelButton = cancel;
        error.AutoSize = true;
        error.MaximumSize = new Size(440, 0);
        error.Margin = new Padding(12, 4, 12, 0);
        error.Dock = DockStyle.None;
        layout.Controls.Add(navigation, 0, 0);
        layout.Controls.Add(pageHost, 0, 1);
        layout.Controls.Add(error, 0, 2);
        layout.Controls.Add(buttons, 0, 3);
        Controls.Add(layout);
        SetValues(settings);
    }

    private static NumericUpDown Number(string name, int min, int max) => new()
    {
        Name = name, Minimum = min, Maximum = max, Width = 85,
        BackColor = Surface, ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle
    };

    private static CheckBox Check(string name, string text) => new()
    {
        Name = name, Text = text, AutoSize = true, FlatStyle = FlatStyle.Flat,
        Margin = new Padding(3, 10, 3, 6)
    };

    private static (string Name, FlowLayoutPanel Content) Page(string title) => (title,
        new FlowLayoutPanel { Name = $"page{title}", Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown,
            WrapContents = false, AutoScroll = true, Padding = new Padding(12, 8, 12, 12), BackColor = Background });

    private static Button NavigationButton(string text)
    {
        var button = new Button { Name = $"tab{text}", Text = text, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Font = new Font("Segoe UI", 13F), FlatStyle = FlatStyle.Flat, BackColor = Background,
            ForeColor = Color.Silver, Margin = new Padding(0, 0, 12, 0), Padding = new Padding(4, 2, 4, 5), UseVisualStyleBackColor = false };
        button.FlatAppearance.BorderSize = 0;
        button.Paint += (_, e) =>
        {
            if (button.Tag is true)
                using (var pen = new Pen(Accent, 2)) e.Graphics.DrawLine(pen, 4, button.Height - 2, button.Width - 5, button.Height - 2);
        };
        return button;
    }

    private static Button ActionButton(string text)
    {
        var button = new Button { Text = text, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlatStyle = FlatStyle.Flat, BackColor = Surface, ForeColor = Color.White, Padding = new Padding(8, 5, 8, 5),
            Margin = new Padding(6, 0, 0, 0), UseVisualStyleBackColor = false };
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = Color.FromArgb(65, 65, 68);
        return button;
    }

    private void SelectPage(int index)
    {
        selectedPage = index;
        for (var i = 0; i < pages.Count; i++)
        {
            var selected = i == index;
            pages[i].Page.Visible = selected;
            pages[i].Button.Tag = selected;
            pages[i].Button.ForeColor = selected ? Color.White : Color.Silver;
            pages[i].Button.AccessibleDescription = selected ? "Selected page" : "Show page";
            pages[i].Button.Invalidate();
        }
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData is (Keys.Control | Keys.Tab) or (Keys.Control | Keys.Shift | Keys.Tab))
        {
            var direction = keyData == (Keys.Control | Keys.Tab) ? 1 : -1;
            SelectPage((selectedPage + direction + pages.Count) % pages.Count);
            pages[selectedPage].Button.Focus();
            return true;
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    private static Label Heading(string text) => new() { Text = text, AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Margin = new Padding(3, 14, 3, 5) };
    private static Label Note(string text) => new() { Text = text, AutoSize = true, MaximumSize = new Size(400, 0), ForeColor = Color.Silver, Margin = new Padding(3, 8, 3, 3) };

    private static FlowLayoutPanel RangeRow(string name, NumericUpDown min, NumericUpDown max)
    {
        var row = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = Padding.Empty };
        foreach (var (label, number) in new[] { ("Minimum", min), ("Maximum", max) })
        {
            number.AccessibleName = $"{name} {label.ToLowerInvariant()}";
            var column = new FlowLayoutPanel { Width = 190, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
                MinimumSize = new Size(190, 0), FlowDirection = FlowDirection.TopDown, WrapContents = false, Margin = Padding.Empty };
            column.Controls.Add(new Label { Text = label, AutoSize = true, Margin = new Padding(3, 0, 3, 3) });
            column.Controls.Add(number);
            row.Controls.Add(column);
        }
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
