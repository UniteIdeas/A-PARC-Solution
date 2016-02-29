using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextClassification.Service.Dataset.Text
{
    public class BaseText
    {
        internal static string GetFileContent(string path)
        {
            if (!File.Exists(path))
                throw new Exception(string.Format("Could not read file: {0}. The file does not exist.", path));

            try
            {
                return File.ReadAllText(path);
            }
            catch (Exception)
            {
                throw;
            }
        }

        internal static string GetFileName(string path)
        {
            if (!File.Exists(path))
                throw new Exception(string.Format("Could not read file: {0}. The file does not exist.", path));

            try
            {
                var fi = new FileInfo(path);

                return fi.Name;
            } 
            catch(Exception)
            {
                return string.Empty;
            }
        }
    }
}
