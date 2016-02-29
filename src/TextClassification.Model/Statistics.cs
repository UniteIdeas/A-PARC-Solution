namespace TextClassification.Model
{
    public class Statistics
    {
        public int Count { get; set; }
        public double Value { get; set; }
        public double Average { get { return Value/Count; } }
        
        public Statistics()
        {
            Count = 0;
            Value = 0;
        }
    }
}
