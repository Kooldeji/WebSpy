using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TikaOnDotNet.TextExtraction;

namespace WebSpy
{
    public class Indexer
    {
        private Corpus _corpus;
        HashSet<string> stopWords;
        private Tokenizer _tokenizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="Indexer"/> class.
        /// </summary>
        /// <param name="corpus">The corpus. Instantiates the corpus</param>
        public Indexer (Corpus corpus, Tokenizer tokenizer)
        {
            _corpus = corpus;
            _tokenizer = tokenizer;
            ////Load stop words into array
            //var reader = new StreamReader("StopWords.txt");
            //stopWords = new HashSet<string>();
            //while (!reader.EndOfStream)
            //{
            //    stopWords.Add(reader.ReadLine());
            //}
        }
        /// <summary>
        /// Indexes the file passed to it
        /// </summary>
        /// <param name="path">The path of a file.</param>
        /// <returns>Returns a list</returns>
        public Tuple<int, List<ITermDocument>> Index(String path)
                    
        {
            var textExtractor = new TextExtractor();

            var fileData = textExtractor.Extract(Path.Combine(_corpus.GetRepository().Result, path)); //Extract the contents of the file in the specified file path

            var text = path + fileData.Text;

            return _tokenizer.Tokenize(text);  
        }
    }
}
