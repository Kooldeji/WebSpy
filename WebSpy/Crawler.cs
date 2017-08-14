using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WebSpy
{
    public class Crawler
    {
        FileSystemWatcher watcher;
        Corpus _corpus;
        public Crawler(Corpus corpus)
        {
            _corpus = corpus;
            var res = init();
            while (res.Status != TaskStatus.RanToCompletion);


        }

        public async Task init()
        {

            var files = crawl(_corpus.getRepository().Result).ToList();//All files in my repository
            bool res;
            foreach (var file in files)
            {
                var sfile = file.Replace(_corpus.getRepository().Result + "\\", "");
                //For each file in the repository, try to get their id from the corpus
                string id = _corpus.getDocumentID(sfile).Result;
                if (id == null)
                {
                    //File is not in the Corpus
                    //Index it and add it to the corpus.
                    Console.WriteLine("{0} was just Created ", file);
                    var index = simulateIndexer(sfile);//new Indexer.index(file);
                    res = await _corpus.addDocument(sfile, index);
                }
                else
                {
                    //File exists in the corpus
                    if (File.GetLastWriteTime(file).Ticks > _corpus.getLastCrawled().Result)
                    {
                        //File has been edited since last checked, re-index it and update it into the corpus
                        Console.WriteLine("{0} Edited ", sfile);
                        var index = simulateIndexer(sfile); // new Indexer.index(file);
                        await _corpus.removeDocument(id);
                        await _corpus.addDocument(sfile, index);
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
                    Console.WriteLine("{0} removed ", file);
                    await _corpus.removeDocument(id);
                }
            }
            res = _corpus.setLastCrawled(DateTime.Now.Ticks).Result;
            //------------Watching for changes--------------------
            watcher = new FileSystemWatcher(_corpus.getRepository().Result);
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.Changed += new FileSystemEventHandler(async (source, e) =>
            {
                await _corpus.setLastCrawled(DateTime.Now.Ticks);
                Console.WriteLine("Changed");
            });
            watcher.Created += new FileSystemEventHandler(async (source, e) =>
            {
                await _corpus.setLastCrawled(DateTime.Now.Ticks);
                Console.WriteLine("Created");
            });
            watcher.Deleted += new FileSystemEventHandler(async (source, e) =>
            {
                await _corpus.setLastCrawled(DateTime.Now.Ticks);
                Console.WriteLine("Deleted");
            });
            watcher.Renamed += new RenamedEventHandler(async (source, e) =>
            {
                
                await _corpus.setLastCrawled(DateTime.Now.Ticks);
                Console.WriteLine("Renamed");
            });

            watcher.EnableRaisingEvents = true;
        }
        public static IEnumerable<string> crawl(String path)
        {
            foreach(var file in Directory.EnumerateFiles(path))
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
            var list = new StreamReader(Path.Combine(_corpus.getRepository().Result, path)).ReadToEnd().Trim().Split(' ', '\n');
            foreach (var item in list)
            {
                try {
                    res[item.ToLower()] += 1;
                }
                catch
                {
                    res[item.ToLower()] = 1;
                }
            }
            return res;
        }
        
    }
}
