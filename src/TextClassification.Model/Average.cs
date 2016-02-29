namespace TextClassification.Model
{
    public class Average
    {
        public int Paragraph { get; set; }
        public double Value { get; set; }
        public int Count { get; set; }

        public Average()
        {
            Paragraph = 0;
            Value = 0;
            Count = 0;
        }
    }
}
