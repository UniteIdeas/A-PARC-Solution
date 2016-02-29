using System.Collections.Generic;
using TextClassification.Model;

namespace TextClassification.Service.Statistic
{
    public class ClassificationAccuracyService
    {
        public double Calculate(List<ClassifiedText> prediction, List<ClassifiedText>  actual)
        {
            var total = actual.Count;
            var score = 0;
            foreach (var classifiedText in actual)
            {
                if(prediction.Find(p => p.Text.ToLower().Equals(classifiedText.Text.ToLower()) && p.GeneralCode == classifiedText.GeneralCode) != null)
                {
                    score++;
                }
            }
            double result = (double) score/(double) total;
            return result;
        }
    }
}
