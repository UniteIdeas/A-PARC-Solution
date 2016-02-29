using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextClassification.Model;

namespace TextClassification.Service
{
    public static class SumNormalization
    {
        public static Dictionary<string, double> Normalize(Dictionary<string, double> termWeights)
        {
            var result = new Dictionary<string, double>();

            var sum = 0.0;
            foreach (var termWeight in termWeights)
            {
                sum += termWeight.Value;
            }

            foreach (var termWeight in termWeights)
            {
                var r = termWeight.Value / sum;

                if (double.IsNaN(r))
                    r = 0.0;

                result.Add(termWeight.Key, r);
            }

            return result;
        }

        public static Dictionary<int, Prediction> Normalize(Dictionary<int, Prediction> predictions)
        {
            var result = new Dictionary<int, Prediction>();

            var sum = 0.0;
            foreach (var prediction in predictions)
            {
                sum += prediction.Value.Confidence;
            }

            foreach (var prediction in predictions)
            {
                var r = prediction.Value.Confidence / sum;

                if (double.IsNaN(r))
                    r = 0.0;

                prediction.Value.Confidence = r;
                result.Add(prediction.Key, prediction.Value);
            }

            return result;
        }
    }
}
