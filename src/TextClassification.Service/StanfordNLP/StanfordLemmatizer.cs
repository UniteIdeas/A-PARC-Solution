using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using edu.stanford.nlp.ling;
using edu.stanford.nlp.pipeline;
using edu.stanford.nlp.util;
using ikvm.extensions;
using java.util;
using TextClassification.Common.Extension;
using TextClassification.Model;
using TextClassification.Repository;

namespace TextClassification.Service.StanfordNLP
{
    
    public class StanfordLemmatizer
    {
        private readonly StanfordCoreNLP pipeline;
        public StanfordLemmatizer()
        {
            //https://sergey-tihon.github.io/Stanford.NLP.NET/StanfordCoreNLP.html
            const string jarRoot = @"C:\Users\Wouter\Documents\Libraries\stanford-corenlp-full-2015-12-09\models";
            // const string text = "Kosgi Santosh sent an email to Stanford University SHELL 6 six DNB Germany. He didn't get a reply. Walking walk walked";

            var props = new Properties();
            props.setProperty("annotators", "tokenize, ssplit, pos, lemma");
            props.setProperty("sutime.binders", "0");

            var curDir = Environment.CurrentDirectory;
            Directory.SetCurrentDirectory(jarRoot);
            pipeline = new StanfordCoreNLP(props);
            Directory.SetCurrentDirectory(curDir);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="text">One word only</param>
        /// <returns></returns>
        public StemWord GetResult(string text)
        {
            var result = new StemWord {Text = text};

            //excecute
            var annotation = new Annotation(text);
            pipeline.annotate(annotation);

            //get data
            var tokenClass = ExtensionMethods.getClass(new CoreAnnotations.TokensAnnotation());
            var tokens = (AbstractList)annotation.get(tokenClass);
            var lemmas = new List<string>();
            foreach (ArrayCoreMap token in tokens)
            {
                var posClass = ExtensionMethods.getClass(new CoreAnnotations.PartOfSpeechAnnotation());
                var lemmaClass = ExtensionMethods.getClass(new CoreAnnotations.LemmaAnnotation());
                //var textClass = ExtensionMethods.getClass(new CoreAnnotations.OriginalTextAnnotation());

                //var word = token.get(textClass);
                var pos = token.get(posClass); //documentation of the pos tags (Pennsylvania (Penn) Treebank Tag-set): https://www.ling.upenn.edu/courses/Fall_2003/ling001/penn_treebank_pos.html, http://www.comp.leeds.ac.uk/amalgam/tagsets/upenn.html
                var lemma = token.get(lemmaClass);
                
                if (lemmas.Count == 0)
                    result.Type = pos.ToString().ToUpper();
                lemmas.Add(lemma.ToString());

                //Console.WriteLine("Text: {0} \t , POS: {1} \t , Lemma: {2}", word, pos, lemma);
            }

            result.Stem = string.Join(" ", lemmas.ToArray());
            return result;
        }

        public string GetPoS(string word)
        {
            word = word.Clean();
            if (string.IsNullOrEmpty(word))
                return string.Empty;

            var stemWordRepository = new StemWordRepository();
            var result = stemWordRepository.Find(word);

            if (result != null) return result.Type;

            Console.WriteLine("Getting information from Stanford about the word: {0}", word);
            result = GetResult(word);

            if (result == null)
                throw new Exception("No result");

            //save stem
            stemWordRepository.Add(result);

            return result.Type;
        }

        //private string GetType(string tag)
        //{
        //    //List of PoS: http://www.ling.upenn.edu/courses/Fall_2003/ling001/penn_treebank_pos.html
        //    /* Supported type in this program
        //  * "N"
        //    "ADJ"
        //    "WW"
        //    "TW"
        //    "BW"
        //    "VNW"
        //    "VG"
        //    "VZ"
        //    "SPEC"
        //    "LID"
        //    "LET"
        //    "TSW"
        //  */
        //    var result = "Other";
        //    tag = tag.ToLower();
        //    if (tag.Contains("n"))
        //    {
        //        result = "N";
        //    } 
        //    else if (tag.Contains("v"))
        //    {
        //        result = "WW";
        //    }
        //    else if (tag.Contains("j"))
        //    {
        //        result = "ADJ";
        //    }
        //    else
        //    {
        //        return tag;
        //    }
        //    return result;
        //}
    }
}
