using System.Collections.Generic;
using System.Text;

namespace TextClassification.Model
{
    public class BagOfWords
    {
        public int GeneralCode { get; set; }
        public Dictionary<string, Word> Words { get; set; }
        public StringBuilder Text { get; set; }
        public Dictionary<string, double> TermWeigths { get; set; }
        public int CountSentences { get; set; }
        public Dictionary<string, int> WordInSentences { get; set; }
        public HashSet<string> WordsToUse { get; set; }

        public BagOfWords()
        {
            GeneralCode = 0;
            Words = new Dictionary<string, Word>();
            Text = new StringBuilder();
            TermWeigths = new Dictionary<string, double>();
            CountSentences = 0;
            WordInSentences = new Dictionary<string, int>();
            WordsToUse = new HashSet<string>();
        }
    }
}
