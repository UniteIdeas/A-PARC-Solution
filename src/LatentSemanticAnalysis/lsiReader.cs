using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.IO;
using System.Data;


namespace lsi
{
    /// <summary>
    /// Structure specifies the document and word relation
    /// </summary>
    public struct DocWordRelation
    {
        public int DocID;
        public int WordID;
    }

    /// <summary>
    /// Reads and indexes the documents. Basically creates the foundations to allow creating term document matrix
    /// </summary>
    public class lsiReader
    {
        private Hashtable goDocList = null; //List of documents
        private Hashtable goWordList = null; // List of words
        private Hashtable goDocWord = null; // List of document word relations
        private StemmerInterface goStemmmer = new PorterStemmer(); // Instance for PorterStemmer


        public Hashtable DocList { get { return goDocList; } } // Returns the list of documents
        public Hashtable WordList { get { return goWordList; } } // Returns the list of words
        public Hashtable DocWordRelation { get { return goDocWord; } } // Returns the document word relations

        /// <summary>
        /// LSI reader constructor
        /// </summary>
        public lsiReader()
        {
            goDocList = new Hashtable();
            goWordList = new Hashtable();

            goDocWord = new Hashtable();
        }

        // Index the directory specified in the path
        public void IndexDirectory(string sPath)
        {
            this.InitDocumentList(sPath);
            this.IndexDocuments();
        }

        // Create the list of documents for the specified path
        public void InitDocumentList(string sPath)
        {
            string[] files = Directory.GetFiles(sPath);
            int docid = 0;
            for (int i = 0; i < files.Length; i++)
            {
                if (!goDocList.ContainsKey(files[i]))
                {
                    goDocList.Add(docid,files[i]);
                    docid++;
                }
            }
        }

        /// <summary>
        /// Index the documents by reading its content
        /// </summary>
        public void IndexDocuments()
        {
            string sText = "";
            foreach (DictionaryEntry oEntry in goDocList)
            {
                sText = ReadFileContent((string)oEntry.Value);
                this.IndexDocumentText(sText, (int)oEntry.Key);
            }
        }

        // Index the text of the passed document
        private void IndexDocumentText(string sDocText, int nDocID)
        {
            string[] words = LSICommon.Instance.GetWords(sDocText);
            for (int i = 0; i < words.Length; i++)
            {
                words[i] = goStemmmer.stemTerm(words[i]);

                if (!goWordList.ContainsKey(words[i]))
                {
                    goWordList.Add(words[i], goWordList.Count);
                }

                DocWordRelation dwr;
                dwr.DocID = nDocID;
                dwr.WordID = (int)goWordList[words[i]];

                if (!goDocWord.ContainsKey(dwr))
                {
                    goDocWord.Add(dwr, 1);
                }
                else goDocWord[dwr] = (int)goDocWord[dwr] + 1;
            }
        }

        // Read the content of the files
        private string ReadFileContent(string sFile)
        {
            string sText = "";
            FileInfo fInfo = new FileInfo(sFile);
            if(fInfo.Extension.ToUpper() == ".TXT")
            {
                sText = this.ReadTextContent(fInfo);
            }
            fInfo = null;

            return sText;
        }

        // Read the content of the text file
        private string ReadTextContent(FileInfo fInfo)
        {
            TextReader oTR = fInfo.OpenText();
            string sText = oTR.ReadToEnd();
            oTR.Close();
            oTR = null;
            return sText;
        }
    }
}
