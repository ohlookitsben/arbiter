using Arbiter.Core;
using Arbiter.Core.Commands;
using Autofac;
using Moq;
using NUnit.Framework;

namespace Arbiter.Tests.Unit
{
    [TestFixture]
    [Category(TestCategory.Unit)]
    public class CommandBuilderTests
    {
        private CommandBuilder _builder;
        private Mock<IFileSystem> _fileReader;
        private Mock<IRepositoryReader> _repositoryReader;

        [SetUp]
        public void Setup()
        {
            _fileReader = new Mock<IFileSystem>();
            _repositoryReader = new Mock<IRepositoryReader>();
            var container = ContainerHelper.ConfigureContainer();
            var scope = container.BeginLifetimeScope(c =>
            {
                c.RegisterInstance(_fileReader.Object);
                c.RegisterInstance(_repositoryReader.Object);
            });

            _builder = scope.Resolve<CommandBuilder>();
        }

        [Test]
        public void ProcessArguments_ArgumentLengthIsTooSmall_ReturnsUsage([Range(0, 3)] int argumentLength)
        {
            string[] args = new string[argumentLength];

            var command = _builder.ProcessArguments(args);

            Assert.IsInstanceOf<UsageCommand>(command);
        }

        [Test]
        public void ProcessArguments_ArgumentLengthIsTooLarge_ReturnsUsage([Range(5, 8)] int argumentLength)
        {
            string[] args = new string[argumentLength];

            var command = _builder.ProcessArguments(args);

            Assert.IsInstanceOf<UsageCommand>(command);
        }

        [Test]
        public void ProcessArguments_SolutionNotFound_ReturnsNotFound()
        {
            string[] args = new string[4];
            _repositoryReader.Setup(r => r.GitExists()).Returns(true);

            var command = _builder.ProcessArguments(args);

            AssertMessageContains(command, "solution");
        }

        [Test]
        public void ProcessArguments_FromCommitNotFound_ReturnsNotFound()
        {
            string[] args = new string[] { "", "commit_id", "", "" };
            _fileReader.Setup(f => f.Exists(It.IsAny<string>())).Returns(true);
            _repositoryReader.Setup(r => r.GitExists()).Returns(true);
            _repositoryReader.Setup(r => r.RepositoryExists()).Returns(true);

            var command = _builder.ProcessArguments(args);

            AssertMessageContains(command, "commit_id");
        }

        [Test]
        public void ProcessArguments_ToCommitNotFound_ReturnsNotFound()
        {
            string[] args = new string[] { "", "", "commit_id", "" };
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
            string[] args = new string[] { "", "", "commit_id", "" };
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
            string[] args = new string[4];
            _fileReader.Setup(f => f.Exists(It.IsAny<string>())).Returns(true);

            var command = _builder.ProcessArguments(args);

            AssertMessageContains(command, "executable");
        }

        [Test]
        public void ProcessArguments_GitRepositoryNotFound_ReturnsNotFound()
        {
            string[] args = new string[4];
            _fileReader.Setup(f => f.Exists(It.IsAny<string>())).Returns(true);
            _repositoryReader.Setup(r => r.GitExists()).Returns(true);

            var command = _builder.ProcessArguments(args);

            AssertMessageContains(command, "repository");
        }

        private static void AssertMessageContains(ICommand command, string contains)
        {
            Assert.IsInstanceOf<PrintMessageCommand>(command);
            var printMessageCommand = (PrintMessageCommand)command;
            Assert.IsTrue(printMessageCommand.Message.Contains(contains), $"The message: '{printMessageCommand.Message}' should contain: '{contains}'");
        }
    }
}
