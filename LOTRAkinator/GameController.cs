using System;
using System.Collections.Generic;
using System.Text;

namespace LOTRAkinator
{
    class GameController
    {
        private bool isFinished;

        private List<string> questions;
        private List<string> characters;

        public bool IsFinished
        {
            get => isFinished;
        }

        public GameController(string languageSetting)
        {
            SetLanguageSetting(languageSetting);
        }


        private void SetLanguageSetting(string languageSetting)
        {
            switch (languageSetting.ToLower())
            {
                case "en":
                case "english":
                    //load english files
                    break;
                case "bg":
                case "bulgarian":
                    //load bulgarian files
                    break;
                default:
                    break;
            }
        }

        public void AskQuestion()
        {

        }

        private void GetQuestion()
        {
            Console.WriteLine(questions[0]);
            string answer = Console.ReadLine();
        }
    }
}
