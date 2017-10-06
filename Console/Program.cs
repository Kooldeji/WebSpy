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
        static Querier _querier;
        static void Main(string[] args)
        {
            _corpus = Corpus.init();
            //_corpus.Empty().Wait();
            var crawler = new Crawler(_corpus);
            _querier = new Querier(_corpus);
            while (true)
            {
                var q = enterQuery();
                Console.Clear();
                Console.WriteLine("Entered: " + q);
                _querier.Query(q);
                Console.WriteLine("\nResults");
                Console.WriteLine("Approx. " + _querier.duration + "ms");
                foreach (var result in _querier.Results)
                {
                    Console.WriteLine();
                    Console.WriteLine("[" + result.Extension + "] - " + result.Title + " - " + result.Size + " bytes");
                    Console.WriteLine("at .../" + result.RelativePath);
                    Console.WriteLine(result.LastModified);
                    Console.WriteLine(result.Value);
                    Console.WriteLine();
                }
                Console.Write("Enter a Key to Keep Searching..");
                Console.ReadKey();
                Console.Clear();


            }
        }
        private static String enterQuery()
        {
            String ret = "";
            const int max = 10;
            Console.Write("Enter Query: ");
            ConsoleKeyInfo q = Console.ReadKey(true);
            Task predictTask = null;
            while (q.Key != ConsoleKey.Enter)
            {
                Console.Clear();
                if (q.Key == ConsoleKey.Backspace && ret.Count()>0)
                {
                    ret = ret.Substring(0, ret.Count() - 1);
                    var split = ret.Split(' ');
                    var text = split[split.Count() - 1];
                    if (predictTask != null && predictTask.Status == TaskStatus.Running) predictTask.Dispose();
                    predictTask = printPredict(text, max);
                }
                else if (Char.IsLetterOrDigit(q.KeyChar) || Char.IsWhiteSpace(q.KeyChar))
                {
                    ret += q.KeyChar;
                    var split = ret.Split(' ');
                    var text = split[split.Count() - 1];
                    if (predictTask != null && predictTask.Status == TaskStatus.Running) predictTask.Dispose();
                    predictTask = printPredict(text, max);
                }
                
                Console.WriteLine();
                Console.Write("Enter Query: "+ret);
                q = Console.ReadKey(true);

            }
            if (predictTask != null && predictTask.Status == TaskStatus.Running) predictTask.Dispose();
            return ret;
        }

        private static async Task printPredict(string v, int max)
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
