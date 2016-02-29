using System;
using TextClassification.Common.Extension;
using TextClassification.Repository;
using TextClassification.Service.StanfordNLP;

namespace TextClassification.Service.Classification
{
    public class StemService
    {
        private readonly StanfordLemmatizer _lemmatizer;
        public StemService()
        {
            _lemmatizer = new StanfordLemmatizer();
        }
        public string GetStem(string text)
        {
            text = text.Clean();
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var repo = new StemWordRepository();
            var result = repo.Find(text);

            if (result != null) return result.Stem;

            //fetch from external source
            result = _lemmatizer.GetResult(text);

            if(result == null)
                throw new Exception("No result");

            //save stem
            repo.Add(result);

            return result.Stem;
        }
    }
}
