using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TextClassification.Model;
using TextClassification.Service.Classification;

namespace TextClassification.Service
{
    public class ChiSquaredService
    {
        private static BaseClassificationService baseClassificationService = new BaseClassificationService();
        public static void GetWeights(Dictionary<int, BagOfWords> bags)
        {
            foreach (var bag in bags)
            {
                bag.Value.TermWeigths = new Dictionary<string, double>();
                foreach (var word in bag.Value.Words)
                {
                    var wordTypeWeight = baseClassificationService.GetWordTypeWeight(word.Key); //pos value included
                    if(wordTypeWeight == 0)
                    {
                        bag.Value.TermWeigths.Add(word.Key, 0);
                        continue;
                    }
                    var termInDocument = word.Value.Count;
                    var termNotInDocument = GetTermNotInDocument(bags, word.Key, bag.Key);
                    var notTermInDocument = GetNotTermInDocument(bag.Value, word.Key);
                    var notTermNotDocument = GetNotTermNotDocument(bags, bag.Key, word.Key);
                    var termWeight = GetWeight(termInDocument, termNotInDocument, notTermInDocument, notTermNotDocument)*wordTypeWeight; //pos value included

                    bag.Value.TermWeigths.Add(word.Key, termWeight);
                }
                //var result1 = SumNormalization.Normalize(bag.Value.TermWeights);
                //var result2 = L2Normalization.Normalize(bag.Value.TermWeights);
                bag.Value.TermWeigths = L2Normalization.Normalize(bag.Value.TermWeigths);// L2Normalization.Normalize(bag.Value.TermWeights);

                //var termWeights = new Dictionary<string, double>();
                //foreach (var tw in bag.Value.TermWeights)
                //{
                //    termWeights.Add(tw.Key, 1- tw.Value);
                //}
                //bag.Value.TermWeights = termWeights;
            }
        }
        public static void GetWeights(List<ClassifiedText> classifiedTexts)
        {
            //test it
            Console.WriteLine("Getting chi weights");
            foreach (var ct in classifiedTexts)
            {
                ct.TermWeights = new Dictionary<string, double>();
                foreach (var word in ct.PreparedWords)
                {
                    if (ct.TermWeights.ContainsKey(word)) continue;
                    
                    var wordTypeWeight = baseClassificationService.GetWordTypeWeight(word); //pos value included
                    if (wordTypeWeight == 0)
                    {
                        if(!ct.TermWeights.ContainsKey(word))
                            ct.TermWeights.Add(word, 0);
                        continue;
                    }

                    var termInDocument = ct.PreparedWords.Count(p => p == word);
                    var termNotInDocument = GetTermNotInDocument(classifiedTexts, word, termInDocument);
                    var notTermInDocument = ct.PreparedWords.Count(p => p != word);
                    var notTermNotDocument = GetNotTermNotDocument(classifiedTexts, word, ct.PreparedWords.Count - termInDocument);
                    var termWeight = GetWeight(termInDocument, termNotInDocument, notTermInDocument, notTermNotDocument) * wordTypeWeight; //pos value included

                    if (!ct.TermWeights.ContainsKey(word))
                        ct.TermWeights.Add(word, termWeight);
                }
                ct.TermWeights = L2Normalization.Normalize(ct.TermWeights);
            }
        }

        /// <summary>
        /// 2x2 x2 equation
        /// </summary>
        /// <param name="termInDocument"></param>
        /// <param name="termNotInDocument"></param>
        /// <param name="notTermInDocument"></param>
        /// <param name="notTermNotDocument"></param>
        /// <returns></returns>
        public static double GetWeight(int termInDocument, int termNotInDocument, int notTermInDocument, int notTermNotDocument)
        {
            //A = #(t,D), B = #(t, (NOT)D), C = #((NOT)t, D), D = #((NOT)t, (NOT)D), N = (A+B+C+D)
            //Equation:  ( (A+B+C+D) * (AD-CB)2 )  /  ( (A+C)x(B+D)x(A+B)x(C+D) )
            var AD_CB = (BigInteger)(termInDocument * notTermNotDocument) - (notTermInDocument * termNotInDocument);
            var AD_CBSquared = AD_CB * AD_CB;
            var v = (BigInteger) (termInDocument + termNotInDocument + notTermInDocument + notTermNotDocument) * (AD_CBSquared);
            var divideBy = (BigInteger)(termInDocument + notTermInDocument) * (termNotInDocument + notTermNotDocument) 
                * (termInDocument + termNotInDocument) * (notTermInDocument + notTermNotDocument);
            var result = (double)v / (double)divideBy;
            return result;
        }

        private static int GetNotTermNotDocument(Dictionary<int, BagOfWords> bags, int excludeBag, string excludeTerm)
        {
            var total = 0;
            foreach (var bag in bags)
            {
                if(bag.Key == excludeBag) continue;

                foreach (var word in bag.Value.Words)
                {
                    if (word.Key.ToLower() == excludeTerm.ToLower()) continue;

                    total += word.Value.Count;
                }
            }

            return total;
        }

        private static int GetNotTermInDocument(BagOfWords bag, string excludeTerm)
        {
            var total = 0;
            foreach (var word in bag.Words)
            {
                if(word.Key.ToLower() == excludeTerm.ToLower()) continue;

                total += word.Value.Count;
            }
            return total;
        }
        private static int GetTermNotInDocument(Dictionary<int, BagOfWords> bags, string term, int excludeKey)
        {
            var total = 0;
            foreach (var bag in bags)
            {
                if(bag.Key == excludeKey) continue;

                if (bag.Value.Words.ContainsKey(term))
                    total += bag.Value.Words.Count;
            }

            return total;
        }

        private static int GetTermNotInDocument(List<ClassifiedText> classifiedTexts, string term, int minus)
        {
            var total = 0;
            foreach (var ct in classifiedTexts)
            {
                //if (ct.PreparedText.Equals(excludePreparedText)) continue;

                if (ct.PreparedWords.Contains(term))
                    total += ct.PreparedWords.Count(p => p == term);
            }

            return total-minus;
        }

        private static int GetNotTermNotDocument(List<ClassifiedText> classifiedTexts, string term, int minus)
        {
            var total = 0;
            foreach (var ct in classifiedTexts)
            {
                //if (bag.Key == excludeBag) continue;
                total += ct.PreparedWords.Count(p => p != term);
                //foreach (var word in bag.Value.Words)
                //{
                //    if (word.Key.ToLower() == excludeTerm.ToLower()) continue;

                //    total += word.Value.Count;
                //}
            }

            return total-minus;
        }
    }
}
