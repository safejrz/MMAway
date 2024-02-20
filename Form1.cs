namespace MMA
{
    /// <summary>
    /// Main window for this project
    /// </summary>
    public partial class Form1 : Form
    {
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
            timer.Interval = random.Next(5000, 20000);
            timer.Start();
        }

        /// <summary>
        /// Method that emulates a random user input (in the form of a mouse movement).
        /// </summary>
        private void MoveMouseAtRandom()
        {
            Cursor.Position = new Point(Cursor.Position.X + random.Next(-25, 25), Cursor.Position.Y + random.Next(-25, 25));
        }

        /// <summary>
        /// Method that handles the click event for the Main window's OK button.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event Arguments</param>
        private void buttonOk_Click(object sender, EventArgs e)
        {
            MoveMouseAtRandom();
        }
    }
}
