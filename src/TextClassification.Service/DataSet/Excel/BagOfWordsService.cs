using System;
using System.Collections.Generic;
using System.Linq;
using TextClassification.Common;
using TextClassification.Common.Extension;
using TextClassification.Model;
using TextClassification.Service.Classification;

namespace TextClassification.Service.DataSet.Excel
{
    public class BagOfWordsService : BaseClassificationService
    {
        public Dictionary<string, double> InverseDocumentWeight;
        public Dictionary<int, BagOfWords> GetBagOfWords(string pathClassifiedText = Constants.PathClassifiedText
            , bool reload = false)
        {
            var classifiedTextService = new ClassifiedSDGSService();
            var list = classifiedTextService.Read(pathClassifiedText, reload);
            return GetBagOfWords(list);
        }
        public Dictionary<int, BagOfWords> GetBagOfWords(string pathClassifiedText = Constants.PathClassifiedText
        , int year = 0)
        {
            throw new NotImplementedException();
        }

        private HashSet<int> _indexProcessed = new HashSet<int>();
        public List<ClassifiedText> BalanceClassifiedTexts(List<ClassifiedText> classifiedTexts, int ignoreYear, bool cleanBags)
        {
            //indexProcessed = new HashSet<int>();
            var result = new List<ClassifiedText>();
            var countClassifiedTexts = new Dictionary<int, int>();
            var amountOfClassifiedTextProcessed = new Dictionary<int, int>();
            var classifiedTextsCategorized = new Dictionary<int, List<ClassifiedText>>();
            foreach (var ct in classifiedTexts) //count lowest amount
            {
                if (ct.Year == ignoreYear)
                    continue;

                if (cleanBags && ct.GeneralCodes.Count > 1) 
                    continue; //clean the bag
                foreach (var generalCode in ct.GeneralCodes)
                {
                    if (countClassifiedTexts.ContainsKey(generalCode))
                    {
                        countClassifiedTexts[generalCode]++;
                        classifiedTextsCategorized[generalCode].Add(ct);
                    } 
                    else
                    {
                        countClassifiedTexts.Add(generalCode, 1);
                        classifiedTextsCategorized.Add(generalCode, new List<ClassifiedText>());
                        classifiedTextsCategorized[generalCode].Add(ct);
                    }

                }
            }

            var lowestCount = countClassifiedTexts.OrderBy(p => p.Value).FirstOrDefault().Value;

            foreach (var countClassifiedText in countClassifiedTexts)
            {
                amountOfClassifiedTextProcessed.Add(countClassifiedText.Key, 0);
            }

            foreach (var categorizedTexts in classifiedTextsCategorized)
            {
                var generalCode = categorizedTexts.Key;
                _indexProcessed = new HashSet<int>();
                for (var i = 0; i < categorizedTexts.Value.Count; i++) 
                {
                    if (_indexProcessed.Count >= (categorizedTexts.Value.Count-1)) break;
                    var index = RandomIndex(0, categorizedTexts.Value.Count - 1);
                    _indexProcessed.Add(index);

                    var ct = categorizedTexts.Value[index];
                    if (ct.Year == ignoreYear) continue;
                    if (cleanBags && ct.GeneralCodes.Count > 1) continue; //clean the bag

                    var takeIn = true;
                    if (amountOfClassifiedTextProcessed[generalCode] >= (lowestCount-1)) 
                        takeIn = false;

                    if (takeIn)
                    {
                        result.Add(ct);
                        amountOfClassifiedTextProcessed[generalCode]++;
                    }
                }
            }

            var check = result.Count/(lowestCount-1);
            if (check != countClassifiedTexts.Count)//test by multiplying the lowest amount by total different general codes.
            {
                Console.WriteLine("Bags not balanced");
                //return null; //ERROR!
            }
            
            return result;
        }

        //private Random rnd = new Random();
        private int RandomIndex(int minIndex, int maxIndex)
        {
            //var rnd = new Random();
            var index = RandomProvider.GetThreadRandomInt(minIndex, maxIndex);// rnd.Next(minIndex, maxIndex);
            while (_indexProcessed.Contains(index))
            {
                index = RandomProvider.GetThreadRandomInt(minIndex, maxIndex);
            }

            return index;
        }

        public Dictionary<int, BagOfWords> GetBagOfWords(List<ClassifiedText> classifiedTexts, int year = -1, bool cleanBags = false)
        {
            throw new NotImplementedException();
        }

