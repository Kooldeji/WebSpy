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
        private ICorpus _corpus;
        HashSet<string> stopWords;
        private Tokenizer _tokenizer;
        private readonly string _repo;

        /// <summary>
        /// Initializes a new instance of the <see cref="Indexer"/> class.
        /// </summary>
        /// <param name="corpus">The corpus. Instantiates the corpus</param>
        public Indexer (ICorpus corpus)
        {
            _corpus = corpus;
            _tokenizer = new Tokenizer(corpus.StopWords);
            _repo = _corpus.GetRepository().Result;
        }
        /// <summary>
        /// Indexes the file passed to it
        /// </summary>
        /// <param name="path">The path of a file.</param>
        /// <returns>Returns a list</returns>
        public Tuple<int, List<ITermDocument>> Index(String path)
                    
        {
            var textExtractor = new TextExtractor();
            
            //Extract the contents of the file in the specified file path
            var fileData = textExtractor.Extract(Path.Combine(_repo, path)); 

            //Adding Term's Path for indexing.
            var text = path + fileData.Text;

            //Return Tokenized form of text
            return _tokenizer.Tokenize(text);  
        }
    }
}
