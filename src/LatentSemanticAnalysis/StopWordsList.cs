using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace lsi
{
    public class StopWordsList
    {
        // Hash table to contain the stop words
        private static System.Collections.Hashtable goHashTable = null;

        // Check if the word is a stop word
        public static bool IsStopWord(string word)
        {
            return goHashTable.ContainsKey(word.ToLower());
        }

        // Initialize the stop word list.
        public static void InitializeList(string sStopListPath)
        {
            goHashTable = new System.Collections.Hashtable();
            TextReader oTR = File.OpenText(sStopListPath);
            string sText = oTR.ReadToEnd();
            oTR.Close();
            oTR = null;

            Regex oRegex = new Regex("([ \\t{}():;. \n])");
            sText = sText.ToLower();

            String[] words = oRegex.Split(sText);
            for (int i = 0; i < words.Length; i++)
            {
                words[i] = words[i].Trim();
            }

            for (int i = 0; i < words.Length; i++)
            {
                MatchCollection mc = oRegex.Matches(words[i]);
                if (mc.Count <= 0 && words[i].Trim().Length > 0
                    && !StopWordsList.IsStopWord(words[i]))
                    goHashTable.Add(words[i],"");
            }
        }
    }
}
