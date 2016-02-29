namespace TextClassification.Model
{
    public class WordScore
    {
        public string Word { get; set; }
        public double Score { get; set; }
        public int Category { get; set; }

        public WordScore()
        {
            Word = string.Empty;
            Score = 0.0;
            Category = -1;
        }
    }
}
