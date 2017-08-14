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
    public class Corpus
    {
        private IMongoClient _client;
        private IMongoDatabase _database;
        private IMongoCollection<BsonDocument> _root;
        private IMongoCollection<BsonDocument> _invertedtable;
        private IMongoCollection<BsonDocument> _documentsTable;
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

        public async Task<int> getDocumentLength(string id)
        {

            BsonDocument ret = await _documentsTable.Find(Builders<BsonDocument>.Filter.Eq("_id", new ObjectId(id))).FirstOrDefaultAsync();
            if (ret == null) return 0;
            return ret["length"].AsInt32;
        }

        public async Task<int> getNoDocuments()
        {
            BsonDocument ret = await _root.Find(new BsonDocument()).FirstOrDefaultAsync();
            if (ret == null) return 0;
            return (int)ret["no_docs"].AsDouble;
        }

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

        public async Task<string> getRepository()
        {
            BsonDocument ret = await _root.Find(new BsonDocument()).FirstOrDefaultAsync();
            if (ret == null) return "";
            return ret["repo"].AsString;
        }

        public async Task<string> getDocumentPath(String id)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", new ObjectId(id));
            BsonDocument ret = await _documentsTable.Find(filter).FirstOrDefaultAsync();
            return ret["path"].AsString;
        }

        public async Task<bool> addDocument(string path)
        {
            await _documentsTable.InsertOneAsync(new BsonDocument { { "path", path }, { "length", 0 } });
            var update = Builders<BsonDocument>.Update.Inc("no_docs", 1);
            await _root.UpdateOneAsync(new BsonDocument(), update);
            return true;
        }

        public async Task<bool> addDocument(String path, Dictionary<string, int> index)
        {
            await addDocument(path);
            var id = getDocumentID(path).Result;
            await updateDocument(id, index);
            return true;
        }

        public async Task<bool> updateDocument(string id, Dictionary<string, int> index)
        {
            foreach (var item in index)
            {
                var filter = Builders<BsonDocument>.Filter.Eq("_id", item.Key);
                BsonDocument doc = await _invertedtable.Find(filter).FirstOrDefaultAsync();

                //Term does not exist add term first, then add document occurence
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

        public async Task<bool> changeDocumentPath(string id, string newPath)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", new ObjectId(id));
            var update = Builders<BsonDocument>.Update.Set("path", newPath);
            await _documentsTable.UpdateOneAsync(filter, update);
            return true;
        }

        public async Task<bool> setRepo(string path)
        {
            var update = Builders<BsonDocument>.Update.Set("repo", path);
            await _root.UpdateOneAsync(new BsonDocument(), update);
            return true;
        }

        public async Task<bool> setLastCrawled(long time)
        {
            var update = Builders<BsonDocument>.Update.Set("crawled", (double)time);
            await _root.UpdateOneAsync(new BsonDocument(), update);
            return true;
        }

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

        /// <summary>
        /// Updates the documents in the corpus and deletes the documents that don't exist.
        /// </summary>
        public void update()
        {

        }
        public static async void test()
        {
            IMongoClient _client;
            IMongoDatabase _database;
            _client = new MongoClient();
            _database = _client.GetDatabase("webspy");
            var corpus = new Corpus(_client, _database);

            Console.WriteLine("\n getTermFrequencies()");
            var res = await corpus.GetTermFrequencies();
            foreach (var i in res)
            {
                Console.WriteLine(i.Key);
            }


            Console.WriteLine("\n getDocuments()");
            var res1 = await corpus.getDocuments();
            foreach (var i in res1)
            {
                Console.WriteLine(i);
            }


            Console.WriteLine("\n getDocuments(learning)");
            var res2 = await corpus.getDocuments("learning");
            foreach (var i in res2)
            {
                Console.WriteLine(i.Key + " " + i.Value);
            }


            Console.WriteLine("\n getNoDocuments()");
            var res3 = await corpus.getNoDocuments();
            Console.WriteLine(res3);

            Console.WriteLine("\n getTerms()");
            var res4 = await corpus.getTerms();
            foreach (var i in res4)
            {
                Console.WriteLine(i);
            }

            Console.WriteLine("\n getTerms(598e28b96f42650033a71cba)");
            var res5 = await corpus.getTerms("598e28b96f42650033a71cba");
            foreach (var i in res5)
            {
                Console.WriteLine(i);
            }

            Console.WriteLine("\n addDocument(5.txt)");
            var res8 = await corpus.addDocument("5.txt");
            Console.WriteLine(res8);

            var a = new Dictionary<string, int>();
            a["stop"] = 1;
            a["learning"] = 2;
            Console.WriteLine("\n addDocument(4.txt, {stop: 1, learning: 2})");
            var res9 = await corpus.addDocument("4.txt", a);
            Console.WriteLine(res9);

            Console.WriteLine("\nCHECK THAT THE DOCUMENTS HAVE BEEN ADDED!!!!!");
            Console.ReadKey();

            Console.WriteLine("\n getDocumentID(4.txt)");
            var res10 = await corpus.getDocumentID("4.txt");
            Console.WriteLine(res10);

            Console.WriteLine("\n getDocumentID(5.txt)");
            var res11 = await corpus.getDocumentID("5.txt");
            Console.WriteLine(res11);

            Console.WriteLine("\n removeDocument(id of 4.txt)");
            var res12 = await corpus.removeDocument(res10);
            Console.WriteLine(res12);

            Console.WriteLine("\n removeDocument(id of 5.txt)");
            var res13 = await corpus.removeDocument(res11);
            Console.WriteLine(res13);

            Console.WriteLine("\n changeDocumentPath(598e28b96f42650033a71cba, 5.txt)");
            var res14 = await corpus.changeDocumentPath("598e28b96f42650033a71cba", "5.txt");
            Console.WriteLine(res14);

            Console.WriteLine("\n getDocumentPath(598e28b96f42650033a71cba)");
            var res7 = await corpus.getDocumentPath("598e28b96f42650033a71cba");
            Console.WriteLine(res7);

            Console.WriteLine("\nCHECK THAT THE PATHS HAVE BEEN CHANGED!!!!!");
            Console.ReadKey();

            Console.WriteLine("\n changeDocumentPath(598e28b96f42650033a71cba, 1.txt)");
            await corpus.changeDocumentPath("598e28b96f42650033a71cba", "1.txt");

            Console.WriteLine("\n setRepo(new repo)");
            var res15 = await corpus.setRepo("new repo");
            Console.WriteLine(res15);

            Console.WriteLine("\n getRepository()");
            var res6 = await corpus.getRepository();
            Console.WriteLine(res6);

            Console.WriteLine("\n getDocumentPath(598e28b96f42650033a71cba)");
            await corpus.setRepo("C:/ Users / kooldeji / Documents / repo");

            Console.WriteLine("\n setLastCrawled(100)");
            var res16 = await corpus.setLastCrawled(100);
            Console.WriteLine(res16);

            Console.WriteLine("\n getLastCrawled()");
            var res17 = await corpus.getLastCrawled();
            Console.WriteLine(res17);

            Console.WriteLine("\n setLastCrawled(0)");
            await corpus.setLastCrawled(0);

            Console.WriteLine("\n updateDocument(5990c62e87621f3034627037, {stop: -5}");
            a = new Dictionary<string, int>();
            a["stop"] = +5;
            await corpus.updateDocument("5990c62e87621f3034627037", a);

            Console.WriteLine("\nCHECK THAT THE DOCUMENTS HAVE BEEN UPDATED!!!!!");
            Console.ReadKey();
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
    public class Term
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string Id { get; set; }
        public DocumentOcur[] Docs { get; set; }
    }

    public class DocumentOcur
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string Doc_ID { get; set; }
        public int nos { get; set; }
    }

}
