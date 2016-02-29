using System;
using System.Diagnostics;

namespace TextClassification.Tf_Idf
{
    /// <summary>
    /// Summary description for Class1.
    /// </summary>
    class Class1
    {
        //http://www.codeproject.com/Articles/12098/Term-frequency-Inverse-document-frequency-implemen

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            StopWordsHandler stopword = new StopWordsHandler();
            string[] doc = new string[4] {
										   "test b c c dd d d d d",
										   "a b c c c d dd d d test"
										,   "c d b f y teyr etre tretr gfgd c",
										"r a e e f n l i f f f f x l"
			};

            TFIDFMeasure tf = new TFIDFMeasure(doc);
            Trace.WriteLine((double)Math.Log(10000 / 50));
            Trace.WriteLine(tf.GetSimilarity(0, 1));
            string[] _3grams = NGram.GenerateNGrams("TEXT", 3);
            //
            // TODO: Add code to start application here
            //
        }
    }
}
