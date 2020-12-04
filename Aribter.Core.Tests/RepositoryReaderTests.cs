using Arbiter.Core;
using Aribter.Core.Tests.Fakes;
using NUnit.Framework;
using static Aribter.Core.Tests.Fakes.FakePowerShellInvoker;

namespace Aribter.Core.Tests
{
    [TestFixture]
    public class RepositoryReaderTests
    {
        private FakePowerShellInvoker _invoker;
        private RepositoryReader _reader;

        [SetUp]
        public void SetUp()
        {
            _invoker = new FakePowerShellInvoker();
            _reader = new RepositoryReader(_invoker);
        }

        [Test]
        public void GitExists_GitFound_ReturnsTrue()
        {
            _invoker.InvokeResult = Responses.GitFound;

            bool result = _reader.GitExists();

            Assert.IsTrue(result);
        }

        [Test]
        public void GitExists_GitNotFound_ReturnsFalse()
        {
            _invoker.InvokeResult = Responses.GitNotFound;

            bool result = _reader.GitExists();

            Assert.IsFalse(result);
        }

        [Test]
        public void GitExists_ResultIsEmpty_ReturnsFalse()
        {
            _invoker.ResultIsEmpty = true;

            bool result = _reader.GitExists();

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
        public void ListChangedFilesNoChangedFiles_EmptyList()
        {
            _invoker.InvokeResult = Responses.NoChangedFiles;

            var files = _reader.ListChangedFiles(Responses.ExpectedCommit, Responses.ExpectedToCommit);

            CollectionAssert.IsEmpty(files);
        }
    }
}
