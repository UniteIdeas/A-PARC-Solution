using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextClassification.Model;
using TextClassification.Service.Dataset.Excel;

namespace TextClassification.Service.Statistic
{
    public class CodeAnalysisService
    {
        //public string CodeDistribution(int startYear = 1985, int endYear = 2010)
        //{
        //    var result = new StringBuilder();
        //    var service = new ClassifiedQueenSpeechService();
        //    var ct = service.Read();
        //    result.AppendLine("<html>");
        //    for (var i = startYear; i <= endYear; i++)
        //    {
        //        var list = ct.Where(p => p.Year == i).ToList();
        //        result.AppendLine(CodeDistribution(list, i));
        //    }
        //    result.AppendLine("</html>");
        //    return result.ToString();
        //}
        //public string CodeDistribution(List<ClassifiedText> qs, int year)
        //{
        //    var result = new StringBuilder();
        //    result.AppendFormat("<h2>{0}</h2>", year);
        //    result.AppendLine("<table>");
        //    result.AppendLine("<tr><th>Row</th><th>General code</th><th>Specific code</th></tr>");
        //    var row = 0;
        //    foreach (var classifiedText in qs)
        //    {
        //        row++;
        //        result.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>", row, classifiedText.GeneralCode, classifiedText.SpecificCode);
        //    }
        //    result.AppendLine("</table>");

        //    return result.ToString();
        //}
    }
}
