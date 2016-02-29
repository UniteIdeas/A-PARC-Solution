using System.Collections.Generic;

namespace TextClassification.Model
{
    public class Word
    {
        public string Text { get; set; }
        public int Count { get; set; }
        public HashSet<int> SpecificCode { get; set; }

        public Word()
        {
            Text = string.Empty;
            Count = 0;
            SpecificCode = new HashSet<int>();
        }
    }
}
