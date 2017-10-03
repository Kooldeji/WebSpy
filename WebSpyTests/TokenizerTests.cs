using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebSpy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSpy.Tests
{
    [TestClass()]
    public class TokenizerTests
    {
        [TestMethod()]
        public void TestEmptyDictionary()
        {
            Dictionary<string, int> dict = new Dictionary<string, int>();
            string[] words = {};
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

            if(dict.Count != 0)
            {
                Assert.Fail();
            }
        }

        public void TestMultipleEntries()
        {
            Dictionary<string, int> dict = new Dictionary<string, int>();
            string[] words = {"Ben","Ben","Ben"};
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

           if(dict.Count != 1)
            {
                Assert.Fail();
            }
        }


    }
}