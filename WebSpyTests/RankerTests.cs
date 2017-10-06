using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebSpy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using TikaOnDotNet.TextExtraction;

namespace WebSpy.Tests
{
    [TestClass()]
    public class RankerTests
    {
        Ranker ranker;
        [TestMethod()]
        public void RankerTest()
        {
            var query = new Dictionary<string, int>();
            query["life"] = 1;
            query["learning"] = 1;
            //ranker = new Ranker(query);

            ////Example of how to use the streamer
            ////Take this Code portion and put it where u want to put it.
            ////Don't forget the using statement above.
            ////Ben make sure to document your code very well and delete all these comments ;)
            //TextExtractor a = new TextExtractor();
            //var b = a.Extract(@"C: \Users\kooldeji\Documents\Finding Your Purpose.docx");
            //var c = b.Text.Trim();
            //Console.WriteLine(c+" + "+b.GetType()+" + "+c.GetType());
                        

        }

        [TestMethod()]
        public void NestedCopyTest()
        {
            Dictionary<string, int> a = new Dictionary<string, int>();
            a["1+1"] = 2;
            a["2+2"] = 4;
            Dictionary<string, int> b = Ranker.NestedCopy(a);
            a["1+1"] = 3;
            Console.WriteLine(a["1+1"]);
            Console.WriteLine(b["1+1"]);
            Assert.AreNotEqual(a["1+1"], b["1+1"]);
        }

        [TestMethod()]
        public void getRankTest()
        {
            var a = new HashSet<int>();
            Dictionary<int, int> b = new Dictionary<int, int>();
            a.Add(1);
            a.Add(2);
            a.Add(5);
            b[3] = 3;
            b[4] = 4;
            b[5] = 5;

            a.UnionWith(b.Keys);
            foreach (var i in a)
            {
                Console.WriteLine(i);
            }
        }

        [TestMethod()]
        public void getRankTest2()
        {

           
        }
    }
}