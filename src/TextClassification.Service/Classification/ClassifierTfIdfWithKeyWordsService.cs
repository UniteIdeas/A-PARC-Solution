using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AForge.Math.Metrics;
using Polenter.Serialization;
using TextClassification.Model;
using TextClassification.Service.DataSet.Excel;
using TextClassification.Service.Dataset.Excel;

namespace TextClassification.Service.Classification
{
    public class ClassifierTfIdfWithKeyWordsService : BaseClassificationService
    {
        private Dictionary<int, List<string[]>> Keywords { get; set; }

        //public void ProductTrainingSets()
        //{
        //    const int startQueenSpeech = 2000;
        //    const int endQueenSpeech = 2010;
        //    var clasifiedService = new ClassifiedQueenSpeechService();
        //    var classifiedTexts = clasifiedService.Read();
        //    var bagOfWordsService = new BagOfWordsService();

        //    for (var i = startQueenSpeech; i <= endQueenSpeech; i++)
        //    {
        //        var bags = bagOfWordsService.GetBagOfWords(classifiedTexts, i);
        //        Train(classifiedTexts, bags, i);
        //    }
        //}
        private void Train(List<ClassifiedText> classifiedTexts, Dictionary<int, BagOfWords> bags, int ignoreYear = -1)
        {
            var tempKeywords = GetKeywords(ignoreYear);
            if (tempKeywords != null)
            {
                Keywords = tempKeywords;
                return;
            }
            //ignore sentences that are mutliple labeled? -> No, there are different values for different bags...

            //remove overlapping keywords in other categories?
            
            InitializeKeywords(bags);

            foreach (var ct in classifiedTexts)
            {
                if(ct.Year == ignoreYear) continue;

                var predictions = ClassifyWithoutKeyWords(bags, ct);
                foreach (var generalCode in ct.GeneralCodes)
                {
                    for(int i = 1; i <= ct.UniquePreparedWords.Count; i++)
                    {
                        //if (i == ct.UniquePreparedWords.Count && !DoesItAlreadyExists(generalCode, ct.UniquePreparedWords.ToArray())) //if max length than take the whole sentence as keywords...
                        //    Keywords[generalCode].Add(ct.UniquePreparedWords.ToArray());

                        var correctPrediction = predictions[generalCode];

                        var wordWeights = correctPrediction.WordScore.OrderByDescending(p => p.Value).Take(i); //get top scoring words
                        var sum = wordWeights.Sum(p => p.Value);

                        var isHighest = true;

                        //check if the keywords combination scores the highest..
                        foreach (var prediction in predictions)
                        {
                            if(prediction.Key == generalCode) continue; //don't check the correct predicition....
                            
                            //Check the highest scoring words
                            //var total = prediction.Value.WordScore.OrderByDescending(p => p.Value).Take(i).Sum(p => p.Value);

                            //Check the score of the same words...
                            var total = 0.0;
                            foreach (var wordWeight in wordWeights)
                            {
                                if (prediction.Value.WordScore.ContainsKey(wordWeight.Key))
                                    total += prediction.Value.WordScore[wordWeight.Key];
                            }

                            if (total >= sum || prediction.Value.WordScore.OrderByDescending(p => p.Value).Take(i).Sum(p => p.Value) >= sum)
                            {
                                isHighest = false;
                                break;
                            }
                        }

                        if (isHighest && !DoesItAlreadyExists(generalCode, wordWeights.Select(p => p.Key).ToArray()))
                        {
                            Keywords[generalCode].Add(wordWeights.Select(p => p.Key).ToArray());
                            break;
                        }
                    }
                }
            }

            StoreKeywords(Keywords, ignoreYear);
        }

