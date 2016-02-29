using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml.Spreadsheet;
using TextClassification.Common.Extension;
using TextClassification.Model;

namespace TextClassification.Service.Dataset.Excel
{
    public class KeywordService : BaseExcel
    {
        private readonly string[] _clean = new[]
                                      {
                                          "~1", "~2", "~3", "~4", "~5", "~6", "~7", "~8", "~9", "~10", "~11", "~12", "~13",
                                          "~14", "~15", "~16", "~17", "~18", "~19", "~20", "(", ")", "/", "”", "“", "\"", "AND", "OR"
                                      }.Reverse().ToArray();
        public List<Keywords> Read(string path)
        {
            var result = ReadRows(path);
            return result;
        }

        private List<Keywords> ReadRows(string path)
        {
            using (Init(path))
            {
                Sst = WorkbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ToList();
                var first = true;
                var list = new List<Keywords>();
                foreach (var row in SheetData.Elements<Row>().ToList())
                {
                    var k = new Keywords();
                    if (first)
                    {
                        first = false;
                        continue;
                    }
                    foreach (var cell in row.Elements<Cell>().ToList())
                    {
                        switch (cell.CellReference.ToString().Substring(0, 1))
                        {
                            case "A":
                                k.SpecificCode = GetValue(cell).Replace(" ", "").Substring(0, 4).ToInt();//  GetInt(GetValue(cell).Replace(" ", "").Substring(0, 4));
                                k.GeneralCode = k.SpecificCode/100;
                                break;
                            case "B":
                                var text = GetValue(cell);
                                text = _clean.Aggregate(text, (current, c) => current.Replace(c, " ")).TrimSpaces();
                                //var matches = Regex.Matches(text, @""".*?""");
                                //foreach (var match in matches)
                                //{
                                //    var sp = match.ToString().Split(new[] { ' ' });
                                //    foreach (var m in sp)
                                //    {
                                //        var v = m.Replace(" ", "");
                                //        if (!string.IsNullOrEmpty(m))
                                //            k.Words.Add(v);
                                //    }
                                //    k.Words.Add(match.ToString().Replace("\"", ""));
                                //    text = text.Replace(match.ToString(), "");
                                //}

                                var splitted = text.Split(new [] { ' ' });
                                foreach (var match in splitted)
                                {
                                    var v = match.Replace(" ", "");
                                    if (!string.IsNullOrEmpty(match))
                                        k.Words.Add(v);
                                }
                                foreach (var match in splitted.Prepare())
                                {
                                    var v = match.Replace(" ", "");
                                    if (!string.IsNullOrEmpty(match))
                                        k.PreparedWords.Add(v);
                                }
                                break;
                        }
                    }
                    list.Add(k);
                }
                return list;
            }
        }
    }
}
