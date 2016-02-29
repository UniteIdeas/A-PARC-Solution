/*
 * tf/idf implementation 
 * Author: Thanh Dao, thanh.dao@gmx.net
 */

using System;
using System.Collections;
using System.Collections.Generic;

namespace TextClassification.Tf_Idf
{
    /// <summary>
    /// Summary description for TF_IDFLib.
    /// </summary>
    public class TFIDFMeasure
    {
        private string[] _docs;
        private string[][] _ngramDoc;
        private int _numDocs = 0;
        private int _numTerms = 0;
        public ArrayList _terms;
        private int[][] _termFreq;
        public double[][] _termWeight;
        private int[] _maxTermFreq;
        private int[] _docFreq;
        public Dictionary<string, double> InverseDocumentWeight;

        public class TermVector
        {
            public static double ComputeCosineSimilarity(double[] vector1, double[] vector2)
            {
                if (vector1.Length != vector2.Length)
                    throw new Exception("DIFER LENGTH");


                double denom = (VectorLength(vector1) * VectorLength(vector2));
                if (denom == 0F)
                    return 0F;
                else
                    return (InnerProduct(vector1, vector2) / denom);

            }

            public static double InnerProduct(double[] vector1, double[] vector2)
            {

                if (vector1.Length != vector2.Length)
                    throw new Exception("DIFFER LENGTH ARE NOT ALLOWED");


                double result = 0F;
                for (int i = 0; i < vector1.Length; i++)
                    result += vector1[i] * vector2[i];

                return result;
            }

            public static double VectorLength(double[] vector)
            {
                double sum = 0.0F;
                for (int i = 0; i < vector.Length; i++)
                    sum = sum + (vector[i] * vector[i]);

                return (double)Math.Sqrt(sum);
            }

        }

        private IDictionary _wordsIndex = new Hashtable();

        public TFIDFMeasure(string[] documents)
        {
            _docs = documents;
            _numDocs = documents.Length;
            MyInit();
        }

        //private void GeneratNgramText()
        //{

        //}

        private ArrayList GenerateTerms(string[] docs)
        {
            ArrayList uniques = new ArrayList();
            _ngramDoc = new string[_numDocs][];
            for (int i = 0; i < docs.Length; i++)
            {
                Tokeniser tokenizer = new Tokeniser();
                string[] words = tokenizer.Partition(docs[i]);

                for (int j = 0; j < words.Length; j++)
                    if (!uniques.Contains(words[j]))
                        uniques.Add(words[j]);

            }
            return uniques;
        }



        private static object AddElement(IDictionary collection, object key, object newValue)
        {
            object element = collection[key];
            collection[key] = newValue;
            return element;
        }

        private int GetTermIndex(string term)
        {
            object index = _wordsIndex[term];
            if (index == null) return -1;
            return (int)index;
        }

        private void MyInit()
        {
            _terms = GenerateTerms(_docs);
            _numTerms = _terms.Count;

            _maxTermFreq = new int[_numDocs];
            _docFreq = new int[_numTerms];
            _termFreq = new int[_numTerms][];
            _termWeight = new double[_numTerms][];

            InverseDocumentWeight = new Dictionary<string, double>();

            for (int i = 0; i < _terms.Count; i++)
            {
                _termWeight[i] = new double[_numDocs];
                _termFreq[i] = new int[_numDocs];

                AddElement(_wordsIndex, _terms[i], i);
            }

            GenerateTermFrequency();
            GenerateTermWeight();
        }

        private double Log(double num)
        {
            return (double)Math.Log(num, 2);//log2
        }

        private void GenerateTermFrequency()
        {
            for (int i = 0; i < _numDocs; i++)
            {
                string curDoc = _docs[i];
                IDictionary freq = GetWordFrequency(curDoc);
                IDictionaryEnumerator enums = freq.GetEnumerator();
                _maxTermFreq[i] = int.MinValue;
                while (enums.MoveNext())
                {
                    string word = (string)enums.Key;
                    int wordFreq = (int)enums.Value;
                    int termIndex = GetTermIndex(word);

                    _termFreq[termIndex][i] = wordFreq;
                    _docFreq[termIndex]++;

                    if (wordFreq > _maxTermFreq[i]) _maxTermFreq[i] = wordFreq;
                }
            }
        }


        private void GenerateTermWeight()
        {
            for (int i = 0; i < _numTerms; i++)
            {
                for (int j = 0; j < _numDocs; j++)
                    _termWeight[i][j] = ComputeTermWeight(i, j);
            }
        }

