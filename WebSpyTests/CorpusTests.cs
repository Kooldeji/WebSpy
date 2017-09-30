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
            corpus.Default().Wait();
        }

        [TestMethod()]
        public void GetTermDocumentsTest()
        {
            CorpusTest();
            var res = corpus.GetTermDocuments();
            res.Wait();
            Console.WriteLine(res.Result.Count());
            foreach (var i in res.Result)
            {
                Console.WriteLine(i.Term);
            }
        }

        [TestMethod()]
        public void getDocumentsTest()
        {
            CorpusTest();
            var res = corpus.GetDocuments();
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
            var res = corpus.GetDocuments("Learn");
            res.Wait();
            foreach (var i in res.Result)
            {
                Console.WriteLine(i.DocID);
            }
        }

        [TestMethod()]
        public void GetDocumentsTest3()
        {
            CorpusTest();
            var res = corpus.GetDocuments(id=>id=="1");
            res.Wait();
            Console.WriteLine(res.Result);
        }

        [TestMethod()]
        public void getDocumentLengthTest()
        {
            CorpusTest();
            var res = corpus.GetDocumentLength("2");
            res.Wait();
            Console.WriteLine(res.Result);
        }

        [TestMethod()]
        public void getNoDocumentsTest()
        {
            CorpusTest();
            var res = corpus.GetNoDocuments();
            res.Wait();
            Console.WriteLine(res.Result);
        }

        [TestMethod()]
        public void getTermsTest()
        {
            CorpusTest();
            var res = corpus.GetTerms();
            res.Wait();
            foreach (var i in res.Result)
            {
                Console.WriteLine(i);
            }
        }

        [TestMethod()]
        public void getTermsTest1()
        {

            var res = corpus.GetTerms("1");
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
            var res6 = corpus.GetRepository();
            res6.Wait();
            Console.WriteLine(res6.Result);
        }

        [TestMethod()]
        public void getDocumentPathTest()
        {
            CorpusTest();
            var res6 = corpus.GetDocumentPath("1");
            res6.Wait();
            Console.WriteLine(res6.Result);
        }

        [TestMethod()]
        public void addDocumentTest()
        {
            CorpusTest();
            var res = corpus.AddDocument("3.txt");
            res.Wait();
            Console.WriteLine(res.Result);

            var a = new TermDocument("stop");
            a.addDoc(new DocumentReference("", "ped", new HashSet<int>(new int[] { 1 })));
            var b = new TermDocument("Learn");
            b.addDoc(new DocumentReference("", "ing", new HashSet<int>(new int[] { 2 })));
            res = corpus.AddDocument("4.txt", 2, new List<ITermDocument>(new TermDocument[] {a,b }));
            res.Wait();
            Console.WriteLine(res.Result);
            
        }

        [TestMethod()]
        public void removeDocumentTest()
        {
            CorpusTest();
            var res = corpus.RemoveDocument("1");
            res.Wait();
            Console.WriteLine(res.Result);
        }

        [TestMethod()]
        public void changePathTest()
        {
            CorpusTest();
            var res = corpus.ChangeDocumentPath("1", "5.txt");
            res.Wait();
            Console.WriteLine(res.Result);
        }

        [TestMethod()]
        public void setRepoTest()
        {
            CorpusTest();
            var res = corpus.SetRepository("C:/ Users / koldeji / Documents / repo");
            res.Wait();
        }

        [TestMethod()]
        public void setLastCrawledTest()
        {
            CorpusTest();
            var res = corpus.SetLastCrawled(900);
            res.Wait();
            Console.WriteLine(res.Result);
        }

        [TestMethod()]
        public void getLastCrawledTest()
        {
            CorpusTest();
            var res = corpus.GetLastCrawled();
            res.Wait();
            Console.WriteLine(res.Result);
        }

        [TestMethod()]
        public void getDocumentIDTest()
        {
            CorpusTest();
            var res = corpus.GetDocumentID("1.txt");
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