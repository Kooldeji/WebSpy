using MongoDB.Driver;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using System.Linq.Expressions;
using MongoDB.Bson.Serialization;

namespace WebSpy
{
    /// <summary>
    /// A class that acts as an interface between the entire API and the runtime database.
    /// </summary>
    public class Corpus : ICorpus
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
        private IMongoCollection<TermDocument> _invertedTable;
        /// A collection that encapsulates the documents in the corpus and their attributes like path and length.
        /// </summary>
        private IMongoCollection<BsonDocument> _documentsTable;

        public static void main(string[] args)
        {
            Corpus corpus = Corpus.init();
            corpus.Default();
        }

        static Corpus()
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(DocumentReference)))
            {
                BsonClassMap.RegisterClassMap<DocumentReference>();
            }
        }
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
            _invertedTable = database.GetCollection<TermDocument>("InvertedTable");
            _documentsTable = database.GetCollection<BsonDocument>("DocumentsTable");

        }
        /// <summary>
        /// Asynchronously gets all the terms in from of TermDocuments.
        /// </summary>
        /// <returns>A Task of List of TermDocuments</returns>
        public async Task<List<ITermDocument>> GetTermDocuments()
        {
            List<ITermDocument> ret = new List<ITermDocument>();
            await (await _invertedTable.FindAsync(FilterDefinition<TermDocument>.Empty)).ForEachAsync(t => ret.Add(t));
            Console.WriteLine(1);
            Console.WriteLine(ret.Count());
            return ret;
        }

        /// <summary>
        /// Gets the documents.
        /// </summary>
        /// <returns></returns>
        public async Task<HashSet<string>> GetDocuments()
        {
            var result = new HashSet<String>();
            using (var cursor = await _documentsTable.FindAsync(FilterDefinition<BsonDocument>.Empty))
            {
                while (await cursor.MoveNextAsync())
                {
                    IEnumerable<BsonDocument> batch = cursor.Current;
                    foreach (BsonDocument document in batch)
                    {
                        result.Add(document["_id"].AsString);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Gets the documents that a term exists.
        /// </summary>
        /// <param name="term">The term.</param>
        /// <returns>A task of the document reference</returns>
        public async Task<HashSet<IDocumentReference>> GetDocuments(String term)
        {
            var res = await _invertedTable.Find(t => t.Term == term).Project(t => t.Docs).FirstOrDefaultAsync();
            if (res == null)
            {
                return new HashSet<IDocumentReference>();
            }
            return res;
        }

        /// <summary>
        /// Gets the documents that match a predicate.
        /// </summary>
        /// <param name="predicate">The predicate to match documents.</param>
        /// <returns></returns>
        public async Task<HashSet<string>> GetDocuments(Func<String, bool> predicate)
        {
            var result = new HashSet<string>();
            using (var cursor = await _documentsTable.FindAsync(FilterDefinition<BsonDocument>.Empty))
            {
                while (await cursor.MoveNextAsync())
                {
                    IEnumerable<BsonDocument> batch = cursor.Current;
                    foreach (BsonDocument document in batch)
                    {
                        var doc = document["_id"].AsString;
                        if (predicate(doc)) result.Add(doc);
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
        public async Task<long> GetDocumentLength(string id)
        {
            BsonDocument ret = await _documentsTable.Find(Builders<BsonDocument>.Filter.Eq("_id", id)).FirstOrDefaultAsync();
            if (ret == null) return 0;
            return ret["length"].AsInt64;
        }

        /// <summary>
        /// Gets the no documents.
        /// </summary>
        /// <returns></returns>
        public async Task<int> GetNoDocuments()
        {
            BsonDocument ret = await _root.Find(new BsonDocument()).FirstOrDefaultAsync();
            if (ret == null) return 0;
            return ret["no_docs"].AsInt32;
        }

        /// <summary>
        /// Gets the terms.
        /// </summary>
        /// <returns></returns>
        public async Task<HashSet<string>> GetTerms()
        {
            var projection = Builders<TermDocument>.Projection.Include(t => t.Term);
            var docs = new HashSet<string>();
            await (await _invertedTable.FindAsync(FilterDefinition<TermDocument>.Empty)).ForEachAsync(d => docs.Add(d.Term));
            return docs;
        }

        /// <summary>
        /// Gets the terms in a a document.
        /// </summary>
        /// <param name="id">The Document id.</param>
        /// <returns></returns>
        public async Task<HashSet<string>> GetTerms(String id)
        {           
            var docs = new HashSet<String>();
            await (await _invertedTable.FindAsync(Builders<TermDocument>.Filter.Eq("docs.doc_id", id))).ForEachAsync(t => docs.Add(t.Term));
            return docs;
        }
        /// <summary>
        /// Gets the terms that match a particular function.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        public async Task<HashSet<String>> GetTerms(Func<String, bool> predicate)
        {
            var result = new HashSet<String>();
            using (var cursor = await _invertedTable.FindAsync(FilterDefinition<TermDocument>.Empty))
            {
                while (await cursor.MoveNextAsync())
                {
                    IEnumerable<TermDocument> batch = cursor.Current;
                    foreach (var termDoc in batch)
                    {
                        if (predicate(termDoc.Term)) result.Add(termDoc.Term);
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// Gets a number of terms that match a particular function.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="no">The no of terms.</param>
        /// <returns></returns>
        public async Task<List<String>> GetTerms(Func<String, bool> predicate, int no)
        {
            var result = new List<String>();
            using (var cursor = await _invertedTable.FindAsync(FilterDefinition<TermDocument>.Empty))
            {
                while (await cursor.MoveNextAsync())
                {
                    IEnumerable<TermDocument> batch = cursor.Current;
                    foreach (var termDoc in batch)
                    {
                        if (predicate(termDoc.Term)) result.Add(termDoc.Term);
                        if (result.Count() == no) return result;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Gets the repository.
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetRepository()
        {
            BsonDocument ret = await _root.Find(FilterDefinition<BsonDocument>.Empty).FirstOrDefaultAsync();
            if (ret == null) return "";
            return ret["repo"].AsString;
        }

        /// <summary>
        /// Gets the document path.
        /// </summary>
        /// <param name="id">The Document id.</param>
        /// <returns></returns>
        public async Task<string> GetDocumentPath(String id)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", id);
            BsonDocument ret = await _documentsTable.Find(filter).Project("{ path: 1 }").FirstOrDefaultAsync();
            return ret["path"].AsString;
        }

        /// <summary>
        /// Adds the document.
        /// </summary>
        /// <param name="path">The path that the document points to.</param>
        /// <returns></returns>
        public async Task<bool> AddDocument(string path)
        {
            await _documentsTable.InsertOneAsync(new BsonDocument { { "_id", ((await GetNoDocuments()) + 1).ToString() }, { "path", path }, { "length", 0 } });
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
        public async Task<bool> AddDocument(String path, long length, List<ITermDocument> index)
        {
            await AddDocument(path);
            var id = await GetDocumentID(path);
            await UpdateDocument(id, length, index);
            return true;
        }

        /// <summary>
        /// Updates the document with the index.
        /// </summary>
        /// <param name="id">The Document id.</param>
        /// <param name="index">Dictionary of terms mapped to their nos of occurences in the doc.</param>
        /// <returns></returns>
        public async Task<bool> UpdateDocument(string id, long length, List<ITermDocument> index)
        {
            
            foreach (var termDoc in index)
            {
                //var filter = Builders<BsonDocument>.Filter.Eq("_id", item.Key);
                var termDoc2 = await _invertedTable.Find(t => t.Term==termDoc.Term).Project(t => t.Term).FirstOrDefaultAsync();

                //Check if term exists
                if (termDoc2 == null)
                //Term does not exist add term first, then add document and no document occurence
                {
                    foreach (var doc in termDoc.Docs)
                    {
                        doc.DocID = id;
                    }
                    await _invertedTable.InsertOneAsync((TermDocument) termDoc);
                }
                else {
                    //Term exists.
                    //var docFilter = Builders<BsonDocument>.Filter.Eq("_id", item.Key) & Builders<BsonDocument>.Filter.ElemMatch<BsonDocument>("docs", new BsonDocument("doc_id", new ObjectId(id)));
                    foreach (var doc in termDoc.Docs)
                    {
                        var postfix = doc.PostFix;
                        //Expression<Func<TermDocument, bool>> find = (t => t.Term == termDoc.Term && t.Docs.Any(d => d.DocID == id && d.PostFix == postfix));
                        var docFilter = Builders<TermDocument>.Filter.Eq("_id", termDoc.Term) & Builders<TermDocument>.Filter.Eq("docs.doc_id", id) & Builders<TermDocument>.Filter.Eq("docs.postfix", postfix); ;
                        termDoc2 = await _invertedTable.Find(docFilter).Project(t => t.Term).FirstOrDefaultAsync();
                        if (termDoc2 == null)
                        {
                            doc.DocID = id;
                            await _invertedTable.UpdateOneAsync(t => t.Term == termDoc.Term, Builders<TermDocument>.Update.Push(t => t.Docs, doc));
                        }
                        else
                        {
                            if (doc.Pos.Count < 1)
                            {
                                var pullUpdateFilter = Builders<TermDocument>.Update.PullFilter(t => t.Docs,
                                    d => d.DocID == id);
                                await _invertedTable.UpdateOneAsync(t => t.Term == termDoc.Term, pullUpdateFilter);
                                //var clearFilter = Builders<BsonDocument>.Filter.Eq("_id", item.Key) & Builders<BsonDocument>.Filter.SizeLte("docs", 0);
                                await _invertedTable.DeleteOneAsync(t => t.Term == termDoc.Term && t.Docs.Count() < 1);
                            }
                            else
                            {
                                await _invertedTable.UpdateOneAsync(docFilter, Builders<TermDocument>.Update.Set("docs.$.pos", doc.Pos));
                            }
                        }
                    }
                }
            }
            Console.WriteLine("l"+length);
            if (id != null)
            {
                Console.WriteLine(length);
                await _documentsTable.UpdateOneAsync(Builders<BsonDocument>.Filter.Eq("_id", id), Builders<BsonDocument>.Update.Set("length", length));
            }

            return true;

        }

        /// <summary>
        /// Removes the document.
        /// </summary>
        /// <param name="id">The Document id.</param>
        /// <returns></returns>
        public async Task<bool> RemoveDocument(string id)
        {
            //Remove document from document table
            var filter = Builders<BsonDocument>.Filter.Eq("_id", id);
            var result = await _documentsTable.DeleteOneAsync(filter);
            if (!result.IsAcknowledged) return false;

            //Pull the document from all occurences in the inverted table 
            var pullUpdateFilter = Builders<TermDocument>.Update.PullFilter("docs", Builders<BsonDocument>.Filter.Eq("doc_id", id));
            await _invertedTable.UpdateManyAsync(Builders<TermDocument>.Filter.Eq("docs.doc_id", id), pullUpdateFilter);

            //Clear terms with empty documents from database
            var clearFilter = Builders<BsonDocument>.Filter.SizeLte("docs", 0);
            await _invertedTable.DeleteManyAsync(t => t.Docs.Count() < 1);

            //Decrement the number of documents
            var update = Builders<BsonDocument>.Update.Inc("no_docs", -(int)result.DeletedCount);
            await _root.UpdateOneAsync(FilterDefinition<BsonDocument>.Empty, update);
            
            return true;
        }

        /// <summary>
        /// Changes the document path.
        /// </summary>
        /// <param name="id">The Document id.</param>
        /// <param name="newPath">The new path.</param>
        /// <returns></returns>
        public async Task<bool> ChangeDocumentPath(string id, string newPath)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", id);
            var update = Builders<BsonDocument>.Update.Set("path", newPath);
            await _documentsTable.UpdateOneAsync(filter, update);
            return true;
        }

        /// <summary>
        /// Sets the repository.
        /// </summary>
        /// <param name="path">The path of the repository.</param>
        /// <returns></returns>
        public async Task<bool> SetRepository(string path)
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
        public async Task<bool> SetLastCrawled(long time)
        {
            var update = Builders<BsonDocument>.Update.Set("crawled", time);
            await _root.UpdateOneAsync(new BsonDocument(), update);
            return true;
        }

        /// <summary>
        /// Gets the last crawled time.
        /// </summary>
        /// <returns></returns>
        public async Task<long> GetLastCrawled()
        {
            BsonDocument ret = await _root.Find(new BsonDocument()).FirstOrDefaultAsync();
            if (ret == null) return 0;
            return ret["crawled"].AsInt64;
        }

        /// <summary>
        /// Gets the document identifier.
        /// </summary>
        /// <param name="path">The path of the document.</param>
        /// <returns>The id</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public async Task<string> GetDocumentID(string path)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("path", path);
            BsonDocument ret = await _documentsTable.Find(filter).FirstOrDefaultAsync();
            if (ret == null) return null;
            return ret["_id"].ToString();
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
        public async Task Empty()
        {
            _database.DropCollection("Root");
            _database.DropCollection("DocumentsTable");
            _database.DropCollection("InvertedTable");
            var doc = new BsonDocument
            {
                {"_id", "1"},
                {"no_docs", 0},
                {"repo", "C:/Users/kooldeji/Documents/repo" },
                {"crawled", long.Parse("0") }
            };
            await _root.InsertOneAsync(doc);
        }
        public async Task Default()
        {
            _database.DropCollection("Root");
            _database.DropCollection("DocumentsTable");
            _database.DropCollection("InvertedTable");
            var doc = new BsonDocument
            {
                {"_id", "1" },
                {"path", "1.txt" },
                {"length", long.Parse("6") }
            };
            var doc1 = new BsonDocument
            {
                {"_id", "2" },
                {"path", "2.txt" },
                {"length",long.Parse("4") }
            };
            await _documentsTable.InsertOneAsync(doc);
            await _documentsTable.InsertOneAsync(doc1);
            doc = new BsonDocument
            {
                {"_id", "1"},
                {"no_docs", 2},
                {"repo", "C:/Users/kooldeji/Documents/repo" },
                {"crawled", long.Parse("0") }
            };
            await _root.InsertOneAsync(doc);
            var index = new List<KeyValuePair<string, string>>();
            var termdict = new Dictionary<string, TermDocument>();
            var docdict = new Dictionary<string, DocumentReference>();
            var id = "1";
            index.Add(new KeyValuePair<string, string>("life", "life"));
            index.Add(new KeyValuePair<string, string>("is", "is"));
            index.Add(new KeyValuePair<string, string>("learn", "learning"));
            index.Add(new KeyValuePair<string, string>("with", "with"));
            index.Add(new KeyValuePair<string, string>("learn", "learners"));
            var c = 0;
            foreach (var item in index)
            {
                c += 1;
                if (item.Key.Count() <= 2) continue;
                TermDocument term;
                if (termdict.Keys.Contains(item.Key)) term = termdict[item.Key];
                else
                {
                    term = new TermDocument(item.Key);
                    termdict.Add(item.Key, term);
                }
                DocumentReference docref;
                if (docdict.Keys.Contains(item.Key + item.Value))
                {
                    docref = docdict[item.Key + item.Value];
                }
                else
                {
                    docref = new DocumentReference(id, item.Value);
                    term.addDoc(docref);
                }
                docref.addPos(c);
            }
            id = "2";
            docdict = new Dictionary<string, DocumentReference>();
            index.Clear();
            index.Add(new KeyValuePair<string, string>("learn", "learning"));
            index.Add(new KeyValuePair<string, string>("is", "is"));
            index.Add(new KeyValuePair<string, string>("very", "very"));
            index.Add(new KeyValuePair<string, string>("good", "good"));
            index.Add(new KeyValuePair<string, string>("infact", "infact"));
            index.Add(new KeyValuePair<string, string>("learn", "learning"));
            index.Add(new KeyValuePair<string, string>("is", "is"));
            index.Add(new KeyValuePair<string, string>("awesome", "awesome"));
            foreach (var item in index)
            {
                c += 1;
                if (item.Key.Count() <= 2) continue;
                TermDocument term;
                if (termdict.Keys.Contains(item.Key)) term = termdict[item.Key];
                else
                {
                    term = new TermDocument(item.Key);
                    termdict.Add(item.Key, term);
                }
                DocumentReference docref;
                if (docdict.Keys.Contains(item.Key + item.Value))
                {
                    docref = docdict[item.Key + item.Value];
                }
                else
                {
                    docref = new DocumentReference(id, item.Value);
                    docdict.Add(item.Key + item.Value, docref);
                    term.addDoc(docref);
                }
                docref.addPos(c);
            }
            await _invertedTable.InsertManyAsync(termdict.Values);

        }
    }

    public class TermDocument : ITermDocument
    {
        private HashSet<IDocumentReference> _docs;
        private string _term;

        [BsonElement("docs")]
        public HashSet<IDocumentReference> Docs
        {
            get
            {
                return new HashSet<IDocumentReference>(_docs);
            }
            set
            {
                _docs = new HashSet<IDocumentReference>(value);
            }
        }

        [BsonId]
        public string Term
        {
            get
            {
                return _term;
            }
            private set
            {
                _term = value.ToLower();
            }
        }

        public TermDocument(String term, HashSet<IDocumentReference> docs)
        {
            this.Term = term;
            this.Docs = docs;

        }
        public TermDocument(String term)
            :this(term, new HashSet<IDocumentReference>())
        {
        }

        public void removeDoc(IDocumentReference doc)
        {
            _docs.Remove(doc);
        }

        public void addDoc(IDocumentReference doc)
        {
            _docs.Add(doc);
        }
    }

    public class DocumentReference : IDocumentReference
    {
        private HashSet<int> _pos;
        private string _postfix;

        [BsonElement("doc_id")]
        public string DocID
        {
            get; set;
        }

        [BsonElement("pos")]
        public HashSet<int> Pos
        {
            get
            {
                return new HashSet<int>(_pos);
            }
            set
            {
                _pos = new HashSet<int>(value);
            }
        }
        [BsonElement("postfix")]
        public string PostFix
        {
            get
            {
                return _postfix;
            }
            private set
            {
                _postfix = value.ToLower();
            }
        }

        public void addPos(int pos)
        {
            _pos.Add(pos);
        }

        public void removePos(int pos)
        {
            _pos.Add(pos);
        }

        public DocumentReference(string docId, string postfix, HashSet<int> pos)
        {
            DocID = docId;
            PostFix = postfix;
            Pos = pos;
        }
        public DocumentReference(string doc_id, string postfix)
            :this(doc_id, postfix, new HashSet<int>())
        {

        }
    }
}