        public Dictionary<int, BagOfWords> GetBagOfWords(List<ClassifiedText> classifiedTexts)
        {
            var result = new Dictionary<int, BagOfWords>();

            //classifiedTexts = BalanceClassifiedTexts(classifiedTexts, ignoreYear, cleanBags); //balance bags

            //Load classified text
            foreach (var ct in classifiedTexts)
            {                
                //if (cleanBags && ct.GeneralCodes.Count > 1) continue; //clean the bag

                //find dataset
                BagOfWords bag = null; 
                
                foreach (var generalCode in ct.GeneralCodes)
                {
                    if (result.ContainsKey(generalCode))
                        bag = result[generalCode]; 

                    if (bag == null)
                    {
                        bag = new BagOfWords { GeneralCode = generalCode };
                        result.Add(generalCode, bag);
                    }

                    bag.Text.AppendLine(ct.PreparedText);
                    bag.CountSentences++;
                    foreach (var word in ct.PreparedWords)
                    {
                        Word w = null;
                        if(bag.Words.ContainsKey(word))
                            w = bag.Words[word];

                        if(w == null)
                        {
                            w = new Word {Count = 0, Text = word};
                            bag.Words.Add(word, w);
                        }
                        w.Count++;     
                    }

                    foreach (var uniqueWord in ct.UniquePreparedWords)
                    {
                        if (bag.WordInSentences.ContainsKey(uniqueWord))
                        {
                            bag.WordInSentences[uniqueWord]++;
                        }
                        else
                        {
                            bag.WordInSentences.Add(uniqueWord, 1);
                        }


                    }
                }
            }
            TfIdfService.GetTfIdfValues(result);
            //InverseDocumentWeight = TfIdfService.GetTfIdfValues(result);
            //ChiSquaredService.GetWeights(bagOfWords);

            //UseMostOcurringWordsPerBag(result);
            //UseHighestScoringWordsPerBag(result);
            //UniqueWordsToUsePerBag(result);
            result = result.OrderBy(pair => pair.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            return result;
        }

        public Dictionary<int[], BagOfWordsCombinedLabels> GetBagOfWordsCombinedLabels(List<ClassifiedText> classifiedTexts, bool includeTfIdf, int ignoreYear = 0)
        {
            //TODO: OUTDATED!!!
            var result = new Dictionary<int[], BagOfWordsCombinedLabels>(new ArrayValueComparer<int>());

            //Load classified text
            foreach (var ct in classifiedTexts)
            {
                if (ct.Year == ignoreYear)
                    continue;

                //find dataset
                BagOfWordsCombinedLabels bag = null;
                var generalCodes = ct.GeneralCodes.OrderBy(p => p).ToArray();
                if (result.ContainsKey(generalCodes))
                    bag = result[generalCodes];

                if (bag == null)
                {
                    bag = new BagOfWordsCombinedLabels { GeneralCodes = generalCodes };
                    result.Add(generalCodes, bag);
                }

                bag.Text.AppendLine(ct.PreparedText);

                foreach (var word in ct.PreparedWords)
                {
                    Word w = null;
                    if (bag.Words.ContainsKey(word))
                        w = bag.Words[word];

                    if (w == null)
                    {
                        w = new Word { Count = 0, Text = word };
                        bag.Words.Add(word, w);
                    }
                    w.Count++;
                }
            }

            return result;
        }

        private readonly HashSet<string> _vocabulary = new HashSet<string>(); //For words to use...
        private void UniqueWordsToUsePerBag(IReadOnlyDictionary<int, BagOfWords> bags)
        {
            foreach (var bag in bags)
            {
                foreach(var word in bag.Value.WordInSentences)
                {
                    if (!_vocabulary.Contains(word.Key))
                        _vocabulary.Add(word.Key);
                }
            }

            foreach (var word in _vocabulary)
            {
                var highestScore = -1.0;
                var highestScoreBag = -1;
                foreach (var bag in bags)
                {
                    if(bag.Value.TermWeigths.ContainsKey(word) && bag.Value.TermWeigths[word] > 0)
                    {
                        if (bag.Value.TermWeigths[word] > highestScore)
                        {
                            highestScore = bag.Value.TermWeigths[word];
                            highestScoreBag = bag.Key;
                        }
                    }
                }

                if(highestScoreBag != -1)
                {
                    bags[highestScoreBag].WordsToUse.Add(word);
                }
            }
        }

        private int _k = 500;
        private void UseMostOcurringWordsPerBag(Dictionary<int, BagOfWords> bags)
        {
            foreach (var bag in bags)
            {
                var result = bag.Value.WordInSentences.OrderByDescending(p => p.Value);
                foreach (var r in result.Take(_k))
                {
                    bag.Value.WordsToUse.Add(r.Key);
                }
            }
        }
        private void UseHighestScoringWordsPerBag(Dictionary<int, BagOfWords> bags)
        {
            foreach (var bag in bags)
            {
                var result = bag.Value.TermWeigths.OrderByDescending(p => p.Value);
                foreach (var r in result.Take(_k))
                {
                    bag.Value.WordsToUse.Add(r.Key);
                }
            }
        }
    }
}
