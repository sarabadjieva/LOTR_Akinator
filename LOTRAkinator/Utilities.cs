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
        }

        public static void RemoveQuestion(this Character character, int index)
        {
            if (character.matchingQuestionIndexes.Contains(index))
            {
                character.matchingQuestionIndexes.Remove(index);
            }
        }

        public static void AddQuestion(this Character character,  int index)
        {
            if (!character.matchingQuestionIndexes.Contains(index))
            {
                character.matchingQuestionIndexes.Add(index);
            }
        }

        public static List<int> GetQuestionIndexes(this Character character)
        {
            return character.matchingQuestionIndexes;
        }
    }
}
