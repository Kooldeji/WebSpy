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
        private Dictionary<String, Dictionary<String, double>> _documentRank;
        /// <summary>
        /// The query document containing each term in the query mapped to it's TF_IDF rank.
        /// </summary>
        private Dictionary<String, double> _queryRank;
        /// <summary>
        /// A list of the relevant documents in other of relevance
        /// </summary>
        /// <value>
        /// A List of KeyValuePairs of string and double
        /// </value>
        public List<KeyValuePair<String, double>> RankList
        {
            get;
            private set;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Ranker"/> class.
        /// </summary>
        /// <param name="query">A mapping of each query term to the frequency in the query document.</param>
        public Ranker(ICorpus corpus, Tuple<int, List<ITermDocument>> query)
        {
            if (query == null || corpus == null) throw new ArgumentNullException();
            this._queryRank = new Dictionary<string, double>();
            _corpus = corpus;
            _documentRank = new Dictionary<string, Dictionary<string, double>>();
            _documents = new HashSet<string>();
            RankList = new List<KeyValuePair<String, double>>();
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
                for (int i = 0; i < RankList.Count; i++)
                {
                    if (value >= RankList[i].Value)
                    {
                        RankList.Insert(i, new KeyValuePair<string, double>(id, value));
                        inserted = true;
                        break;
                    }
                }
                if (!inserted) RankList.Add(new KeyValuePair<string, double>(id, value));
            }
        }
        //Computes the TF-IDF rank of both the query and the documents relevant 
        //to the document
        private void CompTF_IDF(Tuple<int, List<ITermDocument>> query)
        {
            //A cache of Length of each documents needed for ranking.
            IDictionary<String, long> cacheDocumentsLength = new Dictionary<String, long>();

            //No of document in Corpus.
            long noDocs = _corpus.GetNoDocuments().Result;

            //Compute the size of the query to compute each query's IDF
            var querySize = query.Item1;

            foreach (var term in query.Item2)
            {
                IDictionary<String, int> documents = new Dictionary<String, int>();
                foreach (var doc in _corpus.GetDocuments(term.Term).Result)
                {
                    if (documents.Keys.Contains(doc.DocID)) continue;
                    documents.Add(doc.DocID, doc.Pos.Count);
                    this._documents.Add(doc.DocID);
                }
                if (documents.Count < 1) continue;
                var nDocuments = new Dictionary<String, double>();
                double IDF = (1 + Math.Log(1.0 * noDocs / documents.Keys.Count));
                this._queryRank[term.Term] = (1.0 * term.Docs.First().Pos.Count / querySize) * IDF;
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
                    if (length == 0) continue;
                    double tF = 1.0 * item.Value / length;
                    nDocuments[item.Key] = tF * IDF;
                }
                _documentRank[term.Term] = nDocuments;
                ;
            }
        }

        //Computes the cosine similarity between the query and the document parameter
        private double CosineSimilarity(String id)
        {
            double dotProduct = 0;
            double queryMod = 0;
            double documentMod = 0;
            foreach (var term in _queryRank)
            {
                queryMod += Math.Pow(term.Value, 2);
                if (_documentRank[term.Key].ContainsKey(id))
                {
                    documentMod += Math.Pow(_documentRank[term.Key][id], 2);
                    dotProduct += term.Value * _documentRank[term.Key][id];
                }
            }
            queryMod = Math.Sqrt(queryMod);
            documentMod = Math.Sqrt(documentMod);
            if (documentMod == 0) return 0;
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
