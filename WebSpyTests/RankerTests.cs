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
    public class RankerTests
    {
        Ranker ranker;
        [TestMethod()]
        public void RankerTest()
        {
            var query = new Dictionary<string, int>();
            query["life"] = 1;
            query["learning"] = 1;
            ranker = new Ranker(query);

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
        public void getRankTest1()
        {
            var query = new Dictionary<string, int>();
            query["life"] = 1;
            query["learning"] = 1;
            ranker = new Ranker(query);
            List<KeyValuePair<String, double>> ranks = ranker.Rank();
            Console.WriteLine(ranks);
            foreach (var item in ranks)
            {
                Console.WriteLine(item);
            }
            Assert.AreEqual(1,1);
        }
    }
}