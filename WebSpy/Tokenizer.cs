using System;
using System.Collections.Generic;

namespace WebSpy
{
    class Tokenizer
    {
        public void generateToken(string s)
        {
            var newString = s.ToLower();  //Convert all characters i the string to lower case
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
                Console.WriteLine("Key:\t {0} Value: {1}", item.Key, item.Value);
            }
            Console.ReadKey();
        }
    }
}
