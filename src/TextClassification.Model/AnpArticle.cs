using System.Collections.Generic;

namespace TextClassification.Model
{
    public class AnpArticle
    {
        public string Text { get; set; }
        public string Date { get; set; }
        public string Bron { get; set; }
        public string Year { get; set; }
        public string Title { get; set; }
        public int Count { get { return Sentences.Count; } }
        public Dictionary<int, int> Predictions { get; set; }
        public List<ClassifiedText> Sentences { get; set; }

        public AnpArticle()
        {
            Text = string.Empty;
            Date = string.Empty;
            Bron = string.Empty;
            Year = string.Empty;
            Title = string.Empty;
            Predictions = new Dictionary<int, int>();
            Sentences = new List<ClassifiedText>();
        }
    }
}
