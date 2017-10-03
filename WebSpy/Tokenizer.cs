﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace WebSpy
{
  
    public class Tokenizer
    {
        private Stemmer _stemmer;
        private HashSet<string> _stopWords;

        public Tokenizer(HashSet<string> stopWords)
        {
            _stopWords = stopWords;
            _stemmer = new Stemmer();  //Create an instance of the Stemmer class

        }
        /// <summary>
        /// Generates the token.
        /// </summary>
        /// <param name="text">This is the string which is passed into the method.</param>
        public Tuple<int, List<ITermDocument>> Tokenize(string text)
        {
            var termDict = new Dictionary<string, ITermDocument>();
            var docDict = new Dictionary<string, IDocumentReference>();
            //Adding Term's Path for indexing.
            var list = new List<string>(Regex.Split(Regex.Replace(
                                         text,
                                        "[^a-zA-Z0-9']", //Use Regex expressions to include only a selected portion of the data
                                        " "
                                        ).Trim(), "\\s+"));

            //Document Length
            var length = list.Count;


            for (int i = 0; i < list.Count; i++)  //Loop through the list
            {
                var word = list[i];

                //Remove stopwords
                if (_stopWords.Contains(list[i]))
                {
                    length -= 1;
                    continue;
                }

                var rootWord = _stemmer.StemWord(word.ToLower());
                ITermDocument term;
                if (termDict.ContainsKey(rootWord))       //Check if the word has already been added to the dictionary
                {
                    term = termDict[rootWord];
                }
                else                                        //Add words to the dictionary
                {
                    term = new TermDocument(rootWord);
                    termDict.Add(rootWord, term);
                }
                IDocumentReference docref;
                if (docDict.ContainsKey(word))            //Check if document reference contains word
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
    }
}
