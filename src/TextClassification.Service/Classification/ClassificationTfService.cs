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
    public class ClassificationTfService
    {
        public string ClassifyRange(int startQueenSpeech = 2000, int endQueenSpeech = 2010)
        {
            //const int startQueenSpeech = 2000;
            //const int endQueenSpeech = 2010;
            var clasifiedService = new ClassifiedQueenSpeechService();
            var classifiedTexts = clasifiedService.Read();
            var bagOfWordsService = new BagOfWordsService();
            var result = new StringBuilder();

            result.AppendLine("<html><head><meta charset=\"utf-8\"></head><body>");
            for (var i = startQueenSpeech; i <= endQueenSpeech; i++)
            {
                Console.WriteLine("Working on year: {0}", i);
                result.AppendLine(ResultsToHtml(i, classifiedTexts, bagOfWordsService.GetBagOfWords(classifiedTexts, false, ignoreYear: i)));
            }
            result.AppendLine("</body></html>");
            return result.ToString();
        }

        public List<ClassifiedText> Classify(List<ClassifiedText> classifiedText, int ignoreYear, IDictionary<int, BagOfWords> bagOfWords = null)
        {
            if (bagOfWords == null)
            {
                var bagOfWordsService = new BagOfWordsService();
                bagOfWords = bagOfWordsService.GetBagOfWords(false, ignoreYear: ignoreYear);
            }

            foreach (var text in classifiedText)
            {
                text.Predictions = Classify(bagOfWords, text);
            }

            //foreach (var paragraph in qs.Paragraphs)
            //{
            //    foreach (var sentence in paragraph.Sentences)
            //    {
            //        result.Add(sentence);
            //        sentence.Classifications = Classify(bagOfWords, sentence);
            //    }
            //}

            return classifiedText;
        }

        //public List<Sentence> Classify(List<Sentence> list, IDictionary<int, BagOfWords> bagOfWords = null, int ignoreYear = 0)
        //{
        //    if (bagOfWords == null)
        //    {
        //        var bagOfWordsService = new BagOfWordsService();
        //        bagOfWords = bagOfWordsService.GetBagOfWords(ignoreYear: ignoreYear);
        //    }

        //    foreach (var sentence in list)
        //    {
        //        sentence.Classifications = Classify(bagOfWords, sentence);
        //    }

        //    return list;
        //}

        public Dictionary<int, Prediction> Classify(IDictionary<int, BagOfWords> bags, ClassifiedText classifiedText)
        {
            var list = new Dictionary<int, Prediction>();
            var total = classifiedText.PreparedWords.Count;

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
                        score += ((double)bagOfWords.Value.Words[word].Count / GetTotal(bagOfWords)); // Term frequency
                        if (!prediction.WordScore.ContainsKey(word))
                            prediction.WordScore.Add(word, ((double)bagOfWords.Value.Words[word].Count / GetTotal(bagOfWords)));
                    }
                }
                prediction.Confidence = (double)score / (double)total;
                list.Add(prediction.GeneralCode, prediction);
            }
            var result = list.OrderByDescending(p => p.Value.Confidence).ToDictionary(pair => pair.Key, pair => pair.Value); // list.OrderByDescending(p => p.Confidence).ToList();
            return result;
        }

        private int GetTotal(KeyValuePair<int, BagOfWords> bag)
        {
            var total = 0;
            foreach (var word in bag.Value.Words)
            {
                total += word.Value.Count;
            }
            return total;
        }

        public string ResultsToHtml(int year, List<ClassifiedText> classifiedTexts, IDictionary<int, BagOfWords> bagOfWords = null)
        {
            var actual = classifiedTexts.Where(p => p.Year == year).ToList();
            actual = Classify(actual, year, bagOfWords);

            var strB = new StringBuilder();
            strB.AppendLine(string.Format("<h2>{0}</h2>", year));
            strB.AppendLine("<table><tr><th>Sentence</th><th>Cosine similarity</th></tr>");
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
                totalSimilarity = totalSimilarity + similarityScore;

                strB.AppendLine(string.Format("<tr><td><b>{0}...</b></td><td><b>{1}</b></td></tr>",
                          a.Text.Left(30), similarityScore));
            }
            strB.AppendLine("</table>");

            strB.AppendFormat("<h3>Average similarity: {0}, total: {1}</h3>", (double)totalSimilarity / (double)actual.Count, actual.Count);

            return strB.ToString();
        }
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
