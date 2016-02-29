using System.Collections.Generic;

namespace TextClassification.Model
{
    public class Similarity
    {
        public List<int> GeneralCodes { get; set; }
        public double Score { get; set; }

        public Similarity()
        {
            GeneralCodes = new List<int>();
            Score = 0.0;
        }
    }
}
