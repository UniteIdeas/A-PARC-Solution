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
    public class ClassifierTFIDFTweakedService : BaseClassificationService
    {
        private const double MinimumConfidence = 0.3; //0.3 for tfidf values
        private double _minimumAverageWordWeight = 0.01;
        private const double MinimumHighScoringWordWeight = 0.125;

        public void OptimizeMinimumAverageWordWeight()
        {
            const int startQueenSpeech = 2000;
            const int endQueenSpeech = 2010;
            var clasifiedService = new ClassifiedQueenSpeechService();
            var classifiedTexts = clasifiedService.Read();

            var start = 0.001;
            var end = 0.02;
            var best = 0.0;
            var bestAvg = 0.0;
            for (var i = start; i <= end; i += 0.001)
            {
                _minimumAverageWordWeight = i;
                Console.WriteLine("Testing minimum average word weight: {0}", i);
                var result = GetResults(startQueenSpeech, endQueenSpeech, classifiedTexts);

                if (result > bestAvg)
                {
                    Console.WriteLine("New best: {0}. With average of: {1}", i, result);
                    best = i;
                    bestAvg = result;
                }
            }
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

                foreach (var word in classifiedText.PreparedWords)
                {
                    if (bagOfWords.Value.TermWeigths.ContainsKey(word))
                    {
                        var scoreWord = FindWeight(bagOfWords.Value.TermWeigths, word, totalBags) * GetWordTypeWeight(word);
                        score += scoreWord;

                        if (!prediction.WordScore.ContainsKey(word))
                            prediction.WordScore.Add(word, scoreWord);
                    }
                }

                prediction.Confidence = (double)score / (double)total;
                list.Add(prediction.GeneralCode, prediction);
            }

            list = L2Normalization.Normalize(list).OrderByDescending(p => p.Value.Confidence).ToDictionary(pair => pair.Key,
                                                                                             pair => pair.Value);
            
            //Check per category if there is one very high scoring word in the sentence
            var result = new Dictionary<int, Prediction>();
            foreach (var prediction in list)
            {
                if(!prediction.Value.WordScore.Any())
                    continue;
                var maxValue = prediction.Value.WordScore.Max(p => p.Value);
                if (maxValue >= MinimumHighScoringWordWeight)
                    result.Add(prediction.Key, prediction.Value);
            }

            list = list.Where(p => p.Value.Confidence >= MinimumConfidence).OrderByDescending(p => p.Value.Confidence).ToDictionary(pair => pair.Key,
                                                                                 pair => pair.Value);

            
            //Check if 
            foreach (var prediction in list)
            {
                var sum = prediction.Value.WordScore.Sum(p => p.Value);
                var average = sum / total;
                
                if ((average) >= _minimumAverageWordWeight && !result.ContainsKey(prediction.Key) )
                    result.Add(prediction.Key, prediction.Value);
            }
            
            if(!result.Any())
                result.Add(0, new Prediction {Confidence = 1, GeneralCode = 0});

            return result;
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
                var bagOfWords = bagOfWordsService.GetBagOfWords(classifiedTexts, i, false);

                Classify(actual, bagOfWords);

                var totalSimilarity = 0.0;
                foreach (var a in actual)
                {
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
        public Dictionary<int, Statistics> GetResultsPerCategory()
        {
            const int startQueenSpeech = 2000;
            const int endQueenSpeech = 2010;
            var clasifiedService = new ClassifiedQueenSpeechService();
            var classifiedTexts = clasifiedService.Read();

            var result = GetResultsPerCategory(startQueenSpeech, endQueenSpeech, classifiedTexts);

            return result;
        }

        private int _sentenceCount = 0;
        private int _dontKnowCount = 0;
        public Dictionary<int, Statistics> GetResultsPerCategory(int startYear, int endYear, List<ClassifiedText> classifiedTexts)
        {
            var bagOfWordsService = new BagOfWordsService();
            var result = new Dictionary<int, Statistics>();
            for (int i = startYear; i <= endYear; i++)
            {
                var actual = classifiedTexts.Where(p => p.Year == i).ToList();
                var bagOfWords = bagOfWordsService.GetBagOfWords(classifiedTexts, i, false);

                Classify(actual, bagOfWords);

                foreach (var a in actual)
                {
                    _sentenceCount++;
                    if(a.Predictions.Count == 1 && a.Predictions.ContainsKey(0) && !a.Predictions[0].WordScore.Any())
                    {
                        _dontKnowCount++;
                        continue;
                    }
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
                 
                }
                
            }
            result = result.OrderBy(p => p.Key).ToDictionary(p => p.Key, p => p.Value);
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

        public string GetHtml(List<ClassifiedText> classifiedTexts, string name)
        {
            var sb = new StringBuilder();
            var countPredictions = new Dictionary<int, int>()
            {
                {1, 0},{2, 0},{3, 0},{4, 0},{5, 0},{6, 0},{7, 0},{8, 0},{9, 0},{10, 0},{11, 0},{12, 0},{13, 0},{14, 0},{15, 0},{16, 0},{17, 0},
            };
            var countRelations = new Dictionary<string, int>();
            
            sb.AppendLine("<html><head><meta charset=\"utf-8\"><style> table { border-collapse:collapse; } table, td, th { border:1px solid black; }</style></head><body>");
            sb.AppendLine(string.Format("<h1>{0}</h1>", name));
            sb.AppendLine(
                "<p>The TF-IDF algorithm has been used for the results. While using a minimum score of 0.3 and minimum average word weight of 0.01." +
                " If the score per category doesn't contain any scores it has been left out in the overview. Keep in mind that the term weight values" +
                " are multiplied based on their PoS value. Only the PoS tags verb, noun, adjunctive and SPEC have a PoS value of 1. The rest have a value of 0.</p>");
            foreach (var a in classifiedTexts)
            {
                sb.AppendLine("<div>");
                sb.AppendLine(string.Format("<h3>{0}</h3>", a.Text));
                
                foreach (var prediction in a.Predictions)
                {
                    if(prediction.Value.GeneralCode != 0)
                        countPredictions[prediction.Value.GeneralCode] = countPredictions[prediction.Value.GeneralCode]+1;
                    sb.AppendLine(string.Format("<p><b>Label: {0}, with a score of: {1}</b>",
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

                //count predictions
                var items = a.Predictions.Select(d => d.Value).OrderBy(d => d.GeneralCode).ToList();
                for (var i = 0; i < a.Predictions.Count();i++)
                {
                    var currentPrediction = items[i];
                    if(currentPrediction.GeneralCode == 0) continue;
                    if ((i+1) == items.Count()) break;

                    for (var j = (i+1); j < items.Count(); j++)
                    {
                        var otherPrediction = items[j];
                        if (otherPrediction.GeneralCode == 0) continue;
                        var key = string.Format("{0},{1}", currentPrediction.GeneralCode, otherPrediction.GeneralCode);
                        if (countRelations.ContainsKey(key))
                        {
                            countRelations[key] = countRelations[key] + 1;
                        }
                        else
                        {
                            countRelations.Add(key, 1);
                        }
                    }
                }

                sb.AppendLine("</div>");
            }

            //print count predictions
            sb.AppendLine("<p><table><tr><th>SDG</th><th>COUNT</th></tr>");
            foreach (var countPrediction in countPredictions)
            {
                sb.AppendLine(string.Format("<tr><td>{0}</td><td>{1}</td></tr>"
                    , countPrediction.Key, countPrediction.Value));
            }
            sb.AppendLine("</table></p>");

            //print count relations
            sb.AppendLine("<p><table><tr><th>Relation</th><th>COUNT</th></tr>");
            foreach (var countRelation in countRelations.OrderBy(p => p.Key))
            {
                sb.AppendLine(string.Format("<tr><td>{0}</td><td>{1}</td></tr>"
                    , countRelation.Key, countRelation.Value));
            }
            sb.AppendLine("</table></p>");

            sb.AppendLine(string.Format("</body></html>"));

            return sb.ToString();
        }

        public string GetWordScoreResults(int year, List<ClassifiedText> classifiedTexts)
        {
            var sb = new StringBuilder();
            var bagOfWordsService = new BagOfWordsService();

            Console.WriteLine("Working on year: {0}", year);

            var actual = classifiedTexts.Where(p => p.Year == year).ToList();
            var bagOfWords = bagOfWordsService.GetBagOfWords(classifiedTexts, year, false);

            Classify(actual, bagOfWords);

            sb.AppendLine("<html><head><meta charset=\"utf-8\"><style> table { border-collapse:collapse; } table, td, th { border:1px solid black; }</style></head><body>");
            sb.AppendLine(string.Format("<h1>Year {0}</h1>", year));
            sb.AppendLine(
                "<p>The TF-IDF algorithm has been used for the results. While using a minimum score of 0.3 and minimum average word weight of 0.01." +
                " If the score per category doesn't contain any scores it has been left out in the overview. Keep in mind that the term weight values" +
                " are multiplied based on their PoS value. Only the PoS tags verb, noun, adjunctive and SPEC have a PoS value of 1. The rest have a value of 0.</p>");
            foreach (var a in actual)
            {
                sb.AppendLine("<div>");
                sb.AppendLine(string.Format("<h3>{0}</h3>", a.Text));
                sb.AppendLine(string.Format("<p>The correct label(s) are/is: {0}</p>", string.Join(", ", a.GeneralCodes)));
                foreach (var prediction in a.Predictions)
                {
                    sb.AppendLine(string.Format("<p><b>Label: {0}, with a score of: {1}</b>",
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

    }


}
