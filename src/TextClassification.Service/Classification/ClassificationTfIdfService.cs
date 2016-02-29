using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AForge.Math.Metrics;
using TextClassification.Common.Extension;
using TextClassification.Model;
using TextClassification.Service.DataSet.Excel;
using TextClassification.Service.Dataset.Excel;

namespace TextClassification.Service.Classification
{
    public class ClassificationTfIdfService : BaseClassificationService
    {
        public double minimumConfidence = 0.3; //0.3 for tfidf values
        public int TopFeatures = 99; //NOT USED!!!!!
        public int TopScoringWords = 4; //NOT USED!!!!

        public void OptimizeMinimumConfidence()
        {
            const int startQueenSpeech = 2000;
            const int endQueenSpeech = 2010;
            var clasifiedService = new ClassifiedQueenSpeechService();
            var classifiedTexts = clasifiedService.Read();
            
            //TfIdfService.GetTfIdfValues(classifiedTexts);

            var startOfMinimumConfidence = 0.0;
            var endOfMinimumConfidence = 1.0;
            var bestMinimumConfidence = 0.0;
            var bestAvg = 0.0;
            for (double i = startOfMinimumConfidence; i <= endOfMinimumConfidence; i += 0.1)
            {
                minimumConfidence = i;
                Console.WriteLine("Testing minimum confidence value: {0}", i);
                var result = GetResults(startQueenSpeech, endQueenSpeech, classifiedTexts);

                if (result > bestAvg)
                {
                    Console.WriteLine("New best minimum confidence value: {0}. With average of: {1}", i, result);
                    bestMinimumConfidence = i;
                    bestAvg = result;
                }
            }
        }

        public void OptimizeFeatureSelection()
        {
            const int startQueenSpeech = 2000;
            const int endQueenSpeech = 2010;
            var clasifiedService = new ClassifiedQueenSpeechService();
            var classifiedTexts = clasifiedService.Read();

            var startOfTopFeatures = 1;
            var endOfTopFeatures = 50;
            var bestTopFeatures = 99;
            var bestAvg = 0.0;// 0.55376389229181389;
            for (int i = startOfTopFeatures; i <= endOfTopFeatures; i++)
            {
                TopFeatures = i;
                Console.WriteLine("Testing top feature value: {0}", i);
                var result = GetResults(startQueenSpeech, endQueenSpeech, classifiedTexts);

                if (result > bestAvg)
                {
                    Console.WriteLine("New best top feature value: {0}. With average of: {1}", i, result);
                    bestTopFeatures = i;
                    bestAvg = result;
                }
            }
        }
        
        public void OptimizeFeatureSelectionTopScoringWords()
        {
            const int startQueenSpeech = 2000;
            const int endQueenSpeech = 2010;
            var clasifiedService = new ClassifiedQueenSpeechService();
            var classifiedTexts = clasifiedService.Read();
            
            TfIdfService.GetTfIdfValues(classifiedTexts);

            var startOfTopScoringWords = 1;
            var endOfTopScoringWords = 30;
            var bestTopScoringWords = 99;
            var bestAvg = 0.0;
            for (int i = startOfTopScoringWords; i <= endOfTopScoringWords; i++)
            {
                TopScoringWords = i;
                Console.WriteLine("Testing TopScoringWords value: {0}", i);
                var result = GetResults(startQueenSpeech, endQueenSpeech, classifiedTexts);

                if (result > bestAvg)
                {
                    Console.WriteLine("New bestTopScoringWords value: {0}. With average of: {1}", i, result);
                    bestTopScoringWords = i;
                    bestAvg = result;
                }
            }
        }

