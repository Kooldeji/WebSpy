// <copyright file="IndexerTest.cs">Copyright ©  2017</copyright>
using System;
using Microsoft.Pex.Framework;
using Microsoft.Pex.Framework.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebSpy;

namespace WebSpy.Tests
{
    /// <summary>This class contains parameterized unit tests for Indexer</summary>
    [PexClass(typeof(Indexer))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(InvalidOperationException))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(ArgumentException), AcceptExceptionSubtypes = true)]
    [TestClass]
    public partial class IndexerTest
    {
        /// <summary>Test stub for GenerateString(String)</summary>
        [PexMethod]
        public void GenerateStringTest([PexAssumeUnderTest]Indexer target, string path)
        {
            target.GenerateString(path);
            // TODO: add assertions to method IndexerTest.GenerateStringTest(Indexer, String)
        }
    }
}
