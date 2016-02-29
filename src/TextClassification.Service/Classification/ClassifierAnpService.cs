using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TextClassification.Common.Extension;
using TextClassification.Model;
using TextClassification.Service.DataSet.Excel;
using TextClassification.Service.Dataset.Excel;
using TextClassification.Service.Dataset.Text;

namespace TextClassification.Service.Classification
{
    public class ClassifierAnpService : BaseText
    {
        private readonly string[] _clean = new[] { "<p>", "</p>", "\r", "\n", "<b>", "</b>", "<br>", "<br />", Environment.NewLine };
        public void Classify(string path = Common.Constants.AnpArticles)
        {
            if(!File.Exists(path))
                return;
            
            var lines = File.ReadAllLines(path);
            var articles = new List<AnpArticle>();
            foreach (var line in lines)
            {
                var article = ReadLine(line);
                if (article != null)
                    articles.Add(ReadLine(line));
            }

            //foreach (var anpArticle in articles)
            //{
            //   anpArticle.Predictions = CountPredictions(anpArticle);
            //}

            var classifier = new ClassifierTFIDFTweakedService();
            var bagOfWordsService = new BagOfWordsService();
            var clasifiedService = new ClassifiedQueenSpeechService();
            var classifiedTexts = clasifiedService.Read();
            var bags = bagOfWordsService.GetBagOfWords(classifiedTexts);
            var result = new List<string>();
            foreach (var article in articles)
            {
                article.Sentences = classifier.Classify(article.Sentences, bags);
                article.Predictions = CountPredictions(article);
                result.Add(ToHtml(article));
            }
        }
        public AnpArticle ReadLine(string line)
        {
            var result = new AnpArticle();

            line = _clean.Aggregate(line, (current, s) => current.Replace(s, ""));

            var splitter = new[] {"|"};
            var splitted = line.Split(splitter,6,StringSplitOptions.None);
            if (splitted.Count() != 6)
                return null;
            result.Date = splitted[0];
            result.Bron = splitted[1];
            result.Title = splitted[2];
            result.Year = splitted[3];
            result.Text = splitted[5];

            var sentenceSpliter = new[] { ". ", ".", "? ", "?", "! ", "!" };
            foreach (var sentence in result.Text.Split(sentenceSpliter, StringSplitOptions.RemoveEmptyEntries))
            {
                var sen = new ClassifiedText
                {
                    Text = sentence.Clean(),
                    Words = sentence.Clean().Split(' ').ToList()
                };
                if(string.IsNullOrEmpty(sen.Text))
                    continue;
                var stemService = new StemService();
                var stringBuilder = new StringBuilder();
                foreach (var word in sen.Text.Split(' ').Prepare().ToList())
                {
                    var w = stemService.GetStem(word);
                    sen.PreparedWords.Add(w);
                    stringBuilder.AppendFormat("{0} ", w);
                }
                var preparedText = stringBuilder.ToString();
                if (string.IsNullOrEmpty(preparedText))
                    continue;
                sen.PreparedText = preparedText.Remove(preparedText.Length - 1);

                result.Sentences.Add(sen);
            }

            return result;
        }
        private string ToHtml(AnpArticle article)
        {
            var sb = new StringBuilder();
            var baseClassifier = new BaseClassificationService();
            sb.AppendLine("<html><head><meta charset=\"utf-8\"><style> table { border-collapse:collapse; } table, td, th { border:1px solid black; }</style></head><body>");
            sb.AppendLine(string.Format("<h1>{0}</h1>", article.Title));
            sb.AppendLine(string.Format("<p>{0}</p>", article.Text));

            sb.AppendLine("<table>");
            sb.AppendLine("<tr><th>Category</th><th>Count</th></tr>");
            foreach (var prediction in article.Predictions)
            {
                sb.AppendLine(string.Format("<tr><td>{0}</td><td>{1}</td></tr>", prediction.Key, prediction.Value));
            }
            sb.AppendLine("</table>");

            foreach (var a in article.Sentences)
            {
                sb.AppendLine("<div>");
                sb.AppendLine(string.Format("<h3>{0}</h3>", a.Text));
                //sb.AppendLine(string.Format("<p>The correct label(s) are/is: {0}</p>", string.Join(", ", a.GeneralCodes)));
                foreach (var prediction in a.Predictions)
                {
                    sb.AppendLine(string.Format("<p><b>Label: {0}, with a score of: {1}</b>",
                                                prediction.Value.GeneralCode, prediction.Value.Confidence));

                    if (!prediction.Value.WordScore.Any())
                        continue;
                    sb.AppendLine("<table><tr><th>Word</th><th>Score</th><th>PoS</th></tr>");
                    foreach (var wordScore in prediction.Value.WordScore)
                    {
                        sb.AppendLine(string.Format("<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>"
                            , wordScore.Key, wordScore.Value, baseClassifier.GetWordType(wordScore.Key)));
                    }
                    sb.AppendLine("</table></p>");
                }
                sb.AppendLine("</div>");
            }
            sb.AppendLine(string.Format("</body></html>"));

            return sb.ToString();
        }
        private Dictionary<int, int> CountPredictions(AnpArticle article)
        {
            var result = new Dictionary<int, int>();
            foreach (var classifiedText in article.Sentences)
            {
                foreach (var prediction in classifiedText.Predictions)
                {
                    int key = prediction.Key;
                    
                    if(!result.ContainsKey(key))
                        result.Add(key, 0);

                    result[key]++;
                }
            }
            return result;
        }
    }
}
