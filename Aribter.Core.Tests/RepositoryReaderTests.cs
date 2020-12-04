using Arbiter.Core;
using Aribter.Core.Tests.Fakes;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public void ListChangedFiles()
        {
            var reader = new RepositoryReader(new PowerShellInvoker());
            var files = reader.ListChangedFiles("e15dbcda142dff9a89f2d854a79096e6926bcc35", "e15dbcda142dff9a89f2d854a79096e6926bcc35");
        }
    }
}
