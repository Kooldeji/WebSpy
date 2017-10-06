using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Timers;


namespace WebSpy
{
    public class Crawler
    {
        private delegate Task CrawlerTask(String file);

        FileSystemWatcher watcher;
        ICorpus _corpus;
        Timer timer;
        List<Tuple<CrawlerTask, String>> queue = new List<Tuple<CrawlerTask, String>>();
        Indexer _indexer;
        public Crawler(ICorpus corpus)
        {
            timer = new Timer(30000);
            timer.AutoReset = true;
            timer.Elapsed += execute;

            _corpus = corpus;
            init();
            //while (res.Status != TaskStatus.RanToCompletion) ;


        }

        public async Task init()
        {
            Console.WriteLine("Initializing...");
            _indexer = new Indexer(_corpus);
            var repo = await _corpus.GetRepository();
            Console.WriteLine(repo);
            var files = crawl(repo).ToList();//All files in my repository
            foreach (var file in files)
            {
                Console.WriteLine(file);
                if (!isValidFile(file)) continue;
                Console.WriteLine(await _corpus.GetRepository());
                var sfile = file.Replace(await _corpus.GetRepository() + "\\", "");
                Console.WriteLine(await _corpus.GetRepository());
                //For each file in the repository, try to get their id from the corpus
                Console.WriteLine(sfile);
                string id = await _corpus.GetDocumentID(sfile);
                if (id == null)
                {
                    //File is not in the Corpus
                    //Index it and add it to the corpus.
                    Console.WriteLine("deji2");
                    await addFile(file);
                }
                else
                {
                    //File exists in the corpus
                    if (File.GetLastWriteTime(file).Ticks > await _corpus.GetLastCrawled())
                    {
                        //File has been edited since last checked, re-index it and update it into the corpus
                        await editFile(file);
                    }
                }
                //Console.WriteLine("done");
            }
            var kFiles = await _corpus.GetDocuments(); //Files that exist in my database
            foreach (var id in kFiles)
            {
                var file = await _corpus.GetDocumentPath(id);
                if (!files.Contains(Path.Combine(await _corpus.GetRepository(), file)))
                {
                    //File does not exist again in repo, delete it from database
                    await removeFile(file);
                }
            }

            //------------Watching for changes--------------------
            watcher = new FileSystemWatcher(await _corpus.GetRepository());
            Console.WriteLine("watch");
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Attributes;
            watcher.Changed += new FileSystemEventHandler(async (source, e) =>
            {
                Console.WriteLine("changed");
                if (!isValidFile(e.FullPath)) return;
                await editFile(e.FullPath);

            });
            watcher.Created += new FileSystemEventHandler(async (source, e) =>
            {
                Console.WriteLine("created");
                if (!isValidFile(e.FullPath)) return;
                await addFile(e.FullPath);
            });
            watcher.Deleted += new FileSystemEventHandler(async (source, e) =>
            {
                Console.WriteLine("remove");
                await removeFile(e.FullPath);
            });
            watcher.Renamed += new RenamedEventHandler(async (source, e) =>
            {
                if (!isValidFile(e.FullPath)) return;
                await renameFile(e.FullPath);
            });

            watcher.EnableRaisingEvents = true;
            Console.WriteLine("watcher: " + watcher.Path);
        }
        private async Task renameFile(string file)
        {
            //Console.WriteLine("{0} was just renamed ", file);
            file = file.Replace(await _corpus.GetRepository() + "\\", "");
            var id = await _corpus.GetDocumentID(file);
            await _corpus.ChangeDocumentPath(id, file);
            await _corpus.SetLastCrawled(DateTime.Now.Ticks);
            //Console.WriteLine("Renamed");
        }
        private async Task removeFile(string file)
        {
            //Console.WriteLine("{0} was just removed ", file);
            Console.WriteLine("remove met");
            file = file.Replace(await _corpus.GetRepository() + "\\", "");
            var id = await _corpus.GetDocumentID(file);
            await _corpus.RemoveDocument(id);
            Console.WriteLine("removed");
            await _corpus.SetLastCrawled(DateTime.Now.Ticks);
            //Console.WriteLine("Removed");
        }

        private async Task addFile(string file)
        {
            //Console.WriteLine("{0} was just Added ", file);
            file = file.Replace(await _corpus.GetRepository() + "\\", "");
            Console.WriteLine("ss:=L");
            var res = _indexer.Index(file);//new Indexer.index(file);
            Console.WriteLine("ddd");
            await _corpus.AddDocument(file, res.Item1, res.Item2);
            await _corpus.SetLastCrawled(DateTime.Now.Ticks);
            //Console.WriteLine("Added");
        }

