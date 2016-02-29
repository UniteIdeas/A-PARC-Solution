using System;
using System.Collections.Generic;
using TextClassification.Model;
using TextClassification.Service.Classification;
using TextClassification.Tf_Idf;

namespace TextClassification.Service
{
    public class TfIdfService
    {
        private static BaseClassificationService baseClassificationService = new BaseClassificationService();
        public static Dictionary<string, double> GetTfIdfValues(Dictionary<int, BagOfWords> bags)
        {
            //Console.WriteLine("Calculate tf-idf word weight");
            var docs = new List<string>();
            foreach (var bag in bags)
            {
                docs.Add(bag.Value.Text.ToString().Replace("\r\n", " "));
            }
            var tfIdf = new TFIDFMeasure(docs.ToArray());
            var docsWeights = tfIdf._termWeight; // tfIdf.Normalize(tfIdf._termWeight);
            
            //[word][weights per bag]
            for (int j = 0; j < tfIdf._terms.Count; j++)
            {
                var word = tfIdf._terms[j].ToString(); //get the word
                var weights = docsWeights[j];
                var wordTypeWeight = 1;// baseClassificationService.GetWordTypeWeight(word);

                var i = 0;
                foreach (var bag in bags)
                {
                    var weight = weights[i];

                    if (!bag.Value.TermWeigths.ContainsKey(word))
                    {
                        if (double.IsNaN(weight))
                            weight = 0.0;

                        weight = weight * wordTypeWeight;

                        bag.Value.TermWeigths.Add(word, weight);
                    }
                    i++;
                }
            }

            //normalize
            L2Normalization.Normalize(bags);
            return tfIdf.InverseDocumentWeight;
        }
        public void GetTfIdfValues(Dictionary<int[], BagOfWordsCombinedLabels> bags)
        {
            //Console.WriteLine("Calculate tf-idf word weight");
            var docs = new List<string>();
            foreach (var bag in bags)
            {
                docs.Add(bag.Value.Text.ToString().Replace("\r\n", " "));
            }
            var tfIdf = new TFIDFMeasure(docs.ToArray());
            //var docsWeights = tfIdf.Normalize(tfIdf._termWeight);

            //[word][weights per bag]
            for (int j = 0; j < tfIdf._terms.Count; j++)
            {
                var word = tfIdf._terms[j].ToString(); //get the word
                var weights = tfIdf._termWeight[j];

                var i = 0;
                foreach (var bag in bags)
                {
                    var weight = weights[i];

                    if (!bag.Value.TfIdf.ContainsKey(word))
                    {
                        if (double.IsNaN(weight))
                        {
                            weight = 0.0;
                        }
                        bag.Value.TfIdf.Add(word, weight);
                    }
                    i++;
                }
            }

            //normalize
            L2Normalization.Normalize(bags);
        }
        public static void GetTfIdfValues(Dictionary<int, QueenSpeech> queenSpeeches)
        {
            Console.WriteLine("Calculate tf-idf term weight");
            var docs = new List<string>();
            foreach (var qs in queenSpeeches)
            {
                docs.Add(qs.Value.Text);
            }
            var tfIdf = new TFIDFMeasure(docs.ToArray());

            //[word][weights per doc]
            for (int j = 0; j < tfIdf._terms.Count; j++)
            {
                var word = tfIdf._terms[j].ToString(); //get the word
                var weights = tfIdf._termWeight[j];
                var wordTypeWeight = baseClassificationService.GetWordTypeWeight(word);

                var i = 0;
                foreach (var qs in queenSpeeches)
                {
                    var weight = weights[i];
                    if (double.IsNaN(weight))
                    {
                        weight = 0.0;
                    }

                    weight = weight * wordTypeWeight;

                    if (!qs.Value.TermWeights.ContainsKey(word))
                    {
                        qs.Value.TermWeights.Add(word, weight);
                    }
                    i++;
                }
            }

            L2Normalization.Normalize(queenSpeeches);
        }
        public static void GetTfIdfValues(List<ClassifiedText> classifiedTexts)
        {
            Console.WriteLine("Calculate tf-idf term weight");
            var docs = new List<string>();
            foreach (var ct in classifiedTexts)
            {
                docs.Add(ct.PreparedText);
            }
            var tfIdf = new TFIDFMeasure(docs.ToArray());

            //[word][weights per doc]
            for (int j = 0; j < tfIdf._terms.Count; j++)
            {
                var word = tfIdf._terms[j].ToString(); //get the word
                var weights = tfIdf._termWeight[j];
                var wordTypeWeight = baseClassificationService.GetWordTypeWeight(word);

                var i = 0;
                foreach (var ct in classifiedTexts)
                {
                    var weight = weights[i];
                    if (double.IsNaN(weight))
                    {
                        weight = 0.0;
                    }

                    weight = weight * wordTypeWeight;

                    if (!ct.TermWeights.ContainsKey(word) && ct.UniquePreparedWords.Contains(word))
                    {
                        ct.TermWeights.Add(word, weight);
                    }
                    i++;
                }
            }

            L2Normalization.Normalize(classifiedTexts);
        }

    }
}
