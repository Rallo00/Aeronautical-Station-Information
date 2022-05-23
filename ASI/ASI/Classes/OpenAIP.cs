using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace ASI
{
    static class OpenAIP
    {
        private const string URL_ICAO_SEARCH = "https://www.openaip.net/search/node/",
                             URL_ICAO_NODE = "https://www.openaip.net/node/";

        public static async Task<List<Frequency>> GetAirportFrequency(string icao)
        {
            //Finding airport WebPage
            string airportNode = await GetAirportNode(icao);
            //Obtaining airport WebPage
            string airportWebPage = await Http_GetRequestAsync(URL_ICAO_NODE + airportNode);
            //Searching for frequencies in WebPage
            HtmlDocument htmlSnippet = new HtmlDocument();
            htmlSnippet.LoadHtml(airportWebPage);
            List<Frequency> frequenciesList = new List<Frequency>();
            HtmlNodeCollection freqTable = htmlSnippet.DocumentNode.SelectNodes("//table[@class='sticky-enabled']//tr//td");
            //Extracting table data of FrequencyTable from WebPage
            if (freqTable != null)
            {
                for (int i = 0; i < freqTable.Count; i += 4)
                    frequenciesList.Add(new Frequency(freqTable[i].InnerText,
                                                      freqTable[i + 1].InnerText,
                                                      freqTable[i + 2].InnerText,
                                                      freqTable[i + 3].InnerText
                                                      ));
                return frequenciesList;
            }
            else
                frequenciesList.Add(new Frequency("","", " error encountered.", "Possible "));
            return frequenciesList;
        }
        public static async Task<string> GetAirportNode(string icao)
        {
            string result = null;
            //Obtaining search results WebPage
            string searchResults = await Http_GetRequestAsync(URL_ICAO_SEARCH + icao);
            //Searching all href links in WebPage
            HtmlDocument htmlSnippet = new HtmlDocument();
            htmlSnippet.LoadHtml(searchResults);
            List<string> hrefTags = new List<string>();
            foreach (HtmlNode link in htmlSnippet.DocumentNode.SelectNodes("//a[@href]"))
            {
                HtmlAttribute att = link.Attributes["href"];
                hrefTags.Add(att.Value);
            }
            //Searching the correct href having node to searched airport WebPage
            foreach (string link in hrefTags)
            {
                string[] splitted = link.Split('/');
                //Example: https://www.openaip.net/node/154857
                if (splitted.Length > 4 && splitted[2] == "www.openaip.net" && splitted[3] == "node")
                {
                    result = splitted[4];
                    break;
                }
            }
            return result;
        }
        private static async Task<string> Http_GetRequestAsync(string URI)
        {
            System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(URI);
            using (System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)await request.GetResponseAsync())
            using (System.IO.Stream stream = response.GetResponseStream())
            using (System.IO.StreamReader reader = new System.IO.StreamReader(stream))
                return await reader.ReadToEndAsync();
        }

        public class Frequency
        {
            public string Purpose { get; set; }
            public string Type { get; set; }
            public string UsedFrequency { get; set; }
            public string Name { get; set; }
            public Frequency(string p, string t, string u, string n)
            { Purpose = p; Type = t; UsedFrequency = u; Name = n; }
        }
    }
}
