using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSpy
{
    class Corpus
    {
        internal static Dictionary<String, Dictionary<String, int>> GetTermFrequencies()
        {
            var a = new Dictionary<String, Dictionary<String, int>>();
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
            a["life"]["4"] = 2;
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
            a["learning"]["4"] = 2;
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
            return a;
        }

        internal static HashSet<String> getDocuments()
        {
            return new HashSet<String>(new[] { "1", "2", "3", "4" });
        }

        internal static Dictionary<string, int> getDocuments(String term)
        {
            return GetTermFrequencies()[term];
        }

        internal static int getDocumentLength(string key)
        {
            switch (key){
                case "1":
                    return 10;
                case "2":
                    return 7;
                case "3":
                    return 3;
                case "4":
                    return 2;
                default:
                    return 0;

            }
        }

        internal static int getNoDocuments()
        {
            return 3;
        }
    }
}
