using System.Collections.Generic;

namespace TextClassification.Model
{
    public class DataSet
    {
        public int GeneralCode { get; set; }
        
        public HashSet<string> Words { get; set; }

        public DataSet()
        {
            GeneralCode = 0;
            Words = new HashSet<string>();
            //Words = new List<Word>();
        }
    }
}
