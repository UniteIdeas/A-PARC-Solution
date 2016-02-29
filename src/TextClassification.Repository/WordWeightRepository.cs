using System;
using System.Collections.Generic;
using TextClassification.Model;

namespace TextClassification.Repository
{
    public class WordWeightRepository : BaseRepository
    {
        public void Insert(List<WordWeight> list)
        {
            using (var context = Context)
            {
                var count = 1;
                foreach (var entity in list)
                {
                    Console.WriteLine("Working on: {0} of {1}", count, list.Count);
                    context.WordWeights.Add(entity);

                    if ((double)count % 100 == 0)
                        context.SaveChanges();

                    count++;
                }
                context.SaveChanges();
            }
        } 

        public void Truncate()
        {
            using (var context = Context)
            {
                context.Database.ExecuteSqlCommand("DELETE FROM wrw_WordWeight;");
            }
        }
    }
}
