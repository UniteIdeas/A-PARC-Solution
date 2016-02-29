using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextClassification.Common;
using TextClassification.Common.Extension;
using TextClassification.Model;
using TextClassification.Model.Statistic;
using TextClassification.Service.Dataset.Excel;
using TextClassification.Service.Dataset.Text;

namespace TextClassification.Service.Statistic
{
    public class CodePerParagraphService
    {
        //public List<CodePerParagraph> Calculate(List<ClassifiedText> c, QueenSpeech q)
        //{
        //    q = Combine(c, q);
        //    var result = new List<CodePerParagraph>();
        //    foreach (var p in q.Paragraphs)
        //    {
        //        var data = new CodePerParagraph { ParagraphNumber = p.Number, TotalSentences = p.Sentences.Count() };
        //        if (p.Sentences.Count < 2)
        //            data.IsValid = false;

        //        foreach (var s in p.Sentences)
        //        {
        //            if (s.Classifications.Count == 0)
        //                data.IsValid = false;
        //            foreach (var classification in s.Classifications)
        //            {
        //                data.Total++;
        //                if (!data.Values.ContainsKey(classification.GeneralCode))
        //                    data.Values.Add(classification.GeneralCode, 0);

        //                data.Values[classification.GeneralCode]++;
        //            }
        //        }

        //        //calculate percentage
        //        foreach (var d in data.Values)
        //        {
        //            // ReSharper disable RedundantCast
        //            var percentage = ((double)d.Value / (double)data.Total) * (double)100;
        //            // ReSharper restore RedundantCast
        //            data.Percentage.Add(d.Key, percentage.RoundUp());
        //        }
        //        if (data.Percentage.Count == 0)
        //            data.IsValid = false;

        //        if (data.Percentage.Count > 0)
        //            data.Highest = data.Percentage.OrderByDescending(o => o.Value).First();
        //        result.Add(data);
        //    }

        //    ////calculate percentage
        //    //foreach(var r in result)
        //    //{
        //    //    foreach(var data in r.Values)
        //    //    {
        //    //        var percentage = ((double)data.Value / (double)r.Total) * (double)100;
        //    //        r.Percentage.Add(data.Key, percentage.RoundUp());
        //    //    }
        //    //    r.Highest = r.Percentage.OrderByDescending(p => p.Value).First();
        //    //}

        //    return result;
        //}

        public string ToHtml(List<CodePerParagraph> list, int year)
        {
            var strB = new StringBuilder();
            strB.AppendLine(string.Format("<h2>{0}</h2>", year));
            strB.AppendLine("<table><tr><th>Paragraph</th><th>Highest Percentage</th></tr>");

            foreach (var r in list)
            {
                if (r.IsValid)
                    strB.AppendLine(string.Format("<tr><td>{0}</td><td>{1}%</td></tr>", r.ParagraphNumber, r.Highest.Value));
            }
            strB.AppendLine("</table>");
            return strB.ToString();
        }

        //All statistics per paragraph average
        public string ToHtml(List<CodePerParagraph> list)
        {
            var strB = new StringBuilder();
            strB.AppendLine("<h2>Total average</h2>");
            strB.AppendLine("<table><tr><th>Paragraph</th><th>Average</th><th>Count</th></tr>");

            var result = new List<Average>();
            foreach (var r in list)
            {
                if (r.IsValid)
                {
                    var avg = result.FirstOrDefault(c => c.Paragraph == r.ParagraphNumber);
                    if (avg == null)
                    {
                        avg = new Average { Paragraph = r.ParagraphNumber };
                        result.Add(avg);
                    }

                    avg.Count++;
                    avg.Value = avg.Value + r.Highest.Value;

                }
            }
            result = result.OrderBy(c => c.Paragraph).ToList();
            foreach (var average in result)
            {
                if (average.Count > 1)
                    strB.AppendLine(string.Format("<tr><td>{0}</td><td>{1}%</td><td>{2}</td></tr>", average.Paragraph, average.Value / (double)average.Count, average.Count));
            }
            strB.AppendLine("</table>");
            return strB.ToString();
        }

        //public string AverageHighestPercentage(int startQueenSpeech = 1979,
        //    int endQueenSpeech = 2010,
        //    string path = Constants.FolderQueenSpeeches,
        //    string extension = Constants.QueenSpeechExtension,
        //    string classifiedTextsPath = Constants.PathClassifiedText)
        //{
        //    //const int start = 1979;
        //    //const int end = 2010;
        //    var s3 = new QueenSpeechService();
        //    var s4 = new CodePerParagraphService();
        //    var s = new ClassifiedQueenSpeechService();
        //    var classifiedTexts = s.Read(classifiedTextsPath);
        //    var html = new StringBuilder();
        //    var allStatistics = new List<CodePerParagraph>();
        //    for (var i = startQueenSpeech; i <= endQueenSpeech; i++)
        //    {
        //        var queenSpeech = s3.Read(string.Format(@"{0}{1}{2}", path, i, extension));
        //        var statistics = s4.Calculate(classifiedTexts, queenSpeech);
        //        html.AppendLine(s4.ToHtml(statistics, queenSpeech.Year));
        //        var avg = s4.AverageHighestPercentage(statistics);
        //        html.AppendLine(string.Format("<b>Avg: {0}%</b>", avg));
        //        allStatistics = allStatistics.Concat(statistics).ToList();
        //    }
        //    var totalAvg = s4.AverageHighestPercentage(allStatistics);
        //    html.AppendLine(string.Format("<h3>Total Avg: {0}%</h3>", totalAvg));
        //    html.AppendLine(s4.ToHtml(allStatistics));
        //    var result = html.ToString();
        //    return result;
        //}

        ///// <summary>
        ///// Only paragraphs with more than one senctence
        ///// </summary>
        ///// <param name="list"></param>
        ///// <returns></returns>
        //public double AverageHighestPercentage(List<CodePerParagraph> list)
        //{
        //    var total = 0;
        //    double count = 0;
        //    foreach (var r in list.Where(r => r.IsValid))
        //    {
        //        total++;
        //        count = count + r.Highest.Value;
        //    }

        //    return (count / total).RoundUp();
        //}

        private static QueenSpeech Combine(List<ClassifiedText> c, QueenSpeech q)
        {
            foreach (var sentence in q.Paragraphs.SelectMany(paragraph => paragraph.Sentences))
            {
                sentence.Classifications = GetClassifiedText(c, q.Year, sentence.Text);
            }
            return q;
        }

        private static List<ClassifiedText> GetClassifiedText(List<ClassifiedText> c, int year, string sentence)
        {
            return c.FindAll(o => o.Year == year && o.Text.ToLower().Equals(sentence.ToLower()));
        }
    }
}
