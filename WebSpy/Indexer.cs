using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebSpy
{
    class Indexer : Tokenizer
    {
        public static void generateString(string path)
        {
            string filePath = path;
            var text = "";
            foreach(string data in File.ReadAllLines(filePath))
            {
                text += data + " ";
            }
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
                stemmedText += stem + " ";      //Append all stemmed words into a string that will be tokenizer
            }

            var tok = new Tokenizer();  //Call the tokenizer class
            tok.generateToken(stemmedText); //Pass the stemmed text to the method
        }


    }
}
