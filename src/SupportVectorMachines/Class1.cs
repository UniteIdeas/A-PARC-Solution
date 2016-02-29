using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextClassification.Model;
using TextClassification.Tf_Idf_2;
using libsvm;

namespace SupportVectorMachines
{
    public class Class1
    {
        static double C = 1;
        static double gamma = 0.05; // 1 / number of features (20 is this case)
        private static int nr_fold = 5;

        public static void DoSomething(List<ClassifiedText> list, int yearToTest)
        {
            var training = new List<string>();
            var trainingCodes = new List<string>();
            var prediction = new List<string>();
            var predictionCodes = new List<string>();
            foreach (var classifiedText in list)
            {
                if (classifiedText.GeneralCodes.Count == 0)
                    continue;

                if (classifiedText.Year == yearToTest)
                {
                    var stringBuilder = new StringBuilder();
                    foreach (var generalCode in classifiedText.GeneralCodes)
                    {
                        stringBuilder.AppendFormat("{0},", generalCode);
                    }
                    prediction.Add(classifiedText.PreparedText);
                    var label = stringBuilder.ToString();
                    predictionCodes.Add(label.Remove(label.Length - 1));
                }
                else
                {
                    var stringBuilder = new StringBuilder();
                    foreach (var generalCode in classifiedText.GeneralCodes)
                    {
                        stringBuilder.AppendFormat("{0},", generalCode);
                    }
                    training.Add(classifiedText.PreparedText);
                    var label = stringBuilder.ToString();
                    trainingCodes.Add(label.Remove(label.Length - 1));
                }
            }
            //var trainingDocuments = TFIDF.Transform(training.ToArray());
            //trainingDocuments = TFIDF.Normalize(trainingDocuments);
            //var testDocuments = TFIDF.Transform(prediction.ToArray(), 1);
            //testDocuments = TFIDF.Normalize(testDocuments);

            //var i = 0;
            //using (var file = new System.IO.StreamWriter(@"TRAINING_FILE"))
            //{
            //    foreach (var doc in trainingDocuments)
            //    {
            //        if (double.IsNaN(doc[0]))
            //            continue;
                    
            //        file.Write("{0} ", trainingCodes[i]);
            //        var featureId = 1;
            //        foreach (var d in doc)
            //        {
            //            if (!Equals(d, 0.0) && !double.IsNaN(d)) 
            //                file.Write("{0}:{1} ", featureId, d);

            //            featureId++;
            //        }
            //        file.Write(Environment.NewLine);
            //        i++;
            //    }
            //}

            //var j = 0;
            //using (var file = new System.IO.StreamWriter(@"TEST_FILE"))
            //{
            //    foreach (var doc in testDocuments)
            //    {
            //        file.Write("{0} ", predictionCodes[j]);
            //        //file.Write("0 ");
            //        var featureId = 1;
            //        foreach (var d in doc)
            //        {
            //            if (!Equals(d, 0.0) && !double.IsNaN(d))
            //                file.Write("{0}:{1} ", featureId, d);

            //            featureId++;
            //        }
            //        file.Write(Environment.NewLine);
            //        j++;
            //    }
            //}

            var prob = ProblemHelper.ReadAndScaleProblem("TRAINING_FILE");
            var test = ProblemHelper.ReadAndScaleProblem("TEST_FILE");
            var svm = new C_SVC(prob, KernelHelper.RadialBasisFunctionKernel(gamma), C);
            
            //svm.Train();
            var accuracy = svm.GetCrossValidationAccuracy(nr_fold);
            var preds = new List<Double>();
            for (int k = 0; k < test.l; k++)
            {
                var x = test.x[k];
                var y = test.y[k];

                var predict = svm.Predict(x); // returns the predicted value 'y'
                preds.Add(predict);
                var probabilities = svm.PredictProbabilities(x);  // returns the probabilities for each 'y' value
            }

            var o = 0;
            var strB = new StringBuilder();
            strB.AppendLine("<table><tr><th>Actual</th><th>Prediction</th></tr>");
            foreach (var p in predictionCodes)
            {
                strB.AppendLine(string.Format("<tr><td>{0}</td><td>{1}</td></tr>", p, preds[o]));
                o++;
            }
            strB.AppendLine("</table>");
            var result = strB.ToString();
        }
    }
}
