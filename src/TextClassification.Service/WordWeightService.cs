using System.Collections.Generic;
using System.Linq;
using TextClassification.Common;
using TextClassification.Model;
using TextClassification.Repository;
using TextClassification.Service.Classification;
using TextClassification.Service.DataSet.Excel;
using TextClassification.Service.Dataset.Excel;

namespace TextClassification.Service
{
    public class WordWeightService 
    {
        public void FillDatabase(string path = Constants.PathClassifiedText)
        {
            var wordWeightRepository = new WordWeightRepository();
            //var clasifiedService = new ClassifiedQueenSpeechService();

            var bagOfWordsService = new BagOfWordsService();
            //var bags = bagOfWordsService.GetBagOfWords(classifiedTexts);
            var bags = bagOfWordsService.GetBagOfWords(path, reload: false);

            var baseClassificationService = new BaseClassificationService();
            
            var result = new List<WordWeight>();
            foreach (var bag in bags)
            {
                foreach (var wordWeight in bag.Value.TermWeigths)
                {
                    var ww = new WordWeight {Bag = bag.Key
                        , Text = wordWeight.Key
                        , Weight = (wordWeight.Value * baseClassificationService.GetWordTypeWeight(wordWeight.Key))};
                    result.Add(ww);
                }
            }

            wordWeightRepository.Truncate();    
            wordWeightRepository.Insert(result.Where(p => p.Weight > 0).ToList());
        }
    }
}
