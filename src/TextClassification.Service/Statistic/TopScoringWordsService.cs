using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextClassification.Model;
using TextClassification.Service.DataSet.Excel;
using TextClassification.Service.Dataset.Excel;

namespace TextClassification.Service.Statistic
{
    public class TopScoringWordsService
    {
        public void TopScoringPerCategory()
        {
            var topWords = 50;
            var clasifiedService = new ClassifiedQueenSpeechService();
            var classifiedTexts = clasifiedService.Read();
            var bagOfWordsService = new BagOfWordsService();

            var bags = bagOfWordsService.GetBagOfWords(classifiedTexts);

            var result = new Dictionary<int, string>();
            foreach (var bag in bags)
            {
                var top = bag.Value.TermWeigths.OrderByDescending(p => p.Value).Take(topWords);
                var topString = String.Join(", ", top.Select(p => p.Key));
                result.Add(bag.Key, topString);
            }
        }
        public void TopScoringPerYear()
        {
            const int topWords = 20;
            var clasifiedService = new ClassifiedQueenSpeechService();
            var classifiedTexts = clasifiedService.Read();
            var queenSpeeches = new Dictionary<int, QueenSpeech>();
            for (int i = 1945; i <= 2010; i++)
            {
                var queenSpeech = new QueenSpeech { Year = i, Name = i.ToString(CultureInfo.InvariantCulture) };
                var cts = classifiedTexts.Where(p => p.Year == i);
                var stringBuilder = new StringBuilder();
                foreach (var ct in cts)
                {
                    stringBuilder.AppendFormat("{0} ", ct.PreparedText);
                }
                queenSpeech.Text = stringBuilder.ToString().Remove(stringBuilder.ToString().Length -1);
                queenSpeeches.Add(i, queenSpeech);
            }

            TfIdfService.GetTfIdfValues(queenSpeeches);
            
            var result = new Dictionary<int, string>();
            for (int i = 1995; i <= 2010; i++)
            {
                var queenSpeech = queenSpeeches[i];
                var top = queenSpeech.TermWeights.OrderByDescending(p => p.Value).Take(topWords);
                var topString = String.Join(", ", top.Select(p => p.Key));
                result.Add(i, topString);
            }
        }

        public void TopScoringSentences()
        {
            var topSentences = 20;
            var topPerYear = 5;
            var classifiedQueenSpeechService = new ClassifiedQueenSpeechService();
            var classifiedTexts = classifiedQueenSpeechService.Read();
            TfIdfService.GetTfIdfValues(classifiedTexts);

            //var result = new Dictionary<double, ClassifiedText>();
            foreach (var classifiedText in classifiedTexts)
            {
                var sum = classifiedText.TermWeights.Sum(p => p.Value);
                var average = sum/classifiedText.Words.Count;
                classifiedText.AverageTermWeight = average;
            }

            var result2 = new Dictionary<int, string>();
            for (int i = 1995; i <= 2010; i++)
            {
                var cts =
                    classifiedTexts.Where(p => p.Year == i).OrderByDescending(p => p.AverageTermWeight).Take(topPerYear);
                result2.Add(i, string.Join("\", \"", cts.Select(p => p.Text)));
            }
            
            var result = classifiedTexts.OrderByDescending(p => p.AverageTermWeight).Take(topSentences);
        }
    }
}
