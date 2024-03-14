using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMA
{
    internal static class MouseMoves
    {
        /// <summary>
        /// Random object to generate movement.
        /// </summary>
        private static Random? random;

        public static void RandomMouseMove(Random senderRandom)
        {
            random = senderRandom;

            //Perform random mouse function
            var action = random.Next(1, 9);
            if (action >= 5)
            {
                MoveMouseInSquare();
            }
            else
            {
                MoveMouseInCircles();
            }
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
