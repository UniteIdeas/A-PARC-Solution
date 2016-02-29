using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextClassification.Model;
using TextClassification.Common;

namespace TextClassification.Service.Dataset.Excel
{
    public class DataSetService
    {
        public List<Model.DataSet> GetDataSet(string pathClassifiedText = Constants.PathClassifiedText
            , string pathKeywords = Constants.PathKeywords
            , int ignoreYear = 0)
        {
            var result = new List<Model.DataSet>();
            var clasifiedService = new ClassifiedQueenSpeechService();
            var classifiedTexts = clasifiedService.Read(pathClassifiedText);

            var keywordService = new KeywordService();
            var keywords = keywordService.Read(pathKeywords);

            //Load classified text
            foreach (var ct in classifiedTexts)
            {
                if(ct.Year == ignoreYear)
                    continue;

                //find dataset
                var ds = result.Find(p => p.GeneralCode == ct.GeneralCode);
                if (ds == null)
                {
                    ds = new Model.DataSet {GeneralCode = ct.GeneralCode};
                    result.Add(ds);
                }

                foreach (var word in ct.PreparedWords)
                {
                    //var w = ds.Words.Find(p => p.Text.Equals(word.ToLower()));
                    //if (w == null)
                    //{
                    //    w = new Word {Text = word.ToLower()};
                    //    ds.Words.Add(w);
                    //}

                    //w.Count++;
                    if(!ds.Words.Contains(word))
                    {
                        ds.Words.Add(word);
                    }
                }
            }

            //Load keywords
            foreach (var keyword in keywords)
            {
                //find dataset
                var ds = result.Find(p => p.GeneralCode == keyword.GeneralCode);
                if (ds == null)
                {
                    ds = new Model.DataSet { GeneralCode = keyword.GeneralCode };
                    result.Add(ds);
                }

                foreach (var word in keyword.PreparedWords)
                {
                    //var w = ds.Words.Find(p => p.Text.Equals(word.ToLower()));
                    //if (w == null)
                    //{
                    //    w = new Word { Text = word.ToLower() };
                    //    ds.Words.Add(w);
                    //}

                    //w.Count++;
                    if (!ds.Words.Contains(word))
                        ds.Words.Add(word);
                }
            }
            result = result.OrderBy(p => p.GeneralCode).ToList();
            return result;
        }

        public List<Model.DataSet> GetDataSet(List<ClassifiedText> classifiedTexts, string pathKeywords = Constants.PathKeywords, int ignoreYear = 0)
        {
            var result = new List<Model.DataSet>();

            var keywordService = new KeywordService();
            var keywords = keywordService.Read(pathKeywords);

            //Load classified text
            foreach (var ct in classifiedTexts)
            {
                if (ct.Year == ignoreYear)
                    continue;

                //find dataset
                var ds = result.Find(p => p.GeneralCode == ct.GeneralCode);
                if (ds == null)
                {
                    ds = new Model.DataSet { GeneralCode = ct.GeneralCode };
                    result.Add(ds);
                }

                foreach (var word in ct.PreparedWords)
                {
                    //var w = ds.Words.Find(p => p.Text.Equals(word.ToLower()));
                    //if (w == null)
                    //{
                    //    w = new Word {Text = word.ToLower()};
                    //    ds.Words.Add(w);
                    //}

                    //w.Count++;
                    if (!ds.Words.Contains(word))
                    {
                        ds.Words.Add(word);
                    }
                }
            }

            //Load keywords
            foreach (var keyword in keywords)
            {
                //find dataset
                var ds = result.Find(p => p.GeneralCode == keyword.GeneralCode);
                if (ds == null)
                {
                    ds = new Model.DataSet { GeneralCode = keyword.GeneralCode };
                    result.Add(ds);
                }

                foreach (var word in keyword.PreparedWords)
                {
                    //var w = ds.Words.Find(p => p.Text.Equals(word.ToLower()));
                    //if (w == null)
                    //{
                    //    w = new Word { Text = word.ToLower() };
                    //    ds.Words.Add(w);
                    //}

                    //w.Count++;
                    if (!ds.Words.Contains(word))
                        ds.Words.Add(word);
                }
            }
            result = result.OrderBy(p => p.GeneralCode).ToList();
            return result;
        }

