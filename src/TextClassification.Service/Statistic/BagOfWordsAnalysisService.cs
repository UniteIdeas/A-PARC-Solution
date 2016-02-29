using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using TextClassification.Common.Extension;
using TextClassification.Service.Dataset.Excel;

namespace TextClassification.Service.Statistic
{
    public class BagOfWordsAnalysisService
    {
        public string CountBags()
        {
            var classifiedQueenSpeechService = new ClassifiedQueenSpeechService();
            var classifiedTexts = classifiedQueenSpeechService.Read();
            var results = new Dictionary<int[], int>(new ArrayValueComparer<int>());
            
            foreach (var classifiedText in classifiedTexts)
            {
                var ids = classifiedText.GeneralCodes.OrderBy(p => p).ToArray();

                if(results.ContainsKey(ids))
                {
                    results[ids]++;
                } 
                else
                {
                    results.Add(ids, 1);
                }
            }

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("<table><tr><th>ids<th><th>count</th></tr>");
            foreach (var result in results.OrderByDescending(p => p.Value))
            {
                var key = String.Join(", ", new List<int>(result.Key).ConvertAll(i => i.ToString(CultureInfo.InvariantCulture)).ToArray());
                stringBuilder.AppendLine(string.Format("<tr><td>{0}</td><td>{1}</td></tr>", key, result.Value));
            }
            stringBuilder.AppendLine("</table>");
            stringBuilder.AppendLine(string.Format("<h3>Total bags: {0}</h3>", results.Count));
            var s = stringBuilder.ToString();
            return s;
        }
    }
}
