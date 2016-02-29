using System;
using System.Collections.Generic;
using System.Linq;
using TextClassification.Model;

namespace TextClassification.Repository
{
    public class StemWordRepository : BaseRepository
    {
        public StemWord Find(string text)
        {
            using (var context = Context)
            {
                //var result = context.StemWords.Where(p => p.Text.Equals(text));
                return context.StemWords.FirstOrDefault(p => p.Text.Equals(text));
            }
        }

        /// <summary>
        /// List
        /// </summary>
        /// <returns></returns>
        public IEnumerable<StemWord> List()
        {
            //using (var context = Context)
            //{
                var context = Context;
                return context.StemWords.OrderBy(p => p.Text);
            //}
        }

        /// <summary>
        /// List
        /// </summary>
        /// <returns></returns>
        public List<StemWord> GetList()
        {
            using (var context = Context)
            {
                return context.StemWords.OrderBy(p => p.Text).ToList();
            }
        }

        public void Update(StemWord entity)
        {
            using (var context = Context)
            {
                context.StemWords.Attach(entity);
                var entry = context.Entry(entity);
                if (entry != null)
                {
                    entry.Property(e => e.Text).IsModified = true;
                    entry.Property(e => e.Stem).IsModified = true;
                    entry.Property(e => e.Type).IsModified = true;

                } else
                {
                    Add(entity);
                }
                context.SaveChanges();
            }
        }

        public void Add(StemWord entity)
        {
            using (var context = Context)
            {
                try
                {
                    context.StemWords.Add(entity);
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    Console.Write("error occurred");
                }
            }
        }

        public void Delete(StemWord entity)
        {
            using (var context = Context)
            {
                var result = context.StemWords.SingleOrDefault(p => p.Text == entity.Text);
                if (result == null) 
                    return;
                context.StemWords.Remove(result);
                context.SaveChanges();
            }
        }
    }
}
