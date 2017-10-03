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

        /// <summary>
        /// Initializes a new instance of the <see cref="Indexer"/> class.
        /// </summary>
        /// <param name="corpus">The corpus. Instantiates the corpus</param>
        public Indexer (Corpus corpus)
        {
            _corpus = corpus;

            //Load stop words into array
            var reader = new StreamReader("StopWords.txt");
            stopWords = new HashSet<string>();
            while (!reader.EndOfStream)
            {
                stopWords.Add(reader.ReadLine());
            }
        }
        /// <summary>
        /// Indexes the file passed to it
        /// </summary>
        /// <param name="path">The path of a file.</param>
        /// <returns>Returns a list</returns>
        public Tuple<int, List<ITermDocument>> simulateIndexer(String path)
                    
        {
            

            var stemmer = new Stemmer();  //Create an instance of the Stemmer class
            var termDict = new Dictionary<string, ITermDocument>();
            var docDict = new Dictionary<string, IDocumentReference>();
            var textExtractor = new TextExtractor();

            var fileData = textExtractor.Extract(Path.Combine(_corpus.GetRepository().Result, path)); //Extract the contents of the file in the specified file path
            var list = Regex.Split(Regex.Replace(           //Use Regex expressions to include only a selected portion of the data
                                         fileData.Text,
                                        "[^a-zA-Z0-9']+",
                                        " "
                                        ).Trim(), "\\s+");
            
            for (int i = 0; i < list.Length; i++)  //Loop through the list
            {
                var word = list[i];

                //Remove stopwords
                if (stopWords.Contains(list[i]))
                {
                    continue;
                }
                
                var rootWord = stemmer.StemWord(word.ToLower()); 
                ITermDocument term;
                if (termDict.Keys.Contains(rootWord))       //Check if the word has already been added to the dictionary
                {
                    term = termDict[rootWord];
                }
                else                                        //Add words to the dictionary
                {
                    term = new TermDocument(rootWord);
                    termDict.Add(rootWord, term);
                }
                IDocumentReference docref;
                if (docDict.Keys.Contains(word))            //Check if document reference contains word
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
            return Tuple.Create(list.Length, termDict.Values.ToList());  
        }
    }
}
