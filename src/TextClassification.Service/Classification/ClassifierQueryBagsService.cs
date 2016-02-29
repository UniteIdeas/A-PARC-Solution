using System;
using System.Collections.Generic;
using System.Linq;
using AForge.Math.Metrics;
using TextClassification.Model;
using TextClassification.Service.DataSet.Excel;
using TextClassification.Service.Dataset.Excel;

namespace TextClassification.Service.Classification
{
    public class ClassifierQueryBagsService : BaseClassificationService
    {
        internal BagOfWordsService BagOfWordsService = new BagOfWordsService();
        //public int n = 99;
        public double MinimumConfidence = 0.0;
        
        public double GetResults()
        {
            const int startQueenSpeech = 2000;
            const int endQueenSpeech = 2010;
            var clasifiedService = new ClassifiedQueenSpeechService();
            var classifiedTexts = clasifiedService.Read();

            var avg = GetResults(startQueenSpeech, endQueenSpeech, classifiedTexts);

            return avg;
        }

        //public void OptimizeN()
        //{
        //    var classifiedQueenSpeechService = new ClassifiedQueenSpeechService();
        //    var classifiedTexts = classifiedQueenSpeechService.Read();
        //    TfIdfService.GetTfIdfValues(classifiedTexts);
        //    //ChiSquaredService.GetWeights(classifiedTexts);
        //    //L2Normalization.Normalize(classifiedTexts);

        //    var startOfN = 1;
        //    var endOfN = 35;
        //    var bestN = 0;
        //    var bestAvg = 0.0;// tfidif: 20, 0.57887640966805554; chi: 249: 0.54113971063337052
        //    for (int i = startOfN; i <= endOfN; i++)
        //    {
        //        n = i;
        //        Console.WriteLine("Testing n value: {0}", i);
        //        var result = GetResults(2000, 2010, classifiedTexts);

        //        if (result > bestAvg)
        //        {
        //            Console.WriteLine("New best n value: {0}. With average of: {1}", i, result);
        //            bestN = i;
        //            bestAvg = result;
        //        }
        //    }
        //}

        public void Classify(List<ClassifiedText> sentences, Dictionary<int, BagOfWords> bags)
        {
            foreach (var sentence in sentences)
            {
                sentence.Predictions = Classify(bags, sentence);
            }
        }

        public Dictionary<int, Prediction> Classify(Dictionary<int, BagOfWords> bags, ClassifiedText sentence)
        {
            var result = new Dictionary<int, Prediction>();
            
            foreach (var preparedWord in sentence.UniquePreparedWords)
            {
                var idf = Math.Log(bags.Count/1.0 , 2);
                if(BagOfWordsService.InverseDocumentWeight.ContainsKey(preparedWord))
                    idf = BagOfWordsService.InverseDocumentWeight[preparedWord];

                var posValue = GetWordTypeWeight(preparedWord);

                //var frequency = sentence.PreparedWords.Count(p => p == preparedWord);
                //var total = (double) sentence.Words.Count();
                var tf = sentence.PreparedWords.Count(p => p == preparedWord) / (double)sentence.Words.Count();

                sentence.TermWeights.Add(preparedWord, tf * idf * posValue);
            }

            foreach (var bag in bags)
            {
                double[] vectorBag;
                double[] vectorDocument;
                CreateVectors(bag.Value, sentence, out vectorBag, out vectorDocument);
                var sim = new CosineSimilarity();
                var similarity = sim.GetSimilarityScore(vectorBag, vectorDocument);

                var prediction = new Prediction
                                     {
                                         Confidence = similarity,
                                         GeneralCode = bag.Key
                                     };
                result.Add(bag.Key, prediction);
            }
            
            //normalize
            result = L2Normalization.Normalize(result);

            result =
                result.OrderByDescending(p => p.Value.Confidence).Where(p => p.Value.Confidence >= MinimumConfidence).
                    ToDictionary(x => x.Key, x => x.Value);

            return result;
        }
        public double GetResults(int startYear, int endYear, List<ClassifiedText> classifiedTexts)
        {
            var listAverage = new List<double>();
            BagOfWordsService = new BagOfWordsService();
            for (int i = startYear; i <= endYear; i++)
            {
                //Console.WriteLine("Working on year: {0}", i);

                var actual = classifiedTexts.Where(p => p.Year == i).ToList();
                var bagOfWords = BagOfWordsService.GetBagOfWords(classifiedTexts, i);
                //TfIdfService.GetTfIdfValues(bagOfWords);
                //ChiSquaredService.GetWeights(bagOfWords);
                Classify(actual, bagOfWords);

                //var x = bagOfWords[1].TermWeigths["economisch"];
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
                var avgSimilarity = totalSimilarity /actual.Count;
                listAverage.Add(avgSimilarity);
            }
            var total = listAverage.Sum();
            var avg = total / listAverage.Count;

            return avg;
        }
        private void CreateVectors(BagOfWords bag, ClassifiedText ct, out double[] vector1, out double[] vector2)
        {
            var v1 = new List<double>();
            var v2 = new List<double>();
            //var added = new Dictionary<string, string>();

            foreach (var tw in ct.TermWeights) //foreach (var tw in ct1.TermWeights.OrderByDescending(p => p.Value).Take(TopFeatures))
            {
                v1.Add(tw.Value);

                if(bag.TermWeigths.ContainsKey(tw.Key))
                {
                    v2.Add(bag.TermWeigths[tw.Key]);
                } else
                {
                    v2.Add(0.0);
                }

                //added.Add(tw.Key, null);
            }

            //foreach (var tw in ct2.TermWeights)//foreach (var tw in ct2.TermWeights.OrderByDescending(p => p.Value).Take(TopScoringWords))
            //{
            //    if (added.ContainsKey(tw.Key))
            //        continue;

            //    var termWeight = GetWordTypeWeight(tw.Key);//added
            //    v2.Add(tw.Value * termWeight);
            //    v1.Add(0.0);
            //}

            vector1 = v1.ToArray();
            vector2 = v2.ToArray();
        }
    }
}
