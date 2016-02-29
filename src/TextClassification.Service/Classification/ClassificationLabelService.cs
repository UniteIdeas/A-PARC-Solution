using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextClassification.Model;
using TextClassification.Service.DataSet.Excel;
using TextClassification.Service.Dataset.Excel;

namespace TextClassification.Service.Classification
{
    public class ClassificationLabelService : BaseClassificationService
    {
        public void Classify()
        {
            var path = @"C:\Users\Wouter\Dropbox\Thesis\Datasets\Specific Labels.xlsx";
            //var path = @"C:\Users\Wouter\Dropbox\Thesis\Datasets\General Labels.xlsx";
            var classifiedQueenSpeechService = new ClassifiedQueenSpeechService();
            var classifiedLabels = classifiedQueenSpeechService.Read(path);
            var classifiedText = classifiedQueenSpeechService.Read();

            var bagOfWordsService = new BagOfWordsService();
            var bags = bagOfWordsService.GetBagOfWords(classifiedLabels, false, ignoreYear: -1);

            var result = new Dictionary<int, Measure>();
            foreach (var text in classifiedText)
            {
                text.Predictions = Classify(bags, text);

                foreach (var prediction in text.Predictions)
                {
                    Measure m;
                    if(!result.ContainsKey(prediction.Key))
                    {
                        m = new Measure();
                        result.Add(prediction.Key, m);
                    } 
                    else
                    {
                        m = result[prediction.Key];
                    }

                    if(text.GeneralCodes.Contains(prediction.Key))
                    {
                        m.Correct += 1;
                    } else
                    {
                        m.Incorrect += 1;
                    }
                }
            }
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("<table>");
            stringBuilder.AppendLine("<tr><td>Id</td><td>text</td><td>correct</td><td>incorrect</td></tr>");
            foreach (var bag in bags)
            {
                var res = new Measure {Correct = 0, Incorrect = 0};
                if (result.ContainsKey(bag.Key))
                    res = result[bag.Key];
                stringBuilder.AppendLine(string.Format("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td></tr>",
                                                       bag.Key, bag.Value.Text, res.Correct,
                                                       res.Incorrect));
            }
            stringBuilder.AppendLine(string.Format("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td></tr>",
                                       0, " ", result[0].Correct,
                                       result[0].Incorrect));
            stringBuilder.AppendLine("</table>");
            var r = stringBuilder.ToString();
        }

        public Dictionary<int, Prediction> Classify(IDictionary<int, BagOfWords> bags, ClassifiedText classifiedText)
        {
            var list = new Dictionary<int, Prediction>();
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
                        var typeWeight = GetWordTypeWeight(word);
                     
                        var scoreWord = 1 * typeWeight;
                        score += scoreWord;
                        if (!prediction.WordScore.ContainsKey(word))
                            prediction.WordScore.Add(word, scoreWord);
                    }
                }
                if(score > 0)
                {
                    prediction.Confidence = score;
                    list.Add(prediction.GeneralCode, prediction);
                }
            }
            if(!list.Any())
                list.Add(0, new Prediction { GeneralCode = 0, Confidence = 1});
            var result = list.OrderByDescending(p => p.Value.Confidence).ToDictionary(pair => pair.Key, pair => pair.Value); // list.OrderByDescending(p => p.Confidence).ToList();
            return result;
        }
    }
}