        private bool DoesItAlreadyExists(int generalCode, string[] words)
        {
            foreach (var keywords in Keywords[generalCode])
            {
                if(keywords.Count() != words.Count()) continue;

                var doesItContainAll = true;
                foreach (var word in words)
                {
                    var isIn = false;
                    foreach (var keyword in keywords)
                    {
                        if (word.Equals(keyword))
                            isIn = true;
                    }
                    if (!isIn)
                        doesItContainAll = false;
                }
                if (doesItContainAll)
                    return true;
            }

            return false;
        }
        private Dictionary<int, List<string[]>> GetKeywords(int year)
        {
            var name = string.Format("keywords_{0}.xml", year);

            //var mc = MemoryCache.Default;
            //var result = mc[name] as Dictionary<int, List<string[]>>;
            
            //if (result != null)
            //    return result;

            var serializer = new SharpSerializer();
            try
            {
                return serializer.Deserialize(string.Format("{0}", name)) as Dictionary<int, List<string[]>>;
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }
        private void StoreKeywords(Dictionary<int, List<string[]>> keywords, int year)
        {
            var name = string.Format("keywords_{0}.xml", year);
            //var mc = MemoryCache.Default;
            //mc.Set(name, keywords, DateTimeOffset.Now.AddDays(7));

            var serializer = new SharpSerializer();
            serializer.Serialize(keywords, name);
        }
        private void InitializeKeywords(Dictionary<int, BagOfWords> bags)
        {
            Keywords = new Dictionary<int, List<string[]>>();
            foreach (var bag in bags)
            {
                Keywords.Add(bag.Key, new List<string[]>());
            }
        } 
        private List<ClassifiedText> Classify(List<ClassifiedText> classifiedText, Dictionary<int, BagOfWords> bagOfWords)
        {
            foreach (var text in classifiedText)
            {
                text.Predictions = Classify(bagOfWords, text);
            }

            return classifiedText;
        }

        private Dictionary<int, Prediction> ClassifyWithoutKeyWords(IDictionary<int, BagOfWords> bags, ClassifiedText classifiedText)
        {
            var list = new Dictionary<int, Prediction>();
            var total = classifiedText.UniquePreparedWords.Count;
            var totalBags = bags.Count;
            foreach (var bagOfWords in bags)
            {
                var prediction = new Prediction
                {
                    GeneralCode = bagOfWords.Value.GeneralCode
                };
                var score = 0.0;

                foreach (var word in classifiedText.UniquePreparedWords)
                {
                    if (bagOfWords.Value.Words.ContainsKey(word))
                    {
                        var scoreWord = FindWeight(bagOfWords.Value.TermWeigths, word, totalBags) * GetWordTypeWeight(word);
                        score += scoreWord;

                        if (!prediction.WordScore.ContainsKey(word))
                            prediction.WordScore.Add(word, scoreWord);
                    }
                }

                prediction.Confidence = score / total;
                list.Add(prediction.GeneralCode, prediction);
            }
            
            return list;
        }

        private Dictionary<int, Prediction> Classify(IDictionary<int, BagOfWords> bags, ClassifiedText classifiedText)
        {
            var list = new Dictionary<int, Prediction>();
            var total = classifiedText.UniquePreparedWords.Count;
            var totalBags = bags.Count;
            foreach (var bagOfWords in bags)
            {
                var keywords = Keywords[bagOfWords.Key];
                
                var prediction = new Prediction
                {
                    GeneralCode = bagOfWords.Value.GeneralCode
                };

                var score = 0.0;

                foreach (var stringArray in keywords)
                {
                    if(!DoesItContainAllKeywords(classifiedText.UniquePreparedWords, stringArray)) 
                        continue;
                    foreach (var keyword in stringArray)
                    {
                        if (prediction.WordScore.ContainsKey(keyword) || !bagOfWords.Value.Words.ContainsKey(keyword)) 
                            continue;

                        var scoreWord = FindWeight(bagOfWords.Value.TermWeigths, keyword, totalBags) * GetWordTypeWeight(keyword);
                        score += scoreWord;

                        if (!prediction.WordScore.ContainsKey(keyword))
                            prediction.WordScore.Add(keyword, scoreWord);
                    }
                }

                prediction.Confidence = (double)score / (double)total;
                list.Add(prediction.GeneralCode, prediction);
            }


            var result = L2Normalization.Normalize(list).Where(p => p.Value.Confidence > 0).OrderByDescending(p => p.Value.Confidence).ToDictionary(pair => pair.Key,
                                                                                             pair => pair.Value);
            
            //var luceneService = new LuceneService();
            //var luceneCategories = luceneService.FindCategories(classifiedText.PreparedText);
            //foreach (var luceneCategory in luceneCategories)
            //{
            //    if (!result.ContainsKey(luceneCategory))
            //    {
            //        result.Add(luceneCategory, new Prediction { GeneralCode = luceneCategory, Confidence = 1 });
            //    }
            //    else
            //    {
            //        result[luceneCategory].Confidence = 1;
            //    }
            //}

            if(!result.Any())
            {
                result = new Dictionary<int, Prediction> {{0, new Prediction {GeneralCode = 0, Confidence = 1}}};
            }
            //if (result.Count > 1 && result.ContainsKey(0) && result.First().Key != 0)
            //    result.Remove(0);
            if (result.Count > 1 && result.First().Key == 0 && result.First().Value.Confidence > 0.95 && result.ElementAt(1).Value.Confidence != 1)
            {
                result = new Dictionary<int, Prediction> { { 0, new Prediction { GeneralCode = 0, Confidence = result.First().Value.Confidence } } };
            }
            else if (result.Count > 1 && result.ContainsKey(0))
            {
                result.Remove(0);
            }

            return result;
        }
        private bool DoesItContainAllKeywords(List<string> preparedWords, string[] keywords)
        {
            foreach (var keyword in keywords)
            {
                if (!preparedWords.Contains(keyword))
                    return false;
            }

            return true;
        }

        public double GetResults()
        {
            const int startQueenSpeech = 2000;
            const int endQueenSpeech = 2010;
            var clasifiedService = new ClassifiedQueenSpeechService();
            var classifiedTexts = clasifiedService.Read();

            var avg = GetResults(startQueenSpeech, endQueenSpeech, classifiedTexts);

            return avg;
        }

        private int _dontKnowCount = 0;
        private double GetResults(int startYear, int endYear, List<ClassifiedText> classifiedTexts)
        {
            var listAverage = new List<double>();
            var bagOfWordsService = new BagOfWordsService();
            for (int i = startYear; i <= endYear; i++)
            {
                Console.WriteLine("Working on year: {0}", i);
                var actual = classifiedTexts.Where(p => p.Year == i).ToList();
                var bagOfWords = bagOfWordsService.GetBagOfWords(classifiedTexts, i);

                Train(classifiedTexts, bagOfWords, i);

                Classify(actual, bagOfWords);

                var totalSimilarity = 0.0;
                foreach (var a in actual)
                {
                    //TODO: Implement the code below to ignore "don't know" results. Do this also for the results per category overview
                    //if (a.Predictions.Count == 1 && a.Predictions.ContainsKey(0) && !a.Predictions[0].WordScore.Any())
                    //{
                    //    _dontKnowCount++;
                    //    continue;
                    //}
                    // instantiate new similarity class
                    var sim = new CosineSimilarity();

                    // create two vectors for inputs 
                    double[] p;
                    double[] q;
                    VectorService.CreateVectors(a.Predictions, a.GeneralCodes, out p, out q);

                    // get similarity between the two vectors
                    double similarityScore = sim.GetSimilarityScore(p, q);
                    totalSimilarity += similarityScore;
                }
                var avgSimilarity = (double)totalSimilarity / (double)actual.Count;
                listAverage.Add(avgSimilarity);
            }
            var total = 0.0;
            foreach (var d in listAverage)
            {
                total += d;
            }
            var avg = total / listAverage.Count;

            return avg;
        }
        public void GetResultsSingleAndMultiple()
        {
            const int startQueenSpeech = 2000;
            const int endQueenSpeech = 2010;
            var clasifiedService = new ClassifiedQueenSpeechService();
            var classifiedTexts = clasifiedService.Read();

            var listAverageSingle = new List<double>();
            var listAverageMultiple = new List<double>();
            var bagOfWordsService = new BagOfWordsService();
            for (int i = startQueenSpeech; i <= endQueenSpeech; i++)
            {
                Console.WriteLine("Working on year: {0}", i);

                var actual = classifiedTexts.Where(p => p.Year == i).ToList();
                var bagOfWords = bagOfWordsService.GetBagOfWords(classifiedTexts, i, false);

                Train(classifiedTexts, bagOfWords, i);

                Classify(actual, bagOfWords);

                var totalSimilaritySingle = 0.0;
                var totalSimilarityMultiple = 0.0;

                foreach (var a in actual)
                {
                    // instantiate new similarity class
                    var sim = new CosineSimilarity();

                    // create two vectors for inputs 
                    double[] p;
                    double[] q;
                    VectorService.CreateVectors(a.Predictions, a.GeneralCodes, out p, out q);

                    // get similarity between the two vectors
                    var similarityScore = sim.GetSimilarityScore(p, q);

                    if (a.GeneralCodes.Count == 1)
                    {
                        totalSimilaritySingle += similarityScore;
                    }
                    else if (a.GeneralCodes.Count > 1)
                    {
                        totalSimilarityMultiple += similarityScore;
                    }
                }

                var avgSimilaritySingle = totalSimilaritySingle / actual.Count(p => p.GeneralCodes.Count == 1);
                var avgSimilarityMultiple = totalSimilarityMultiple / actual.Count(p => p.GeneralCodes.Count > 1);
                listAverageSingle.Add(avgSimilaritySingle);
                listAverageMultiple.Add(avgSimilarityMultiple);
            }
        }
        public List<string> GetWordScoreResults()
        {
            const int startQueenSpeech = 2000;
            const int endQueenSpeech = 2010;
            var clasifiedService = new ClassifiedQueenSpeechService();
            var classifiedTexts = clasifiedService.Read();
            var list = new List<string>();
            for (int i = startQueenSpeech; i <= endQueenSpeech; i++)
            {
                list.Add(GetWordScoreResults(i, classifiedTexts));
            }

            return list;
        }
        public string GetWordScoreResults(int year, List<ClassifiedText> classifiedTexts)
        {
            var sb = new StringBuilder();
            var bagOfWordsService = new BagOfWordsService();

            Console.WriteLine("Working on year: {0}", year);

            var actual = classifiedTexts.Where(p => p.Year == year).ToList();
            var bagOfWords = bagOfWordsService.GetBagOfWords(classifiedTexts, year);

            Train(classifiedTexts, bagOfWords, year);

            Classify(actual, bagOfWords);
            sb.AppendLine("<html><head><meta charset=\"utf-8\"><style> table { border-collapse:collapse; } table, td, th { border:1px solid black; }</style></head><body>");
            sb.AppendLine(string.Format("<h1>Year {0}</h1>", year));
            sb.AppendLine(
                "<p>For the results below the TF-IDF classifier using keywords is used." +
                " If the score per category doesn't contain any scores it has been left out in the overview. Keep in mind that the term weight values" +
                " are multiplied based on their PoS value. Only the PoS tags verb, noun, adjunctive and SPEC have a PoS value of 1. The rest have a value of 0.</p>");
            foreach (var a in actual)
            {
                sb.AppendLine("<div>");
                sb.AppendLine(string.Format("<h3>{0}</h3>", a.Text));
                sb.AppendLine(string.Format("<p>The correct label(s) are/is: {0}</p>", string.Join(", ", a.GeneralCodes)));
                foreach (var prediction in a.Predictions)
                {
                    if (!prediction.Value.WordScore.Any())
                        continue;

                    sb.AppendLine(string.Format("<p><b>Label: {0}, with a confidence of: {1}</b>",
                                                prediction.Value.GeneralCode, prediction.Value.Confidence));
                    sb.AppendLine("<table><tr><th>Word</th><th>Score</th><th>PoS</th></tr>");
                    foreach (var wordScore in prediction.Value.WordScore)
                    {
                        sb.AppendLine(string.Format("<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>"
                            , wordScore.Key, wordScore.Value, GetWordType(wordScore.Key)));
                    }
                    sb.AppendLine("</table></p>");
                }
                sb.AppendLine("</div>");
            }
            sb.AppendLine(string.Format("</body></html>"));

            return sb.ToString();
        }
        public Dictionary<int, Statistics> GetResultsPerCategory()
        {
            const int startQueenSpeech = 2000;
            const int endQueenSpeech = 2010;
            var clasifiedService = new ClassifiedQueenSpeechService();
            var classifiedTexts = clasifiedService.Read();

            var result = GetResultsPerCategory(startQueenSpeech, endQueenSpeech, classifiedTexts);

            return result;
        }
        public Dictionary<int, Statistics> GetResultsPerCategory(int startYear, int endYear, List<ClassifiedText> classifiedTexts)
        {
            //var listAverage = new List<double>();
            var bagOfWordsService = new BagOfWordsService();
            var result = new Dictionary<int, Statistics>();
            for (int i = startYear; i <= endYear; i++)
            {
                var actual = classifiedTexts.Where(p => p.Year == i).ToList();
                var bagOfWords = bagOfWordsService.GetBagOfWords(classifiedTexts, i);
                Train(classifiedTexts, bagOfWords, i);
                Classify(actual, bagOfWords);

                //var totalSimilarity = 0.0;
                foreach (var a in actual)
                {
                    // instantiate new similarity class
                    var sim = new CosineSimilarity();

                    // create two vectors for inputs 
                    double[] p;
                    double[] q;
                    VectorService.CreateVectors(a.Predictions, a.GeneralCodes, out p, out q);

                    // get similarity between the two vectors
                    double similarityScore = sim.GetSimilarityScore(p, q);
                    foreach (var generalCode in a.GeneralCodes)
                    {
                        if (!result.ContainsKey(generalCode))
                            result.Add(generalCode, new Statistics { Count = 0, Value = 0.0 });

                        result[generalCode].Value += similarityScore;
                        result[generalCode].Count++;
                    }
                    //totalSimilarity += similarityScore;
                }
                //var avgSimilarity = (double)totalSimilarity / (double)actual.Count;
                //listAverage.Add(avgSimilarity);
            }
            result = result.OrderBy(p => p.Key).ToDictionary(p => p.Key, p => p.Value);
            var secondResult = result.OrderBy(p => p.Key).Select(p => p.Value.Average).ToList();
            return result;
        }
    }
}
