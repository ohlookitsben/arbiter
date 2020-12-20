using Arbiter.Core;
using Arbiter.Tests.Fakes;
using NUnit.Framework;
using static Arbiter.Tests.Fakes.FakePowerShellInvoker;

namespace Arbiter.Tests.Unit
{
    [TestFixture]
    [Category(TestCategory.Unit)]
    public class RepositoryReaderTests
    {
        private FakePowerShellInvoker _invoker;
        private GitRepositoryReader _reader;

        [SetUp]
        public void SetUp()
        {
            _invoker = new FakePowerShellInvoker();
            _reader = new GitRepositoryReader(_invoker);
        }

        [Test]
        public void GitExists_GitFound_ReturnsTrue()
        {
            _invoker.InvokeResult = Responses.GitFound;

            bool result = _reader.ToolExists();

            Assert.IsTrue(result);
        }

        [Test]
        public void GitExists_GitNotFound_ReturnsFalse()
        {
            _invoker.InvokeResult = Responses.GitNotFound;

            bool result = _reader.ToolExists();

            Assert.IsFalse(result);
        }

        [Test]
        public void GitExists_ResultIsEmpty_ReturnsFalse()
        {
            _invoker.ResultIsEmpty = true;

            bool result = _reader.ToolExists();

            Assert.IsFalse(result);
        }

        [Test]
        public void CommitExists_CommitFound_ReturnsTrue()
        {
            _invoker.InvokeResult = Responses.CommitFound;

            bool result = _reader.CommitExists(Responses.ExpectedCommit);

            Assert.IsTrue(result);
        }

        [Test]
        public void CommitExists_CommitNotFound_ReturnsFalse()
        {
            _invoker.InvokeResult = Responses.CommitNotFound;

            bool result = _reader.CommitExists(Responses.ExpectedCommit);

            Assert.IsFalse(result);
        }

        [Test]
        public void CommitExists_ResultIsEmpty_ReturnsFalse()
        {
            _invoker.ResultIsEmpty = true;

            bool result = _reader.CommitExists(Responses.ExpectedCommit);

            Assert.IsFalse(result);
        }

        [Test]
        public void ListChangedFiles_ChangedFiles_ListsFiles()
        {
            _invoker.InvokeResult = Responses.ChangedFiles;

            var files = _reader.ListChangedFiles(Responses.ExpectedCommit, Responses.ExpectedToCommit);

            CollectionAssert.AreEqual(Responses.ExpectedChangedFiles, files);
        }

        [Test]
        public void ListChangedFiles_ChangedFilesHaveDuplicates_ListsFilesWithoutDuplicates()
        {
            _invoker.InvokeResult = Responses.ChangedFilesWithDuplicates;

            var files = _reader.ListChangedFiles(Responses.ExpectedCommit, Responses.ExpectedToCommit);

            CollectionAssert.AreEqual(Responses.ExpectedChangedFiles, files);
        }

        [Test]
        public void ListChangedFiles_NoChangedFiles_EmptyList()
        {
            _invoker.InvokeResult = Responses.NoChangedFiles;

            var files = _reader.ListChangedFiles(Responses.ExpectedCommit, Responses.ExpectedToCommit);

            CollectionAssert.IsEmpty(files);
        }

        [Test]
        public void RepositoryExists_RepositoryFound_ReturnsTrue()
        {
            _invoker.InvokeResult = Responses.RepositoryFound;

            bool result = _reader.RepositoryExists();

            Assert.IsTrue(result);
        }

        [Test]
        public void RepositoryExists_RepositoryNotFound_ReturnsFalse()
        {
            _invoker.InvokeResult = Responses.RepositoryNotFound;

            bool result = _reader.RepositoryExists();

            Assert.IsFalse(result);
        }

        [Test]
        public void CommitIsAncestor_IsAncestor_ReturnsTrue()
        {
            _invoker.InvokeResult = Responses.CommitIsAncestor;

            bool result = _reader.CommitIsAncestor("fe972a89a56006182f8836fe1dc338b39d889792a", "969e26db332760f755d2429ce65e04061b61e207");

            Assert.IsTrue(result);
        }

        [Test]
        public void CommitIsAncestor_NotAncestor_ReturnsFalse()
        {
            _invoker.InvokeResult = Responses.CommitNotAncestor;

            bool result = _reader.CommitIsAncestor("fe972a89a56006182f8836fe1dc338b39d889792a", "969e26db332760f755d2429ce65e04061b61e207");

            Assert.IsFalse(result);
        }
    }
}
