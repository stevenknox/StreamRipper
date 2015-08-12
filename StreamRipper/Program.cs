using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamRipper
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                ProcessStreams();

                Console.WriteLine("Process Complete");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Process Failed" + ex.Message);
            }
          


        }

        private static void ProcessStreams()
        {
            var baseUrl = ConfigurationManager.AppSettings["SiteUrl"];
            var htmlElement = ConfigurationManager.AppSettings["HtmlElement"];
            var streamList = new List<Streams>();
            //load raw webpage
            HtmlWeb web = new HtmlWeb();
            HtmlDocument htmlDocument = web.Load(baseUrl);

            //find teh 'customers' table then select all the hyperlinks in that table
            var table = htmlDocument.DocumentNode.Descendants().SingleOrDefault(x => x.Id == htmlElement);
            if (table != null)
            {
                IEnumerable<HtmlNode> links = table.Descendants("a").Where(x => x.Attributes.Contains("href"));
                foreach (var link in links)
                {
                    Console.WriteLine(string.Format("Extracting {0}", link.InnerText));
                    //navigate to stream page
                    try
                    {
                        var streamToAdd = GetStreams(link);

                        if (streamToAdd != null)
                        {
                            streamList.Add(new Streams
                            {
                                Name = link.InnerText,
                                Url = streamToAdd.Attributes["href"].Value
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed to process stream - " + link.InnerText + ". " + ex.Message);
                    }
                   
                }

                WritetoFile(streamList);

            }
        }

        private static HtmlNode GetStreams(HtmlNode link)
        {
            var htmlElement = ConfigurationManager.AppSettings["HtmlElement"];
            //load raw webpage
            HtmlWeb web = new HtmlWeb();
            HtmlDocument htmlDocument = web.Load(link.Attributes["href"].Value);

            var table = htmlDocument.DocumentNode.Descendants().SingleOrDefault(x => x.Id == htmlElement);
            if (table != null)
            {
                IEnumerable<HtmlNode> links = table.Descendants("a").Where(x => x.Attributes.Contains("href"));
                foreach (var streamUrl in links.Where(f => f.Attributes["href"].Value.Contains("acestream://")))
                {
                    Console.WriteLine(string.Format("Adding stream {0}", streamUrl.InnerText));
                    return streamUrl;
                }
            }
            return null;
        }

        private static void WritetoFile(List<Streams> streamList)
        {
            var htmlElement = ConfigurationManager.AppSettings["OutputFileName"];
            Console.WriteLine("Writing to File");
            // Compose a string that consists of three lines.
            var s = new StringBuilder();
            foreach (var i in streamList)
            {
                s.AppendFormat("#EXTINF:0,{0}\r\n", i.Name);
                s.AppendFormat("{0}\r\n", i.Url);
            }

            // Write the string to a file.
            System.IO.StreamWriter file = new System.IO.StreamWriter("streams.m3u");
            file.WriteLine(s.ToString());

            file.Close();
        }

    
    }
}
