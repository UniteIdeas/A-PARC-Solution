using System.Collections.Generic;

namespace TextClassification.Model
{
    public class ClassifiedText
    {
        public List<int> SpecificCodes { get; set; }
        public List<int> GeneralCodes { get; set; }
        //public List<int> PredictedSpecificCodes { get; set; }
        //public List<int> PredictedGeneralCodes { get; set; }
        public string Text { get; set; }
        public string Value { get; set; }
        public string PreparedText { get; set; }
        public int Year { get; set; }
        public string HighLightedText { get; set; }
        public List<string> Words { get; set; }
        public List<string> PreparedWords { get; set; }
        public List<string> UniquePreparedWords { get; set; }
        public double Confidence { get; set; }
        public IDictionary<string, double> WordScore { get; set; }
        public Dictionary<int, Prediction> Predictions { get; set; }
        public Dictionary<int[], Prediction> Predictions2 { get; set; }
        public double[] Vector { get; set; }
        public Dictionary<string, double> TermWeights { get; set; }
        public Dictionary<int[], double> SimilarityScores { get; set; }
        public double AverageTermWeight { get; set; }

        public ClassifiedText()
        {
            SpecificCodes = new List<int>();
            GeneralCodes = new List<int>();
            //PredictedSpecificCodes = new List<int>();
            //PredictedGeneralCodes = new List<int>();
            Text = string.Empty;
            PreparedText = string.Empty;
            Value = string.Empty;
            Year = 0;
            HighLightedText = string.Empty;
            Words = new List<string>();
            PreparedWords = new List<string>();
            UniquePreparedWords = new List<string>();
            //Accuracy = 0.0;
            Confidence = 0.0;
            WordScore = new Dictionary<string, double>();
            Predictions = new Dictionary<int, Prediction>();
            Predictions2 = new Dictionary<int[], Prediction>();
            Vector = null;
            TermWeights = new Dictionary<string, double>();
            SimilarityScores = new Dictionary<int[], double>();
            AverageTermWeight = 0.0;
        }
    }
}
