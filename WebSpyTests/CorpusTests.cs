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
    public class CorpusTests
    {
        Dictionary<String, Dictionary<String, int>> a;
        [TestInitialize()]
        public void CorpusTest()
        {
            a = new Dictionary<String, Dictionary<String, int>>();
            a["the"] = new Dictionary<string, int>();
            a["the"]["1"] = 1;
            a["the"]["2"] = 1;
            a["game"] = new Dictionary<string, int>();
            a["game"]["1"] = 2;
            a["of"] = new Dictionary<string, int>();
            a["of"]["1"] = 2;
            a["life"] = new Dictionary<string, int>();
            a["life"]["1"] = 1;
            a["life"]["2"] = 1;
            a["is"] = new Dictionary<string, int>();
            a["is"]["1"] = 1;
            a["is"]["2"] = 1;
            a["a"] = new Dictionary<string, int>();
            a["a"]["1"] = 1;
            a["everlasting"] = new Dictionary<string, int>();
            a["everlasting"]["1"] = 1;
            a["learning"] = new Dictionary<string, int>();
            a["learning"]["1"] = 1;
            a["learning"]["3"] = 1;
            a["unexamined"] = new Dictionary<string, int>();
            a["unexamined"]["2"] = 1;
            a["not"] = new Dictionary<string, int>();
            a["not"]["2"] = 1;
            a["worth"] = new Dictionary<string, int>();
            a["worth"]["2"] = 1;
            a["living"] = new Dictionary<string, int>();
            a["living"]["2"] = 1;
            a["never"] = new Dictionary<string, int>();
            a["never"]["3"] = 1;
            a["stop"] = new Dictionary<string, int>();
            a["stop"]["3"] = 1;
            
        }

        [TestMethod()]
        public void GetTermFrequenciesTest()
        {

            Assert.AreEqual(a.Count, 14);
        }

        [TestMethod()]
        public void getDocumentsTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void getDocumentsTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void getDocumentsTest2()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void getDocumentLengthTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void getNoDocumentsTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void getTermsTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void getTermsTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void getRepositoryTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void getDocumentPathTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void addDocumentTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void removeDocumentTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void changePathTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void setRepoTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void setLastCrawledTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void getLastCrawledTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void getDocumentIDTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void updateTest()
        {
            Assert.Fail();
        }
    }
}