        public void TestYears()
        {
            const int startQueenSpeech = 2000;
            const int endQueenSpeech = 2010;
            var startFrom = 1945;
            var endFrom = 1990;
            var clasifiedService = new ClassifiedQueenSpeechService();
            var classifiedTexts = clasifiedService.Read();
            var bagOfWordsService = new BagOfWordsService();

            var bestAverage = 0.0;
            var bestYear = 0;
            for (int i = startFrom; i <= endFrom; i++)
            {
                //Remove everything before year i
                Console.WriteLine("Removing everything before: {0}", i);
                var ct = classifiedTexts.Where(p => p.Year >= i).ToList();

                var listAverage = new List<double>();
                //Test it against the range
                for (var j = startQueenSpeech; j <= endQueenSpeech; j++)
                {
                    var actual = ct.Where(p => p.Year == j).ToList();
                    var bagOfWords = bagOfWordsService.GetBagOfWords(ct, j);
                    //TfIdfService.GetTfIdfValues(bagOfWords);
                    actual = Classify(actual, bagOfWords);

                    var totalSimilarity = 0.0;
                    foreach (var a in actual)
                    {
                        // instantiate new similarity class
                        var sim = new CosineSimilarity();
                        // create two vectors for inputs
                        var p = InitVector(bagOfWords);
                        var bags = InitBags(bagOfWords);
                        var total2 = 0.0;
                        foreach (var prediction in a.Predictions)
                        {
                            total2 += prediction.Value.Confidence;
                            p[Array.IndexOf(bags, prediction.Value.GeneralCode)] = (prediction.Value.Confidence);
                        }
                        var q = InitVector(bagOfWords);
                        foreach (var generalCode in a.GeneralCodes)
                        {
                            q[Array.IndexOf(bags, generalCode)] = ((double)1 / (double)a.GeneralCodes.Count);
                        }
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
                if (avg > bestAverage)
                {
                    bestAverage = avg;
                    bestYear = i;
                }
            }
        }

        public string ClassifyRange(int startQueenSpeech = 2000, int endQueenSpeech = 2010)
        {
            //const int startQueenSpeech = 2000;
            //const int endQueenSpeech = 2010;
            var clasifiedService = new ClassifiedQueenSpeechService();
            var classifiedTexts = clasifiedService.Read();

            var bagOfWordsService = new BagOfWordsService();
            var result = new StringBuilder();

            result.AppendLine("<html><head><meta charset=\"utf-8\"></head><body>");
            for (var i = startQueenSpeech; i <= endQueenSpeech; i++)
            {
                Console.WriteLine("Working on year: {0}", i);
                var bags = bagOfWordsService.GetBagOfWords(classifiedTexts, i);
                //TfIdfService.GetTfIdfValues(bags);
                result.AppendLine(ResultsToHtml(i, classifiedTexts, bags));
            }
            result.AppendLine("</body></html>");
            return result.ToString();
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

        public Dictionary<int, Statistics> GetResultsPerCategory()
        {
            const int startQueenSpeech = 2000;
            const int endQueenSpeech = 2010;
            var clasifiedService = new ClassifiedQueenSpeechService();
            var classifiedTexts = clasifiedService.Read();

            var result = GetResultsPerCategory(startQueenSpeech, endQueenSpeech, classifiedTexts);
            
            return result;
        }
        public List<ClassifiedText> Classify(List<ClassifiedText> classifiedText, Dictionary<int, BagOfWords> bagOfWords)
        {
            foreach (var text in classifiedText)
            {
                text.Predictions = Classify(bagOfWords, text);
            }

            return classifiedText;
        }

        public Dictionary<int, Prediction> Classify(IDictionary<int, BagOfWords> bags, ClassifiedText classifiedText)
        {
            var list = new Dictionary<int, Prediction>();
            var total = classifiedText.PreparedWords.Count;
            var totalBags = bags.Count;
            foreach (var bagOfWords in bags)
            {
                var prediction = new Prediction
                {
                    GeneralCode = bagOfWords.Value.GeneralCode
                };
                var score = 0.0;
                //foreach (var word in classifiedText.TermWeights.OrderByDescending(p => p.Value).Take(TopScoringWords))
                //{
                //    if (bagOfWords.Value.Words.ContainsKey(word.Key))
                //    {
                //        var scoreWord = FindWeight(bagOfWords.Value.TermWeights, word.Key, totalBags);// *GetWordTypeWeight(word.Key);
                //        score += scoreWord;

                //        if (!prediction.WordScore.ContainsKey(word.Key))
                //            prediction.WordScore.Add(word.Key, scoreWord);
                //    }
                //}
                foreach (var word in classifiedText.PreparedWords)
                {
                    //if (bagOfWords.Key == 1 && word.Equals("economisch"))
                    //{
                    //    score += 0.33;
                    //    if (!prediction.WordScore.ContainsKey(word))
                    //        prediction.WordScore.Add(word, 0.33);
                    //}

                    //if (!bagOfWords.Value.WordsToUse.Contains(word)) //attempt to feature selection
                    //    continue; //skip word
                    if (bagOfWords.Value.TermWeigths.ContainsKey(word))
                    {
                        var scoreWord = FindWeight(bagOfWords.Value.TermWeigths, word, totalBags) * GetWordTypeWeight(word);
                        score += scoreWord;

                        if (!prediction.WordScore.ContainsKey(word))
                            prediction.WordScore.Add(word, scoreWord);
                    }
                }
                //var score = prediction.WordScore.OrderByDescending(p => p.Value).Take(TopFeatures).Sum(p => p.Value); //take top k features
                prediction.Confidence = (double)score / (double)total;
                list.Add(prediction.GeneralCode, prediction);
            }

            //var result = list.OrderByDescending(p => p.Value.Confidence).ToDictionary(pair => pair.Key, pair => pair.Value); // list.OrderByDescending(p => p.Confidence).ToList();
            //var generalCodes = classifiedText.GeneralCodes;
            var result = L2Normalization.Normalize(list).OrderByDescending(p => p.Value.Confidence).ToDictionary(pair => pair.Key,
                                                                                             pair => pair.Value);
            if (result.Any(p => p.Value.Confidence >= minimumConfidence))
            {
                result = result.Where(p => p.Value.Confidence >= minimumConfidence).OrderByDescending(p => p.Value.Confidence).ToDictionary(pair => pair.Key,
                                                                                                 pair => pair.Value);
            } 
            else if(result.Max(p => p.Value.Confidence) == 0)
            {
                var maxWords = result.Max(p => p.Value.WordScore.Count);
                result = result.Where(p => p.Value.WordScore.Count >= maxWords).OrderByDescending(p => p.Value.Confidence).ToDictionary(pair => pair.Key,
                                                                                                 pair => pair.Value);
                foreach (var prediction in result)
                {
                    prediction.Value.Confidence = (double)1/result.Count;
                }
            } 
            else
            {
                var maxConfidence = result.Max(p => p.Value.Confidence);
                result = result.Where(p => p.Value.Confidence >= maxConfidence).OrderByDescending(p => p.Value.Confidence).ToDictionary(pair => pair.Key,
                                                                                                 pair => pair.Value); 
            }
            return result;
        }

        public string ResultsToHtml(int year, List<ClassifiedText> classifiedTexts, Dictionary<int, BagOfWords> bagOfWords = null)
        {
            var actual = classifiedTexts.Where(p => p.Year == year).ToList();
            actual = Classify(actual, bagOfWords);

            var strB = new StringBuilder();
            strB.AppendLine(string.Format("<h2>{0}</h2>", year));
            strB.AppendLine("<table><tr><th>Sentence</th><th>Cosine similarity</th></tr>");
            var totalSimilarity = 0.0;
            foreach (var a in actual)
            {
                // instantiate new similarity class
                var sim = new CosineSimilarity();
                // create two vectors for inputs
                var p = InitVector(bagOfWords);
                var bags = InitBags(bagOfWords);
                foreach (var prediction in a.Predictions)
                {
                    p[Array.IndexOf(bags, prediction.Value.GeneralCode)] = (prediction.Value.Confidence);
                }
                var q = InitVector(bagOfWords);
                foreach (var generalCode in a.GeneralCodes)
                {
                    q[Array.IndexOf(bags, generalCode)] = 1; //TODO find the correct weight
                }
                // get similarity between the two vectors
                double similarityScore = sim.GetSimilarityScore(p, q);
                totalSimilarity += similarityScore;

                strB.AppendLine(string.Format("<tr><td><b>{0}...</b></td><td><b>{1}</b></td></tr>",
                          a.Text.Left(30), similarityScore));
            }
            strB.AppendLine("</table>");

            strB.AppendFormat("<h3>Average similarity: {0}, total: {1}</h3>", (double)totalSimilarity / (double)actual.Count, actual.Count);

            return strB.ToString();
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
            var bagOfWords = bagOfWordsService.GetBagOfWords(classifiedTexts, year, false);
            //TfIdfService.GetTfIdfValues(bagOfWords);
            //ChiSquaredService.GetWeights(bagOfWords);
            Classify(actual, bagOfWords);
            sb.AppendLine("<html><head><meta charset=\"utf-8\"><style> table { border-collapse:collapse; } table, td, th { border:1px solid black; }</style></head><body>");
            sb.AppendLine(string.Format("<h1>Year {0}</h1>", year));
            sb.AppendLine(
                "<p>The results below are based on a predifined list per category. The list . The TF-IDF algorithm has been used for the results." +
                " If the score per category doesn't contain any scores it has been left out in the overview. Keep in mind that the term weight values" +
                " are multiplied based on their PoS value. Only the PoS tags verb, noun, adjunctive and SPEC have a PoS value of 1. The rest have a value of 0.</p>");
            foreach (var a in actual)
            {
                sb.AppendLine("<div>");
                sb.AppendLine(string.Format("<h3>{0}</h3>", a.Text));
                sb.AppendLine(string.Format("<p>The correct label(s) are/is: {0}</p>", string.Join(", ", a.GeneralCodes)));
                foreach(var prediction in a.Predictions)
                {
                    sb.AppendLine(string.Format("<p><b>Label: {0}, with a confidence of: {1}</b>",
                                                prediction.Value.GeneralCode, prediction.Value.Confidence));
                    if (!prediction.Value.WordScore.Any())
                        continue;
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

        public double GetResults(int startYear, int endYear, List<ClassifiedText> classifiedTexts)
        {
            var listAverage = new List<double>();
            var bagOfWordsService = new BagOfWordsService();
            for (int i = startYear; i <= endYear; i++)
            {
                //Console.WriteLine("Working on year: {0}", i);

                var actual = classifiedTexts.Where(p => p.Year == i).ToList();
                var bagOfWords = bagOfWordsService.GetBagOfWords(classifiedTexts, i, false);
                //TfIdfService.GetTfIdfValues(bagOfWords);
                //ChiSquaredService.GetWeights(bagOfWords);
                Classify(actual, bagOfWords);

                //var x = bagOfWords[1].TermWeights["economisch"];
                var totalSimilarity = 0.0;
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

                    if(a.GeneralCodes.Count == 1)
                    {
                        totalSimilaritySingle += similarityScore;
                    } 
                    else if(a.GeneralCodes.Count > 1)
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

        public Dictionary<int, Statistics> GetResultsPerCategory(int startYear, int endYear, List<ClassifiedText> classifiedTexts)
        {
            //var listAverage = new List<double>();
            var bagOfWordsService = new BagOfWordsService();
            var result = new Dictionary<int, Statistics>();
            for (int i = startYear; i <= endYear; i++)
            {
                var actual = classifiedTexts.Where(p => p.Year == i).ToList();
                var bagOfWords = bagOfWordsService.GetBagOfWords(classifiedTexts, i, false);

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
                        if(!result.ContainsKey(generalCode))
                            result.Add(generalCode, new Statistics {Count = 0, Value = 0.0});

                        result[generalCode].Value += similarityScore;
                        result[generalCode].Count++;
                    }
                    //totalSimilarity += similarityScore;
                }
                //var avgSimilarity = (double)totalSimilarity / (double)actual.Count;
                //listAverage.Add(avgSimilarity);
            }
            result = result.OrderBy(p => p.Key).ToDictionary(p => p.Key, p => p.Value);
            return result;
        }

        private static double[] InitVector(Dictionary<int, BagOfWords> bagOfWords)
        {
            var result = new List<double>();
            for (var i = 0; i < bagOfWords.Count(); i++)
            {
                result.Add(0.0);
            }
            return result.ToArray();
        }
        private static int[] InitBags(IEnumerable<KeyValuePair<int, BagOfWords>> bagOfWords)
        {
            var result = new List<int>();
            foreach (var bag in bagOfWords)
            {
                result.Add(bag.Key);
            }
            return result.ToArray();
        }


    }
}
