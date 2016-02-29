using System.Collections.Generic;
using System.Text;

namespace TextClassification.Model
{
    public class BagOfWordsCombinedLabels
    {
        public int[] GeneralCodes { get; set; }
        public Dictionary<string, Word> Words { get; set; }
        public StringBuilder Text { get; set; }
        public Dictionary<string, double> TfIdf { get; set; }
        public BagOfWordsCombinedLabels()
        {
            GeneralCodes = null;
            Words = new Dictionary<string, Word>();
            Text = new StringBuilder();
            TfIdf = new Dictionary<string, double>();
        }
    }
}
