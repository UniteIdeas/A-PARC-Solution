using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TextClassification.Model;

namespace TextClassification.Service.Frog
{
    public class FrogClient
    {
        public List<StemWord> GetResults(string text)
        {
            var ws = new FrogProxyWebService.FrogProxyWebServiceClient();
            var result = ws.GetStem(text);
            return Read(XDocument.Parse(result));
        }
        public StemWord GetResult(string text)
        {
            var ws = new FrogProxyWebService.FrogProxyWebServiceClient();
            var result = ws.GetStem(text);
            return Read(XDocument.Parse(result)).FirstOrDefault();
        }
        
        //public List<StemWord> GetResults(string text, bool isSecondTime = false)
        //{
        //    var ws = new FrogProxyWebService.FrogProxyWebServiceClient();
        //    var result = ws.GetStem(text);
        //    return Read(XDocument.Parse(result), isSecondTime);
        //}

        private static List<StemWord> Read(XDocument xdoc)
        {
            var result = new List<StemWord>();
            var root = xdoc.Root;
            if (root == null)
                return result;
            var wElements = root.Descendants().Where(p => p.Name.LocalName == "w");

            foreach (var wElement in wElements)
            {
                var fw = new StemWord
                               {
                                   Text = wElement.Descendants().First(p => p.Name.LocalName == "t").Value,
                                   Type = wElement.Descendants().First(p => p.Name.LocalName == "pos").Attribute("head").Value,
                                   Stem = wElement.Descendants().First(p => p.Name.LocalName == "lemma").Attribute("class").Value
                               };

                result.Add(fw);
            }

            return result;
        }
    }
}
