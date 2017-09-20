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
        Corpus _corpus;
        Timer timer;
        List<Tuple<CrawlerTask, String>> queue = new List<Tuple<CrawlerTask, String>>();
        public Crawler(Corpus corpus)
        {
            timer = new Timer(30000);
            timer.AutoReset = true;
            timer.Elapsed += execute;
            _corpus = corpus;
            var res = init();
            while (res.Status != TaskStatus.RanToCompletion) ;


        }

        public async Task init()
        {
            Console.WriteLine("Initializing...");

            var files = crawl(_corpus.getRepository().Result).ToList();//All files in my repository
            foreach (var file in files)
            {
                var sfile = file.Replace(_corpus.getRepository().Result + "\\", "");
                //For each file in the repository, try to get their id from the corpus
                string id = _corpus.getDocumentID(sfile).Result;
                if (id == null)
                {
                    //File is not in the Corpus
                    //Index it and add it to the corpus.
                    await addFile(file);
                }
                else
                {
                    //File exists in the corpus
                    if (File.GetLastWriteTime(file).Ticks > _corpus.getLastCrawled().Result)
                    {
                        //File has been edited since last checked, re-index it and update it into the corpus
                        await editFile(file);
                    }
                }
                Console.WriteLine("done");
            }
            var kFiles = await _corpus.getDocuments(); //Files that exist in my database
            foreach (var id in kFiles)
            {
                var file = _corpus.getDocumentPath(id).Result;
                if (!files.Contains(Path.Combine(_corpus.getRepository().Result, file)))
                {
                    //File does not exist again in repo, delete it from database
                    await removeFile(file);
                }
            }

            //------------Watching for changes--------------------
            watcher = new FileSystemWatcher(_corpus.getRepository().Result);
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Attributes;
            watcher.Changed += new FileSystemEventHandler((source, e) =>
            {
                try
                {
                    editFile(e.FullPath);
                }
                catch (IOException)
                {
                    Console.WriteLine("Exception, enqued");
                    enQueue(editFile, e.FullPath);
                }

            });
            watcher.Created += new FileSystemEventHandler(async (source, e) =>
            {
                try
                {
                    await _corpus.addDocument(e.FullPath);
                }
                catch (IOException)
                {
                    Console.WriteLine("Exception, enqued");
                    enQueue(addFile, e.FullPath);
                }
            });
            watcher.Deleted += new FileSystemEventHandler((source, e) =>
            {
                removeFile(e.FullPath);
            });
            watcher.Renamed += new RenamedEventHandler((source, e) =>
            {
                renameFile(e.FullPath);
            });

            watcher.EnableRaisingEvents = true;
        }
        private async Task renameFile(string file)
        {
            Console.WriteLine("{0} was just renamed ", file);
            file = file.Replace(_corpus.getRepository().Result + "\\", "");
            var id = await _corpus.getDocumentID(file);
            await _corpus.changeDocumentPath(id, file);
            await _corpus.setLastCrawled(DateTime.Now.Ticks);
            Console.WriteLine("Renamed");
        }
        private async Task removeFile(string file)
        {
            Console.WriteLine("{0} was just removed ", file);
            file = file.Replace(_corpus.getRepository().Result + "\\", "");
            var id = await _corpus.getDocumentID(file);
            await _corpus.removeDocument(id);
            await _corpus.setLastCrawled(DateTime.Now.Ticks);
            Console.WriteLine("Removed");
        }

        private async Task addFile(string file)
        {
            Console.WriteLine("{0} was just Added ", file);
            file = file.Replace(_corpus.getRepository().Result + "\\", "");
            var index = simulateIndexer(file);//new Indexer.index(file);
            await _corpus.addDocument(file, index);
            await _corpus.setLastCrawled(DateTime.Now.Ticks);
            Console.WriteLine("Added");
        }

        public async Task editFile(String file)
        {
            Console.WriteLine("{0} was just Modified ", file);
            file = file.Replace(_corpus.getRepository().Result + "\\", "");
            var id = _corpus.getDocumentID(file).Result;
            var index = simulateIndexer(file);
            await _corpus.removeDocument(id);
            await _corpus.addDocument(file, index);
            await _corpus.setLastCrawled(DateTime.Now.Ticks);
            Console.WriteLine("Edited");
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
                if (String.Compare(tuple.Item2, oldName, true)==0)
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

        public Dictionary<String, int> simulateIndexer(String path)
        {
            var res = new Dictionary<String, int>();
            Console.WriteLine("try");
            var reader = new StreamReader(Path.Combine(_corpus.getRepository().Result, path));
            var list = Regex.Split(Regex.Replace(
                                         reader.ReadToEnd(),
                                        "[^\\w\\s]+",
                                        ""
                                        ).Trim(), "\\s+");
            var stemmer = new Stemmer();
            reader.Close();
            foreach (var item in list)
            {
                var word = stemmer.StemWord(item.ToLower());
                try
                {
                    res[word] += 1;
                }
                catch
                {
                    res[word] = 1;
                }
            }
            return res;
        }
    }

}
