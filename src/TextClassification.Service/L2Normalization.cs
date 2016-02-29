using System;
using System.Collections.Generic;
using TextClassification.Model;

namespace TextClassification.Service
{
    public static class L2Normalization
    {
        public static void Normalize(Dictionary<int, BagOfWords> bags)
        {
            //Console.WriteLine("Normalizing term weights");
            foreach (var bag in bags)
            {
                bag.Value.TermWeigths = Normalize(bag.Value.TermWeigths);
            }
        }
        public static void Normalize(List<ClassifiedText> classifiedTexts)
        {
            Console.WriteLine("Normalizing term weight");
            foreach(var ct in classifiedTexts)
            {
                ct.TermWeights = Normalize(ct.TermWeights);
            }
        }
        public static void Normalize(Dictionary<int, QueenSpeech> queenSpeeches)
        {
            Console.WriteLine("Normalizing term weight");
            foreach (var queenSpeech in queenSpeeches)
            {
                queenSpeech.Value.TermWeights = Normalize(queenSpeech.Value.TermWeights);
            }
        }
        internal static void Normalize(Dictionary<int[], BagOfWordsCombinedLabels> bags)
        {
            //Console.WriteLine("Normalizing term weights");
            foreach (var bag in bags)
            {
                bag.Value.TfIdf = Normalize(bag.Value.TfIdf);
            }
        }

        public static Dictionary<string, double> Normalize(Dictionary<string, double> termWeights)
        {
            var result = new Dictionary<string, double>();

            var sumSquared = 0.0;
            foreach (var termWeight in termWeights)
            {
                sumSquared += termWeight.Value * termWeight.Value;
            }

            var sqrtSumSquared = Math.Sqrt(sumSquared);

            foreach (var termWeight in termWeights)
            {
                // L2-norm: Xi = Xi / Sqrt(X0^2 + X1^2 + .. + Xn^2)
                var r = termWeight.Value / sqrtSumSquared;
               
                if (double.IsNaN(r))
                    r = 0.0;
                
                result.Add(termWeight.Key, r);
            }

            return result;
        }
        public static Dictionary<int, Prediction> Normalize(Dictionary<int, Prediction> predictions)
        {
            var result = new Dictionary<int, Prediction>();

            var sumSquared = 0.0;
            foreach (var termWeight in predictions)
            {
                sumSquared += termWeight.Value.Confidence * termWeight.Value.Confidence;
            }

            var sqrtSumSquared = Math.Sqrt(sumSquared);

            foreach (var termWeight in predictions)
            {
                // L2-norm: Xi = Xi / Sqrt(X0^2 + X1^2 + .. + Xn^2)
                var r = termWeight.Value.Confidence / sqrtSumSquared;
                
                if (double.IsNaN(r))
                    r = 0.0;
                
                result.Add(termWeight.Key, new Prediction { Confidence = r, GeneralCode = termWeight.Value.GeneralCode, WordScore = termWeight.Value.WordScore});
            }

            return result;
        }


        public static Dictionary<int[], Prediction> Normalize(Dictionary<int[], Prediction> predictions)
        {
            var result = new Dictionary<int[], Prediction>();

            var sumSquared = 0.0;
            foreach (var termWeight in predictions)
            {
                sumSquared += termWeight.Value.Confidence * termWeight.Value.Confidence;
            }

            var sqrtSumSquared = Math.Sqrt(sumSquared);

            foreach (var termWeight in predictions)
            {
                // L2-norm: Xi = Xi / Sqrt(X0^2 + X1^2 + .. + Xn^2)
                var r = termWeight.Value.Confidence / sqrtSumSquared;

                if (double.IsNaN(r))
                    r = 0.0;

                result.Add(termWeight.Key, new Prediction { Confidence = r, GeneralCode = termWeight.Value.GeneralCode });
            }

            return result;
        }
    }
}
