using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using TextClassification.Common;
using TextClassification.Common.Extension;
using TextClassification.Model;
using TextClassification.Service.Dataset.Excel;
using TextClassification.Service.Dataset.Text;

namespace TextClassification.Service.Classification
{
    public class ClassificationService : BaseClassificationService
    {
        public List<ClassifiedText> Classify(QueenSpeech qs, List<Model.DataSet> bagOfWords = null)
        {
            if (bagOfWords == null)
            {
                var dataSetService = new DataSetService();
                bagOfWords = dataSetService.GetDataSet(ignoreYear: qs.Year);
            }
            var result = new List<ClassifiedText>();

            foreach (var paragraph in qs.Paragraphs)
            {
                foreach (var sentence in paragraph.Sentences)
                {
                    result.Add(Classify(bagOfWords, sentence));
                }
            }

            return result;
        }

        //public List<ClassifiedText> Classify(string path)
        //{
        //    var queenSpeechService = new QueenSpeechService();
        //    return Classify(queenSpeechService.Read(path));
        //}

        public List<Sentence> GetAllClassify(QueenSpeech qs, List<Model.DataSet> bagOfWords = null)
        {
            if (bagOfWords == null)
            {
                var dataSetService = new DataSetService();
                bagOfWords = dataSetService.GetDataSet(ignoreYear: qs.Year);
            }
            var result = new List<Sentence>();

            foreach (var paragraph in qs.Paragraphs)
            {
                foreach (var sentence in paragraph.Sentences)
                {
                    result.Add(sentence);
                    sentence.Classifications = GetAllClassifications(bagOfWords, sentence);
                }
            }

            return result;
        }

        //public List<Sentence> GetAllClassify(string path)
        //{
        //    var queenSpeechService = new QueenSpeechService();
        //    return GetAllClassify(queenSpeechService.Read(path));
        //}

        private ClassifiedText Classify(IEnumerable<Model.DataSet> bagOfWords, Sentence sentence)
        {
            var list = new List<ClassifiedText>();
            var total = sentence.PreparedWords.Count;
            foreach (var bagOfWord in bagOfWords)
            {
                var ct = new ClassifiedText
                             {GeneralCode = bagOfWord.GeneralCode, Text = sentence.Text, Words = sentence.Words};
                var score = 0;// sentence.Words.Count(word => bagOfWord.Words.Find(p => p.Text.ToLower().Equals(word.ToLower())) != null);

                foreach (var word in sentence.PreparedWords)
                {
                    if (bagOfWord.Words.Contains(word))
                        score++;
                }
                ct.Accuracy = (double)score / (double) total;
                list.Add(ct);
            }
            var result = list.OrderByDescending(p => p.Accuracy).FirstOrDefault(); //TODO: Add a confidence measure
            return result;
        }

        private List<ClassifiedText> GetAllClassifications(IEnumerable<Model.DataSet> bagOfWords, Sentence sentence)
        {
            var list = new List<ClassifiedText>();
            var total = sentence.PreparedWords.Count;
            foreach (var bagOfWord in bagOfWords)
            {
                var ct = new ClassifiedText { GeneralCode = bagOfWord.GeneralCode, Text = sentence.Text, Words = sentence.Words };
                var score = 0;// sentence.Words.Count(word => bagOfWord.Words.Find(                                                                              p => p.Text.ToLower().Equals(word.ToLower())) != null);
                foreach (var word in sentence.PreparedWords)
                {
                    if (bagOfWord.Words.Contains(word))
                        score++;
                }
                ct.Accuracy = (double)score / (double)total;
                list.Add(ct);
            }
            var result = list.OrderByDescending(p => p.Accuracy).ToList(); //TODO: Add a confidence measure
            return result;
        }

        public string Classify()
        {
            const int startQueenSpeech = 2000;
            const int endQueenSpeech = 2010;
            var result = new StringBuilder();
            var clasifiedService = new ClassifiedQueenSpeechService();
            var classifiedTexts = clasifiedService.Read();
            var dataSetService = new DataSetService();

            result.AppendLine("<html>");
            for (var i = startQueenSpeech; i <= endQueenSpeech; i++)
            {
                Console.WriteLine("Working on year: {0}", i);
                result.AppendLine(Results(i, classifiedTexts, dataSetService.GetDataSet(classifiedTexts, ignoreYear: i)));
            }
            result.AppendLine("</html>");
            return result.ToString();
        }

