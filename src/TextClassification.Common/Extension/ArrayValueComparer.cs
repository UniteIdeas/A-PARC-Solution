using System.Collections.Generic;
using System.Linq;

namespace TextClassification.Common.Extension
{
    public class ArrayValueComparer<T> : IEqualityComparer<T[]>
    {
        public bool Equals(T[] x, T[] y)
        {
            return x.SequenceEqual(y, EqualityComparer<T>.Default);
        }

        public int GetHashCode(T[] obj)
        {
            return obj.Aggregate(0, (total, next) => total ^ next.GetHashCode());
        }
    }
}
