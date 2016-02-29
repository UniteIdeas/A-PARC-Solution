using System.Collections.Generic;
using System.Linq;
using TextClassification.Model;

namespace TextClassification.Service
{
    public static class VectorService
    {
        public static void CreateVectors(Dictionary<int, Prediction> predictions, List<int> generalCodes
                , out double[] vectorPredictions, out double[] vectorGeneralCodes)
        {
            var v1 = new List<double>();
            var v2 = new List<double>();
            var added = new Dictionary<int, int>();

            foreach (var tw in predictions)
            {
                v1.Add(tw.Value.Confidence);

                if (generalCodes.Contains(tw.Key))
                {
                    v2.Add(1); //TODO: maybe change the default value eg. 1/k where k is the number of generalCodes
                }
                else
                {
                    v2.Add(0.0);
                }

                added.Add(tw.Key, tw.Key);
            }

            foreach (var tw in generalCodes)
            {
                if (added.ContainsKey(tw))
                    continue;

                v2.Add(1); //TODO: maybe change the default value eg. 1/k where k is the number of generalCodes
                v1.Add(0.0);
            }

            vectorPredictions = v1.ToArray();
            vectorGeneralCodes = v2.ToArray();
        }

        public static void CreateVectors(Dictionary<int[], Prediction> predictions, List<int> generalCodes
                , out double[] vectorPredictions, out double[] vectorGeneralCodes)
        {
            var v1 = new List<double>();
            var v2 = new List<double>();
            var added = new Dictionary<int, int>();

            foreach (var tw in predictions)
            {
                v1.Add(tw.Value.Confidence);

                if (tw.Key.Count() == 1 && generalCodes.Contains(tw.Key.First())) //generalcodes can only have one
                {
                    v2.Add(1); //TODO: maybe change the default value eg. 1/k where k is the number of generalCodes
                }
                else
                {
                    v2.Add(0.0);
                }

                if (tw.Key.Count() == 1)
                    added.Add(tw.Key.First(), tw.Key.First());
            }

            foreach (var tw in generalCodes)
            {
                if (added.ContainsKey(tw))
                    continue;

                v2.Add(1); //TODO: maybe change the default value eg. 1/k where k is the number of generalCodes
                v1.Add(0.0);
            }

            vectorPredictions = v1.ToArray();
            vectorGeneralCodes = v2.ToArray();
        }
    }
}
