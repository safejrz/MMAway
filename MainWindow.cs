using MMA.Properties;
using System.Runtime.InteropServices;

namespace MMA
{
    /// <summary>
    /// Main window for this project
    /// </summary>
    public partial class MainWindow : Form
    {
        /// <summary>
        /// Code to prevent going into sleep-mode.
        /// </summary>
        /// <param name="esFlags"></param>
        /// <returns>Nothing</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetThreadExecutionState(EXECUTION_STATE esFlags);

        /// <summary>
        /// Static enum flags, part of the code to prevent going into sleep-mode.
        /// </summary>
        [Flags]
        enum EXECUTION_STATE : uint
        {
            ES_CONTINUOUS = 0x80000000,
            ES_SYSTEM_REQUIRED = 0x00000001,
            ES_DISPLAY_REQUIRED = 0x00000002
        }

        #region Fields
        /// <summary>
        /// Keeps track of where is the original mouse position to avoid colliding with the screen edges
        /// </summary>
        Point OgStart;

        /// <summary>
        /// Keeps track of how many random moves had occurred to insert another random action
        /// </summary>
        int pass = 0;

        /// <summary>
        /// Random instance which will help us add unpredictability to the emulated actions and when they occur
        /// </summary>
        private Random random = new Random(DateTime.Now.Millisecond);

        /// <summary>
        /// Timer object that will help us triggering the emulated user input each time it occurs
        /// </summary>
        private System.Timers.Timer timer = new System.Timers.Timer();

        /// <summary>
        /// This variable is set to false by default. Used to identify if the program timer should be started or stopped.
        /// </summary>
        private bool shouldBeRunning = false;

        /// <summary>
        /// Dispatcher to modify the textbox in the UI
        /// </summary>
        /// <param name="text"></param>
        delegate void SetTextCallback(string text);

        /// <summary>
        /// Dispatcher to set the focus on the textbox in the UI
        /// </summary>
        delegate void FocusTextBoxCallBack();

        /// <summary>
        /// Dispatcher to clear the textbox in the UI
        /// </summary>
        delegate void ClearTextBoxCallback();

        /// <summary>
        /// Dispatcher to set the focus on the main window
        /// </summary>
        delegate void WindowFocusCallBack();
        #endregion

        /// <summary>
        /// Initialize MainWindow
        /// </summary>
        public MainWindow()
        {
            //Obtain screen size to get the center
            var primaryScreen = Screen.PrimaryScreen ?? Screen.AllScreens.FirstOrDefault();
            Size screenSize = primaryScreen?.Bounds.Size ?? new Size(512, 384);
            OgStart = new Point(screenSize.Width / 4, (screenSize.Height / 5) * 4);
            Location = new Point(screenSize.Width / 10, screenSize.Height / 5);

            InitializeComponent();
            timer.Interval = random.Next(10000, 15000);
            timer.Elapsed += Timer_Elapsed;
        }

        /// <summary>
        /// Timer Tick handler
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event Arguments</param>
        private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            timer.Stop();
            if (shouldBeRunning)
            {
                MouseMoves.MoveMouse(random);
                WriteRandomText();

                if (pass == 0)
                {
                    Cursor.Position = OgStart;
                }
                if (pass > 2 && random.Next(0, 3) <= pass)
                {
                    MouseMoves.RandomMouseMove(random);
                    WriteRandomText();
                }
                if (pass >= 4)
                {
                    MouseMoves.MoveMouse(random);
                    pass = 0;
                    ClearText();
                }
                pass++;
                timer.Interval = random.Next(5000, 15000);
                timer.Start();
            }
        }

        /// <summary>
        /// Method that handles the click event for the Main window's OK button.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event Arguments</param>
        private void buttonOk_Click(object sender, EventArgs e)
        {
            shouldBeRunning = !shouldBeRunning;
            var lastOpenedForm = Application.OpenForms.Cast<Form>().Last();

            if (!shouldBeRunning)
            {
                SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
                lastOpenedForm.Icon = Resources.StopIcon;
                buttonOk.Text = Resources.ButtonStartLabel;
                timer.Stop();
                return;
            }

            SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_SYSTEM_REQUIRED | EXECUTION_STATE.ES_DISPLAY_REQUIRED);
            Cursor.Position = OgStart;
            lastOpenedForm.Icon = Resources.RunIcon;
            buttonOk.Text = Resources.ButtonStopLabel;
            timer.Start();

            //Perform random mouse function
            MouseMoves.RandomMouseMove(random);
        }

        #region Text Methods
        /// <summary>
        /// Clears the textbox's text
        /// </summary>
        private void ClearText()
        {
            SetTextBoxClear();
        }

        /// <summary>
        /// Writes something random in the textboxt
        /// </summary>
        private void WriteRandomText()
        {
            SetTextBoxFocus();
            var repeat = random.Next(2, 7);

            for (int i = 0; i < repeat; i++)
            {
                var delayInMotion = repeat * random.Next(20, 100);
                Char c = Convert.ToChar(random.Next(48, 122));
                SetTextboxText(textBox.Text + c.ToString());
                Thread.Sleep(delayInMotion);
            }
            SetWindowFocus();
        }

        #region Delegate handlers
        private void SetWindowFocus()
        {
            if (this.InvokeRequired)
            {
                WindowFocusCallBack d = new WindowFocusCallBack(SetWindowFocus);
                Invoke(d, null);
            }
            else
            {
                this.Focus();
            }
        }

        /// <summary>
        /// Method to be called by the dispatcher to modify the textbox in the UI
        /// </summary>
        /// <param name="text"></param>
        private void SetTextboxText(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (textBox.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetTextboxText);
                Invoke(d, new object[] { text });
            }
            else
            {
                textBox.Text = text;
            }
        }

        private void SetTextBoxFocus()
        {
            if (textBox.InvokeRequired)
            {
                FocusTextBoxCallBack d = new FocusTextBoxCallBack(SetTextBoxFocus);
                Invoke(d, null);
            }
            else
            {
                textBox.Focus();
            }
        }

        private void SetTextBoxClear()
        {
            if (textBox.InvokeRequired)
            {
                ClearTextBoxCallback d = new ClearTextBoxCallback(SetTextBoxClear);
                Invoke(d, null);
            }
            else
            {
                textBox.Clear();
            }
        }
        #endregion
        #endregion
    }
}
