namespace MMA
{
    /// <summary>
    /// Main window for this project
    /// </summary>
    public partial class Form1 : Form
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
        /// Initialize Form1
        /// </summary>
        public Form1()
        {
            OgStart = Cursor.Position;
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

            if (pass > 2 && random.Next(0, 3) <= pass)
            {
                MoveMouseInCircles();
            }
            if (pass >= 4)
            {
                Cursor.Position = OgStart;
                pass = 0;
            }
            pass++;
            timer.Interval = random.Next(5000, 15000);
            timer.Start();
        }

        /// <summary>
        /// Method that emulates a random user input (in the form of a mouse movement).
        /// </summary>
        private void MoveMouseAtRandom()
        {
            Cursor.Position = new Point(Cursor.Position.X + random.Next(-25, 25), Cursor.Position.Y + random.Next(-25, 25));
        }

        private void MoveMouseInCircles()
        {
            var circleSize = 300;
            var delayInMotion = 1;

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
                var xVariance = random.Next(1, 3);
                Cursor.Position = new Point(Cursor.Position.X - xVariance, Cursor.Position.Y + 1);
                i++;
                Cursor.Position = new Point(Cursor.Position.X + xVariance, Cursor.Position.Y + 1);
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
                var xVariance = random.Next(1, 3);
                Cursor.Position = new Point(Cursor.Position.X - xVariance, Cursor.Position.Y + 1);
                i++;
                if (i % 11 == 0)
                {
                    Thread.Sleep(delayInMotion);
                }
                Cursor.Position = new Point(Cursor.Position.X + xVariance, Cursor.Position.Y + 1);
            }
        }

        /// <summary>
        /// Method that handles the click event for the Main window's OK button.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event Arguments</param>
        private void buttonOk_Click(object sender, EventArgs e)
        {
            MoveMouseInCircles();
        }
    }
}