        public async Task editFile(String file)
        {
            //Console.WriteLine("{0} was just Modified ", file);
            file = file.Replace(await _corpus.GetRepository() + "\\", "");
            Console.WriteLine("add " + file);
            var id = await _corpus.GetDocumentID(file);
            Console.WriteLine("add " + id);
            var res = _indexer.Index(file);
            await _corpus.RemoveDocument(id);
            await _corpus.AddDocument(file, res.Item1, res.Item2);
            await _corpus.SetLastCrawled(DateTime.Now.Ticks);
            //Console.WriteLine("Edited");
        }

        private void enQueue(CrawlerTask task, String file)
        {
            //check for duplicates
            foreach (var tuple in queue)
            {
                var run = true;
                if (String.Compare(tuple.Item2, file, true) == 0)
                {
                    if (tuple.Item1 == task)
                    {
                        if (execute(tuple.Item1, tuple.Item2)) queue.Remove(tuple);
                        return;
                    }
                    else
                    {
                        if (!execute(tuple.Item1, tuple.Item2)) run = false;
                        else queue.Remove(tuple);
                    }
                }
                if (run)
                {
                    if (execute(task, file)) return;
                }
                queue.Add(new Tuple<CrawlerTask, string>(task, file));

            }
            queue.Add(Tuple.Create(task, file));
            if (!timer.Enabled) timer.Enabled = true;
        }

        private bool execute(CrawlerTask task, string file)
        {
            try
            {
                File.Open(file, FileMode.Open);
                task(file);
                return true;
            }
            catch (FileNotFoundException e)
            {
                return true;
            }
            catch (IOException e)
            {
                return false;
            }
        }

        private void execute(Object source, System.Timers.ElapsedEventArgs e)
        {
            //check for duplicates
            foreach (var tuple in queue)
            {
                if (execute(tuple.Item1, tuple.Item2)) queue.Remove(tuple);
            }
        }

        private void renameTaskFile(String oldName, String newName)
        {
            foreach (var tuple in queue.ToArray())
            {
                if (String.Compare(tuple.Item2, oldName, true) == 0)
                {
                    queue.Remove(tuple);
                    if (!execute(tuple.Item1, newName))
                    {
                        queue.Add(Tuple.Create(tuple.Item1, newName));
                    }

                }
            }
        }

        public static IEnumerable<string> crawl(String path)
        {
            foreach (var file in Directory.EnumerateFiles(path))
            {
                yield return file;
            }
            foreach (var folder in Directory.EnumerateDirectories(path))
            {
                foreach (var item in crawl(folder))
                {
                    yield return item;
                }
            }
        }

        public async Task<Tuple<long, List<ITermDocument>>> simulateIndexer(String path)
        {
            var stemmer = new Stemmer();

            Console.WriteLine("ree");
            var reader = new StreamReader(Path.Combine(await _corpus.GetRepository(), path));
            var list = new List<string>(Regex.Split(Regex.Replace(
                                         reader.ReadToEnd(),
                                        "[^a-zA-Z0-9']+",
                                        " "
                                        ).Trim(), "\\s+"));
            reader.Close();

            //Document Length
            long length = list.Count;

            //Adding Term's Path for indexing.
            list.AddRange(Regex.Split(Regex.Replace(
                                         path,
                                        "[^a-zA-Z0-9']",
                                        " "
                                        ).Trim(), "\\s+"));

            var termDict = new Dictionary<string, ITermDocument>();
            var docDict = new Dictionary<string, IDocumentReference>();
            for (int i = 0; i < list.Count; i++)
            {
                var word = list[i].ToLower();
                var rootWord = stemmer.StemWord(word);
                ITermDocument term;
                if (termDict.Keys.Contains(rootWord)) term = termDict[rootWord];
                else
                {
                    term = new TermDocument(rootWord);
                    termDict.Add(rootWord, term);
                }
                IDocumentReference docref;
                if (docDict.Keys.Contains(word))
                {
                    docref = docDict[word];
                }
                else
                {
                    docref = new DocumentReference("", word);
                    docDict.Add(word, docref);
                    term.addDoc(docref);
                }
                docref.addPos(i);
            }
            return Tuple.Create(length, termDict.Values.ToList());
        }

        public bool isValidFile(string path)
        {
            Console.WriteLine(Path.GetExtension(path));
            var validTypes = new HashSet<string>(new string[] { "pdf", "doc", "docx", "ppt", "ppts", "xls", "xlsx", "txt", "html", "xml" });
            if (!File.Exists(path)) return false;
            if (!validTypes.Contains(Path.GetExtension(path).Remove(0, 1))) return false;
            return true;
        }
    }

}
