using MMA.Input;
using MMA.Properties;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MMA
{
    public partial class MainWindow : Form
    {
        private const int WmNcLButtonDown = 0x00A1;
        private const int WmHotkey = 0x0312;
        private const int HtCaption = 2;
        private readonly IInputInjector input = new WindowsInput();
        private readonly IKeepAwake keepAwake = new WindowsKeepAwake();
        private readonly ActivityEngine engine;
        private readonly SettingsStore settingsStore;
        private ActivitySettings settings = new();
        private bool settingsReady;
        private sealed record BoundsSnapshot(Rectangle? Bounds, IntPtr Window, IntPtr Output, bool Active);
        private BoundsSnapshot boundsSnapshot = new(null, IntPtr.Zero, IntPtr.Zero, false);
        private bool running;
        private bool transitionInProgress;
        private bool optionsInProgress;
        private WindowsGlobalHotkey? globalHotkey;
        private System.Windows.Forms.Timer autoStopTimer = null!;
        private long autoStopStarted;
        private int autoStopMilliseconds;
        private int session;
        private int statsTicks;

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr WindowFromPoint(Point point);

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr window, int message, IntPtr wParam, IntPtr lParam);

        public MainWindow() : this(new SettingsStore()) { }

        internal MainWindow(SettingsStore settingsStore)
        {
            this.settingsStore = settingsStore;
            var workingArea = (Screen.PrimaryScreen ?? Screen.AllScreens.FirstOrDefault())?.WorkingArea
                ?? new Rectangle(0, 0, 1024, 768);
            InitializeComponent();
            autoStopTimer = new System.Windows.Forms.Timer(components!);
            autoStopTimer.Tick += AutoStopTimer_Tick;
            buttonOk.Enabled = false;
            optionsButton.Enabled = false;
            Location = WindowPlacement.GetStartupPosition(workingArea, Size);
            Shown += LoadSettingsAsync;
            Move += (_, _) => UpdateActivityBounds();
            Resize += (_, _) => UpdateActivityBounds();
            VisibleChanged += (_, _) => UpdateActivityBounds();
            Shown += (_, _) => UpdateActivityBounds();
            Activated += (_, _) => UpdateActivityBounds();
            Deactivate += (_, _) => UpdateActivityBounds();
            DpiChanged += (_, _) => UpdateActivityBounds();
            textBox.SizeChanged += (_, _) => UpdateActivityBounds();
            textBox.LocationChanged += (_, _) => UpdateActivityBounds();
            engine = new ActivityEngine(input, () => Volatile.Read(ref boundsSnapshot).Bounds,
                CanClick, () => Volatile.Read(ref settings), text => PostToSession(() => AppendGeneratedText(text)),
                () => PostToSession(SelectRandomText),
                () => PostToSession(textBox.Clear), ReportEngineError);
        }

        private async void LoadSettingsAsync(object? sender, EventArgs e)
        {
            optionsInProgress = true;
            try
            {
                var loaded = await settingsStore.LoadAsync();
                if (IsDisposed || Disposing) return;
                Volatile.Write(ref settings, loaded);
                settingsReady = true;
                buttonOk.Enabled = true;
                ConfigureGlobalHotkey();
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Text.Json.JsonException or ArgumentException)
            {
                MessageBox.Show(this, $"Could not load settings: {ex.Message}\nOpen Options and save valid settings to enable activity.",
                    "MMAway", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                optionsInProgress = false;
                if (!IsDisposed && !Disposing) optionsButton.Enabled = true;
            }
        }

        private async void optionsButton_Click(object? sender, EventArgs e)
        {
            if (optionsInProgress || transitionInProgress || running) return;
            optionsInProgress = true;
            try
            {
                using var dialog = new OptionsForm(Volatile.Read(ref settings));
                if (dialog.ShowDialog(this) != DialogResult.OK || dialog.SavedSettings is not { } updated) return;
                optionsButton.Enabled = false;
                buttonOk.Enabled = false;
                await settingsStore.SaveAsync(updated);
                Volatile.Write(ref settings, updated);
                settingsReady = true;
                sessionLabel.Visible = updated.ShowSessionInfo && sessionLabel.Text.Length > 0;
                ConfigureGlobalHotkey();
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
            {
                MessageBox.Show(this, $"Could not save settings: {ex.Message}", "MMAway", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                optionsInProgress = false;
                optionsButton.Enabled = true;
                buttonOk.Enabled = settingsReady;
            }
        }

        private void ConfigureGlobalHotkey()
        {
            globalHotkey ??= new WindowsGlobalHotkey(Handle);
            try
            {
                globalHotkey.Configure(settings.GlobalHotkey);
            }
            catch (Win32Exception ex)
            {
                MessageBox.Show(this, $"Could not activate {settings.GlobalHotkey}: {ex.Message}\nThe previous hotkey, if any, remains active. Choose another hotkey in Options.",
                    "MMAway", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (settingsReady) ConfigureGlobalHotkey();
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            try
            {
                globalHotkey?.Dispose();
            }
            catch (Win32Exception ex)
            {
                MessageBox.Show($"Could not release the global hotkey: {ex.Message}", "MMAway", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                globalHotkey = null;
            }
            base.OnHandleDestroyed(e);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WmHotkey && globalHotkey?.IsMessage(m.WParam) == true)
            {
                _ = ToggleActivityAsync();
                return;
            }
            base.WndProc(ref m);
        }

        private void titleBar_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            ReleaseCapture();
            SendMessage(Handle, WmNcLButtonDown, new IntPtr(HtCaption), IntPtr.Zero);
        }

        private void minimizeButton_Click(object? sender, EventArgs e) => WindowState = FormWindowState.Minimized;

        private void closeButton_Click(object? sender, EventArgs e) => Close();

        private void closeButton_Paint(object? sender, PaintEventArgs e)
        {
            var scale = closeButton.DeviceDpi / 96f;
            var centerX = closeButton.ClientSize.Width / 2f;
            var centerY = closeButton.ClientSize.Height / 2f;
            var radius = 4f * scale;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using var pen = new Pen(closeButton.ForeColor, 1.5f * scale);
            e.Graphics.DrawLine(pen, centerX - radius, centerY - radius, centerX + radius, centerY + radius);
            e.Graphics.DrawLine(pen, centerX + radius, centerY - radius, centerX - radius, centerY + radius);
        }

        private void spinnerTimer_Tick(object? sender, EventArgs e)
        {
            leftSpinner.Advance();
            rightSpinner.Advance();
            activityProgress.Advance();
            if (++statsTicks >= 50)
            {
                statsTicks = 0;
                UpdateRunningStats();
            }
        }

        private void UpdateRunningStats()
        {
            var elapsed = engine.SessionDuration;
            activityProgress.Text = settings.ShowSessionInfo
                ? $"Running · {elapsed:hh\\:mm\\:ss} · {engine.ActivityCount} cycles"
                : "Running";
            activityProgress.Invalidate();
        }

        private void SetRunningVisuals(bool isRunning)
        {
            runningIndicator.Visible = isRunning;
            if (isRunning)
            {
                sessionLabel.Visible = false;
                statsTicks = 0;
                UpdateRunningStats();
                spinnerTimer.Start();
            }
            else
            {
                spinnerTimer.Stop();
                leftSpinner.Reset();
                rightSpinner.Reset();
                activityProgress.Reset();
            }
        }

        private void AppendGeneratedText(string text)
        {
            var start = Math.Clamp(textBox.SelectionStart, 0, textBox.TextLength);
            var length = Math.Clamp(textBox.SelectionLength, 0, textBox.TextLength - start);
            if (length == 0)
            {
                textBox.AppendText(text);
                return;
            }

            textBox.Select(start, length);
            textBox.SelectedText = text;
        }

        private void SelectRandomText()
        {
            var length = textBox.TextLength;
            if (length == 0) return;
            var start = Random.Shared.Next(length);
            var selectedLength = Random.Shared.Next(1, Math.Min(12, length - start) + 1);
            textBox.Select(start, selectedLength);
        }

        private void UpdateActivityBounds()
        {
            if (!IsHandleCreated || !Visible || WindowState == FormWindowState.Minimized ||
                !textBox.IsHandleCreated || !textBox.Visible || textBox.ClientSize.Width <= 0 || textBox.ClientSize.Height <= 0)
            {
                Volatile.Write(ref boundsSnapshot, new BoundsSnapshot(null, IntPtr.Zero, IntPtr.Zero, false));
                return;
            }

            var bounds = textBox.RectangleToScreen(textBox.ClientRectangle);
            Volatile.Write(ref boundsSnapshot, new BoundsSnapshot(bounds, Handle, textBox.Handle, ContainsFocus));
        }

        private bool CanClick(Point target, Rectangle area)
        {
            var snapshot = Volatile.Read(ref boundsSnapshot);
            return snapshot.Active && snapshot.Bounds is { } bounds && bounds.Contains(area) &&
                GetForegroundWindow() == snapshot.Window && WindowFromPoint(target) == snapshot.Output;
        }

        private void PostToSession(Action action)
        {
            var postedSession = Volatile.Read(ref session);
            BeginInvoke(new Action(() =>
            {
                if (running && postedSession == session) action();
            }));
        }

        private async void buttonOk_Click(object sender, EventArgs e) => await ToggleActivityAsync();

        private async Task ToggleActivityAsync()
        {
            if (!settingsReady || optionsInProgress || transitionInProgress || IsDisposed || Disposing) return;
            transitionInProgress = true;
            buttonOk.Enabled = false;
            try
            {
                if (running)
                {
                    await StopActivityAsync();
                }
                else
                {
                    keepAwake.Enable();
                    running = true;
                    Interlocked.Increment(ref session);
                    Icon = Resources.RunIcon;
                    buttonOk.Text = Resources.ButtonStopLabel;
                    engine.Start();
                    optionsButton.Enabled = false;
                    SetRunningVisuals(true);
                    StartAutoStop();
                }
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                if (running)
                {
                    try { await StopActivityAsync(); }
                    catch (Exception stopError) { message += $"\nCould not stop activity: {stopError.Message}"; }
                }
                MessageBox.Show(this, message, "MMAway", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                transitionInProgress = false;
                buttonOk.Enabled = settingsReady;
            }
        }

        private void StartAutoStop()
        {
            autoStopTimer.Stop();
            autoStopMilliseconds = settings.AutoStopMinutes * 60_000;
            if (autoStopMilliseconds == 0) return;
            autoStopStarted = Stopwatch.GetTimestamp();
            autoStopTimer.Interval = autoStopMilliseconds;
            autoStopTimer.Start();
        }

        private async void AutoStopTimer_Tick(object? sender, EventArgs e)
        {
            if (!running || autoStopMilliseconds == 0) return;
            var remaining = autoStopMilliseconds - Stopwatch.GetElapsedTime(autoStopStarted).TotalMilliseconds;
            if (remaining > 0)
            {
                autoStopTimer.Interval = Math.Max(1, (int)Math.Ceiling(remaining));
                return;
            }
            autoStopTimer.Stop();
            await ToggleActivityAsync();
        }

        private async Task StopActivityAsync()
        {
            autoStopTimer.Stop();
            autoStopMilliseconds = 0;
            running = false;
            Interlocked.Increment(ref session);
            SetRunningVisuals(false);
            try
            {
                await engine.StopAsync();
            }
            finally
            {
                try { keepAwake.Disable(); }
                finally
                {
                    var elapsed = engine.SessionDuration;
                    sessionLabel.Text = $"Last session: {elapsed:hh\\:mm\\:ss} · {engine.ActivityCount} cycles";
                    sessionLabel.Visible = settings.ShowSessionInfo;
                    Icon = Resources.StopIcon;
                    buttonOk.Text = Resources.ButtonStartLabel;
                    optionsButton.Enabled = true;
                }
            }
        }

        private void ReportEngineError(Exception error)
        {
            var failedSession = Volatile.Read(ref session);
            BeginInvoke(new Action(async () => await HandleEngineErrorAsync(error, failedSession)));
        }

        private async Task HandleEngineErrorAsync(Exception error, int failedSession)
        {
            if (!running || failedSession != session) return;
            transitionInProgress = true;
            buttonOk.Enabled = false;
            var message = $"Activity stopped: {error.Message}";
            try
            {
                await StopActivityAsync();
            }
            catch (Exception ex)
            {
                message += $"\nCould not stop activity: {ex.Message}";
            }
            finally
            {
                transitionInProgress = false;
                buttonOk.Enabled = settingsReady;
            }
            MessageBox.Show(this, message, "MMAway", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (e.Cancel) return;
            if (transitionInProgress || optionsInProgress || (!buttonOk.Enabled && settingsReady) || (!optionsButton.Enabled && !running))
            {
                e.Cancel = true;
                return;
            }

            autoStopTimer.Stop();
            autoStopMilliseconds = 0;
            running = false;
            Interlocked.Increment(ref session);
            SetRunningVisuals(false);
            try
            {
                engine.StopAsync().GetAwaiter().GetResult();
                keepAwake.Dispose();
                input.Dispose();
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                Icon = Resources.StopIcon;
                buttonOk.Text = Resources.ButtonStartLabel;
                optionsButton.Enabled = true;
                MessageBox.Show(this, $"Could not close MMAway: {ex.Message}", "MMAway", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
