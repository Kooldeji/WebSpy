using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace WebSpy
{
    public class Querier
    {
        public string QueryString
        {
            get;
            private set;
        }
        public double duration
        {
            get;
            private set;
        }
        public List<DocumentResult> Results
        {
            get;
        } = new List<DocumentResult>();

        private ICorpus _corpus;

        public Querier(ICorpus corpus)
        {
            _corpus = corpus;
        }
        public void Query(string query)
        {
            Results.Clear();
            Stopwatch watch = new Stopwatch();
            watch.Start();
            var queryDict = new Dictionary<string, int>();
            var stemmer = new Stemmer();
            //Tokenizer's work 
            var list = Regex.Split(Regex.Replace(
                                         query,
                                        "[^\\w\\s]+",
                                        ""
                                        ).Trim(), "\\s+");
            for (int i = 0; i < list.Length; i++)
            {
                var word = stemmer.StemWord(list[i].ToLower());
                if (queryDict.ContainsKey(word))
                {
                    queryDict[word] += 1;
                }
                else
                {
                    queryDict[word] = 1;
                }
            }
            var ranker = new Ranker(_corpus, queryDict);
            var result = ranker.RankList;
            watch.Stop();
            duration = watch.ElapsedMilliseconds;
            watch.Reset();
            foreach (var item in result)
            {
                Results.Add(new DocumentResult(_corpus.GetRepository().Result, _corpus.GetDocumentPath(item.Key).Result, item.Value));
            }
        }

        public string AutoCorrect(string term)
        {
            return term;
        }

        public HashSet<string> AutoCompleteWord(string sentence, CancellationTokenSource cts)
        {
            var set = new HashSet<string>();
            var text = sentence.Split(' ').Last();
            var rootText = new Stemmer().StemWord(text);
            //var pattern = new Regex("("+text + ").*");
            var terms =  _corpus.GetWords(term => {
                //return pattern.IsMatch(term);
                //Console.WriteLine(term);
                if (term.Count() >= text.Count())
                {
                    if ( String.Compare(text, term.Substring(0, text.Count()), true) == 0)
                    {
                        if (cts.IsCancellationRequested) return false;
                        return true;
                    }
                }
                return false;
            }, 10);
            return terms.Result;
        }

    }

    public class DocumentResult
    {
        public string Extension
        {
            get
            {
                return Path.GetExtension(FullPath).Remove(0, 1);
            }
        }
        public string Title
        {
            get
            {
                return Path.GetFileName(FullPath).Replace("."+Extension, "");
            }
        }
        public long Size
        {
            get
            {
                return new FileInfo(FullPath).Length;
            }
        }
        public DateTime LastModified
        {
            get
            {
                return File.GetLastWriteTime(FullPath);
            }
        }
        public string SubText
        {
            get;
            private set;
        }
        public string FullPath
        {
            get;
            private set;
        }
        public string RelativePath
        {
            get;
            private set;
        }
        public decimal Value
        {
            get;
            private set;
        }
        public DocumentResult(string repo, string file, decimal value)
        {
            FullPath = Path.Combine(repo, file);
            RelativePath = file;
            Value = value;

            
        }
    }
}
