using System.Collections.Generic;

namespace TextClassification.Model
{
    public class DoubleWord
    {
        public string Text { get; set; }
        public int FirstWordCount { get; set; }
        public int SecondWordCount { get; set; }
        public int Count { get; set; }
        public List<int> GeneralCodes { get; set; }
        public List<int> SpecificCodes { get; set; }
        public DoubleWord()
        {
            Text = string.Empty;
            FirstWordCount = 0;
            SecondWordCount = 0;
            Count = 0;
            GeneralCodes = new List<int>();
            SpecificCodes = new List<int>();
        }
    }
}
