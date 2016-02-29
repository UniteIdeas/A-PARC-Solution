using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextClassification.Common.Extension;
using TextClassification.Model;
using TextClassification.Service.Classification;
using TextClassification.Service.Dataset.Text;

namespace TextClassification.Service.DataSet.Text
{
    public class TextService : BaseText
    {
        private readonly string[] _clean = new[] { "<p>", "</p>", "\r", "\n", "<b>", "</b>", "<br>", "<br />", Environment.NewLine };

        public List<ClassifiedText> Read(string path)
        {
            var result = new List<ClassifiedText>();
            var content = GetFileContent(path);
            content = _clean.Aggregate(content, (current, s) => current.Replace(s, " "));

            var sentenceSpliter = new[] { ". ", ".", "? ", "?", "! ", "!" };
            var sentences = content.Split(sentenceSpliter, StringSplitOptions.RemoveEmptyEntries);
            var total = sentences.Count();
            Console.WriteLine("Reading {0} sentences from input text", total);
            var count = 1;
            foreach (var sentence in sentences)
            {
                Console.WriteLine("Working on input sentence {0} of {1}.", count, total);
                var sen = new ClassifiedText
                {
                    Text = sentence.Clean(),
                    Words = sentence.Clean().Split(' ').ToList()
                };
                if (string.IsNullOrEmpty(sen.Text))
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

                result.Add(sen);
                count++;
            }
            
            return result;
        }

        public ClassifiedText ReadSentence(string text)
        {
            text = _clean.Aggregate(text, (current, s) => current.Replace(s, ""));

            var result = new ClassifiedText
            {
                Text = text.Clean(),
                Words = text.Clean().Split(new [] {' '}, StringSplitOptions.RemoveEmptyEntries).ToList()
            };
            var stemService = new StemService();
            var stringBuilder = new StringBuilder();
            foreach (var word in result.Text.Split(' ').Prepare().ToList())
            {
                var w = stemService.GetStem(word);
                result.PreparedWords.Add(w);
                stringBuilder.AppendFormat("{0} ", w);
            }
            var preparedText = stringBuilder.ToString();
            result.PreparedText = preparedText.Remove(preparedText.Length - 1);

            return result;
        }
    }
}
