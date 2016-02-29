using System;
using System.Threading;

namespace TextClassification.Common
{
    public static class RandomProvider
    {
        private static int seed = Environment.TickCount;

        private static ThreadLocal<Random> randomWrapper = new ThreadLocal<Random>(() =>
            new Random(Interlocked.Increment(ref seed))
        );

        public static Random GetThreadRandom()
        {
            return randomWrapper.Value;
        }

        public static int GetThreadRandomInt(int minValue, int maxValue)
        {
            var i = randomWrapper.Value.Next(minValue, maxValue);
            //Console.WriteLine("Random: {0}", i);
            return i;
        }
    }
}
