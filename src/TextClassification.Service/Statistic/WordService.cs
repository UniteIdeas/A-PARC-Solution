using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextClassification.Model;

namespace TextClassification.Service.Statistic
{
    public class WordService
    {
        //public string CountWords(string path = Common.Constants.PathClassifiedText)
        //{
        //    var classifiedQueenSpeechService = new ClassifiedQueenSpeechService();
        //    return CountWords(classifiedQueenSpeechService.Read(path));
        //}

        public static Dictionary<string, Word> CountWords(List<ClassifiedText> list)
        {
            Console.WriteLine("Counting words....");
            var words = new Dictionary<string, Word>();
            foreach (var ct in list)
            {
                foreach (var preparedWord in ct.PreparedWords)
                {
                    Word w;
                    if(words.ContainsKey(preparedWord))
                    {
                        w = words[preparedWord];
                    }
                    else
                    {
                        w = new Word {Text = preparedWord};
                        words.Add(preparedWord, w);
                    }
                    w.Count++;
                    //foreach (var generalCode in ct.GeneralCodes)
                    //{
                    //    if (!w.GeneralCodes.Contains(generalCode))
                    //        r.GeneralCodes.Add(generalCode);
                    //}
                    foreach (var specificCode in ct.SpecificCodes)
                    {
                        if (!w.SpecificCode.Contains(specificCode))
                            w.SpecificCode.Add(specificCode);
                    }
                }
            }

            return words;
        }

        public static string ToHtml(Dictionary<string, Word> list)
        {
            var result = new StringBuilder();
            //Print out results
            var words = list.Values.ToList().OrderByDescending(p => p.Count).ToList();
            result.AppendLine("<html><h2>Word count</h2><table>");
            result.AppendLine("<tr><th>Word</th><th>Count</th><th>Codes</th></tr>");
            foreach (var word in words)
            {
                var specificCodes = new StringBuilder();

                foreach (var sc in word.SpecificCode)
                {
                    specificCodes.AppendFormat("{0}, ", sc);
                }
                result.AppendLine(string.Format("<tr><th>{0}</th><th>{1}</th><th>{2}</th></tr>", word.Text, word.Count, specificCodes));
            }
            result.AppendLine("</table></html>");
            return result.ToString();
        }

        //public static Dictionary<string, DoubleWord> CountDoubleWords(List<ClassifiedText> list)
        //{
        //    var result = new Dictionary<string, DoubleWord>();
        //    var countWords = CountWords(list);
        //    Console.WriteLine("Counting double words...");
        //    foreach (var ct in list)
        //    {
        //        var words = CombineWords(ct.PreparedWords);
        //        foreach (var word in words)
        //        {
        //            if(result.ContainsKey(word))
        //            {
        //                //update
        //                var doubleWord = result[word];
        //                doubleWord.Count++;

        //                foreach (var generalCode in ct.GeneralCodes)
        //                {
        //                    if (!doubleWord.GeneralCodes.Contains(generalCode))
        //                        doubleWord.GeneralCodes.Add(generalCode);
        //                }
        //                foreach (var specificCode in ct.SpecificCodes)
        //                {
        //                    if (!doubleWord.SpecificCodes.Contains(specificCode))
        //                        doubleWord.SpecificCodes.Add(specificCode);
        //                }
        //            } 
        //            else
        //            {
        //                //add a new one
        //                var doubleWord = new DoubleWord
        //                                     {
        //                                         Text = word,
        //                                         GeneralCodes = ct.GeneralCodes,
        //                                         SpecificCodes = ct.SpecificCodes,
        //                                         Count = 1
        //                                     };
        //                var firstWord = word.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries)[0];
        //                if (countWords.ContainsKey(firstWord))
        //                {
        //                    doubleWord.FirstWordCount = countWords[firstWord].Count;
        //                } 
        //                else
        //                {
        //                    doubleWord.FirstWordCount = -1;
        //                }
        //                var secondWord = word.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries)[1];
        //                if (countWords.ContainsKey(secondWord))
        //                {
        //                    doubleWord.SecondWordCount = countWords[secondWord].Count;
        //                } 
        //                else
        //                {
        //                    doubleWord.SecondWordCount = -1;
        //                }
        //                result.Add(word, doubleWord);
        //            }
        //        }
        //    }
        //    return result;
        //}

        public static string ToHtml(Dictionary<string, DoubleWord> list)
        {
            Console.WriteLine("Converting results to a html table");
            var words = list.Values.ToList().OrderByDescending(p => p.Count).ToList();
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("<table><tr><th>Text</th><th>Count</th><th>Codes</th><th>First word count</th><th>Second word count</th></tr>");
            foreach (var d in words)
            {
                var generalCodes = new StringBuilder();
                foreach (var code in d.GeneralCodes)
                {
                    generalCodes.AppendFormat("{0}, ", code);
                }
                stringBuilder.AppendLine(string.Format("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td></tr>", d.Text, d.Count, generalCodes, d.FirstWordCount, d.SecondWordCount));
            }
            stringBuilder.AppendLine("</table>");
            return stringBuilder.ToString();
        }

        private static IEnumerable<string> CombineWords(List<string> words)
        {
            var result = new List<string>();
            for (int i = 0; i < words.Count; i++)
            {
                var currentWord = words[i];
                if ((i + 1) != words.Count)
                {
                    //not latest word
                    for (int j = (i + 1); j < words.Count; j++)
                    {
                        result.Add(string.Format("{0} {1}", currentWord, words[j]));
                    }
                }
            }
            return result;
        }
    }
}
