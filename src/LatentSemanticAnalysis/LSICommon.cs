
/*
 * LSI Common Class. To be extended further
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Text.RegularExpressions;
using System.Collections;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace lsi
{
    /// <summary>
    /// Common class used in the application
    /// </summary>
    public class LSICommon
    {
        // Maintains the singular instance for the class
        private static LSICommon _LSICommonInstance = null;

        // Returns the singular instance for this class
        public static LSICommon Instance
        {
            get
            {
                if (_LSICommonInstance == null)
                {
                    _LSICommonInstance = new LSICommon();
                }
                return _LSICommonInstance;
            }
        }


        // Stores the LSI application config
        LSIConfiguration _lsiConfig;

        // Return the LSI application config
        public LSIConfiguration LSIConfig
        {
            get
            {
                return _lsiConfig;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        private LSICommon()
        {
        }


        // List of initializations. 
        public void Init()
        {
            StopWordsList.InitializeList(this.LSIAppPath + @"\StopList\StopList.txt");
            _lsiConfig = new LSIConfiguration();
        }

        // stores the application path that will be used across the layers of the application
        private string _lsiAppPath = "";

        // Return the LSI application path
        public string LSIAppPath
        {
            get
            {
                return _lsiAppPath;
            }
        }

        // Sets the LSI application path
        public void SetLSIAppPath(string sPath)
        {
            // Avoiding accidental set by not writing in property
            _lsiAppPath = sPath;
        }


        // Get the words from the string. Forces the use of stopwords
        public string[] GetWords(string sText)
        {
            return this.GetWords(sText, true);
        }

        // Get the words from the string. Optional use of stopwords
        public string[] GetWords(string sText, bool bUseStopWords)
        {

            // Tokenize the string
            Regex oRegex = new Regex("([ \\t{}():;. \n])");
            sText = sText.ToLower();

            String[] words = oRegex.Split(sText);
            ArrayList oArraylist = new ArrayList();

            for (int i = 0; i < words.Length; i++)
            {
                MatchCollection mc = oRegex.Matches(words[i]);
                if (mc.Count <= 0 && words[i].Trim().Length > 0)
                {
                    if (bUseStopWords)
                    {
                        if (!StopWordsList.IsStopWord(words[i]))
                        {
                            oArraylist.Add(words[i]);
                        }
                    }
                    else oArraylist.Add(words[i]);
                }
            }

            // Cleaning up the extra characters after tokenization.
            char[] bothsidestrimchar = { '\'', '<', '>', '/', ':', ';', '"', '{', '}', '|', '\\', '[', ']', '.', ',', '~', '`', '!', '?', '@', '#', '%', '^', '&', '*', '(', ')', '_', '-', '+', '=' };
            char[] endtrimchar = { '$' };
            for (int i = 0; i < oArraylist.Count; i++)
            {
                string sObj = (oArraylist[i] as string);
                sObj = sObj.Trim();
                sObj = sObj.Trim(bothsidestrimchar);
                sObj = sObj.TrimEnd(endtrimchar);
                oArraylist[i] = sObj;
            }

            int arr_cnt = 0;
            for (int i = 0; i < oArraylist.Count; i++) if (((string)oArraylist[i]).Trim().Length > 0) arr_cnt++;
            string[] oArray = new string[arr_cnt];
            for (int i = 0, j = 0; i < oArraylist.Count; i++)
            {
                if (((string)oArraylist[i]).Trim().Length > 0)
                {
                    oArray[j] = (string)oArraylist[i];
                    j++;
                }
            }
            return oArray;
        }
    }

    /// <summary>
    /// Stores the configuration for LSI application
    /// </summary>
    [Serializable]
    public class LSIConfiguration
    {
        public string document_directory;
        public int k_percent_for_rank_approx;
        private string config_path;

        /// <summary>
        /// Constructor
        /// </summary>
        public LSIConfiguration()
        {
            config_path = LSICommon.Instance.LSIAppPath + @"\lsiconfig.bin";
            this.LoadFromFile();
        }

        /// <summary>
        /// Set the default configuration. (As per my choice)
        /// </summary>
        public void SetDefault()
        {
            this.document_directory = @"C:\temptoeess";
            this.k_percent_for_rank_approx = 65;
        }

        /// <summary>
        /// Load the configuration from the file
        /// </summary>
        public void LoadFromFile()
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream f;

            if (System.IO.File.Exists(config_path))
            {
                f = File.Open(config_path, FileMode.Open);
                try
                {
                    LSIConfiguration lsic = (LSIConfiguration)bf.Deserialize(f);
                    this.document_directory = lsic.document_directory;
                    this.k_percent_for_rank_approx = lsic.k_percent_for_rank_approx;

                }
                catch
                {
                    f.Close();
                    f = null;
                    this.SetDefault();
                    this.SaveToFile();
                }
                if(f!=null)f.Close();
            }
            else
            {
                this.SetDefault();
                this.SaveToFile();
            }
        }

        /// <summary>
        /// Save the configuration to a file.
        /// </summary>
        public void SaveToFile()
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream f = File.Open(config_path, FileMode.Create);
            bf.Serialize(f, this);
            f.Close();
        }

    }

    /// <summary>
    /// Serializable object to store the hash table values.
    /// </summary>
    [Serializable]
    public struct HashObj
    {
        public object key;
        public object value;
    }

    /// <summary>
    /// The structure for the index.
    /// </summary>
    [Serializable]
    public struct DocIndex
    {
        public HashObj [] DocList;
        public HashObj []WordList;
        public double[][] WTDM;
        public double[][] skinv;
        public double[][] uk;
    }
}
