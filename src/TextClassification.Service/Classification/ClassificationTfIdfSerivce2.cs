using System;
using System.Collections.Generic;
using System.Linq;
using AForge.Math.Metrics;
using TextClassification.Model;
using TextClassification.Service.DataSet.Excel;
using TextClassification.Service.Dataset.Excel;

namespace TextClassification.Service.Classification
{
    public class ClassificationTfIdfSerivce2 : BaseClassificationService
    {
        public int n = 8;
        public void OptimizeN()
        {
            var classifiedService = new ClassifiedQueenSpeechService();
            var classifiedTexts = classifiedService.Read();

            var startOfN = 1;
            var endOfN = 50;
            var bestN = 8;
            var bestAvg = 0.28820061981811157;
            for (int i = startOfN; i <= endOfN; i++)
            {
                n = i;
                Console.WriteLine("Testing n value: {0}", i);
                var result = GetResult(classifiedTexts);

                if (result > bestAvg)
                {
                    Console.WriteLine("New best n value: {0}. With average of: {1}", i, result);
                    bestN = i;
                    bestAvg = result;
                }
            }
        }

        public double ClassifyRange()
        {
            var classifiedService = new ClassifiedQueenSpeechService();
            var classifiedTexts = classifiedService.Read();

            //TODO some optimizing here...
            var avg = GetResult(classifiedTexts);
            return avg;
        }

        public List<ClassifiedText> Classify(List<ClassifiedText> classifiedText, Dictionary<int[], BagOfWordsCombinedLabels> bagOfWords)
        {
            foreach (var text in classifiedText)
            {
                text.Predictions2 = Classify(bagOfWords, text);
            }

            return classifiedText;
        }

        public Dictionary<int[], Prediction> Classify(IDictionary<int[], BagOfWordsCombinedLabels> bags, ClassifiedText classifiedText)
        {
            var list = new Dictionary<int[], Prediction>();
            var total = classifiedText.PreparedWords.Count;
            var totalBags = bags.Count;
            foreach (var bagOfWords in bags)
            {
                var prediction = new Prediction
                {
                    GeneralCodes = bagOfWords.Value.GeneralCodes
                };
                var score = 0.0;
                foreach (var word in classifiedText.PreparedWords)
                {
                    if (bagOfWords.Value.Words.ContainsKey(word))
                    {
                        var scoreWord = FindWeight(bagOfWords.Value.TfIdf, word, totalBags);
                        score += scoreWord;
                        if (!prediction.WordScore.ContainsKey(word))
                            prediction.WordScore.Add(word, scoreWord);
                    }
                }
                prediction.Confidence = (double)score / (double)total;
                list.Add(prediction.GeneralCodes, prediction);
            }
            
            var result = list.OrderByDescending(p => p.Value.Confidence).Take(n).ToDictionary(pair => pair.Key, pair => pair.Value); // list.OrderByDescending(p => p.Confidence).ToList();
            
            result = L2Normalization.Normalize(result);
            
            return result;
        }

        public double GetResult(List<ClassifiedText> classifiedTexts)
        {
            var startYear = 2000;
            var endYear = 2010;
            //var classifiedService = new ClassifiedQueenSpeechService();
            //var classifiedTexts = classifiedService.Read();
            var bagOfWordsService = new BagOfWordsService();

            var listAverage = new List<double>();
            for (int i = startYear; i <= endYear; i++)
            {
                var actual = classifiedTexts.Where(p => p.Year == i).ToList();
                var bagOfWords = bagOfWordsService.GetBagOfWordsCombinedLabels(classifiedTexts, true, i);
                actual = Classify(actual, bagOfWords);

                var totalSimilarity = 0.0;
                foreach (var a in actual)
                {
                    // instantiate new similarity class
                    var sim = new CosineSimilarity();
                    // create two vectors for inputs
                    double[] p; // = InitVector(bagOfWords);
                    double[] q;

                    VectorService.CreateVectors(a.Predictions2, a.GeneralCodes, out p, out q);

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

        private double FindWeight(Dictionary<string, double> weights, string word, int totalBags)
        {
            if (weights.ContainsKey(word))
                return weights[word];

            return 1.0 / (double)totalBags;
        }
    }
}
