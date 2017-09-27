using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebSpy
{
    public interface ICorpus
    {
        /// <summary>
        /// Asynchronously gets all the terms in from of TermDocuments.
        /// </summary>
        /// <returns>A Task of List of TermDocuments</returns>
        Task<List<ITermDocument>> GetTermDocuments();

        /// <summary>
        /// Gets the documents.
        /// </summary>
        /// <returns></returns>
        Task<HashSet<string>> GetDocuments();

        /// <summary>
        /// Gets the documents that a term exists.
        /// </summary>
        /// <param name="term">The term.</param>
        /// <returns>A task of the document reference</returns>
        Task<HashSet<IDocumentReference>> GetDocuments(String term);

        /// <summary>
        /// Gets the documents that match a predicate.
        /// </summary>
        /// <param name="predicate">The predicate to match documents.</param>
        /// <returns></returns>
        Task<HashSet<string>> GetDocuments(Func<String, bool> predicate);

        /// <summary>
        /// Gets the length of the document.
        /// </summary>
        /// <param name="id">The Document id.</param>
        /// <returns></returns>
        Task<long> GetDocumentLength(string id);

        /// <summary>
        /// Gets the no documents.
        /// </summary>
        /// <returns></returns>
        Task<int> GetNoDocuments();
        /// <summary>
        /// Gets the terms.
        /// </summary>
        /// <returns></returns>
        Task<HashSet<string>> GetTerms();

        /// <summary>
        /// Gets the terms in a a document.
        /// </summary>
        /// <param name="id">The Document id.</param>
        /// <returns></returns>
        Task<HashSet<string>> GetTerms(String id);
        /// <summary>
        /// Gets the terms that match a particular function.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        Task<HashSet<String>> GetTerms(Func<String, bool> predicate);
        /// <summary>
        /// Gets a number of terms that match a particular function.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="no">The no of terms.</param>
        /// <returns></returns>
        Task<List<String>> GetTerms(Func<String, bool> predicate, int no);

        /// <summary>
        /// Gets the repository.
        /// </summary>
        /// <returns></returns>
        Task<string> GetRepository();

        /// <summary>
        /// Gets the document path.
        /// </summary>
        /// <param name="id">The Document id.</param>
        /// <returns></returns>
        Task<string> GetDocumentPath(String id);

        /// <summary>
        /// Adds the document.
        /// </summary>
        /// <param name="path">The path that the document points to.</param>
        /// <returns></returns>
        Task<bool> AddDocument(string path);

        /// <summary>
        /// Adds the document to the Corpus.
        /// </summary>
        /// <param name="path">The path that the document points to.</param>
        /// <param name="index">Dictionary of terms mapped to their nos of occurences in the doc.</param>
        /// <returns></returns>
        Task<bool> AddDocument(String path, long length, List<ITermDocument> index);

        /// <summary>
        /// Updates the document with the index.
        /// </summary>
        /// <param name="id">The Document id.</param>
        /// <param name="index">Dictionary of terms mapped to their nos of occurences in the doc.</param>
        /// <returns></returns>
        Task<bool> UpdateDocument(string id, long length, List<ITermDocument> index);

        /// <summary>
        /// Removes the document.
        /// </summary>
        /// <param name="id">The Document id.</param>
        /// <returns></returns>
        Task<bool> RemoveDocument(string id);

        /// <summary>
        /// Changes the document path.
        /// </summary>
        /// <param name="id">The Document id.</param>
        /// <param name="newPath">The new path.</param>
        /// <returns></returns>
        Task<bool> ChangeDocumentPath(string id, string newPath);

        /// <summary>
        /// Sets the repository.
        /// </summary>
        /// <param name="path">The path of the repository.</param>
        /// <returns></returns>
        Task<bool> SetRepository(string path);

        /// <summary>
        /// Sets the last crawled time.
        /// </summary>
        /// <param name="time">The last crawled time.</param>
        /// <returns></returns>
        Task<bool> SetLastCrawled(long time);

        /// <summary>
        /// Gets the last crawled time.
        /// </summary>
        /// <returns></returns>
        Task<long> GetLastCrawled();

        /// <summary>
        /// Gets the document identifier.
        /// </summary>
        /// <param name="path">The path of the document.</param>
        /// <returns>The id</returns>
        /// <exception cref="System.NotImplementedException"></exception>

        Task<string> GetDocumentID(string path);


    }

    public interface ITermDocument
    {
        string Term { get;}
        HashSet<IDocumentReference> Docs { get; set; }
        void removeDoc(IDocumentReference doc);
        void addDoc(IDocumentReference doc);

    }
    public interface IDocumentReference
    {
        string DocID { get; set; }
        string PostFix { get;}
        HashSet<int> Pos { get; }
        void removePos(int pos);
        void addPos(int pos); 
    }
}