using System.Collections.Generic;

namespace TextClassification.Model
{
    public class Sentence
    {
        public string Text { get; set; }
        public string PreparedText { get; set; }
        public List<ClassifiedText> Classifications { get; set; }
        public Dictionary<int, Prediction> Predictions { get; set; }
        public List<string> Words { get; set; }
        public List<string> PreparedWords { get; set; }
        public ClassifiedText ClassifiedText { get; set; }

        public Sentence()
        {
            Text = string.Empty;
            PreparedText = string.Empty;
            Classifications = new List<ClassifiedText>();
            Predictions = new Dictionary<int, Prediction>();
            Words = new List<string>();
            PreparedWords = new List<string>();
            ClassifiedText = new ClassifiedText();
        }
    }
}
