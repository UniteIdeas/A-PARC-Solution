using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextClassification.Common;
using TextClassification.Common.Extension;
using TextClassification.Model;
using TextClassification.Model.Statistic;
using TextClassification.Repository;
using TextClassification.Service;
using TextClassification.Service.Classification;
using TextClassification.Service.DataSet.Excel;
using TextClassification.Service.DataSet.Text;
using TextClassification.Service.Dataset.Excel;
using TextClassification.Service.Dataset.Text;
using TextClassification.Service.Frog;
using TextClassification.Service.StanfordNLP;
using TextClassification.Service.Statistic;

namespace TextClassification
{
    class Program
    {
        static void Main(string[] args)
        {
            //var wordWeightService = new WordWeightService();
            //wordWeightService.FillDatabase(@"C:\Users\Wouter\Documents\GitHub\linkssdgs\training\Augmented\training.xlsx");

            //const string filePath = @"C:\Users\Wouter\Documents\Github\linkssdgs\input\IDFC_Investing_in_Sustainable_Cities_Paper_01-12-14.txt";// @"C:\Users\Wouter\Documents\GitHub\linkssdgs\input\IDFC_Investing_in_Sustainable_Cities_Paper_01-12-14.txt";
            const string directory = @"D:\Users\Wouter\Documents\GitHub\linkssdgs\input";
            var bagsOfWordsService = new BagOfWordsService();
            var bags = bagsOfWordsService.GetBagOfWords(
                @"D:\Users\Wouter\Documents\GitHub\linkssdgs\training\training.xlsx"
                ,reload: false);

            var count = 1;
            var total = Directory.GetFiles(directory).Count();
            foreach (var file in Directory.GetFiles(directory))
            {
                var resultLocation = String.Format("{0}\\{1}{2}", Path.GetDirectoryName(file)
                    , Path.GetFileNameWithoutExtension(file), "_out.html");
                Console.WriteLine("Working on document {0} of {1}", count, total);
                count++;
                //if(!file.ToLower().EndsWith(".txt")) continue;
                if (!file.ToLower().EndsWith(".txt") || File.Exists(resultLocation)) continue;

                var textService = new TextService();
                var sentences = textService.Read(file);
                var classifierTweaked = new ClassifierTFIDFTweakedService();

                var predictions = classifierTweaked.Classify(sentences, bags);
                var html = classifierTweaked.GetHtml(predictions, Path.GetFileName(resultLocation));

                File.WriteAllText(resultLocation, html);
            }
            //var textService = new TextService();
            //var sentences = textService.Read(filePath);
            //var classifierTweaked = new ClassifierTFIDFTweakedService();

            //var predictions = classifierTweaked.Classify(sentences, bags);
            //var html = classifierTweaked.GetHtml(predictions, System.IO.Path.GetFileName(filePath));
            var s = "";






            //var similarity = new CosineSimilarity();
            //var p = new List<double> {1.0, 0, 1.0};
            //var q = new List<double> {0.33, 0.1, 0.23};

            //var result = similarity.GetSimilarityScore(p.ToArray(), q.ToArray());

            //var classifierTfIdfService = new ClassificationTfIdfService();
            //classifierTfIdfService.GetResultsSingleAndMultiple();
            //var resultTfIdf = classifierTfIdfService.GetResults();
            //classifierTfIdfService.OptimizeFeatureSelectionTopScoringWords();
            //classifierTfIdfService.OptimizeMinimumConfidence();
            //var htmlWordScores = classifierTfIdfService.GetWordScoreResults();
            //classifierTfIdfService.GetResultsPerCategory();
            //classifierTfIdfService.OptimizeMinimumConfidence(); 

            //var classifierSimilarityService = new ClassificationSimilarityService(); // k nearest neighbor
            //classifierSimilarityService.GetResultsSingleAndMultiple();
            //classifierSimilarityService.GetResults();
            //classifierSimilarityService.OptimizeMinimumConfidence();
            //classifierSimilarityService.GetResultsPerCategory();
            //var resultKNN = classifierSimilarityService.GetResults();
            //classifierSimilarityService.OptimizeFeatureSelectionTopScoringWords();
            //classifierSimilarityService.GetWordScoreResults();

            //var classifierNaiveBayesService = new ClassifierNaiveBayesService();
            //classifierNaiveBayesService.GetResultsSingleAndMultiple();
            //var resultNaiveBayes = classifierNaiveBayesService.GetResults();
            //classifierNaiveBayesService.OptimizeMinimumConfidence();
            //classifierNaiveBayesService.GetResultsPerCategory();
            //classifierNaiveBayesService.GetWordScoreResults();
            ////classifierNaiveBayesService.OptimizeFeatureSelection();
            //classifierNaiveBayesService.OptimizeFeatureSelectionTopScoringWords();

            //var classifierUsingParagraphService = new ClassifierUsingParagraphService();
            //classifierUsingParagraphService.GetResults();
            //classifierUsingParagraphService.OptimizeMinimumConfidence();

            //var classifierQueryBagsService = new ClassifierQueryBagsService();
            //classifierQueryBagsService.GetResults();

            //var classifierTfIdfWithKeyWordsService = new ClassifierTfIdfWithKeyWordsService();
            //classifierTfIdfWithKeyWordsService.GetResults();
            //classifierTfIdfWithKeyWordsService.GetWordScoreResults();
            //classifierTfIdfWithKeyWordsService.GetResultsSingleAndMultiple();
            //classifierTfIdfWithKeyWordsService.ProductTrainingSets();
            //classifierTfIdfWithKeyWordsService.GetResultsPerCategory();
            //var result = classifierTfIdfWithKeyWordsService.GetResults();

            //var luceneService = new LuceneService();
            //luceneService.FindCategories("Maar maatregelen zijn noodzakelijk omdat de stijgende levensverwachting de pensioenuitkeringen onder druk zet");

            //var topScoringWordsService = new TopScoringWordsService();
            //topScoringWordsService.TopScoringPerCategory();
            //topScoringWordsService.TopScoringPerYear();
            //topScoringWordsService.TopScoringSentences();

            //var lsa = new LatentSemanticAnalysisService();
            //lsa.DoSomething();

            //var path = @"C:\Users\Wouter\Dropbox\Thesis\Datasets\Copy of PA QS COMPLETE 1945_2013.xlsx";
            //var wordWeightService = new WordWeightService();
            //wordWeightService.FillDatabase(path);

            //var classifierTfIdfTweakedService = new ClassifierTFIDFTweakedService();
            //classifierTfIdfTweakedService.GetResults();
            //classifierTfIdfTweakedService.GetWordScoreResults();
            //classifierTfIdfTweakedService.OptimizeMinimumAverageWordWeight();
            //classifierTfIdfTweakedService.GetResultsPerCategory();

            //var classifierAnpService = new ClassifierAnpService();
            //classifierAnpService.Classify();

        }

