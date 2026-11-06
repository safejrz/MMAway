using MMA.Input;
using MMA.Properties;

namespace MMA
{
    public partial class MainWindow : Form
    {
        private readonly IInputInjector input = new WindowsInput();
        private readonly IKeepAwake keepAwake = new WindowsKeepAwake();
        private readonly ActivityEngine engine;
        private sealed record WindowPosition(Point Value);
        private WindowPosition windowPosition = new(Point.Empty);
        private bool running;
        private int session;

        public MainWindow()
        {
            var bounds = (Screen.PrimaryScreen ?? Screen.AllScreens.FirstOrDefault())?.Bounds
                ?? new Rectangle(0, 0, 512, 384);
            var home = new Point(bounds.X + bounds.Width / 4, bounds.Y + bounds.Height / 5 * 4);
            InitializeComponent();
            Location = new Point(bounds.X + bounds.Width / 10, bounds.Y + bounds.Height / 5);
            UpdateWindowPosition();
            Move += (_, _) => UpdateWindowPosition();
            engine = new ActivityEngine(input, home, () => Volatile.Read(ref windowPosition).Value,
                text => PostToSession(() => textBox.AppendText(text)),
                () => PostToSession(textBox.Clear), ReportEngineError);
        }

        private void UpdateWindowPosition() => Volatile.Write(ref windowPosition, new WindowPosition(Location));

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
