using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using DocumentFormat.OpenXml.Spreadsheet;
using Polenter.Serialization;
using TextClassification.Common.Extension;
using TextClassification.Model;
using TextClassification.Service.Classification;
using TextClassification.Service.Dataset.Excel;

namespace TextClassification.Service.DataSet.Excel
{
    public class ClassifiedSDGSService : BaseExcel
    {
        public List<ClassifiedText> List
        {
            get
            {
                var mc = MemoryCache.Default;
                var result = mc["classified_texts"] as List<ClassifiedText>;
                if (result != null)
                    return result;

                var serializer = new SharpSerializer();
                try
                {
                    return serializer.Deserialize("classified_texts.xml") as List<ClassifiedText>;
                }
                catch (FileNotFoundException)
                {
                    return null;
                }
            }
            set
            {
                var mc = MemoryCache.Default;
                mc.Set("classified_texts", value, DateTimeOffset.Now.AddDays(7));

                var serializer = new SharpSerializer();
                serializer.Serialize(value, "classified_texts.xml");
            }
        }

        public List<ClassifiedText> Read(string path = Common.Constants.PathClassifiedText, bool reload = false)
        {
            Console.WriteLine("Reading classified text...");
            //TODO: Add check that is is the latest version. Maybe by word count?
            if (!reload)
                return List;

            var result = ReadRows(path);
            if (reload)
                List = result;

            return result;
        }

        private static List<ClassifiedText> ReadRows(string path)
        {
            using (Init(path))
            {
                Sst = WorkbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ToList();
                var first = true;
                var list = new List<ClassifiedText>();
                var elements = SheetData.Elements<Row>().ToList();
                var stemService = new StemService();

                //for the console output
                var total = elements.Count;
                var number = 1;

                foreach (var row in elements)
                {
                    var c = new ClassifiedText();
                    if (first) //skip first row
                    {
                        first = false;
                        continue;
                    }

                    foreach (var cell in row.Elements<Cell>().ToList())
                    {
                        switch (cell.CellReference.ToString().Substring(0, 1))
                        {
                            case "B":
                                c.GeneralCodes.Add(GetValue(cell).ToInt());
                                break;
                            case "A":
                                Console.WriteLine("processing row: {0} out of {1}", number, total);
                                number++;

                                c.Value = GetValue(cell, c, false).Clean();
                                c.Text = GetValue(cell, c).Clean();
                                c.Words = c.Text.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList();
                                
                                var stringBuilder = new StringBuilder();
                                foreach (var word in c.Words.Prepare())
                                {
                                    var w = stemService.GetStem(word);
                                    c.PreparedWords.Add(w);
                                    if (!c.UniquePreparedWords.Contains(w))
                                        c.UniquePreparedWords.Add(w);
                                    stringBuilder.AppendFormat("{0} ", w);
                                }
                                var preparedText = stringBuilder.ToString();
                                c.PreparedText = preparedText.Remove(preparedText.Length - 1);
                                break;
                        }
                    }
                    if (c.GeneralCodes.Count == 0)
                    {
                        c.GeneralCodes.Add(0);
                        c.SpecificCodes.Add(0);
                    }
                    list.Add(c);
                }
                return CheckForDoubles(list);
            }
        }


        private static List<ClassifiedText> CheckForDoubles(IEnumerable<ClassifiedText> list)
        {
            Console.WriteLine("Checking for double sentences and combining them");
            var result = new List<ClassifiedText>();
            foreach (var ct in list)
            {
                var r = result.FirstOrDefault(p => p.Text == ct.Text && p.Year == ct.Year);
                if (r == null)
                {
                    result.Add(ct);
                }
                else
                {
                    foreach (var generalCode in ct.GeneralCodes)
                    {
                        if (!r.GeneralCodes.Contains(generalCode))
                            r.GeneralCodes.Add(generalCode);
                    }
                    foreach (var specificCode in ct.SpecificCodes)
                    {
                        if (!r.SpecificCodes.Contains(specificCode))
                            r.SpecificCodes.Add(specificCode);
                    }
                }
            }
            return result;
        }
    }
}
