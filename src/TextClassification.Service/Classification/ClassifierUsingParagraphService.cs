using System;
using System.Collections.Generic;
using System.Linq;
using AForge.Math.Metrics;
using TextClassification.Common;
using TextClassification.Model;
using TextClassification.Service.DataSet.Excel;
using TextClassification.Service.Dataset.Excel;
using TextClassification.Service.Dataset.Text;

namespace TextClassification.Service.Classification
{
    public class ClassifierUsingParagraphService : BaseClassificationService
    {
        private double _minimumConfidence = 0.3;

        public double GetResults()
        {
            const int startQueenSpeech = 2000;
            const int endQueenSpeech = 2010;
            var clasifiedService = new ClassifiedQueenSpeechService();
            var classifiedTexts = clasifiedService.Read();

            var avg = GetResults(startQueenSpeech, endQueenSpeech, classifiedTexts);

            return avg;
        }

        public void OptimizeMinimumConfidence()
        {
            const int startQueenSpeech = 2000;
            const int endQueenSpeech = 2010;
            var clasifiedService = new ClassifiedQueenSpeechService();
            var classifiedTexts = clasifiedService.Read();

            const double startOfMinimumConfidence = 0.0;
            const double endOfMinimumConfidence = 1.0;
            var bestMinimumConfidence = 0.0;
            var bestAvg = 0.0;
            for (double i = startOfMinimumConfidence; i <= endOfMinimumConfidence; i += 0.1)
            {
                _minimumConfidence = i;
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

        private void Classify(QueenSpeech queenSpeech, Dictionary<int, BagOfWords> bagOfWords)
        {
            foreach (var paragraph in queenSpeech.Paragraphs)
            {
                var first = true;
                var last = false;
                Sentence firstSentence = null;
                for (int i = 0; i < paragraph.Sentences.Count; i++)
                {
                    var sentence = paragraph.Sentences[i];
                    if (first)
                    {
                        firstSentence = sentence;
                        first = false;
                    }
                    if (i == (paragraph.Sentences.Count - 1))
                        last = true;

                    sentence.Predictions = Classify(sentence, bagOfWords);

                    if(last)
                    {
                        foreach (var sent in paragraph.Sentences)
                        {
                            if (sent.PreparedText.Equals(firstSentence.PreparedText)) continue;
                            
                            if(!sent.Predictions.ContainsKey(firstSentence.Predictions.First().Key))
                            {
                                sent.Predictions.Add(firstSentence.Predictions.First().Key, firstSentence.Predictions.First().Value);
                            } 
                            else
                            {
                                sent.Predictions[firstSentence.Predictions.First().Key] =
                                    firstSentence.Predictions.First().Value;
                            }
                        }
                    }
                    
                }
            }
        }

        private Dictionary<int, Prediction> Classify(Sentence sentence, Dictionary<int, BagOfWords> bags)
        {
            var list = new Dictionary<int, Prediction>();
            var total = sentence.PreparedWords.Count;
            var totalBags = bags.Count;
            foreach (var bagOfWords in bags)
            {
                var prediction = new Prediction
                {
                    GeneralCode = bagOfWords.Value.GeneralCode
                };
                var score = 0.0;

                foreach (var word in sentence.PreparedWords)
                {
                    if (!bagOfWords.Value.Words.ContainsKey(word)) continue;

                    var scoreWord = FindWeight(bagOfWords.Value.TermWeigths, word, totalBags) * GetWordTypeWeight(word);
                    score += scoreWord;

                    if (!prediction.WordScore.ContainsKey(word))
                        prediction.WordScore.Add(word, scoreWord);
                }
                
                prediction.Confidence = score / total;
                list.Add(prediction.GeneralCode, prediction);
            }

            var result = L2Normalization.Normalize(list).OrderByDescending(p => p.Value.Confidence).ToDictionary(pair => pair.Key,
                                                                                             pair => pair.Value);
            if (result.Any(p => p.Value.Confidence >= _minimumConfidence))
            {
                result = result.Where(p => p.Value.Confidence >= _minimumConfidence).OrderByDescending(p => p.Value.Confidence).ToDictionary(pair => pair.Key,
                                                                                                 pair => pair.Value);
            }
            else if (result.Max(p => p.Value.Confidence) == 0)
            {
                var maxWords = result.Max(p => p.Value.WordScore.Count);
                result = result.Where(p => p.Value.WordScore.Count >= maxWords).OrderByDescending(p => p.Value.Confidence).ToDictionary(pair => pair.Key,
                                                                                                 pair => pair.Value);
                foreach (var prediction in result)
                {
                    prediction.Value.Confidence = (double)1 / result.Count;
                }
            }
            else
            {
                var maxConfidence = result.Max(p => p.Value.Confidence);
                result = result.Where(p => p.Value.Confidence >= maxConfidence).OrderByDescending(p => p.Value.Confidence).ToDictionary(pair => pair.Key,
                                                                                                 pair => pair.Value);
            }
            return result;
        }

        public double GetResults(int startYear, int endYear, List<ClassifiedText> classifiedTexts)
        {
            var listAverage = new List<double>();
            var queenSpeechService = new QueenSpeechService();
            var bagOfWordsService = new BagOfWordsService();
            for (int i = startYear; i <= endYear; i++)
            {
                var actual = classifiedTexts.Where(p => p.Year == i).ToList();
                var queenSpeech =
                    queenSpeechService.Read(string.Format("{0}{1}.txt", Constants.FolderQueenSpeeches, i), actual);
                
                var bagOfWords = bagOfWordsService.GetBagOfWords(classifiedTexts, i);
                AddWordScores(bagOfWords);

                Classify(queenSpeech, bagOfWords);

                //var x = bagOfWords[1].TermWeights["economisch"];
                var totalSimilarity = 0.0;
                foreach (var paragraph in queenSpeech.Paragraphs)
                {
                    foreach (var sentence in paragraph.Sentences)
                    {
                        // instantiate new similarity class
                        var sim = new CosineSimilarity();

                        // create two vectors for inputs 
                        double[] p;
                        double[] q;
                        VectorService.CreateVectors(sentence.Predictions, sentence.ClassifiedText.GeneralCodes, out p, out q);

                        // get similarity between the two vectors
                        double similarityScore = sim.GetSimilarityScore(p, q);
                        totalSimilarity += similarityScore;
                    }
                }
                var avgSimilarity = totalSimilarity / actual.Count;
                listAverage.Add(avgSimilarity);
            }
            var total = listAverage.Sum();
            var avg = total / listAverage.Count;

            return avg;
        }
        List<WordScore> wordScores = new List<WordScore>
                                         {
                                             new WordScore {Category = 1, Score = 0.33, Word = "economie"},
                                             new WordScore {Category = 1, Score = 0.33, Word = "economisch"},
                                             new WordScore {Category = 5, Score = 0.33, Word = "pensioenstelsel"},
                                             new WordScore {Category = 5, Score = 0.33, Word = "pensioenvermogen"},
                                         };
        private void AddWordScores(Dictionary<int, BagOfWords> bags)
        {
            foreach (var wordScore in wordScores)
            {
                var bag = bags[wordScore.Category];
                if (!bag.TermWeigths.ContainsKey(wordScore.Word))
                {
                    bag.TermWeigths.Add(wordScore.Word, wordScore.Score);
                } 
                else
                {
                    bag.TermWeigths[wordScore.Word] = wordScore.Score;
                }
            }
        }

    }
}