        public List<Model.DataSet> GetNeighborDataSet(string pathClassifiedText = Constants.PathClassifiedText, int ignoreYear = 0, int kNeighbors = 1)
        {
            var result = new List<Model.DataSet>();
            var clasifiedService = new ClassifiedQueenSpeechService();
            var classifiedTexts = clasifiedService.Read(pathClassifiedText);

            //Load classified text
            foreach (var ct in classifiedTexts)
            {
                if (ct.Year == ignoreYear)
                    continue;

                //find dataset
                var ds = result.Find(p => p.GeneralCode == ct.GeneralCode);
                if (ds == null)
                {
                    ds = new Model.DataSet { GeneralCode = ct.GeneralCode };
                    result.Add(ds);
                }
                for (int i = 0; i < ct.PreparedWords.Count; i++)
                {
                    if((i+kNeighbors) >= ct.PreparedWords.Count)
                        continue;
                    //var word = ct.PreparedWords[i];
                    var word = new StringBuilder();
                    word.Append(ct.PreparedWords[i]);
                    for (int x = 1; x <= kNeighbors; x++)
                    {
                        word.AppendFormat(" {0}", ct.PreparedWords[i + x]);
                    }

                    if (!ds.Words.Contains(word.ToString()))
                    {
                        ds.Words.Add(word.ToString());
                    }
                }
            }

            result = result.OrderBy(p => p.GeneralCode).ToList();
            return result;
        }

        public string ToHtml(List<Model.DataSet> list)
        {
            var result = new StringBuilder();
            result.AppendLine("<html><table><tr><th>General code</th><th>Amount</th></tr>");
            foreach (var dataSet in list)
            {
                result.AppendLine(string.Format("<tr><td>{0}</td><td>{1}</td></tr>", dataSet.GeneralCode, dataSet.Words.Count));
            }
            result.AppendLine("</table></html>");
            return result.ToString();
        }


        public List<Word> CountWords(string pathClassifiedText = Constants.PathClassifiedText
            , string pathKeywords = Constants.PathKeywords)
        {
            var list = new List<Word>();
            
            var clasifiedService = new ClassifiedQueenSpeechService();
            var classifiedTexts = clasifiedService.Read(pathClassifiedText);

            var keywordService = new KeywordService();
            var keywords = keywordService.Read(pathKeywords);

            //Load classified text
            foreach (var ct in classifiedTexts)
            {
                foreach (var word in ct.Words)
                {
                    var w = list.Find(p => p.Text.Equals(word.ToLower()));
                    if (w == null)
                    {
                        w = new Word { Text = word.ToLower() };
                        list.Add(w);
                    }

                    w.Count++;
                }
            }

            //Load keywords
            foreach (var keyword in keywords)
            {
                foreach (var word in keyword.Words)
                {
                    var w = list.Find(p => p.Text.Equals(word.ToLower()));
                    if (w == null)
                    {
                        w = new Word { Text = word.ToLower() };
                        list.Add(w);
                    }

                    w.Count++;
                }
            }

            return list;
        }

        public string ToHtml(List<Word> list)
        {
            var result = new StringBuilder();
            list = list.OrderByDescending(p => p.Count).ToList();

            result.AppendLine("<html><table><tr><th>Word</th><th>Amount</th></tr>");
            foreach (var word in list)
            {
                result.AppendLine(string.Format("<tr><td>{0}</td><td>{1}</td></tr>", word.Text, word.Count));
            }
            result.AppendLine("</table></html>");
            return result.ToString();

        }
    }


}
