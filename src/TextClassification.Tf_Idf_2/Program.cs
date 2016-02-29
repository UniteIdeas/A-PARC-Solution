using System;

namespace TextClassification.Tf_Idf_2
{
    class Program
    {
        //https://github.com/primaryobjects/TFIDF
        static void Main(string[] args)
        {
            // Some example documents.
            //string[] documents =
            //{
            //    "The sun in the sky is bright.",
            //    "We can see the shining sun, the bright sun."
            //};

            string[] documents = new string[4] {
										   "test b c c dd d d d d",
										   "a b c c c d dd d d test"
										,   "c d b f y teyr etre tretr gfgd c",
										"r a e e f n l i f f f f x l"
			};
            // Apply TF*IDF to the documents and get the resulting vectors.
            double[][] inputs = TFIDF.Transform(documents, 0);
            inputs = TFIDF.Normalize(inputs);
            
            // Display the output.
            for (int index = 0; index < inputs.Length; index++)
            {
                Console.WriteLine(documents[index]);

                foreach (double value in inputs[index])
                {
                    Console.Write(value + ", ");
                }

                Console.WriteLine("\n");
            }

            Console.WriteLine("Press any key ..");
            Console.ReadKey();
        }
    }
}
