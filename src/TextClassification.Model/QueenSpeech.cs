using System.Collections.Generic;

namespace TextClassification.Model
{
    public class QueenSpeech
    {
        public string Name { get; set; }
        public List<Paragraph> Paragraphs { get; set; }
        public int Year { get; set; }
        public string Text { get; set; }
        public Dictionary<string, double> TermWeights { get; set; }

        public QueenSpeech()
        {
            Name = string.Empty;
            Paragraphs = new List<Paragraph>();
            Year = 0;
            Text = string.Empty;
            TermWeights = new Dictionary<string, double>();
        }
    }
}
