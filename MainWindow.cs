using MMA.Input;
using MMA.Properties;
using System.Runtime.InteropServices;

namespace MMA
{
    public partial class MainWindow : Form
    {
        private readonly IInputInjector input = new WindowsInput();
        private readonly IKeepAwake keepAwake = new WindowsKeepAwake();
        private readonly ActivityEngine engine;
        private sealed record BoundsSnapshot(Rectangle? Bounds, IntPtr Window, IntPtr Output, bool Active);
        private BoundsSnapshot boundsSnapshot = new(null, IntPtr.Zero, IntPtr.Zero, false);
        private bool running;
        private int session;

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr WindowFromPoint(Point point);

        public MainWindow()
        {
            var bounds = (Screen.PrimaryScreen ?? Screen.AllScreens.FirstOrDefault())?.Bounds
                ?? new Rectangle(0, 0, 512, 384);
            InitializeComponent();
            Location = new Point(bounds.X + bounds.Width / 10, bounds.Y + bounds.Height / 5);
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
                CanClick, text => PostToSession(() => textBox.AppendText(text)),
                () => PostToSession(textBox.Clear), ReportEngineError);
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

        private async void buttonOk_Click(object sender, EventArgs e)
        {
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
                buttonOk.Enabled = true;
            }
        }

        private async Task StopActivityAsync()
        {
            running = false;
            Interlocked.Increment(ref session);
            try
            {
                await engine.StopAsync();
            }
            finally
            {
                try { keepAwake.Disable(); }
                finally
                {
                    Icon = Resources.StopIcon;
                    buttonOk.Text = Resources.ButtonStartLabel;
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
                buttonOk.Enabled = true;
            }
            MessageBox.Show(this, message, "MMAway", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (e.Cancel) return;
            if (!buttonOk.Enabled)
            {
                e.Cancel = true;
                return;
            }

            running = false;
            Interlocked.Increment(ref session);
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
                MessageBox.Show(this, $"Could not close MMAway: {ex.Message}", "MMAway", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
