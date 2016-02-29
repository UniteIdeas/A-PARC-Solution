using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextClassification.Common;
using TextClassification.Model;
using TextClassification.Service.Dataset.Excel;
using TextClassification.Service.Dataset.Text;

namespace TextClassification.Service.Classification
{
    public class NeighborScrambledClassificationService
    {
        public List<ClassifiedText> Classify(QueenSpeech qs, int kNeighbors = 1)
        {
            var dataSetService = new DataSetService();
            var bagOfWords = dataSetService.GetDataSet(ignoreYear: qs.Year);
            var result = new List<ClassifiedText>();

            foreach (var paragraph in qs.Paragraphs)
            {
                foreach (var sentence in paragraph.Sentences)
                {
                    result.Add(Classify(bagOfWords, sentence, kNeighbors));
                }
            }

            return result;
        }

        public List<ClassifiedText> Classify(string path)
        {
            var queenSpeechService = new QueenSpeechService();
            return Classify(queenSpeechService.Read(path));
        }

        private ClassifiedText Classify(IEnumerable<Model.DataSet> bagOfWords, Sentence sentence, int kNeighbors)
        {
            var list = new List<ClassifiedText>();
            var words = new HashSet<string>();

            //Create neighbors of the sentence
            for (int i = 0; i < sentence.PreparedWords.Count; i++)
            {
                if ((i + kNeighbors) >= sentence.PreparedWords.Count)
                    continue;
                //var word = ct.PreparedWords[i];
                var word = new StringBuilder();
                word.Append(sentence.PreparedWords[i]);
                for (int x = 1; x <= kNeighbors; x++)
                {
                    word.AppendFormat(" {0}", sentence.PreparedWords[i + x]);
                }

                if (!words.Contains(word.ToString()))
                    words.Add(word.ToString());
            }

            //Classify
            var total = words.Count;
            foreach (var bagOfWord in bagOfWords)
            {
                var ct = new ClassifiedText { GeneralCode = bagOfWord.GeneralCode, Text = sentence.Text, Words = sentence.Words };
                var score = 0;
                foreach (var word in words)
                {
                    var containsAllWords = true;
                    foreach (var w in word.Split(' '))
                    {
                        if(!bagOfWord.Words.Contains(w))
                        {
                            containsAllWords = false;
                            break;
                        }
                    }
                    if (containsAllWords)
                        score++;
                }
                ct.Accuracy = (double)score / (double)total;
                list.Add(ct);
            }
            var result = list.OrderByDescending(p => p.Accuracy).FirstOrDefault(); //TODO: Add a confidence measure
            return result;
        }



        public string ClassifyAverage(int kNeighbor = 1)
        {
            const int startQueenSpeech = 2000;
            const int endQueenSpeech = 2010;
            var result = new StringBuilder();

            result.AppendLine("<html>");
            result.AppendLine("<table><tr><th>Year</th><th>Score</th></tr>");
            for (var i = startQueenSpeech; i <= endQueenSpeech; i++)
            {
                Console.WriteLine("Working on year: {0}", i);
                result.AppendFormat("<tr><td>{0}</td><td>{1}</td></tr>", i, ResultAverage(i, kNeighbor));
            }
            result.AppendLine("</table></html>");
            return result.ToString();
        }
        public double ResultAverage(int year, int kNeighbor)
        {
            var queenSpeechService = new QueenSpeechService();
            var queenSpeech = queenSpeechService.Read(string.Format(@"{0}{1}{2}", Constants.FolderQueenSpeeches, year, Constants.QueenSpeechExtension));

            var classifiedQueenSpeechService = new ClassifiedQueenSpeechService();
            var actual = classifiedQueenSpeechService.Read().Where(p => p.Year == year).ToList();

            var prediction = Classify(queenSpeech, kNeighbor);

            var correct = 0;
            foreach (var r in actual)
            {
                var pred = prediction.Find(p => p.Text.ToLower() == r.Text.ToLower()) ??
                           new ClassifiedText { GeneralCode = -1 };

                if (pred.GeneralCode == r.GeneralCode)
                {
                    correct++;
                }

            }
            return (double) correct/(double) actual.Count;
        }

        public string Classify(int kNeighbor = 1)
        {
            const int startQueenSpeech = 2000;
            const int endQueenSpeech = 2010;
            var result = new StringBuilder();

            result.AppendLine("<html>");
            for (var i = startQueenSpeech; i <= endQueenSpeech; i++)
            {
                Console.WriteLine("Working on year: {0}", i);
                result.AppendLine(Results(i, kNeighbor));
            }
            result.AppendLine("</html>");
            return result.ToString();
        }

        public string Results(int year, int kNeighbor)
        {
            var queenSpeechService = new QueenSpeechService();
            var queenSpeech = queenSpeechService.Read(string.Format(@"{0}{1}{2}", Constants.FolderQueenSpeeches, year, Constants.QueenSpeechExtension));

            var classifiedQueenSpeechService = new ClassifiedQueenSpeechService();
            var actual = classifiedQueenSpeechService.Read().Where(p => p.Year == year).ToList();

            var prediction = Classify(queenSpeech, kNeighbor);

            var strB = new StringBuilder();

            strB.AppendLine(string.Format("<h2>{0}</h2>", year));
            strB.AppendLine("<table><tr><th>Sentence</th><th>Prediction</th><th>Actual</th></tr>");
            var correct = 0;
            var missing = 0;
            foreach (var r in actual)
            {
                var pred = prediction.Find(p => p.Text.ToLower() == r.Text.ToLower());
                if (pred == null)
                {
                    pred = new ClassifiedText { GeneralCode = -1 };
                    missing++;
                }

                if (pred.GeneralCode == r.GeneralCode)
                {
                    correct++;
                    strB.AppendLine(string.Format("<tr><td><b>{0}</b></td><td><b>{1}</b></td><td><b>{2}</b></td></tr>", r.Text, pred.GeneralCode, r.GeneralCode));
                }
                else
                {
                    strB.AppendLine(string.Format("<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>", r.Text, pred.GeneralCode, r.GeneralCode));
                }
            }
            strB.AppendLine("</table>");

            strB.AppendFormat("<h3>Result: {0}, total: {1}, correct: {2}, missing: {3}</h3>", (double)correct / (double)actual.Count, actual.Count, correct, missing);

            return strB.ToString();
        }
    }
}
