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
        private Mock<IRepositoryReader> _repositoryReader;

        [SetUp]
        public void Setup()
        {
            _fileReader = new Mock<IFileReader>();
            _repositoryReader = new Mock<IRepositoryReader>();
            _builder = new CommandBuilder(_repositoryReader.Object, _fileReader.Object);
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
            _repositoryReader.Setup(r => r.GitExists()).Returns(true);

            var command = _builder.ProcessArguments(args);

            AssertMessageContains(command, "solution");
        }

        [Test]
        public void ProcessArguments_FromCommitNotFound_ReturnsNotFound()
        {
            var args = new string[] { "", "commit_id", "", "" };
            _fileReader.Setup(f => f.Exists(It.IsAny<string>())).Returns(true);
            _repositoryReader.Setup(r => r.GitExists()).Returns(true);
            _repositoryReader.Setup(r => r.RepositoryExists()).Returns(true);

            var command = _builder.ProcessArguments(args);

            AssertMessageContains(command, "commit_id");
        }

        [Test]
        public void ProcessArguments_ToCommitNotFound_ReturnsNotFound()
        {
            var args = new string[] { "", "", "commit_id", "" };
            _fileReader.Setup(f => f.Exists(It.IsAny<string>())).Returns(true);
            _repositoryReader.Setup(r => r.GitExists()).Returns(true);
            _repositoryReader.Setup(r => r.RepositoryExists()).Returns(true);
            _repositoryReader.Setup(r => r.CommitExists("")).Returns(true);

            var command = _builder.ProcessArguments(args);

            AssertMessageContains(command, "commit_id");
        }

        [Test]
        public void ProcessArguments_ToCommitNotAncestor_ReturnsNotFound()
        {
            var args = new string[] { "", "", "commit_id", "" };
            _fileReader.Setup(f => f.Exists(It.IsAny<string>())).Returns(true);
            _repositoryReader.Setup(r => r.GitExists()).Returns(true);
            _repositoryReader.Setup(r => r.RepositoryExists()).Returns(true);
            _repositoryReader.Setup(r => r.CommitExists(It.IsAny<string>())).Returns(true);

            var command = _builder.ProcessArguments(args);

            AssertMessageContains(command, "ancestor");
        }

        [Test]
        public void ProcessArguments_GitNotFound_ReturnsNotFound()
        {
            var args = new string[4];
            _fileReader.Setup(f => f.Exists(It.IsAny<string>())).Returns(true);

            var command = _builder.ProcessArguments(args);

            AssertMessageContains(command, "executable");
        }

        [Test]
        public void ProcessArguments_GitRepositoryNotFound_ReturnsNotFound()
        {
            var args = new string[4];
            _fileReader.Setup(f => f.Exists(It.IsAny<string>())).Returns(true);
            _repositoryReader.Setup(r => r.GitExists()).Returns(true);

            var command = _builder.ProcessArguments(args);

            AssertMessageContains(command, "repository");
        }

        private void AssertMessageContains(ICommand command, string contains)
        {
            Assert.IsInstanceOf<PrintMessageCommand>(command);
            var printMessageCommand = (PrintMessageCommand)command;
            Assert.IsTrue(printMessageCommand.Message.Contains(contains), $"The message: '{printMessageCommand.Message}' should contain: '{contains}'");
        }
    }
}
