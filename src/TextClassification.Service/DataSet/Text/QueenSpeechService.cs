using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextClassification.Common.Extension;
using TextClassification.Model;
using TextClassification.Service.Classification;

namespace TextClassification.Service.Dataset.Text
{
    public class QueenSpeechService : BaseText
    {
        private readonly string[] _clean = new [] {"<p>", "\r", "\n", "<b>", "</b>", "<br>", "<br />", Environment.NewLine};
        public QueenSpeech Read(string path)
        {
            var q = new QueenSpeech();
            var content = GetFileContent(path);
            q.Name = GetFileName(path);
            q.Year = int.Parse(q.Name.Remove(q.Name.LastIndexOf('.')));
            content = _clean.Aggregate(content, (current, s) => current.Replace(s, "")); 
            var splitter = new [] {"</p>"};
            var splitted = content.Split(splitter, StringSplitOptions.RemoveEmptyEntries);
            var number = 1;
            foreach (var s in splitted)
            {
                var p = new Paragraph
                            {
                                Number = number,
                                Text = s,
                            };

                var sentenceSpliter = new [] { ". ", ".", "? ", "?", "! ", "!" };
                foreach (var sentence in s.Split(sentenceSpliter, StringSplitOptions.RemoveEmptyEntries))
                {
                    var sen = new Sentence
                                       {
                                           Text = sentence.Clean(),
                                           Words = sentence.Clean().Split(' ').ToList()
                                       };
                    var stemService = new StemService();
                    var stringBuilder = new StringBuilder();
                    foreach (var word in sen.Text.Split(' ').Prepare().ToList())
                    {
                        var w = stemService.GetStem(word);
                        sen.PreparedWords.Add(w);
                        stringBuilder.AppendFormat("{0} ", w);
                    }
                    var preparedText = stringBuilder.ToString();
                    sen.PreparedText = preparedText.Remove(preparedText.Length - 1);
                    
                    p.Sentences.Add(sen);
                }

                q.Paragraphs.Add(p);

                number++;
            }
            return q;
        }
        public QueenSpeech Read(string path, List<ClassifiedText> classifiedTexts)
        {
            var q = new QueenSpeech();
            var content = GetFileContent(path);
            q.Name = GetFileName(path);
            q.Year = int.Parse(q.Name.Remove(q.Name.LastIndexOf('.')));
            content = _clean.Aggregate(content, (current, s) => current.Replace(s, ""));
            var splitter = new[] { "</p>" };
            var splitted = content.Split(splitter, StringSplitOptions.RemoveEmptyEntries);
            var number = 1;
            foreach (var s in splitted)
            {
                var p = new Paragraph
                {
                    Number = number,
                    Text = s,
                };

                var sentenceSpliter = new[] { ". ", ".", "? ", "?", "! ", "!" };
                foreach (var sentence in s.Split(sentenceSpliter, StringSplitOptions.RemoveEmptyEntries))
                {
                    var sen = new Sentence
                    {
                        Text = sentence.Clean(),
                        Words = sentence.Clean().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList()
                    };
                    if(string.IsNullOrEmpty(sen.Text)) continue;
                    var stemService = new StemService();
                    var stringBuilder = new StringBuilder();
                    foreach (var word in sen.Text.Split(' ').Prepare().ToList())
                    {
                        var w = stemService.GetStem(word);
                        sen.PreparedWords.Add(w);
                        stringBuilder.AppendFormat("{0} ", w);
                    }
                    var preparedText = stringBuilder.ToString();
                    sen.PreparedText = preparedText.Remove(preparedText.Length - 1);

                    if (classifiedTexts.Count(pre => pre.PreparedText.Equals(sen.PreparedText)) == 0)
                        continue;

                    sen.ClassifiedText = classifiedTexts.First(pre => pre.PreparedText.Equals(sen.PreparedText));

                    p.Sentences.Add(sen);
                }

                q.Paragraphs.Add(p);

                number++;
            }
            return q;
        }
    }
}
