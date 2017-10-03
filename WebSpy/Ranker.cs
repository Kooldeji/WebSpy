using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSpy
{
    public class Ranker
    {
        /// <summary>
        /// An instance of the corpus.
        /// </summary>
        private ICorpus _corpus;
        /// <summary>
        /// The documents that have any of the query terms
        /// </summary>
        private HashSet<String> _documents;
        /// <summary>
        /// An inverted index of query terms mapped to a mapping of each document to 
        /// the term's TF_IDF rank.
        /// </summary>
        private Dictionary<String, Dictionary<String, decimal>> _documentRank;
        /// <summary>
        /// The query document containing each term in the query mapped to it's TF_IDF rank.
        /// </summary>
        private Dictionary<String, decimal> _queryRank;
        /// <summary>
        /// A list of the relevant documents in other of relevance
        /// </summary>
        /// <value>
        /// A List of KeyValuePairs of string and double
        /// </value>
        public List<KeyValuePair<String, decimal>> RankList
        {
            get;
            private set;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Ranker"/> class.
        /// </summary>
        /// <param name="query">A mapping of each query term to the frequency in the query document.</param>
        public Ranker(ICorpus corpus, Dictionary<String, int> query)
        {
            if (query == null || corpus == null) throw new ArgumentNullException();
            this._queryRank = new Dictionary<string, decimal>();
            _corpus = corpus;
            _documentRank = new Dictionary<string, Dictionary<string, decimal>>();
            _documents = new HashSet<string>();
            RankList = new List<KeyValuePair<String, decimal>>();
            CompTF_IDF(query);
            Rank();
            
        }

        /// <summary>
        /// Returns a list of keyValuePair of the documents (in descending order) and their rank.
        /// </summary>
        /// <returns></returns>
        public void Rank()
        {

            //Iterates through the documents and inserts it into the right index according to 
            //descending cosine similarity.
            foreach (String id in _documents)
            {
                var inserted = false;
                var value = CosineSimilarity(id);
                for (int i=0; i<RankList.Count; i++)
                {
                    if (value >= RankList[i].Value)
                    {
                        RankList.Insert(i, new KeyValuePair<string, decimal>(id, value));
                        inserted = true;
                        break;
                    }
                }
                if (!inserted) RankList.Add(new KeyValuePair<string, decimal>(id, value));
            }
        }
        //Computes the TF-IDF rank of both the query and the documents relevant 
        //to the document
        private void CompTF_IDF(Dictionary<string, int> query)
        {
            //A cache of Length of each documents needed for ranking.
            IDictionary<String, long> cacheDocumentsLength = new Dictionary<String, long>();

            //No of document in Corpus.
            long noDocs = _corpus.GetNoDocuments().Result;

            //Compute the size of the query to compute each query's IDF
            var querySize = 0;
            foreach (var term in query)
            {
                querySize += term.Value;
            }

            foreach (var term in query)
            {
                IDictionary<String, int> documents = new Dictionary<String, int>();
                foreach (var doc in _corpus.GetDocuments(term.Key).Result)
                {
                    if (documents.Keys.Contains(doc.DocID)) continue;
                    documents.Add(doc.DocID, doc.Pos.Count);
                    this._documents.Add(doc.DocID);
                }
                if (documents.Count < 1) continue;
                var nDocuments = new Dictionary<String, decimal>();
                decimal IDF = (decimal) (1 + Math.Log(1.0 *  noDocs / documents.Keys.Count));
                this._queryRank[term.Key] = (decimal) (1.0 * term.Value / querySize) * IDF;
                foreach (var item in documents)
                {
                    long length;
                    if (cacheDocumentsLength.ContainsKey(item.Key))
                        length = cacheDocumentsLength[item.Key];
                    else
                    {
                        length = _corpus.GetDocumentLength(item.Key).Result;
                        cacheDocumentsLength[item.Key] = length;
                    }
                    decimal tF = (decimal) 1.0 * item.Value /length;
                    nDocuments[item.Key] = tF * IDF;
                }
                _documentRank[term.Key] = nDocuments;
                ;
            }
        }

        //Computes the cosine similarity between the query and the document parameter
        private decimal CosineSimilarity(String id)
        {
            decimal dotProduct = 0;
            decimal queryMod = 0;
            decimal documentMod = 0;
            foreach (var term in _queryRank)
            {
                queryMod += (decimal) Math.Pow((double) term.Value,2);
                if (_documentRank[term.Key].ContainsKey(id))
                {
                    documentMod += (decimal)Math.Pow((double)_documentRank[term.Key][id], 2);
                    dotProduct += term.Value * _documentRank[term.Key][id];
                }
            }
            queryMod = (decimal) Math.Sqrt((double)queryMod);
            documentMod = (decimal)Math.Sqrt((double)documentMod);
            //Console.WriteLine(dotProduct+" "+ documentMod+" "+ queryMod+" "+   (documentMod * queryMod));
            return dotProduct / (documentMod * queryMod);

        }

        public static Dictionary<TKey, TValue> NestedCopy<TKey, TValue>(
            Dictionary<TKey, TValue> nestedDict)
        {
            var retDict = new Dictionary<TKey, TValue>();
            foreach (var dict in nestedDict)
            {
                if (typeof(TValue).IsGenericType && typeof(TValue).GetGenericTypeDefinition() == typeof(Dictionary<,>))
                {
                    retDict[dict.Key] = (TValue)NestedCopy((dynamic)dict.Value);
                }
                else
                {
                    retDict[dict.Key] = dict.Value;
                }
            }
            return retDict;
        }
        
    }
}
