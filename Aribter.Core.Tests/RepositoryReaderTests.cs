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
        public void GitExists_GitIsFound_ReturnsTrue()
        {
            _invoker.SetResponse(Responses.GitFound);

            bool result = _reader.GitExists();

            Assert.IsTrue(result);
        }

        [Test]
        public void GitExists_GitNotFound_ReturnsFalse()
        {
            _invoker.SetResponse(Responses.GitNotFound);

            bool result = _reader.GitExists();

            Assert.IsFalse(result);
        }
    }
}
