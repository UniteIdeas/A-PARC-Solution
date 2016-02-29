using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetMatrix;
using Polenter.Serialization;
using TextClassification.Service.Dataset.Excel;
using TextClassification.Tf_Idf;

namespace TextClassification.Service
{
    public class LatentSemanticAnalysisService
    {
        public void DoSomething()
        {
            var classifiedQueenSpeechService = new ClassifiedQueenSpeechService();
            var classifiedTexts = classifiedQueenSpeechService.Read();
            Console.WriteLine("Calculate tf-idf term weight");
            var docs = new List<string>();
            foreach (var ct in classifiedTexts)
            {
                docs.Add(ct.PreparedText);
            }
            var tfIdf = new TFIDFMeasure(docs.ToArray());

            //[word][weights per doc]

            SingularValueDecomposition svd;
            try
            {
                var generalMatrix = new GeneralMatrix(tfIdf._termWeight); //TODO FIX it takes to long + out of memory exception
                svd = new SingularValueDecomposition(generalMatrix);

                var serializer = new SharpSerializer();
                serializer.Serialize(svd, "singular_value_decomposition.xml");
            } catch(Exception ex)
            {
                
            }
            //var left = svd.GetU();
            //var right = svd.GetV();
            //svd.Norm2()
        }
    }
}
