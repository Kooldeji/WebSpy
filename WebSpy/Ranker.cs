using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TikaOnDotNet.TextExtraction;

namespace WebSpy
{
    public class Ranker
    {
        /// <summary>
        /// The documents that have any of the query terms
        /// </summary>
        private HashSet<String> documents;
        /// <summary>
        /// An inverted index of terms mapped to a mapping of each document mapped to 
        /// the term's TF_IDF rank.
        /// </summary>
        private Dictionary<String, Dictionary<String, double>> documentRank;
        /// <summary>
        /// The query document containing each term mapped it's TF_IDF rank.
        /// </summary>
        private Dictionary<String, double> query;
        /// <summary>
        /// Initializes a new instance of the <see cref="Ranker"/> class.
        /// </summary>
        /// <param name="query">A mapping of each query term to the frequency in the query document.</param>
        public Ranker( Dictionary<String, int> query)
        {
            if (query == null) throw new ArgumentNullException();
            this.query = new Dictionary<string, double>();
            documentRank = new Dictionary<string, Dictionary<string, double>>();
            documents = new HashSet<string>();
            CompTF_IDF(query);
            
        }

        public List<KeyValuePair<String, double>> Rank()
        {
            //A list of the relevant documents in other of relevance
            var rank = new List<KeyValuePair<String, double>>();

            //Iterates through the documents and inserts it into the right index according to 
            //descending cosine similarity.
            foreach (String id in documents)
            {
                var inserted = false;
                var value = CosineSimilarity(id);
                for (int i=0; i<rank.Count; i++)
                {
                    if (value >= rank[i].Value)
                    {
                        rank.Insert(i, new KeyValuePair<string, double>(id, value));
                        inserted = true;
                        break;
                    }
                }
                if (!inserted) rank.Add(new KeyValuePair<string, double>(id, value));
            }
            return rank;
        }
        //Computes the TF-IDF rank of both the query and the documents relevant 
        //to the document
        private void CompTF_IDF(Dictionary<string, int> query)
        {
            //Compute the size of the query to compute each query's IDF
            var querySize = 0;
            foreach (var term in query)
            {
                querySize += term.Value;
            }

            foreach (KeyValuePair<String, int> term in query)
            {
                IDictionary<String, int> documents = Corpus.getDocuments(term.Key);
                this.documents.UnionWith(documents.Keys);
                var nDocuments = new Dictionary<String, double>();
                var IDF = 1+Math.Log(1.0 * Corpus.getNoDocuments() / documents.Keys.Count);
                this.query[term.Key] = 1.0*term.Value / querySize *IDF;
                foreach (var item in documents)
                {
                    var tF = 1.0* item.Value / Corpus.getDocumentLength(item.Key) ;
                    nDocuments[item.Key] = tF * IDF;
                }
                documentRank[term.Key] = nDocuments;
                ;
            }
        }

        //Computes the cosine similarity between the query and the document parameter
        private double CosineSimilarity(String id)
        {
            double dotProduct = 0;
            double queryMod = 0;
            double documentMod = 0;
            foreach (var term in query)
            {
                queryMod += Math.Pow(term.Value,2);
                if (documentRank[term.Key].ContainsKey(id))
                {
                    documentMod += Math.Pow(documentRank[term.Key][id], 2);
                    dotProduct += term.Value * documentRank[term.Key][id];
                }
            }
            queryMod = Math.Sqrt(queryMod);
            documentMod = Math.Sqrt(documentMod);
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
