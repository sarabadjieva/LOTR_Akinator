using System;
using System.Collections.Generic;

namespace LOTRAkinator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, please choose language!");
            string lang = Console.ReadLine();

            GameController game = new GameController(lang);

            //game loop
            while (!game.IsFinished)
            {
                game.AskQuestion();
            }
        }

    }
}
