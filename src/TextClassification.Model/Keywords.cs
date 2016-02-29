namespace TextClassification.Model
{
    public class LuceneQuery
    {
        public int SpecificCode { get; set; }
        public int GeneralCode { get; set; }
        //public List<string> Words { get; set; }
        //public List<string> PreparedWords { get; set; }
        public string Text { get; set; }

        public LuceneQuery()
        {
            SpecificCode = 0;
            GeneralCode = 0;
            //Words = new List<string>();
            //PreparedWords = new List<string>();
            Text = string.Empty;
        }
    }
}
