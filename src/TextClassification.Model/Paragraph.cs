using System.Collections.Generic;

namespace TextClassification.Model
{
    public class Paragraph
    {
        public string Text { get; set; }
        public int Number { get; set; }
        public List<Sentence> Sentences { get; set; }

        public Paragraph()
        {
            Text = string.Empty;
            Number = 0;
            Sentences = new List<Sentence>();
        }
    }
}
