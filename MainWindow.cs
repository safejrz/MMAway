using static System.Net.Mime.MediaTypeNames;

namespace MMA
{
    /// <summary>
    /// Main window for this project
    /// </summary>
    public partial class MainWindow : Form
    {
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

        /// <summary>
        /// Initialize MainWindow
        /// </summary>
        public MainWindow()
        {
            //Obtain screen size to get the center
            var primaryScreen = Screen.PrimaryScreen ?? Screen.AllScreens.FirstOrDefault();
            Size screenSize = primaryScreen?.Bounds.Size ?? new Size(512, 384);
            OgStart = new Point(screenSize.Width / 2, screenSize.Height / 2);

            InitializeComponent();
            timer.Interval = random.Next(10000, 15000);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        /// <summary>
        /// Timer Tick handler
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event Arguments</param>
        private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            timer.Stop();
            MoveMouseAtRandom();
            WriteRandomText();

            if (pass == 0)
            {
                Cursor.Position = OgStart;
            }
            if (pass > 2 && random.Next(0, 3) <= pass)
            {
                if (random.Next(0, 1) == 0)
                {
                    MoveMouseInCircles();
                }
                else
                {
                    MoveMouseInSquare();
                }

                WriteRandomText();
            }
            if (pass >= 4)
            {
                pass = 0;
                ClearText();
            }
            pass++;
            timer.Interval = random.Next(5000, 15000);
            timer.Start();
        }

        private void ClearText()
        {
            SetTextBoxClear();
        }

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
        /// Method that emulates a random user input (in the form of a mouse movement).
        /// </summary>
        private void MoveMouseAtRandom()
        {
            var moves = random.Next(20, 55);
            var delayInMotion = 3;
            var variance = random.Next(1, 3);

            // X
            if (random.Next(1, 6) >= 4)
            {
                for (int i = 0; i < moves; i++)
                {
                    Cursor.Position = new Point(Cursor.Position.X + random.Next(-5, 5), Cursor.Position.Y - variance);
                    i++;
                    Cursor.Position = new Point(Cursor.Position.X + random.Next(-5, 5), Cursor.Position.Y + variance);
                    if (i % 3 == 0)
                    {
                        Thread.Sleep(delayInMotion);
                    }
                }
            }

            else
            {
                // Y
                for (int i = 0; i < moves; i++)
                {
                    Cursor.Position = new Point(Cursor.Position.X + variance, Cursor.Position.Y + random.Next(-5, 5));
                    i++;
                    Cursor.Position = new Point(Cursor.Position.X + variance, Cursor.Position.Y + random.Next(-5, 5));
                    if (i % 3 == 0)
                    {
                        Thread.Sleep(delayInMotion);
                    }
                }
            }
        }

        private void MoveMouseInSquare()
        {
            var squareSize = 350;
            var delayInMotion = 1;
            var variance = random.Next(1, 3);

            // Positive right (==>x+350,y+0)
            for (int i = 0; i < squareSize; i++)
            {
                Cursor.Position = new Point(Cursor.Position.X + 1, Cursor.Position.Y - variance);
                i++;
                Cursor.Position = new Point(Cursor.Position.X + 1, Cursor.Position.Y + variance);
                if (i % 7 == 0)
                {
                    Thread.Sleep(delayInMotion);
                }
            }

            // Downward (==>x+0,y+350)
            for (int i = 0; i < squareSize; i++)
            {
                Cursor.Position = new Point(Cursor.Position.X - variance, Cursor.Position.Y + 1);
                i++;
                if (i % 9 == 0)
                {
                    Thread.Sleep(delayInMotion);
                }
                Cursor.Position = new Point(Cursor.Position.X + variance, Cursor.Position.Y + 1);
            }

            // Positive left (==>x-350,y+0)
            for (int i = 0; i < squareSize; i++)
            {
                Cursor.Position = new Point(Cursor.Position.X - 1, Cursor.Position.Y - variance);
                i++;
                Cursor.Position = new Point(Cursor.Position.X - 1, Cursor.Position.Y + variance);
                if (i % 8 == 0)
                {
                    Thread.Sleep(delayInMotion);
                }
            }

            // Upward (==>x+0,y-350)
            for (int i = 0; i < squareSize; i++)
            {
                Cursor.Position = new Point(Cursor.Position.X - variance, Cursor.Position.Y - 1);
                i++;
                if (i % 7 == 0)
                {
                    Thread.Sleep(delayInMotion);
                }
                Cursor.Position = new Point(Cursor.Position.X + variance, Cursor.Position.Y - 1);
            }
        }

        private void MoveMouseInCircles()
        {
            var circleSize = 300;
            var delayInMotion = 1;
            var variance = random.Next(1, 3);

            // Positive diagonal right (==>x+200,y+200)
            for (int i = 0; i < circleSize; i++)
            {
                Cursor.Position = new Point(Cursor.Position.X + 1, Cursor.Position.Y - 1);
                if (i % 7 == 0)
                {
                    Thread.Sleep(delayInMotion);
                }
            }

            // Downward (==>x+200,y)
            for (int i = 0; i < circleSize; i++)
            {
                Cursor.Position = new Point(Cursor.Position.X - variance, Cursor.Position.Y + 1);
                i++;
                Cursor.Position = new Point(Cursor.Position.X + variance, Cursor.Position.Y + 1);
                if (i % 11 == 0)
                {
                    Thread.Sleep(delayInMotion);
                }
            }

            // Positive diagonal left (==>x,y+200)
            for (int i = 0; i < circleSize; i++)
            {
                Cursor.Position = new Point(Cursor.Position.X - 1, Cursor.Position.Y - 1);
                if (i % 7 == 0)
                {
                    Thread.Sleep(delayInMotion);
                }
            }

            // Downward (==>x,y)            
            for (int i = 0; i < circleSize; i++)
            {
                Cursor.Position = new Point(Cursor.Position.X - variance, Cursor.Position.Y + 1);
                i++;
                if (i % 9 == 0)
                {
                    Thread.Sleep(delayInMotion);
                }
                Cursor.Position = new Point(Cursor.Position.X + variance, Cursor.Position.Y + 1);
            }
        }

        /// <summary>
        /// Method that handles the click event for the Main window's OK button.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event Arguments</param>
        private void buttonOk_Click(object sender, EventArgs e)
        {
            //Reset x,y origin
            OgStart = Cursor.Position;

            //Perform random mouse function
            //MoveMouseInCircles();
            //MoveMouseInSquare();
            MoveMouseAtRandom();
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
    }
}
