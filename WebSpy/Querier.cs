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
        private Tokenizer _tokenizer;

        private ICorpus _corpus;
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

        public Querier(ICorpus corpus)
        {
            _corpus = corpus;
            _tokenizer = new Tokenizer(corpus.StopWords);
        }
        public void Query(string query)
        {
            Results.Clear();
            Stopwatch watch = new Stopwatch();
            watch.Start();
            var queryDoc = _tokenizer.Tokenize(query);
            Console.WriteLine(queryDoc);
            var ranker = new Ranker(_corpus, queryDoc);
            var result = ranker.RankList;
            watch.Stop();
            duration = watch.ElapsedMilliseconds;
            watch.Reset();
            foreach (var item in result)
            {
                Console.WriteLine(item);
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
            var terms = _corpus.GetWords(term => {
                //return pattern.IsMatch(term);
                //Console.WriteLine(term);
                if (term.Count() >= text.Count())
                {
                    if (String.Compare(text, term.Substring(0, text.Count()), true) == 0)
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
        private string _subtext = "A search engine API with support for a generic repository as a project of csc322 of the University of Lagos, Nigeria.It is…….";
        public string Extension
        {
            get
            {
                return Path.GetExtension(FullPath);
            }
        }
        public string Title
        {
            get
            {
                return new FileInfo(FullPath).Name;
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
            get
            {
                return _subtext;
            }
            private set
            {
                _subtext = value;
            }
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
        public double Value
        {
            get;
            private set;
        }
        public string Author
        {
            get
            {
                return File.GetAccessControl(FullPath).GetOwner(typeof(System.Security.Principal.NTAccount)).ToString();
            }
        }
        public DocumentResult(string repo, string file, double value)
        {
            FullPath = Path.Combine(repo, file);
            RelativePath = file;
            Value = value;

        }
    }
}
