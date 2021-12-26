using System;
using System.Collections.Generic;
using System.Text;

namespace LOTRAkinator
{
    static class Utilities
    {
        public static bool ContainsQuestion(this Character character, int index)
        {
            return character.matchingQuestionIndexes.Contains(index);
            /*foreach (Question question in character.matchingQuestions)
            {
                if (question.index == index) return true;
            }

            return false;*/
        }

        public static void RemoveQuestion(this Character character, int index)
        {
            if (character.matchingQuestionIndexes.Contains(index))
            {
                character.matchingQuestionIndexes.Remove(index);
            }
            /*Question questionToRemove = new Question(-1);

            foreach (Question question in character.matchingQuestions)
            {
                if (question.index == index)
                {
                    questionToRemove = question;
                    break;
                }
            }

            if (questionToRemove.index != -1)
            {
                character.matchingQuestions.Remove(questionToRemove);
            }*/
        }

        /*
        public static void SetQuestionAnswer(this Character character, int index, bool answer = true)
        {
            foreach (Question question in character.matchingQuestions)
            {
                if (question.index == index)
                {
                    character.matchingQuestions.Remove(question);
                    break;
                }
            }
        }
        */
        public static List<int> GetQuestionIndexes(this Character character)
        {
            return character.matchingQuestionIndexes;
            /*List<int> indexes = new List<int>();

            foreach (Question question in character.matchingQuestions)
            {
                indexes.Add(question.index);
            }

            return indexes;*/
        }
    }
}
