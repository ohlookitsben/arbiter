using Arbiter.Core;
using Arbiter.Core.Commands;
using Moq;
using NUnit.Framework;
using System;

namespace Aribter.Core.Tests
{
    [TestFixture]
    public class CommandBuilderTests
    {
        private CommandBuilder _builder;
        private Mock<IFileReader> _fileReader;

        [SetUp]
        public void Setup()
        {
            _fileReader = new Mock<IFileReader>();
            _builder = new CommandBuilder(new RepositoryReader(new PowerShellInvoker()), _fileReader.Object);
        }

        [Test]
        public void ProcessArguments_ArgumentLengthIsTooSmall_ReturnsUsage([Range(0, 3)] int argumentLength)
        {
            var args = new string[argumentLength];

            var command = _builder.ProcessArguments(args);

            Assert.IsInstanceOf<UsageCommand>(command);
        }

        [Test]
        public void ProcessArguments_ArgumentLengthIsTooLarge_ReturnsUsage([Range(5, 8)] int argumentLength)
        {
            var args = new string[argumentLength];

            var command = _builder.ProcessArguments(args);

            Assert.IsInstanceOf<UsageCommand>(command);
        }

        [Test]
        public void ProcessArguments_SolutionNotFound_ReturnsNotFound()
        {
            var args = new string[4];
            _fileReader.Setup(f => f.Exists(It.IsAny<string>())).Returns(false);

            var command = _builder.ProcessArguments(args);

            Assert.IsInstanceOf<SolutionNotFoundCommand>(command);
        }
    }
}
