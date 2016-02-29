using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AForge.Math.Metrics;
using TextClassification.Model;
using TextClassification.Service.Dataset.Excel;

namespace TextClassification.Service.Classification
{
    public class ClassificationSimilarityService : BaseClassificationService
    {
        public int n = 20;
        public double minimumConfidence = 0.0;
        public int TopScoringWords = 99; //NOT USED!!!!

        public void OptimizeN()
        {
            var classifiedQueenSpeechService = new ClassifiedQueenSpeechService();
            var classifiedTexts = classifiedQueenSpeechService.Read();
            TfIdfService.GetTfIdfValues(classifiedTexts);
            //ChiSquaredService.GetWeights(classifiedTexts);
            //L2Normalization.Normalize(classifiedTexts);

            var startOfN = 1;
            var endOfN = 35;
            var bestN = 0;
            var bestAvg = 0.0;// tfidif: 20, 0.57887640966805554; chi: 249: 0.54113971063337052
            for (int i = startOfN; i <= endOfN; i++)
            {
                n = i;
                Console.WriteLine("Testing n value: {0}", i);
                var result = GetResults(2000, 2010, classifiedTexts);
                
                if(result > bestAvg)
                {
                    Console.WriteLine("New best n value: {0}. With average of: {1}", i, result);
                    bestN = i;
                    bestAvg = result;
                }
            }
        }
        