        public string GetAllClassify()
        {
            const int startQueenSpeech = 2000;
            const int endQueenSpeech = 2010;
            var clasifiedService = new ClassifiedQueenSpeechService();
            var classifiedTexts = clasifiedService.Read();
            var dataSetService = new DataSetService();
            var result = new StringBuilder();
            
            result.AppendLine("<html>");
            for (var i = startQueenSpeech; i <= endQueenSpeech; i++)
            {
                Console.WriteLine("Working on year: {0}", i);
                result.AppendLine(GetAllResults(i, classifiedTexts, dataSetService.GetDataSet(classifiedTexts, ignoreYear:i)));
            }
            result.AppendLine("</html>");
            return result.ToString();
        }


        public string Results(int year, List<ClassifiedText> classifiedTexts, List<Model.DataSet> bagOfWords = null)
        {
            var queenSpeechService = new QueenSpeechService();
            var queenSpeech = queenSpeechService.Read(string.Format(@"{0}{1}{2}", Constants.FolderQueenSpeeches, year, Constants.QueenSpeechExtension));

            //var classifiedQueenSpeechService = new ClassifiedQueenSpeechService();
            var actual = classifiedTexts.Where(p => p.Year == year).ToList();

            //var classificationService = new ClassificationService();
            var prediction = Classify(queenSpeech, bagOfWords);

            //var classificationAccuracyService = new ClassificationAccuracyService();
            //var result = classificationAccuracyService.Calculate(prediction, actual);

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
                    strB.AppendLine(string.Format("<tr><td><b>{0}...</b></td><td><b>{1}</b></td><td><b>{2}</b></td></tr>", r.Text.Left(30), pred.GeneralCode, r.GeneralCode));
                }
                else
                {
                    strB.AppendLine(string.Format("<tr><td>{0}...</td><td>{1}</td><td>{2}</td></tr>", r.Text.Left(30), pred.GeneralCode, r.GeneralCode));
                }
            }
            strB.AppendLine("</table>");

            strB.AppendFormat("<h3>Result: {0}, total: {1}, correct: {2}, missing: {3}</h3>", (double)correct / (double)actual.Count, actual.Count, correct, missing);

            return strB.ToString();
        }

        public string GetAllResults(int year, List<ClassifiedText> classifiedTexts, List<Model.DataSet> bagOfWords = null)
        {
            var queenSpeechService = new QueenSpeechService();
            var queenSpeech = queenSpeechService.Read(string.Format(@"{0}{1}{2}", Constants.FolderQueenSpeeches, year, Constants.QueenSpeechExtension));

            //var classifiedQueenSpeechService = new ClassifiedQueenSpeechService();
            var actual = classifiedTexts.Where(p => p.Year == year).ToList();

            //var classificationService = new ClassificationService();
            var predictions = GetAllClassify(queenSpeech, bagOfWords);

            //var classificationAccuracyService = new ClassificationAccuracyService();
            //var result = classificationAccuracyService.Calculate(prediction, actual);

            var strB = new StringBuilder();
            strB.AppendLine(string.Format("<h2>{0}</h2>", year));
            strB.AppendLine("<table><tr><th>Sentence</th><th>Prediction position</th><th>Accuracy</th><th>Top 1 accu</th></tr>");
            var correct = 0.0;
            var missing = 0;
            foreach (var a in actual)
            {
                var pred = predictions.Find(p => p.Text.ToLower() == a.Text.ToLower());
                if (pred == null)
                {
                    pred = new Sentence();
                    missing++;
                }

                if(pred.Classifications.Count == 0)
                {
                    strB.AppendLine(string.Format("<tr><td><b>{0}...</b></td><td><b>{1}</b></td><td></td><td></td></tr>",
                        a.Text.Left(30), "-"));
                } 
                else
                {
                    var index = pred.Classifications.FindIndex(p => p.GeneralCode == a.GeneralCode);
                    if(index < 6)
                        correct = (1.0/(index+1)) + correct;
                    strB.AppendLine(string.Format("<tr><td><b>{0}...</b></td><td><b>{1}</b></td><td>{2}</td><td>{3}</td></tr>",
                              a.Text.Left(30), index +1, pred.Classifications[index].Accuracy.ToString(CultureInfo.InvariantCulture).Left(5), pred.Classifications[0].Accuracy.ToString(CultureInfo.InvariantCulture).Left(5)));
                }
            }
            strB.AppendLine("</table>");

            strB.AppendFormat("<h3>Result: {0}, total: {1}, correct: {2}, missing: {3}</h3>", (double)correct / (double)actual.Count, actual.Count, correct, missing);

            return strB.ToString();
        }
    }
}
