using System.Threading;

namespace MMA
{
    internal static class Program
    {
        private const string SingleInstanceMutexName = @"Local\MMAway.SingleInstance";

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using var singleInstanceMutex = new Mutex(initiallyOwned: true, SingleInstanceMutexName, out bool isFirstInstance);
            if (!isFirstInstance) return;

            try
            {
                // To customize application configuration such as set high DPI settings or default font,
                // see https://aka.ms/applicationconfiguration.
                ApplicationConfiguration.Initialize();
                Application.Run(new MainWindow());
            }
            finally
            {
                singleInstanceMutex.ReleaseMutex();
            }
        }
    }
}
