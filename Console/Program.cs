using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WebSpy;

namespace WebSpyConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var corpus = Corpus.init();
            var crawler = new Crawler(corpus);
            //Corpus.test();
            while (true)
            {
                Console.Write("Enter Query: ");
                var q = Console.ReadLine();
                Console.WriteLine("Entered: " + q);
                var query = new Dictionary<string, int>();
                var list = q.Trim().Split(' ');
                var stemmer = new Stemmer();
                foreach (var item in list)
                {
                    var word = stemmer.StemWord(item.ToLower());
                    try
                    {
                        query[word] += 1;
                    }
                    catch
                    {
                        query[word] = 1;
                    }
                }
                var ranker = new Ranker(corpus, query);
                foreach (var item in ranker.Rank())
                {
                    Console.WriteLine(corpus.getDocumentPath(item.Key).Result + " -- " + item.Value);
                }
            }
        }
    }
}
