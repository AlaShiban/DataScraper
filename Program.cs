using System;
using System.Collections.Generic;
using System.IO;
using CsQuery;
using CsQuery.ExtensionMethods;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DataScraper
{
    internal class Program
    {
        private static JToken _dataToScrape;
        private static IDictionary<string, object> _scrapeElements;

        private static void Main(string[] args)
        {
            string inputFile = "input.json";
            String outputFile = "output.json";
            if (args.Length == 2)
            {
                inputFile = args[0];
                outputFile = args[1];
            }
            if (args.Length == 1)
            {
                inputFile = args[0];
            }

            Console.WriteLine("Scraper Loaded");

            string json = File.ReadAllText(inputFile);
            object input = JsonConvert.DeserializeObject(json);

            var scrapeUrls = ((JObject)input)["SiteConfiguration"];

            _dataToScrape = ((JObject)input)["DataToScrape"];
            string dataJson = JsonConvert.SerializeObject(_dataToScrape);
            _scrapeElements = JsonConvert.DeserializeObject<IDictionary<string, object>>(dataJson);
            int cnt = 0;
            foreach (var url in scrapeUrls)
            {
                List<Dictionary<string, string>> content = ScrapeOnePage(url.ToString());
                File.WriteAllText(outputFile + "." + cnt + ".json", content.ToJSON());
                Console.WriteLine(JsonConvert.SerializeObject(content, Formatting.Indented));
                cnt++;
            }



        }

        private static List<Dictionary<string, string>> ScrapeOnePage(string url)
        {
            var listOfPageElements = new List<Dictionary<string, string>>();
            Console.WriteLine("Scraping: " + url);

            CQ doc = CQ.CreateFromUrl(url);

            object elementsSelector = _scrapeElements["elementscontainer"];
            string csspath = elementsSelector.ToString();

            foreach (IDomObject itemElement in doc[csspath])
            {
                CQ fragment = CQ.CreateFragment(itemElement.InnerHTML);
                Dictionary<string, string> pageScrape = ScrapeAllElements(elementsSelector, fragment);
                listOfPageElements.Add(pageScrape);
            }


            return listOfPageElements;
        }

        private static Dictionary<string, string> ScrapeAllElements(object elementsSelector, CQ doc)
        {
            var pageScrape = new Dictionary<string, string>();

            foreach (var item in _scrapeElements)
            {
                if (item.Key == "elementscontainer") continue;

                string content = doc[item.Value.ToString()].Text();
                pageScrape.Add(item.Key, content);
            }

            return pageScrape;
        }
    }
}