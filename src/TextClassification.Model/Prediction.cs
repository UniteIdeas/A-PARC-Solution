using System.Collections.Generic;

namespace TextClassification.Model
{
    public class Prediction
    {
        public int GeneralCode { get; set; }
        public int[] GeneralCodes { get; set; }
        public int SpecificCode { get; set; }
        public double Confidence { get; set; }
        public IDictionary<string, double> WordScore { get; set; } 

        public Prediction()
        {
            GeneralCode = 0;
            SpecificCode = 0;
            Confidence = 0.0;
            WordScore = new Dictionary<string, double>();
            GeneralCodes = null;
        }
    }
}