        //public static void DoubleWordNumbers()
        //{
        //    var classifiedQueenSpeechService = new ClassifiedQueenSpeechService();
        //    var classifiedText = classifiedQueenSpeechService.Read();
        //    var result = WordService.CountDoubleWords(classifiedText);
        //    //var table = WordService.ToHtml(result);
        //    //var result = WordService.CountWords(classifiedText);
        //    //var table = WordService.ToHtml(result);

        //    var countOneOccurence = 0;
        //    var lessThan3OccurrenceAndLessThan3Codes = 0;
        //    var total = 0;
        //    var lessThan3Codes = 0;
        //    var OneCode = 0;
        //    foreach (var doubleWord in result.Values.ToList())
        //    {
        //        if (doubleWord.Count == 1)
        //            countOneOccurence++;

        //        if (doubleWord.Count < 3 && doubleWord.GeneralCodes.Count() < 3)
        //            lessThan3OccurrenceAndLessThan3Codes++;

        //        if (doubleWord.GeneralCodes.Count() < 3)
        //            lessThan3Codes++;

        //        if (doubleWord.GeneralCodes.Count() == 1)
        //            OneCode++;

        //        total++;
        //    }
        //}
        //public static ClassifiedText ClassifySentence(string text, int ignoreYear = 0)
        //{
        //    //Don't know if this still works
        //    var bagOfWordsService = new BagOfWordsService();
        //    var bagOfWords = bagOfWordsService.GetBagOfWords(false, ignoreYear: ignoreYear);
        //    var textService = new TextService();
        //    var sentence = textService.ReadSentence(text);
        //    var classified = new ClassificationWordWeightsService();
        //    sentence.Predictions = classified.Classify(bagOfWords, sentence);
        //    return sentence;
        //}

        //public static List<ClassifiedText> ClassifyText(string path)
        //{
        //    //Don't know if this still works
        //    var textService = new TextService();
        //    var list = textService.Read(path);
        //    var bagOfWordsService = new BagOfWordsService();
        //    var bagOfWords = bagOfWordsService.GetBagOfWords(false);
        //    var classified = new ClassificationWordWeightsService();
        //    foreach (var sentence in list)
        //    {
        //        sentence.Predictions = classified.Classify(bagOfWords, sentence);
        //    }
        //    return list;
        //}

        //public static void ClassifyWordWeights()
        //{
        //    var classificationService = new ClassificationWordWeightsService();
        //    var wordRepository = new StemWordRepository();
        //    var list = wordRepository.List();
        //    foreach (var frogWord in list)
        //    {
        //        classificationService.WordType.Add(frogWord.Text, frogWord.Type);
        //    }
        //    var result2 = classificationService.ClassifyRange();
        //}

    }
}
