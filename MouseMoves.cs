using System.Runtime.InteropServices;

namespace MMA
{
    internal static class MouseMoves
    {
        /// <summary>
        /// Method that emulates a mouse event.
        /// </summary>
        /// <param name="dwFlags">Flags for the button presses to emulate</param>
        /// <param name="dx">Mouse X axis position</param>
        /// <param name="dy">Mouse Y axis position</param>
        /// <param name="dwData">Nothing</param>
        /// <param name="dwExtraInfo">Nothing</param>
        [DllImport("user32.dll", SetLastError = true)]
        static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;

        /// <summary>
        /// Random object to generate movement.
        /// </summary>
        private static Random? random;

        /// <summary>
        /// Scheduled async thread for random mouse move.
        /// </summary>
        public static void RandomMouseMove(Random senderRandom)
        {
            random = senderRandom;

            var scheduledAsyncMouseMove = new Thread(NextMouseMove);
            scheduledAsyncMouseMove.Start();
        }

        /// <summary>
        /// Method that executes the next mouse actions.
        /// </summary>
        private static void NextMouseMove()
        {
            //Perform random mouse function
            var action = random?.Next(1, 9);
            if (action >= 5)
            {
                MoveMouseInSquare();
            }
            else
            {
                MoveMouseInCircles();
            }

            EmulatedMouseClick();
        }

        /// <summary>
        /// Method that emulates a mouse left click.
        /// </summary>
        private static void EmulatedMouseClick()
        {
            // Get the current mouse position
            uint x = (uint)Cursor.Position.X;
            uint y = (uint)Cursor.Position.Y;

            // Perform the mouse click
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, x, y, 0, UIntPtr.Zero);
        }

        public static void MoveMouse(Random senderRandom)
        {
            random = senderRandom;
            MoveMouse();
        }

        /// <summary>
        /// Method that emulates a random user input (in the form of a mouse movement).
        /// </summary>
        private static void MoveMouse()
        {
            if (random == null) return;
            var moves = random.Next(20, 55);
            var variance = random.Next(1, 3);
            var delayInMotion = 3;

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

        /// <summary>
        /// Moves the mouse in way that resembles drawing a square.
        /// </summary>
        private static void MoveMouseInSquare()
        {
            if (random == null) return;
            var variance = random.Next(1, 3);
            var squareSize = 350;
            var delayInMotion = 1;

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

        /// <summary>
        /// Moves the mouse in way that resembles drawing an infinite sign with two triangles.
        /// </summary>
        private static void MoveMouseInCircles()
        {
            if (random == null) return;
            var variance = random.Next(1, 3);
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
    }
}
