﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace LOTRAkinator
{
    public enum Language
    {
        English,
        Bulgarian
    }

    public enum Answer
    {
        Yes,
        No
    }

    public struct Character
    {
        public string name;
        //public List<Question> matchingQuestions;
        public List<int> matchingQuestionIndexes;

        public Character(string name, List<int> matchingQuestions)
        {
            this.name = name;
            this.matchingQuestionIndexes = matchingQuestions;
        }

        public Character(Character characterToCopy)
        {
            this.name = characterToCopy.name;
            this.matchingQuestionIndexes = new List<int>(characterToCopy.matchingQuestionIndexes);
        }
    }

    public struct Question
    {
        public int index;
        public bool positiveAnswer;

        public Question(int i)
        {
            index = i;
            positiveAnswer = false;
        }
    }

    class GameController
    {
        private const string enCharactersFilePath = "Characters.txt";
        private const string enQuestionsFilePath = "Questions.txt";
        private const string bgCharactersFilePath = "";
        private const string bgQuestionsFilePath = "";
        
        private readonly List<Character> characters = new List<Character>();
        private readonly Dictionary<int, string> questionsById = new Dictionary<int, string>();

        private Language currentLanguage = Language.English;

        private List<Character> possibleCharacters = new List<Character>();
        private List<Question> askedQuestions = new List<Question>();

        private bool stop = false;

        public GameController(string languageSetting)
        {
            SetLanguageSetting(languageSetting);
        }

        public void StartGame()
        {
            ResetData();
            GameLoop();
        }

        private void SetLanguageSetting(string languageSetting)
        {
            switch (languageSetting.ToLower())
            {
                case "en":
                case "english":
                    currentLanguage = Language.English;
                    break;
                case "bg":
                case "bulgarian":
                    currentLanguage = Language.Bulgarian;
                    break;
                default:
                    break;
            }

            ReadFiles();
        }

        private void ReadFiles()
        {
            string charactersFile = string.Empty;
            string questionsFile = string.Empty;

            switch (currentLanguage)
            {
                case Language.English:
                    charactersFile = enCharactersFilePath;
                    questionsFile = enQuestionsFilePath;
                    break;
                case Language.Bulgarian:
                    break;
                default:
                    break;
            }

            //open the questions file into a streamreader
            using (System.IO.StreamReader sr = new System.IO.StreamReader(questionsFile))
            {
                while (!sr.EndOfStream) // Keep reading until we get to the end
                {
                    string splitMe = sr.ReadLine();
                    string[] bananaSplits = splitMe.Split(new char[] { ':' }); //Split at the colons

                    if (bananaSplits.Length < 2) // If we get less than 2 results, discard them
                        continue;
                    else if (bananaSplits.Length == 2) // Easy part. If there are 2 results, add them to the dictionary
                        questionsById.Add(int.Parse(bananaSplits[0].Trim()), bananaSplits[1].Trim());
                }
                sr.Close();
            }

            //open the characters file
            try
            {
                //Pass the file path and file name to the StreamReader constructor
                System.IO.StreamReader sr = new System.IO.StreamReader(charactersFile);

                while (!sr.EndOfStream) // Keep reading until we get to the end
                {
                    string splitMe = sr.ReadLine();
                    string[] bananaSplits = splitMe.Split(new char[] { '-' }); //Split at the dash

                    if (bananaSplits.Length < 2) // If we get less than 2 results, discard them
                        continue;
                    else if (bananaSplits.Length == 2) // Easy part. If there are 2 results, add them to the dictionary
                    {
                        List<int> indexes = new List<int>();
                        if(bananaSplits[1].Length > 0)
                        {
                        string[] questionIndexes = (bananaSplits[1].Trim()).Split(new char[] { ',' });

                        for (int i = 0; i < questionIndexes.Length; i++)
                        {
                            indexes.Add(int.Parse(questionIndexes[i]));
                        }
                        }

                        characters.Add(new Character(bananaSplits[0].Trim(), indexes));
                    }
                }

                sr.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {
                Console.WriteLine("Executing finally block.");
            }
        }

        private void WriteToFiles()
        {
            string charactersFile = string.Empty;
            string questionsFile = string.Empty;

            switch (currentLanguage)
            {
                case Language.English:
                    charactersFile = enCharactersFilePath;
                    questionsFile = enQuestionsFilePath;
                    break;
                case Language.Bulgarian:
                    break;
                default:
                    break;
            }

            string overwriteCharacters = string.Empty;
            foreach (var character in characters)
            {
                overwriteCharacters += character.name + "-" + string.Join(",",character.GetQuestionIndexes()) + Environment.NewLine;
            }

            using (StreamWriter writer = new StreamWriter(charactersFile, false))
            {
                writer.Write(overwriteCharacters);
            }

            string overwriteQuestions = string.Empty;
            foreach (var questionStringWithID in questionsById)
            {
                overwriteQuestions += questionStringWithID.Key + ":" + questionStringWithID.Value + Environment.NewLine;
            }

            using (StreamWriter writer = new StreamWriter(questionsFile, false))
            {
                writer.Write(overwriteQuestions);
            }
        }

        private void GameLoop()
        {
            while (possibleCharacters.Count > 1 && !stop)
            {
                AskQuestion();
            }

            if (possibleCharacters.Count != 0)
            {
                for (int i = 0; i < possibleCharacters.Count; i++)
                {
                    //stops if the player has guessed right
                    MakeGuess(possibleCharacters[i].name, i == possibleCharacters.Count - 1);
                }
            }
        }

        private void AskQuestion()
        {
            //find most seen question index in possible answers
            List<int> allIndexes = new List<int>();
            foreach (Character character in possibleCharacters)
            {
                allIndexes.AddRange(character.GetQuestionIndexes());
            }

            foreach (Question askedQuestion in askedQuestions)
            {
                allIndexes.RemoveAll(questionIndex => questionIndex == askedQuestion.index);
            }

            //test
            foreach (var item in possibleCharacters)
            {
                Console.Write(item.name + ' ') ;
            }

            if (allIndexes.Count == 0)
            {
                stop = true;
                return;
            }

            int mostCommonQuestionIndex = allIndexes.GroupBy(i => i).OrderByDescending(grp => grp.Count())
                .Select(grp => grp.Key).First();

            Console.WriteLine(questionsById[mostCommonQuestionIndex]);

            //process answer
            bool answer = GetAnswer() == Answer.Yes;
            Question lastAskedQuestion = new Question(mostCommonQuestionIndex);
            lastAskedQuestion.positiveAnswer = answer;
            askedQuestions.Add(lastAskedQuestion);

            List<Character> charactersToRemove = new List<Character>();

            foreach (var character in possibleCharacters)
            {
                if ((character.ContainsQuestion(mostCommonQuestionIndex) && !answer) ||
                    (!character.ContainsQuestion(mostCommonQuestionIndex) && answer))
                {
                    charactersToRemove.Add(character);
                }

                /*if (character.matchingQuestionIndexes.Count == 0 || )
                {
                    //removes characters with no questions
                    charactersToRemove.Add(character);
                }*/
            }

            for (int i = 0; i < charactersToRemove.Count; i++)
            {
                possibleCharacters.Remove(charactersToRemove[i]);
            }
        }

        private void MakeGuess(string name, bool last = false)
        {
            PrintMessage("Is your character " + name + "?", "Героят ви " + name + " ли е?");

            if (GetAnswer() == Answer.Yes)
            {
                PlayAgain();
            }
            else if (last)
            {
                PrintMessage("What is the name of your character?", "Как се казва героят ви? Напишете на английски (засега)");

                string playerCharacter = Console.ReadLine();

                foreach (Character character in characters)
                {
                    if (character.name.ToLower() == playerCharacter.ToLower())
                    {
                        PrintMessage("Here are your answers:", "Ето как сте отговорили:");
                        foreach (Question question in askedQuestions)
                        {
                            Console.WriteLine(questionsById[question.index] + " " + question.positiveAnswer);
                        }

                        PrintMessage("Have I made a mistake?", "Аз ли съм направила грешка?");
                        if (GetAnswer() == Answer.Yes)
                        {

                            return;
                        }

                        PrintMessage("Have you made a mistake?", "Вие ли сте направили грешка?");
                        if (GetAnswer() == Answer.Yes)
                        {

                            return;
                        }

                        return;
                    }
                }

                //else new character with same answers? -> new question
            }
        }

        private void PlayAgain()
        {
            //do you want to start again question
            PrintMessage("Do you want to play again?", "Искате ли да играете отново?");

            if (GetAnswer() == Answer.Yes)
            {
                StartGame();
            }
            else
            {
                Environment.Exit(1);
            }
        }

        private void ResetData()
        {
            stop = false;
            possibleCharacters = new List<Character>();
            foreach (Character item in characters)
            {
                possibleCharacters.Add(new Character(item));
            }

            askedQuestions.Clear();
        }

        private Answer GetAnswer()
        {
            string answer = Console.ReadLine();
            switch (answer.ToLower())
            {
                case "y":
                case "yes":
                case "да":
                    return Answer.Yes;
                case "n":
                case "no":
                case "не":
                default:
                    return Answer.No;
            }
        }

        private void PrintMessage(string enString, string bgString)
        {
            switch (currentLanguage)
            {
                case Language.English:
                    Console.WriteLine(enString);
                    Console.WriteLine();
                    break;
                case Language.Bulgarian:
                    Console.WriteLine(bgString);
                    break;
                default:
                    break;
            }
        }
    }
}
