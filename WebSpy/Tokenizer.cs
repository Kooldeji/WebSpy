using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace WebSpy
{
    class Tokenizer
    {
        public void generateToken(string s)
        {

            string editString = s;
            var regexItem = new Regex("[^a-zA-Z0-9_']+"); //create a regex object
            if (regexItem.IsMatch(s[s.Length - 1].ToString()))
            {
                editString = s.Remove(s.Length - 1);
            }
            string newString = Regex.Replace(editString, "[^a-zA-Z0-9_']+", " "); //Replace characterds with space

            newString = newString.ToLower();  //Convert all characters i the string to lower case
            string[] words = newString.Split(' '); //Add all words in the string to  array
            Dictionary<string, int> dict = new Dictionary<string, int>();  //Create a dictionary to save the number of occurece of every word in the array
            for (int i = 0; i < words.Length - 1; i++)
            {
                if (dict.ContainsKey(words[i]))
                {
                    dict[words[i]] += 1;
                }
                else
                {
                    dict.Add(words[i], 1);
                }
            }

            //Print the dictionary
            foreach (KeyValuePair<string, int> item in dict)
            {
                Console.WriteLine("Key: {0} Value: {1}", item.Key, item.Value);
            }
            Console.ReadKey();
        }
    }
}
