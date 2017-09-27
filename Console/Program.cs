using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WebSpy;
using System.Text.RegularExpressions;
using System.Windows;

namespace WebSpyConsole
{
    class Program
    {
        static Corpus _corpus;
        static void Main(string[] args)
        {

            _corpus = Corpus.init();
            _corpus.reset().Wait();
            //Console.WriteLine(_corpus.GetTerms("1").Result.Count());
            var crawler = new Crawler(_corpus);
            //Corpus.test();
            while (true)
            {
                var q = enterQuery();
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
                var ranker = new Ranker(_corpus, query);
                foreach (var item in ranker.Rank())
                {
                    Console.WriteLine(_corpus.GetDocumentPath(item.Key).Result + " -- " + item.Value);
                }
            }
        }
        private static String enterQuery()
        {
            String ret = "";
            const int max = 10;
            Console.Write("Enter Query: ");
            ConsoleKeyInfo q = Console.ReadKey(true);
            while (q.Key != ConsoleKey.Enter)
            {
                if (q.Key == ConsoleKey.Backspace && ret.Count()>1)
                {
                    Console.WriteLine(ret.Count());
                    ret = ret.Substring(0, ret.Count() - 1);
                    var split = ret.Split(' ');
                    var text = split[split.Count() - 1];
                    printPredict(text, max);
                }
                else if (Char.IsLetterOrDigit(q.KeyChar) || Char.IsWhiteSpace(q.KeyChar))
                {
                    ret += q.KeyChar;
                    var split = ret.Split(' ');
                    var text = split[split.Count() - 1];
                    printPredict(text, max);
                }
                
                Console.WriteLine();
                Console.Write("Enter Query: "+ret);
                q = Console.ReadKey(true);

            }
            return ret;
        }

        private static async void printPredict(string v, int max)
        {

            var pattern = new Regex(v + ".*");
            var terms = await _corpus.GetTerms(term => {
                return pattern.IsMatch(term);
                //if (term.Count() >= v.Count())
                //{
                //    return String.Compare(v, term.Substring(0, v.Count()), true) == 0;
                //}
                //return false;
                }, max);
            Console.WriteLine();
            foreach (var term in terms)
            {
                Console.WriteLine(term);
            }

        }
    }
}
