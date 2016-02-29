using System;
using System.Collections.Generic;
using TextClassification.Repository;
using TextClassification.Service.StanfordNLP;

namespace TextClassification.Service.Classification
{
    public class BaseClassificationService
    {
        private Dictionary<string, string> _wordType;
        public Dictionary<string, string> WordType
        {
            get
            {
                if (_wordType != null && _wordType.Count != 0)
                    return _wordType;

                var wordRepository = new StemWordRepository();
                var list = wordRepository.List();
                _wordType = new Dictionary<string, string>();
                foreach (var frogWord in list)
                {
                    _wordType.Add(frogWord.Text, frogWord.Type);
                }
                return _wordType;
            }
            set { _wordType = value; }
        }
        
        //public Dictionary<string, double> WordTypeWeight = new Dictionary<string, double>
        //                                                       {
        //                                                           {"N", 1},
        //                                                           {"WW", 1},
        //                                                           {"ADJ", 1},
        //                                                           {"BW", 0},
        //                                                           {"SPEC", 1},
        //                                                           {"TW", 0},
        //                                                           {"VNW", 0},
        //                                                           {"VZ", 0},
        //                                                           {"VG", 0},
        //                                                           {"LET", 0},
        //                                                           {"LID", 0},
        //                                                           {"TSW", 0}
        //                                                       };
        public Dictionary<string, double> WordTypeWeight = new Dictionary<string, double>
                                                           {
                                                               {"CC", 0},
                                                               {"CD", 0},
                                                               {"DT", 0},
                                                               {"EX", 0},
                                                               {"FW", 0},
                                                               {"IN", 0},
                                                               {"JJ", 1},
                                                               {"JJR", 1},
                                                               {"JJS", 1},
                                                               {"LS", 0},
                                                               {"MD", 0},
                                                               {"NN", 1},
                                                               {"NNS", 1},
                                                               {"NNP", 1},
                                                               {"NNPS", 1},
                                                               {"PDT", 0},
                                                               {"POS", 0},
                                                               {"PRP", 0},
                                                               {"PRP$", 0},
                                                               {"RB", 0},
                                                               {"RBR", 0},
                                                               {"RBS", 0},
                                                               {"RP", 0},
                                                               {"SYM", 0},
                                                               {"TO", 0},
                                                               {"UH", 0},
                                                               {"VB", 1},
                                                               {"VBD", 1},
                                                               {"VBG", 1},
                                                               {"VBN", 1},
                                                               {"VBP", 1},
                                                               {"VBZ", 1},
                                                               {"WDT", 0},
                                                               {"WP", 0},
                                                               {"WP$", 0},
                                                               {"WRB", 0}
                                                           };

        public BaseClassificationService()
        {
            WordType = new Dictionary<string, string>();
        }

        public string GetWordType(string word)
        {
            if (WordType.ContainsKey(word))
                return WordType[word];

            var lemmatizer = new StanfordLemmatizer();
            var type = lemmatizer.GetPoS(word);
            WordType.Add(word, type);
            return type;
        }
        public double GetWordTypeWeight(string word)
        {
            if (WordType.ContainsKey(word) && WordTypeWeight.ContainsKey(WordType[word]))
                return WordTypeWeight[WordType[word]];

            if (WordType.ContainsKey(word))
            {
                Console.WriteLine("Added PoS: {0}", WordType[word]);
                WordTypeWeight.Add(WordType[word], 0);
                return 0.0;
            }

            var lemmatizer = new StanfordLemmatizer();
            var type = lemmatizer.GetPoS(word);

            if (WordTypeWeight.ContainsKey(type))
                return WordTypeWeight[type];

            WordType.Add(word, type);
            Console.WriteLine("Added PoS: {0}", WordType[word]);
            WordTypeWeight.Add(WordType[word], 0);
            return 0.0;
        }

        internal double FindWeight(Dictionary<string, double> weights, string word, int totalBags)
        {
            if (weights.ContainsKey(word))
                return weights[word];

            return 1.0 / (double)totalBags;
        }
    }
}
