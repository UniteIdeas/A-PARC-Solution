namespace TextClassification.Model
{
    public class Measure
    {
        public int Correct { get; set; }
        public int Incorrect { get; set; }
        public Measure()
        {
            Correct = 0;
            Incorrect = 0;
        }
    }
}