        private double GetTermFrequency(int term, int doc)
        {
            int freq = _termFreq[term][doc];
            int maxfreq = _maxTermFreq[doc];

            var tf = ((double) freq/(double) maxfreq);
            if(double.IsNaN(tf))
                Console.WriteLine("NaN!");
            return tf;
        }

        private double GetInverseDocumentFrequency(int term)
        {
            var df = _docFreq[term];
            var idf = Log((double)(_numDocs) / (double)df);
            //var idf = (double)(_numDocs) / (double)df;

            var t = _terms[term] as string;
            if(!InverseDocumentWeight.ContainsKey(t))
                InverseDocumentWeight.Add(t, idf);

            if(double.IsNaN(idf))
                Console.WriteLine("NaN!");
            return idf;
        }

        private double ComputeTermWeight(int term, int doc)
        {
            var tf = GetTermFrequency(term, doc);
            var idf = GetInverseDocumentFrequency(term);
            var tfIdf = tf * idf;
            if (double.IsNaN(tfIdf))
                Console.WriteLine("NaN!");
            return tfIdf;
        }

        private double[] GetTermVector(int doc)
        {
            double[] w = new double[_numTerms];
            for (int i = 0; i < _numTerms; i++)
                w[i] = _termWeight[i][doc];


            return w;
        }

        public double GetSimilarity(int doc_i, int doc_j)
        {
            double[] vector1 = GetTermVector(doc_i);
            double[] vector2 = GetTermVector(doc_j);

            return TermVector.ComputeCosineSimilarity(vector1, vector2);

        }

        private IDictionary GetWordFrequency(string input)
        {
            string convertedInput = input.ToLower();

            Tokeniser tokenizer = new Tokeniser();
            String[] words = tokenizer.Partition(convertedInput);
            Array.Sort(words);

            String[] distinctWords = GetDistinctWords(words);

            IDictionary result = new Hashtable();
            for (int i = 0; i < distinctWords.Length; i++)
            {
                object tmp;
                tmp = CountWords(distinctWords[i], words);
                result[distinctWords[i]] = tmp;

            }

            return result;
        }

        private string[] GetDistinctWords(String[] input)
        {
            if (input == null)
                return new string[0];
            else
            {
                ArrayList list = new ArrayList();

                for (int i = 0; i < input.Length; i++)
                    if (!list.Contains(input[i])) // N-GRAM SIMILARITY?				
                        list.Add(input[i]);

                return Tokeniser.ArrayListToArray(list);
            }
        }



        private int CountWords(string word, string[] words)
        {
            int itemIdx = Array.BinarySearch(words, word);

            if (itemIdx > 0)
                while (itemIdx > 0 && words[itemIdx].Equals(word))
                    itemIdx--;

            int count = 0;
            while (itemIdx < words.Length && itemIdx >= 0)
            {
                if (words[itemIdx].Equals(word)) count++;

                itemIdx++;
                if (itemIdx < words.Length)
                    if (!words[itemIdx].Equals(word)) break;

            }

            return count;
        }
        ///// <summary>
        ///// Normalizes a TF*IDF array of vectors using L2-Norm.
        ///// Xi = Xi / Sqrt(X0^2 + X1^2 + .. + Xn^2)
        ///// </summary>
        ///// <param name="vectors">double[][]</param>
        ///// <returns>double[][]</returns>
        //public double[][] Normalize(double[][] vectors)
        //{
        //    // Normalize the vectors using L2-Norm.
        //    List<double[]> normalizedVectors = new List<double[]>();
        //    foreach (var vector in vectors)
        //    {
        //        var normalized = Normalize(vector);
        //        normalizedVectors.Add(normalized);
        //    }

        //    return normalizedVectors.ToArray();
        //}

        ///// <summary>
        ///// Normalizes a TF*IDF vector using L2-Norm.
        ///// Xi = Xi / Sqrt(X0^2 + X1^2 + .. + Xn^2)
        ///// </summary>
        ///// <param name="vector">double[][]</param>
        ///// <returns>double[][]</returns>
        //public double[] Normalize(double[] vector)
        //{
        //    List<double> result = new List<double>();

        //    double sumSquared = 0;
        //    foreach (var value in vector)
        //    {
        //        sumSquared += (double)value * value;
        //    }

        //    double SqrtSumSquared = Math.Sqrt(sumSquared);

        //    foreach (var value in vector)
        //    {
        //        // L2-norm: Xi = Xi / Sqrt(X0^2 + X1^2 + .. + Xn^2)
        //        var r = value/SqrtSumSquared;
        //        if (double.IsNaN(r))
        //        {
        //            //Console.WriteLine("NaN!");
        //            r = 0.0;
        //        }
        //        result.Add(r);
        //    }

        //    return result.ToArray();
        //}

    }
}
