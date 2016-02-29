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
    public class ClassificationWordWeightsService : BaseClassificationService
    {
        public double minimumConfidence = 0.3;

        public void OptimizeMinimumConfidence()
        {
            const int startQueenSpeech = 2000;
            const int endQueenSpeech = 2010;
            var clasifiedService = new ClassifiedQueenSpeechService();
            var classifiedTexts = clasifiedService.Read();

            var startOfMinimumConfidence = 0.0;
            var endOfMinimumConfidence = 1.0;
            var bestMinimumConfidence = 0.3;
            var bestAvg = 0.55987154880800316;
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
                var count = ct.Count();
                var listAverage = new List<double>();
                //Test it against the range
                for (var j = startQueenSpeech; j <= endQueenSpeech; j++)
                {
                    var actual = ct.Where(p => p.Year == j).ToList();
                    var bagOfWords = bagOfWordsService.GetBagOfWords(ct, j);
                    TfIdfService.GetTfIdfValues(bagOfWords);
                    actual = Classify(actual, bagOfWords);

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
                var avg = total/listAverage.Count;
                if(avg > bestAverage)
                {
                    bestAverage = avg;
                    bestYear = i;
                }
            }
        }

        //public string ClassifyRange(int startQueenSpeech = 2000, int endQueenSpeech = 2010)
        //{
        //    //const int startQueenSpeech = 2000;
        //    //const int endQueenSpeech = 2010;
        //    var clasifiedService = new ClassifiedQueenSpeechService();
        //    var classifiedTexts = clasifiedService.Read();
        //    var bagOfWordsService = new BagOfWordsService();
        //    var result = new StringBuilder();

        //    result.AppendLine("<html><head><meta charset=\"utf-8\"></head><body>");
        //    for (var i = startQueenSpeech; i <= endQueenSpeech; i++)
        //    {
        //        Console.WriteLine("Working on year: {0}", i);
        //        result.AppendLine(ResultsToHtml(i, classifiedTexts, bagOfWordsService.GetBagOfWords(classifiedTexts, false, ignoreYear: i)));
        //    }
        //    result.AppendLine("</body></html>");
        //    return result.ToString();
        //}

        public List<ClassifiedText> Classify(List<ClassifiedText> classifiedText, IDictionary<int, BagOfWords> bagOfWords)
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
            //var total = 0.0;
            foreach (var bagOfWords in bags)
            {
                var prediction = new Prediction
                                     {
                                         GeneralCode = bagOfWords.Value.GeneralCode
                                     };
                var score = 0.0;
                foreach (var word in classifiedText.PreparedWords)
                {
                    if (bagOfWords.Value.Words.ContainsKey(word))
                    {
                        var scoreWord = ((double) bagOfWords.Value.Words[word].Count/GetTotal(bags, word));
                        var typeWeight = GetWordTypeWeight(word);
                        //total += typeWeight;
                        scoreWord = scoreWord*typeWeight;
                        score += scoreWord;
                        if (!prediction.WordScore.ContainsKey(word))
                            prediction.WordScore.Add(word, scoreWord);
                    }
                }
                prediction.Confidence = (double)score / (double)total;
                if (total == 0.0 && score == 0.0)
                    prediction.Confidence = 0.0;

                //if (double.IsNaN(prediction.Confidence))
                //    Console.WriteLine("NaN!");

                if(prediction.WordScore.Any())
                    prediction.Confidence = prediction.WordScore.Max(p => p.Value);
                list.Add(prediction.GeneralCode, prediction);
            }
            var result = list.OrderByDescending(p => p.Value.Confidence).ToDictionary(pair => pair.Key, pair => pair.Value);
            result = L2Normalization.Normalize(result).Where(p => p.Value.Confidence >= minimumConfidence).ToDictionary(pair => pair.Key, pair => pair.Value); // list.OrderByDescending(p => p.Confidence).ToList();;
            return result;
        }

        private double GetTotal(IDictionary<int, BagOfWords> bags, string word)
        {
            var total = 0.0;
            foreach (var bag in bags)
            {
                if (bag.Value.Words.ContainsKey(word))
                    total += bag.Value.Words[word].Count;
            }
            return total;
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

        public double GetResults(int startYear, int endYear, List<ClassifiedText> classifiedTexts)
        {
            var listAverage = new List<double>();
            var bagOfWordsService = new BagOfWordsService();
            for (int i = startYear; i <= endYear; i++)
            {
                var actual = classifiedTexts.Where(p => p.Year == i).ToList();
                var bagOfWords = bagOfWordsService.GetBagOfWords(classifiedTexts, i);
                TfIdfService.GetTfIdfValues(bagOfWords);
                Classify(actual, bagOfWords);

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

        //public string ResultsToHtml(int year, List<ClassifiedText> classifiedTexts, IDictionary<int, BagOfWords> bagOfWords = null)
        //{
        //    var actual = classifiedTexts.Where(p => p.Year == year).ToList();
        //    actual = Classify(actual, year, bagOfWords);

        //    var strB = new StringBuilder();
        //    strB.AppendLine(string.Format("<h2>{0}</h2>", year));
        //    strB.AppendLine("<table><tr><th>Sentence</th><th>Cosine similarity</th></tr>");
        //    var totalSimilarity = 0.0;
        //    foreach (var a in actual)
        //    {
        //        // instantiate new similarity class
        //        var sim = new CosineSimilarity();
        //        // create two vectors for inputs
        //        var p = InitVector(bagOfWords);
        //        var bags = InitBags(bagOfWords);
        //        foreach (var prediction in a.Predictions)
        //        {
        //            p[Array.IndexOf(bags, prediction.Value.GeneralCode)] = (prediction.Value.Confidence);
        //        }
        //        var q = InitVector(bagOfWords);
        //        foreach (var generalCode in a.GeneralCodes)
        //        {
        //            q[Array.IndexOf(bags, generalCode)] = ((double)1 / (double)a.GeneralCodes.Count);
        //        }
        //        // get similarity between the two vectors
        //        double similarityScore = sim.GetSimilarityScore(p, q);
        //        if(double.IsNaN(similarityScore))
        //            Console.WriteLine("NaN!");
        //        totalSimilarity += similarityScore;

        //        strB.AppendLine(string.Format("<tr><td><b>{0}...</b></td><td><b>{1}</b></td></tr>",
        //                  a.Text.Left(30), similarityScore));
        //    }
        //    strB.AppendLine("</table>");

        //    strB.AppendFormat("<h3>Average similarity: {0}, total: {1}</h3>", (double)totalSimilarity / (double)actual.Count, actual.Count);

        //    return strB.ToString();
        //}
        private static double[] InitVector(IEnumerable<KeyValuePair<int, BagOfWords>> bagOfWords)
        {
            var result = new List<double>();
            for (var i = 0; i < bagOfWords.Count(); i++)
            {
                result.Add(0.0);
            }
            //foreach (var bag in bagOfWords)
            //{
                
            //}
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