        public void GetListResults()
        {
            var classifiedQueenSpeechService = new ClassifiedQueenSpeechService();
            var classifiedTexts = classifiedQueenSpeechService.Read();
            TfIdfService.GetTfIdfValues(classifiedTexts);
            //L2Normalization.Normalize(classifiedTexts);

            var start = 1;
            var end = 5;
            var results = new List<double>();
            for (int i = start; i <= end; i++)
            {
                var r = GetResults(2000, 2010, classifiedTexts);
                Console.WriteLine("Added result of: {0}", r);
                results.Add(r);
            }
        }
        public void OptimizeMinimumConfidence()
        {
            var classifiedQueenSpeechService = new ClassifiedQueenSpeechService();
            var classifiedTexts = classifiedQueenSpeechService.Read();
            TfIdfService.GetTfIdfValues(classifiedTexts);
            //L2Normalization.Normalize(classifiedTexts);

            var startOfMinimumConfidence = 0.0;
            var endOfMinimumConfidence = 1.0;
            var bestMinimumConfidence = 0.2;
            var bestAvg = 0.0;
            for (double i = startOfMinimumConfidence; i <= endOfMinimumConfidence; i += 0.1)
            {
                minimumConfidence = i;
                Console.WriteLine("Testing minimum confidence value: {0}", i);
                var result = GetResults(2000, 2010, classifiedTexts);

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
        public double GetResults()
        {
            var classifiedQueenSpeechService = new ClassifiedQueenSpeechService();
            var classifiedTexts = classifiedQueenSpeechService.Read();
            TfIdfService.GetTfIdfValues(classifiedTexts);
            //ChiSquaredService.GetWeights(classifiedTexts);
            //L2Normalization.Normalize(classifiedTexts);
            var avg = GetResults(2000, 2010, classifiedTexts);
            return avg;
        }
        public Dictionary<int, Statistics> GetResultsPerCategory()
        {
            var classifiedQueenSpeechService = new ClassifiedQueenSpeechService();
            var classifiedTexts = classifiedQueenSpeechService.Read();
            TfIdfService.GetTfIdfValues(classifiedTexts);

            var result = GetResultsPerCategory(2000, 2010, classifiedTexts);

            return result;
        }
        public double GetResults(int startYear, int endYear, List<ClassifiedText> classifiedTexts)
        {
            var listAverage = new List<double>();
            for (int i = startYear; i <= endYear; i++)
            {
                var actual = classifiedTexts.Where(p => p.Year == i).ToList();
                //var bagOfWordsService = new BagOfWordsService();
                //var ctx = bagOfWordsService.BalanceClassifiedTexts(classifiedTexts, i, false); //balanced bags

                var ctx = classifiedTexts.Where(p => p.Year != i).ToList();
                Classify(ctx, actual);// Classify(classifiedTexts.Where(p => p.Year != i && p.GeneralCodes.Count == 1).ToList(), actual); //clean bags

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
            TfIdfService.GetTfIdfValues(classifiedTexts);

            var listAverageSingle = new List<double>();
            var listAverageMultiple = new List<double>();
            
            for (int i = startQueenSpeech; i <= endQueenSpeech; i++)
            {
                Console.WriteLine("Working on year: {0}", i);

                var actual = classifiedTexts.Where(p => p.Year == i).ToList();
                var ctx = classifiedTexts.Where(p => p.Year != i).ToList();

                Classify(ctx, actual);

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
        public Dictionary<int, Statistics> GetResultsPerCategory(int startYear, int endYear, List<ClassifiedText> classifiedTexts)
        {           
            var result = new Dictionary<int, Statistics>();
            for (int i = startYear; i <= endYear; i++)
            {
                var actual = classifiedTexts.Where(p => p.Year == i).ToList();
                //var bagOfWordsService = new BagOfWordsService();
                //var ctx = bagOfWordsService.BalanceClassifiedTexts(classifiedTexts, i, false); //balanced bags

                var ctx = classifiedTexts.Where(p => p.Year != i).ToList();
                Classify(ctx, actual);

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
                }
            }
            result = result.OrderBy(p => p.Key).ToDictionary(p => p.Key, p => p.Value);
            return result;
        }
        public void Classify(List<ClassifiedText> classifiedTexts, List<ClassifiedText> documents)
        {
            foreach (var document in documents)
            {
                document.Predictions = Classify(classifiedTexts, document);
            }   
        }

        public Dictionary<int, Prediction> Classify(List<ClassifiedText> classifiedTexts, ClassifiedText document)
        {
            var result = new Dictionary<int, Prediction>();
            var similarityScores = new Dictionary<int[], double>();
            foreach (var classifiedText in classifiedTexts)
            {
                double[] vectorClassifiedText;
                double[] vectorDocument;
                CreateVectors(classifiedText, document, out vectorClassifiedText, out vectorDocument);
                var sim = new CosineSimilarity();
                var similarity = sim.GetSimilarityScore(vectorClassifiedText, vectorDocument);
                
                similarityScores.Add(classifiedText.GeneralCodes.OrderBy(p => p).ToArray(), similarity);
            }

            //Sum up the value per label of the top n
            similarityScores = similarityScores.OrderByDescending(p => p.Value).Take(n).ToDictionary(x => x.Key,
                                                                                                     x => x.Value);
            foreach (var similarityScore in similarityScores)
            {
                foreach (var generalCode in similarityScore.Key)
                {
                    Prediction prediction = null;
                    if (result.ContainsKey(generalCode))
                        prediction = result[generalCode];

                    if(prediction == null)
                    {
                        prediction = new Prediction {Confidence = 0, GeneralCode = 0};
                        result.Add(generalCode, prediction);
                    }

                    prediction.Confidence += similarityScore.Value;
                }
            }

            //normalize
            result = L2Normalization.Normalize(result);

            result =
                result.OrderByDescending(p => p.Value.Confidence).Where(p => p.Value.Confidence >= minimumConfidence).
                    ToDictionary(x => x.Key, x => x.Value);

            return result;
        }

        public List<string> GetWordScoreResults()
        {
            const int startQueenSpeech = 2000;
            const int endQueenSpeech = 2010;
            var classifiedQueenSpeechService = new ClassifiedQueenSpeechService();
            var classifiedTexts = classifiedQueenSpeechService.Read();
            TfIdfService.GetTfIdfValues(classifiedTexts);
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
          //  var bagOfWordsService = new BagOfWordsService();
            
            var actual = classifiedTexts.Where(p => p.Year == year).ToList();
            //var bagOfWordsService = new BagOfWordsService();
            //var ctx = bagOfWordsService.BalanceClassifiedTexts(classifiedTexts, i, false); //balanced bags

            var ctx = classifiedTexts.Where(p => p.Year != year).ToList();
            Classify(ctx, actual);

           // var actual = classifiedTexts.Where(p => p.Year == year).ToList();
           // var bagOfWords = bagOfWordsService.GetBagOfWords(classifiedTexts, year, false);
            //TfIdfService.GetTfIdfValues(bagOfWords);
            //ChiSquaredService.GetWeights(bagOfWords);
           // Classify(actual, bagOfWords);
            sb.AppendLine("<html><head><meta charset=\"utf-8\"><style> table { border-collapse:collapse; } table, td, th { border:1px solid black; }</style></head><body>");
            sb.AppendLine(string.Format("<h1>Year {0}</h1>", year));
            sb.AppendLine(
                "<p>The results below are based on a predifined list per category. The list . The k-NN algorithm has been used for the results." +
                " If the score per category doesn't contain any scores it has been left out in the overview. Keep in mind that the term weight values" +
                " are multiplied based on their PoS value. Only the PoS tags verb, noun, adjunctive and SPEC have a PoS value of 1. The rest have a value of 0.</p>");
            foreach (var a in actual)
            {
                sb.AppendLine("<div>");
                sb.AppendLine(string.Format("<h3>{0}</h3>", a.Text));
                sb.AppendLine(string.Format("<p>The correct label(s) are/is: {0}</p>", string.Join(", ", a.GeneralCodes)));
                foreach (var prediction in a.Predictions)
                {
                    sb.AppendLine(string.Format("<p><b>Label: {0}, with a confidence of: {1}</b></p>",
                            prediction.Key, prediction.Value.Confidence));

                    //if (!prediction.Value.WordScore.Any())
                    //    continue;

                    //sb.AppendLine(string.Format("<p><b>Label: {0}, with a confidence of: {1}</b>",
                    //                            prediction.Value.GeneralCode, prediction.Value.Confidence));
                    //sb.AppendLine("<table><tr><th>Word</th><th>Score</th><th>PoS</th></tr>");
                    //foreach (var wordScore in prediction.Value.WordScore)
                    //{
                    //    sb.AppendLine(string.Format("<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>"
                    //        , wordScore.Key, wordScore.Value, GetWordType(wordScore.Key)));
                    //}
                    //sb.AppendLine("</table></p>");
                }
                sb.AppendLine("</div>");
            }
            sb.AppendLine(string.Format("</body></html>"));

            return sb.ToString();
        }
        
        private void CreateVectors(ClassifiedText ct1, ClassifiedText ct2, out double[] vector1, out double[] vector2)
        {
            var v1 = new List<double>();
            var v2 = new List<double>();
            var added = new Dictionary<string, string>();

            foreach (var tw in ct1.TermWeights) //foreach (var tw in ct1.TermWeights.OrderByDescending(p => p.Value).Take(TopFeatures))
            {
                //TODO: CHECK TERM WEIGHT. TfIdfService doesn't implement word type value anymore...
                v1.Add(tw.Value);
                var termWeight = GetWordTypeWeight(tw.Key);//added
                if(ct2.TermWeights.ContainsKey(tw.Key))
                {
                    v2.Add(ct2.TermWeights[tw.Key] * termWeight);
                } 
                else
                {
                    v2.Add(0.0);
                }

                added.Add(tw.Key, null);
            }

            foreach (var tw in ct2.TermWeights)//foreach (var tw in ct2.TermWeights.OrderByDescending(p => p.Value).Take(TopScoringWords))
            {
                if(added.ContainsKey(tw.Key))
                    continue;

                var termWeight = GetWordTypeWeight(tw.Key);//added
                v2.Add(tw.Value * termWeight);
                v1.Add(0.0);
            }

            vector1 = v1.ToArray();
            vector2 = v2.ToArray();
        }
    }
}
