using Arbiter.Core;
using Arbiter.Core.Analysis;
using Arbiter.Tests.Fakes;
using Autofac;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Xml;

namespace Arbiter.Tests.Unit
{
    [TestFixture]
    [Category(TestCategory.Unit)]
    public class BuildOutputCommandTests
    {
        private MockFileSystem _fileSystem;
        private Mock<IRepositoryReader> _repositoryReader;
        private Mock<IMSBuildSolutionAnalyzer> _analyzer;
        private string[] _args;
        private ArbiterRootCommand _command;

        [SetUp]
        public void SetUp()
        {
            _fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"C:\build\MySolution.sln", new MockFileData(string.Empty) },
                { @"C:\build\all.nunit", new MockFileData(@"
<NUnitProject>
    <Settings activeConfig=""Default"" />
    <Config name=""Default"">
        <assembly path=""SomeOtherProject.Tests\bin\Debug\SomeOtherProject.Tests.dll"" />
        <assembly path=""SomeProject.Tests\bin\Debug\SomeProject.Tests.dll"" />
    </Config>
</NUnitProject>
") }
            });

            _repositoryReader = new Mock<IRepositoryReader>();
            _analyzer = new Mock<IMSBuildSolutionAnalyzer>();
            var container = ContainerHelper.ConfigureContainer();
            var scope = container.BeginLifetimeScope(c =>
            {
                c.RegisterInstance<IFileSystem>(_fileSystem);
                c.RegisterInstance(_repositoryReader.Object);
                c.RegisterInstance(_analyzer.Object);
            });

            _args = new string[] 
            { 
                "--solution", @"C:\build\MySolution.sln", 
                "--from-commit", "commit", 
                "--to-commit", "other_commit", 
                "--nunit-project", @"C:\build\all.nunit", 
                "--nunit-configuration", "Default" 
            };
            _command = scope.Resolve<ArbiterRootCommand>();
        }

        private void SetupValidRepositoryReader()
        {
            _repositoryReader.Setup(r => r.ToolExists()).Returns(true);
            _repositoryReader.Setup(r => r.RepositoryExists()).Returns(true);
            _repositoryReader.Setup(r => r.CommitExists(It.IsAny<string>())).Returns(true);
            _repositoryReader.Setup(r => r.CommitIsAncestor(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        }

        [Test]
        public void Parser_WithValidArgs_ReturnsNoParseErrors()
        {
            var parser = new CommandLineBuilder(_command).UseDefaults().Build();
            var parseResult = parser.Parse(_args);

            // Testing the parser directly saves debugging parse errors in the larger test.
            Assert.IsEmpty(parseResult.Errors);
        }

        [Test]
        public void Execute_DependentTestsFound_OutputContainsTests()
        {
            SetupValidRepositoryReader();

            var changedFiles = new List<string>
            {
                @"C:\build\SomeProject\File.cs",
            };
            _repositoryReader.Setup(r => r.ListChangedFiles(It.IsAny<string>(), It.IsAny<string>())).Returns(changedFiles);
            var containingProjects = new List<string>()
            {
                @"C:\build\SomeProject\SomeProject.csproj",
            };
            _analyzer.Setup(a => a.FindContainingProjects(It.IsAny<IEnumerable<string>>())).Returns(containingProjects);
            var dependentProjects = new List<AnalysisResult>
            {
                new AnalysisResult("SomeProject", @"C:\build\SomeProject\SomeProject.csproj", 0, "SomeProject.dll"),
                new AnalysisResult("SomeProject.Tests", @"C:\build\SomeProject.Tests\SomeProject.Tests.csproj", 1, "SomeProject.Tests.dll")
            };
            _analyzer.Setup(a => a.FindDependentProjects(It.IsAny<IEnumerable<string>>())).Returns(dependentProjects);
            var dependentTestProjects = new List<AnalysisResult>
            {
                new AnalysisResult("SomeProject.Tests", @"C:\build\SomeProject.Tests\SomeProject.Tests.csproj", 1, "SomeProject.Tests.dll")
            };
            _analyzer.Setup(a => a.ExcludeNonTestProjects(It.IsAny<IEnumerable<AnalysisResult>>())).Returns(dependentTestProjects);

            var console = new FakeConsole();
            int result = _command.InvokeAsync(_args, console).Result;

            Assert.AreEqual(0, result, "Command should execute successfully. Error stream contents:\n{0}", console.ErrorString);

            var document = new XmlDocument();
            string fileContents = _fileSystem.File.ReadAllText(@"C:\build\arbiter.nunit");
            document.LoadXml(fileContents);

            var assemblyNode = document.SelectSingleNode("//assembly");
            var assemblyPath = assemblyNode.Attributes["path"].Value;
            Assert.AreEqual(@"SomeProject.Tests\bin\Debug\SomeProject.Tests.dll", assemblyPath);
        }
    }
}
