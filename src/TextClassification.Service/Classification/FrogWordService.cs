using System;
using TextClassification.Common.Extension;
using TextClassification.Repository;
using TextClassification.Service.Frog;

namespace TextClassification.Service.Classification
{
    public class FrogWordService
    {
        public string GetPoS(string word)
        {
            word = word.Clean();
            if (string.IsNullOrEmpty(word))
                return string.Empty;

            var frogWordRepository = new StemWordRepository();
            var result = frogWordRepository.Find(word);

            if (result == null)
            {
                Console.WriteLine("Getting information from Frog about the word: {0}", word);
                //fetch from the webservice
                var frogClient = new FrogClient();
                result = frogClient.GetResult(word);

                if (result == null)
                    throw new Exception("No frog result");

                //save stem
                frogWordRepository.Add(result);
            }

            return result.Type;
        }
    }
}
