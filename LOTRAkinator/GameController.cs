using System;
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
        
        private List<Character> characters = new List<Character>();
        private Dictionary<int, string> questionsById = new Dictionary<int, string>();

        private Language currentLanguage = Language.English;

        private List<Character> possibleCharacters = new List<Character>();
        private List<Question> askedQuestions = new List<Question>();

        private bool stop = false;

        public GameController(string languageSetting)
        {
            SetLanguageSetting(languageSetting);
            ReadFiles();
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
            questionsById = new Dictionary<int, string>();
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
            characters = new List<Character>();
            try
            {
                System.IO.StreamReader sr = new System.IO.StreamReader(charactersFile);

                while (!sr.EndOfStream)
                {
                    string splitMe = sr.ReadLine();
                    string[] bananaSplits = splitMe.Split(new char[] { '-' });

                    if (bananaSplits.Length < 2)
                        continue;
                    else if (bananaSplits.Length == 2)
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
        }

        private void OverwriteFiles()
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

            if (allIndexes.Count == 0)
            {
                stop = true;
                return;
            }

            int mostCommonQuestionIndex = allIndexes.GroupBy(i => i).OrderByDescending(grp => grp.Count())
                .Select(grp => grp.Key).First();
            int mostCommonOccurences = allIndexes.Where(i => i.Equals(mostCommonQuestionIndex)).Count();

            int leastCommonQuestionIndex = allIndexes.GroupBy(i => i).OrderByDescending(grp => grp.Count())
                .Select(grp => grp.Key).Last();
            int leastCommonOccurences = allIndexes.Where(i => i.Equals(leastCommonQuestionIndex)).Count();

            int questionIndexToAsk = mostCommonQuestionIndex;

            if (leastCommonOccurences == 1 && mostCommonOccurences < possibleCharacters.Count/2)

            if (allIndexes.Where(i => i.Equals(leastCommonQuestionIndex)).Count() == 1)
            {
                //if there is a unique character maybe guess earlier
                questionIndexToAsk = leastCommonQuestionIndex;
            }

            PrintMessage(defaultString: questionsById[questionIndexToAsk]);

            //process answer
            bool answer = GetAnswer() == Answer.Yes;
            Question lastAskedQuestion = new Question(questionIndexToAsk);
            lastAskedQuestion.positiveAnswer = answer;
            askedQuestions.Add(lastAskedQuestion);

            List<Character> charactersToRemove = new List<Character>();

            foreach (var character in possibleCharacters)
            {
                if ((character.ContainsQuestion(questionIndexToAsk) && !answer) ||
                    (!character.ContainsQuestion(questionIndexToAsk) && answer))
                {
                    charactersToRemove.Add(character);
                }
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
                //if there were more possible guesses add a question to distinguish the player's character
                if (possibleCharacters.Count > 1)
                {
                    PrintMessage("Please add a question which will help me find the perfect answer straight away", "Моля добавете въпрос, който ще ми помогне да позная героя веднага");
                    int newQuestionIndex = AddPlayersQuestion(name);

                    foreach (var character in characters)
                    {
                        if (character.name == name)
                        {
                            character.AddQuestion(newQuestionIndex);
                            break;
                        }
                    }

                    OverwriteFiles();
                    ResetData();
                    ReadFiles();
                }

                PlayAgain();
            }
            else if (last)
            {
                PrintMessage("What is the name of your character?", "Как се казва героят ви? Напишете на английски (засега)");

                string playerCharacterName = Console.ReadLine();

                if(string.IsNullOrEmpty(playerCharacterName))
                {
                    PlayAgain();
                    return;
                }

                foreach (Character character in characters)
                {
                    if (character.name.ToLower() == playerCharacterName.ToLower())
                    {
                        PrintMessage("Here are your answers:", "Ето как сте отговорили:");
                        foreach (Question askedQuestion in askedQuestions)
                        {
                            Console.WriteLine(askedQuestion.index + " " + questionsById[askedQuestion.index] + " " + askedQuestion.positiveAnswer);
                        }

                        PrintMessage("Have I made a mistake?", "Аз ли съм направила грешка?");
                        if (GetAnswer() == Answer.Yes)
                        {
                            PrintMessage("Please write the index of the wrong answer", "Моля напишете индекса на грешния въпрос");
                            int index = int.Parse(Console.ReadLine().Trim());

                            if (character.ContainsQuestion(index))
                            {
                                character.RemoveQuestion(index);
                            }
                            else
                            {
                                character.AddQuestion(index);
                            }

                            OverwriteFiles();
                            ResetData();
                            ReadFiles();
                            PlayAgain();
                            return;
                        }

                        PrintMessage("Have you made a mistake?", "Вие ли сте направили грешка?");
                        if (GetAnswer() == Answer.Yes)
                        {
                            PrintMessage("Well, be more careful next time", "Бъдете по-внимателни следващия път");
                            PlayAgain();
                            return;
                        }

                        PlayAgain();
                        return;
                    }
                }

                //if the player's character does not match with any of the programs
                PrintMessage("I don't know this character. Can you please add a question with which I can distinguish him from others?",
                    "Не знам този герой. Може ли да ми кажете въпрос, с който бих могла да го различа?");

                int newQuestionIndex = AddPlayersQuestion();

                //add the character and the asked matching questions to the array
                //the newly added question is directly added
                List<int> matchingQuestions = new List<int>() { newQuestionIndex };

                foreach (Question askedQuestion in askedQuestions)
                {
                    if (askedQuestion.positiveAnswer)
                    {
                        matchingQuestions.Add(askedQuestion.index);
                    }
                }

                Character newCharacter = new Character(playerCharacterName, matchingQuestions);
                characters.Add(newCharacter);

                OverwriteFiles();
                ResetData();
                ReadFiles();
                PlayAgain();
            }
        }

        private int AddPlayersQuestion(string nameToSkip = "")
        {
            string question = Console.ReadLine();
            int newQuestionIndex = questionsById.Count;
            questionsById.Add(newQuestionIndex, question);

            //add the question to others
            PrintMessage("For which other characters from the list is this question also valid (if it is). Each character is written on a new line" + Environment.NewLine),
                "За кои други герои от списъка е валиден този въпрос(ако има такива). Всеки герой се пише на отделен ред" + Environment.NewLine);
            foreach (var character in characters)
            {
                if (!character.name.Equals(nameToSkip))
                {
                    Console.WriteLine(character.name);
                }
            }
            Console.WriteLine();

            int numOfCharacters = characters.Count;
            List<string> charactersToAddQuestionTo = new List<string>();
            string input = string.Empty;
            for (int i = 0; i < numOfCharacters; i++)
            {
                input = Console.ReadLine();

                if (string.IsNullOrEmpty(input))
                {
                    break;
                }

                charactersToAddQuestionTo.Add(input);
            }

            foreach (var character in characters)
            {
                if (charactersToAddQuestionTo.Contains(character.name))
                {
                    character.AddQuestion(newQuestionIndex);
                }
            }

            return newQuestionIndex;
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

        private void PrintMessage(string enString = "", string bgString = "", string defaultString = "")
        {
            Console.WriteLine("------------------------------------------------");

            if (!string.IsNullOrEmpty(defaultString))
            {
                Console.WriteLine(defaultString);
                return;
            }

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
