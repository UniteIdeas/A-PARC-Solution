using System.Collections.Generic;

namespace TextClassification.Model.Statistic
{
    public class CodePerParagraph
    {
        public int ParagraphNumber { get; set; }
        public int Total { get; set; }
        public Dictionary<int, int> Values { get; set; }
        public Dictionary<int, double> Percentage { get; set; }
        public int TotalSentences { get; set; }
        public KeyValuePair<int, double> Highest { get; set; }
        public bool IsValid { get; set; }

        public CodePerParagraph()
        {
            ParagraphNumber = 0;
            Total = 0;
            Values = new Dictionary<int, int>();
            Percentage = new Dictionary<int, double>();
            TotalSentences = 0;
            IsValid = true;
        }
    }
}
