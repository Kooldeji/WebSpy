using MongoDB.Driver;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;

namespace WebSpy
{
    /// <summary>
    /// A class that acts as an interface between the entire API and the runtime database.
    /// </summary>
    public class Corpus
    {
        /// <summary>
        /// The Mongo client
        /// </summary>
        private IMongoClient _client;
        /// <summary>
        /// The Mongo database
        /// </summary>
        private IMongoDatabase _database;
        /// <summary>
        /// The root collection
        /// </summary>
        private IMongoCollection<BsonDocument> _root;
        /// <summary>
        /// A collection that consists of an inverted table of the terms and the documents they exist
        /// </summary>
        private IMongoCollection<BsonDocument> _invertedtable;
        /// A collection that encapsulates the documents in the corpus and their attributes like path and length.
        /// </summary>
        private IMongoCollection<BsonDocument> _documentsTable;
        /// <summary>
        /// Initializes a new instance of the <see cref="Corpus"/> class.
        /// </summary>
        /// <param name="client">The Mongo client.</param>
        /// <param name="database">The Mongo database.</param>
        /// <exception cref="System.ArgumentNullException">'Client' and 'Database' must be set to an instance of an object</exception>
        public Corpus(IMongoClient client, IMongoDatabase database)
        {
            if (client == null || database == null)
            {
                throw new ArgumentNullException("'Client' and 'Database' must be set to an instance of an object");
            }
            _client = client;
            _database = database;
            _root = database.GetCollection<BsonDocument>("Root");
            _invertedtable = database.GetCollection<BsonDocument>("InvertedTable");
            _documentsTable = database.GetCollection<BsonDocument>("DocumentsTable");

        }
        /// <summary>
        /// Gets the term frequencies.
        /// </summary>
        /// <returns></returns>
        public async Task<Dictionary<string, Dictionary<string, int>>> GetTermFrequencies()
        {
            var result = new Dictionary<String, Dictionary<string, int>>();
            using (var cursor = await _invertedtable.FindAsync(new BsonDocument()))
            {
                while (await cursor.MoveNextAsync())
                {
                    IEnumerable<BsonDocument> batch = cursor.Current;
                    foreach (BsonDocument document in batch)
                    {
                        result[document["_id"].AsString] = new Dictionary<string, int>();
                        foreach (BsonDocument doc in document["docs"].AsBsonArray)
                        {
                            result[document["_id"].AsString][doc["doc_id"].AsObjectId.ToString()] = doc["no"].AsInt32;
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Gets the documents.
        /// </summary>
        /// <returns></returns>
        public async Task<HashSet<string>> getDocuments()
        {
            var result = new HashSet<String>();
            using (var cursor = await _documentsTable.FindAsync(new BsonDocument()))
            {
                while (await cursor.MoveNextAsync())
                {
                    IEnumerable<BsonDocument> batch = cursor.Current;
                    foreach (BsonDocument document in batch)
                    {
                        result.Add(document["_id"].AsObjectId.ToString());
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Gets the documents that a term exists.
        /// </summary>
        /// <param name="term">The term.</param>
        /// <returns></returns>
        public async Task<Dictionary<string, int>> getDocuments(String term)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", term);
            var result = new Dictionary<String, int>();
            BsonDocument doc = await _invertedtable.Find(filter).FirstOrDefaultAsync();
            if (doc == null)
                return result;
            foreach (var item in doc["docs"].AsBsonArray)
            {
                result[item["doc_id"].AsObjectId.ToString()] = item["no"].AsInt32;
            }
            return result;
        }

        /// <summary>
        /// Gets the documents that match a predicate.
        /// </summary>
        /// <param name="predicate">The predicate to match documents.</param>
        /// <returns></returns>
        public async Task<Dictionary<string, int>> getDocuments(Func<String, bool> predicate)
        {
            var result = new Dictionary<String, int>();
            using (var cursor = await _documentsTable.FindAsync(new BsonDocument()))
            {
                while (await cursor.MoveNextAsync())
                {
                    IEnumerable<BsonDocument> batch = cursor.Current;
                    foreach (BsonDocument document in batch)
                    {
                        var doc = document["_id"].AsObjectId.ToString();
                        var no = (int)document["no"].AsDouble;
                        if (predicate(doc)) result.Add(doc, no);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Gets the length of the document.
        /// </summary>
        /// <param name="id">The Document id.</param>
        /// <returns></returns>
        public async Task<int> getDocumentLength(string id)
        {

            BsonDocument ret = await _documentsTable.Find(Builders<BsonDocument>.Filter.Eq("_id", new ObjectId(id))).FirstOrDefaultAsync();
            if (ret == null) return 0;
            return ret["length"].AsInt32;
        }

        /// <summary>
        /// Gets the no documents.
        /// </summary>
        /// <returns></returns>
        public async Task<int> getNoDocuments()
        {
            BsonDocument ret = await _root.Find(new BsonDocument()).FirstOrDefaultAsync();
            if (ret == null) return 0;
            return (int)ret["no_docs"].AsDouble;
        }

        /// <summary>
        /// Gets the terms.
        /// </summary>
        /// <returns></returns>
        public async Task<List<string>> getTerms()
        {
            List<BsonDocument> docs = await _invertedtable.Find(new BsonDocument()).ToListAsync();
            if (docs == null) return null;
            var ret = new List<String>();
            foreach (var doc in docs)
            {
                ret.Add(doc["_id"].AsString);
            }
            return ret;
        }

        /// <summary>
        /// Gets the terms in a a document.
        /// </summary>
        /// <param name="id">The Document id.</param>
        /// <returns></returns>
        public async Task<List<string>> getTerms(String id)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("docs.doc_id", new ObjectId(id));
            List<BsonDocument> docs = await _invertedtable.Find(filter).ToListAsync();
            if (docs == null) return null;
            var ret = new List<String>();
            foreach (var doc in docs)
            {
                ret.Add(doc["_id"].AsString);
            }
            return ret;
        }

        /// <summary>
        /// Gets the repository.
        /// </summary>
        /// <returns></returns>
        public async Task<string> getRepository()
        {
            BsonDocument ret = await _root.Find(new BsonDocument()).FirstOrDefaultAsync();
            if (ret == null) return "";
            return ret["repo"].AsString;
        }

        /// <summary>
        /// Gets the document path.
        /// </summary>
        /// <param name="id">The Document id.</param>
        /// <returns></returns>
        public async Task<string> getDocumentPath(String id)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", new ObjectId(id));
            BsonDocument ret = await _documentsTable.Find(filter).FirstOrDefaultAsync();
            return ret["path"].AsString;
        }

        /// <summary>
        /// Adds the document.
        /// </summary>
        /// <param name="path">The path that the document points to.</param>
        /// <returns></returns>
        public async Task<bool> addDocument(string path)
        {
            await _documentsTable.InsertOneAsync(new BsonDocument { { "path", path }, { "length", 0 } });
            var update = Builders<BsonDocument>.Update.Inc("no_docs", 1);
            await _root.UpdateOneAsync(new BsonDocument(), update);
            return true;
        }

        /// <summary>
        /// Adds the document to the Corpus.
        /// </summary>
        /// <param name="path">The path that the document points to.</param>
        /// <param name="index">Dictionary of terms mapped to their nos of occurences in the doc.</param>
        /// <returns></returns>
        public async Task<bool> addDocument(String path, Dictionary<string, int> index)
        {
            await addDocument(path);
            var id = getDocumentID(path).Result;
            await updateDocument(id, index);
            return true;
        }

        /// <summary>
        /// Updates the document with the index.
        /// </summary>
        /// <param name="id">The Document id.</param>
        /// <param name="index">Dictionary of terms mapped to their nos of occurences in the doc.</param>
        /// <returns></returns>
        public async Task<bool> updateDocument(string id, Dictionary<string, int> index)
        {
            foreach (var item in index)
            {
                var filter = Builders<BsonDocument>.Filter.Eq("_id", item.Key);
                BsonDocument doc = await _invertedtable.Find(filter).FirstOrDefaultAsync();

                //Term does not exist add term first, then add no document occurence
                if (doc == null)
                {
                    doc = new BsonDocument
                    {
                        {"_id", item.Key},
                        {"docs", new BsonArray
                            {
                                new BsonDocument
                                {
                                    { "doc_id", new ObjectId(id)},
                                    {"no", item.Value }
                                }
                            }
                        }
                    };
                    await _invertedtable.InsertOneAsync(doc);
                }
                else {

                    var docFilter = Builders<BsonDocument>.Filter.Eq("_id", item.Key) & Builders<BsonDocument>.Filter.ElemMatch<BsonDocument>("docs", new BsonDocument("doc_id", new ObjectId(id)));
                    //Term exist in document previously increment the number of occurences
                    doc = await _invertedtable.Find(docFilter).FirstOrDefaultAsync();
                    if (doc != null)
                    {
                        var inc = doc["docs"].AsBsonArray.First(x => x["doc_id"] == new ObjectId(id))["no"].AsInt32+item.Value;
                        if (inc <= 0)
                        {
                            var pullUpdateFilter = Builders<BsonDocument>.Update.PullFilter("docs",
                                Builders<BsonDocument>.Filter.Eq("doc_id", new ObjectId(id)));
                            await _invertedtable.UpdateOneAsync(docFilter, pullUpdateFilter);

                            var clearFilter = Builders<BsonDocument>.Filter.Eq("_id", item.Key) & Builders<BsonDocument>.Filter.SizeLte("docs", 0);
                            var clearResult = await _invertedtable.DeleteOneAsync(clearFilter);
                        }
                        else
                        {
                            var update = Builders<BsonDocument>.Update.Set("docs.$.no", inc);
                            await _invertedtable.UpdateOneAsync(docFilter, update);
                        }
                        
                        
                    }
                    //First occurence of term in document.
                    else
                    {
                        var update = Builders<BsonDocument>.Update.Push("docs", new BsonDocument
                                {
                                    { "doc_id", new ObjectId(id)},
                                    {"no", item.Value }
                                });
                        await _invertedtable.UpdateOneAsync(filter, update);
                    }
                }
                await _documentsTable.UpdateOneAsync(Builders<BsonDocument>.Filter.Eq("_id", new ObjectId(id)), Builders<BsonDocument>.Update.Inc("length", item.Value));
            }

            return true;

        }

        /// <summary>
        /// Removes the document.
        /// </summary>
        /// <param name="id">The Document id.</param>
        /// <returns></returns>
        public async Task<bool> removeDocument(string id)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", new ObjectId(id));
            var result = await _documentsTable.DeleteManyAsync(filter);

            filter = Builders<BsonDocument>.Filter.Eq("docs.doc_id", new ObjectId(id));
            var update = Builders<BsonDocument>.Update.PullFilter("docs",
                Builders<BsonDocument>.Filter.Eq("doc_id", new ObjectId(id)));
            var result2 = await _invertedtable.UpdateManyAsync(filter, update);
            var clearFilter = Builders<BsonDocument>.Filter.SizeLte("docs", 0);
            var clearResult = await _invertedtable.DeleteManyAsync(clearFilter);

            update = Builders<BsonDocument>.Update.Inc("no_docs", -1);
            await _root.UpdateOneAsync(new BsonDocument(), update);
            
            return true;
        }

        /// <summary>
        /// Changes the document path.
        /// </summary>
        /// <param name="id">The Document id.</param>
        /// <param name="newPath">The new path.</param>
        /// <returns></returns>
        public async Task<bool> changeDocumentPath(string id, string newPath)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", new ObjectId(id));
            var update = Builders<BsonDocument>.Update.Set("path", newPath);
            await _documentsTable.UpdateOneAsync(filter, update);
            return true;
        }

        /// <summary>
        /// Sets the repository.
        /// </summary>
        /// <param name="path">The path of the repository.</param>
        /// <returns></returns>
        public async Task<bool> setRepository(string path)
        {
            var update = Builders<BsonDocument>.Update.Set("repo", path);
            await _root.UpdateOneAsync(new BsonDocument(), update);
            return true;
        }

        /// <summary>
        /// Sets the last crawled time.
        /// </summary>
        /// <param name="time">The last crawled time.</param>
        /// <returns></returns>
        public async Task<bool> setLastCrawled(long time)
        {
            var update = Builders<BsonDocument>.Update.Set("crawled", (double)time);
            await _root.UpdateOneAsync(new BsonDocument(), update);
            return true;
        }

        /// <summary>
        /// Gets the last crawled time.
        /// </summary>
        /// <returns></returns>
        public async Task<long> getLastCrawled()
        {
            BsonDocument ret = await _root.Find(new BsonDocument()).FirstOrDefaultAsync();
            if (ret == null) return 0;
            return (long)ret["crawled"].AsDouble;
        }

        /// <summary>
        /// Gets the document identifier.
        /// </summary>
        /// <param name="path">The path of the document.</param>
        /// <returns>The id</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public async Task<string> getDocumentID(string path)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("path", path);
            BsonDocument ret = await _documentsTable.Find(filter).FirstOrDefaultAsync();
            if (ret == null) return null;
            return ret["_id"].AsObjectId.ToString();
        }
        public static Corpus init()
        {
            IMongoClient _client;
            IMongoDatabase _database;
            _client = new MongoClient();
            _database = _client.GetDatabase("webspy");
            var corpus = new Corpus(_client, _database);
            return corpus;
        }
    }
}
