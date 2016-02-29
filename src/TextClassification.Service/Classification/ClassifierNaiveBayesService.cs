using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AForge.Math.Metrics;
using TextClassification.Model;
using TextClassification.Service.DataSet.Excel;
using TextClassification.Service.Dataset.Excel;

namespace TextClassification.Service.Classification
{
    public class ClassifierNaiveBayesService : BaseClassificationService
    {
        public int TopFeatures = 4; //4
        public int TopScoringWords = 99; //NOT USED!!!!
        public double minimumConfidence = 0.0; //yes, 0 gives the best result

        public void OptimizeMinimumConfidence()
        {
            const int startQueenSpeech = 2000;
            const int endQueenSpeech = 2010;
            var clasifiedService = new ClassifiedQueenSpeechService();
            var classifiedTexts = clasifiedService.Read();

            var startOfMinimumConfidence = 0.0;
            var endOfMinimumConfidence = 1.0;
            var bestMinimumConfidence = 0.0;
            var bestAvg = 0.0;// 0.45299826157169;
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

        public void OptimizeFeatureSelection()
        {
            const int startQueenSpeech = 2000;
            const int endQueenSpeech = 2010;
            var clasifiedService = new ClassifiedQueenSpeechService();
            var classifiedTexts = clasifiedService.Read();

            var startOfTopFeatures = 1;
            var endOfTopFeatures = 50;
            var bestTopFeatures = 4;
            var bestAvg = 0.0;// 0.55376389229181389;
            for (var i = startOfTopFeatures; i <= endOfTopFeatures; i++)
            {
                TopFeatures = i;
                Console.WriteLine("Testing top feature value: {0}", i);
                var result = GetResults(startQueenSpeech, endQueenSpeech, classifiedTexts);
                Console.WriteLine("Result for top feature of {0}. With an average of: {1}", i, result);
                if (result > bestAvg)
                {
                    Console.WriteLine("New best top feature value: {0}. With average of: {1}", i, result);
                    bestTopFeatures = i;
                    bestAvg = result;
                }
            }
        }

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

            foreach (var bagOfWords in bags)
            {
                var prediction = new Prediction
                {
                    GeneralCode = bagOfWords.Value.GeneralCode
                };
                //foreach (var word in classifiedText.TermWeights.OrderByDescending(p => p.Value).Take(TopScoringWords))
                //{
                //    if (!(GetWordTypeWeight(word.Key ) > 0)) continue; //PoS

                //    var wordCount = 1;
                //    if (bagOfWords.Value.WordInSentences.ContainsKey(word.Key))
                //        wordCount += bagOfWords.Value.WordInSentences[word.Key];

                //    if (wordCount == 0)
                //        continue; //error

                //    var sentenceCount = bagOfWords.Value.CountSentences + classifiedText.UniquePreparedWords.Count;

                //    var scoreWord = (double)wordCount / sentenceCount;

                //    if (!prediction.WordScore.ContainsKey(word.Key))
                //        prediction.WordScore.Add(word.Key, scoreWord);
                //}
                foreach (var word in classifiedText.UniquePreparedWords)
                {
                    //if (!bagOfWords.Value.WordsToUse.Contains(word)) //TODO: this doesn't work...
                    //    continue; //skip word

                    if (!(GetWordTypeWeight(word) > 0)) continue; //PoS

                    var wordCount = 1;
                    if (bagOfWords.Value.WordInSentences.ContainsKey(word))
                        wordCount += bagOfWords.Value.WordInSentences[word];

                    if (wordCount == 0)
                        continue; //error

                    var sentenceCount = bagOfWords.Value.CountSentences + classifiedText.UniquePreparedWords.Count;

                    var scoreWord = (double)wordCount / sentenceCount;

                    if (!prediction.WordScore.ContainsKey(word))
                        prediction.WordScore.Add(word, scoreWord);

                    //var totalWordsOccurrenceInBag = 1.0;
                    //if (bagOfWords.Value.Words.ContainsKey(word))
                    //    totalWordsOccurrenceInBag += bagOfWords.Value.Words[word].Count;

                    //var totalWordsInBag = GetTotalInBag(bagOfWords.Value);
                    //var totalWordOccurrence = GetTotal(bags, word);

                    //var divideBy = totalWordsInBag + totalWordOccurrence;// ;+ classifiedText.UniquePreparedWords.Count

                    //var scoreWord = totalWordsOccurrenceInBag / divideBy;

                    //if (!prediction.WordScore.ContainsKey(word))
                    //    prediction.WordScore.Add(word, scoreWord);
                }

                //var confidence = 0.0; //without exentension
                var confidence = bagOfWords.Value.CountSentences/GetTotalSentences(bags); //with extension
                foreach (var wordScore in prediction.WordScore.OrderByDescending(p => p.Value).Take(TopFeatures))
                {
                    if (!(wordScore.Value > 0)) continue;

                    if (confidence == 0.0)
                    {
                        confidence = wordScore.Value;
                        continue;
                    }

                    confidence = confidence*wordScore.Value;
                }

                if (prediction.WordScore.Any())
                    prediction.Confidence = confidence;
                list.Add(prediction.GeneralCode, prediction);
            }
            var result = list.OrderByDescending(p => p.Value.Confidence).ToDictionary(pair => pair.Key, pair => pair.Value);
            //var result2 = L2Normalization.Normalize(result);
            result = SumNormalization.Normalize(result);
            result = result.Where(p => p.Value.Confidence >= minimumConfidence).ToDictionary(pair => pair.Key,
                                                                                 pair => pair.Value);
            return result;
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
                "<p>The results below are based on a predifined list per category. The list . The Naive Bayes classifier has been used for the results." +
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
        public double GetResults(int startYear, int endYear, List<ClassifiedText> classifiedTexts)
        {
            var listAverage = new List<double>();
            var bagOfWordsService = new BagOfWordsService();
            for (int i = startYear; i <= endYear; i++)
            {
                var actual = classifiedTexts.Where(p => p.Year == i).ToList();
                var bagOfWords = bagOfWordsService.GetBagOfWords(classifiedTexts, i, false); //no tfidf value
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
                var avgSimilarity = totalSimilarity / actual.Count;
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
            return result;
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
                var bagOfWords = bagOfWordsService.GetBagOfWords(classifiedTexts, i);

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

        private double GetTotal(IEnumerable<KeyValuePair<int, BagOfWords>> bags, string word)
        {
            var total = 0.0;
            foreach (var bag in bags)
            {
                if (bag.Value.Words.ContainsKey(word))
                    total += bag.Value.Words[word].Count;
            }
            return total;
        }

        private double GetTotalInBag(BagOfWords bag)
        {
            var total = 0.0;
            foreach (var word in bag.Words)
            {
                total += word.Value.Count;
            }
            return total;
        }

        private double GetTotalSentences(IEnumerable<KeyValuePair<int, BagOfWords>> bags)
        {
            var total = 0.0;
            foreach (var bag in bags)
            {
                total += bag.Value.CountSentences;
            }
            return total;
        }
    }
}
