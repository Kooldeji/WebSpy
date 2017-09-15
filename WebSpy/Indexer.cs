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
    class Indexer
    {
        public void GenerateString(string path)
        {
            //use tika to extract data and store inside variable fileData
            var textExtractor = new TextExtractor();

            var fileData = textExtractor.Extract(path);
            string text = fileData.Text;

            var regexItem = new Regex("[^a-zA-Z0-9_']+"); //create a regex object
            if (regexItem.IsMatch(text[text.Length - 1].ToString()))
            {
                text = text.Remove(text.Length - 1);
            }

            string editedText = Regex.Replace(text, "[^a-zA-Z0-9_']+", " "); //Replace characters with space
            string[] words = editedText.Split(' '); //Add all words in the string to  array

            var stemmer = new Stemmer(); //Create a stemmer object
            var stemmedText = "";

            for (int i = 0; i<words.Length; i++)
            {
                var stem = stemmer.StemWord(words[i]);
                stemmedText += stem + " ";      //Append all stemmed words into a string that will be tokenized
            }

            var tok = new Tokenizer();  //Call the tokenizer class
            tok.generateToken(stemmedText); //Pass the stemmed text to the method
        }


    }
}
