using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebSpy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB;


namespace WebSpy.Tests
{
    [TestClass()]
    public class CorpusTests
    {
        Corpus corpus;
        String testID;
        [TestInitialize()]
        public void CorpusTest()
        {
            corpus = Corpus.init();
        }

        [TestMethod()]
        public void GetTermFrequenciesTest()
        {
            CorpusTest();
            var res = corpus.GetTermFrequencies();
            res.Wait();
            foreach (var i in res.Result)
            {
                Console.WriteLine(i.Key);
            }
        }

        [TestMethod()]
        public void getDocumentsTest()
        {
            CorpusTest();
            var res = corpus.getDocuments();
            res.Wait();
            foreach (var i in res.Result)
            {
                Console.WriteLine(i);
            }
        }

        [TestMethod()]
        public void getDocumentsTest1()
        {
            CorpusTest();
            var res = corpus.getDocuments("learning");
            res.Wait();
            foreach (var i in res.Result)
            {
                Console.WriteLine(i.Key + " " + i.Value);
            }
        }

        [TestMethod()]
        public void getDocumentsTest2()
        {
            CorpusTest();
            var res = corpus.getNoDocuments();
            res.Wait();
            Console.WriteLine(res.Result);
            Console.WriteLine(res);
        }

        [TestMethod()]
        public void getDocumentLengthTest()
        {
        }

        [TestMethod()]
        public void getNoDocumentsTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void getTermsTest()
        {
            CorpusTest();
            var res = corpus.getTerms();
            res.Wait();
            foreach (var i in res.Result)
            {
                Console.WriteLine(i);
            }
        }

        [TestMethod()]
        public void getTermsTest1()
        {
            
            var res = corpus.getTerms("598e28b96f42650033a71cba");
            res.Wait();
            foreach (var i in res.Result)
            {
                Console.WriteLine(i);
            }
        }

        [TestMethod()]
        public void getRepositoryTest()
        {
            CorpusTest();
            var res6 = corpus.getRepository();
            res6.Wait();
            Console.WriteLine(res6.Result);
        }

        [TestMethod()]
        public void getDocumentPathTest()
        {
            CorpusTest();
            corpus.changeDocumentPath(testID, "1.txt").Wait();
        }

        [TestMethod()]
        public void addDocumentTest()
        {
            var res =  corpus.addDocument("5.txt");
            res.Wait();
            Console.WriteLine(res.Result);

            var a = new Dictionary<string, int>();
            a["stop"] = 1;
            a["learning"] = 2;
            res = corpus.addDocument("4.txt", a);
            res.Wait();
            Console.WriteLine(res.Result);
        }
        
        [TestMethod()]
        public void removeDocumentTest()
        {
            CorpusTest();
            var res = corpus.removeDocument(testID);
            res.Wait();
            Console.WriteLine(res.Result);
        }

        [TestMethod()]
        public void changePathTest()
        {
            CorpusTest();
            var res = corpus.changeDocumentPath("598e28b96f42650033a71cba", "5.txt");
            res.Wait();
            Console.WriteLine(res.Result);
        }

        [TestMethod()]
        public void setRepoTest()
        {
            var res = corpus.setRepository("C:/ Users / kooldeji / Documents / repo");
            res.Wait();
        }

        [TestMethod()]
        public void setLastCrawledTest()
        {
            CorpusTest();
            var res = corpus.setLastCrawled(100);
            res.Wait();
            Console.WriteLine(res.Result);
        }

        [TestMethod()]
        public void getLastCrawledTest()
        {
            CorpusTest();
            var res = corpus.getLastCrawled();
            res.Wait();
            Console.WriteLine(res.Result);
        }

        [TestMethod()]
        public void getDocumentIDTest()
        {
            var res = corpus.getDocumentID("4.txt");
            res.Wait();
            Console.WriteLine(res.Result);
        }

        [TestMethod()]
        public void updateTest()
        {
            Assert.Fail();
        }
    }
}