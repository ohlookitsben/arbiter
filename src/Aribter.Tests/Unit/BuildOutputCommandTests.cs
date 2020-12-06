using Arbiter.Core;
using Arbiter.Core.Analysis;
using Arbiter.Core.Commands;
using Arbiter.Tests.Fakes;
using Autofac;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Xml;

namespace Arbiter.Tests.Unit
{
    [TestFixture]
    [Category(TestCategory.Unit)]
    public class BuildOutputCommandTests
    {
        private FakeFileSystem _fileSystem;
        private Mock<IRepositoryReader> _repositoryReader;
        private Mock<IMSBuildSolutionAnalyzer> _analyzer;
        private BuildOutputCommand _command;

        [SetUp]
        public void SetUp()
        {
            _fileSystem = new FakeFileSystem();
            _repositoryReader = new Mock<IRepositoryReader>();
            _analyzer = new Mock<IMSBuildSolutionAnalyzer>();
            var container = ContainerHelper.ConfigureContainer();
            var scope = container.BeginLifetimeScope(c =>
            {
                c.RegisterInstance<IFileSystem>(_fileSystem);
                c.RegisterInstance(_repositoryReader.Object);
                c.RegisterInstance(_analyzer.Object);
            });

            var runSettings = new RunSettings(new string[4]);
            _command = scope.Resolve<BuildOutputCommand>(new TypedParameter(typeof(RunSettings), runSettings));
        }

        [Test]
        public void Execute_DependantTestsFound_OutputContainsTests()
        {
            var changedFiles = new List<string>
            {
                @"C:\Source\SomeProject\File.cs",
            };
            _repositoryReader.Setup(r => r.ListChangedFiles(It.IsAny<string>(), It.IsAny<string>())).Returns(changedFiles);
            var containingProjects = new List<string>()
            {
                @"C:\Source\SomeProject\SomeProject.csproj",
            };
            _analyzer.Setup(a => a.FindContainingProjects(It.IsAny<IEnumerable<string>>())).Returns(containingProjects);
            var dependantProjects = new List<AnalysisResult>
            {
                new AnalysisResult { Distance = 0, FilePath = @"C:\Source\SomeProject\SomeProject.csproj", Project = "SomeProject" },
                new AnalysisResult { Distance = 1, FilePath = @"C:\Source\SomeProject.Tests\SomeProject.Tests.csproj", Project = "SomeProject.Tests" },
            };
            _analyzer.Setup(a => a.FindDependantProjects(It.IsAny<IEnumerable<string>>())).Returns(dependantProjects);
            var dependantTestProjects = new List<AnalysisResult>
            {
                new AnalysisResult { Distance = 1, FilePath = @"C:\Source\SomeProject.Tests\SomeProject.Tests.csproj", Project = "SomeProject.Tests" }
            };
            _analyzer.Setup(a => a.ExcludeNonTestProjects(It.IsAny<IEnumerable<AnalysisResult>>())).Returns(dependantTestProjects);

            int result = _command.Execute();

            Assert.AreEqual(0, result, "Command should execute successfully.");

            var document = new XmlDocument();
            document.LoadXml(_fileSystem.File);

            var assemblyNode = document.SelectSingleNode("//assembly");
            var assemblyPath = assemblyNode.Attributes["path"].Value;
            Assert.AreEqual(@"C:\Source\SomeProject.Tests\SomeProject.Tests.csproj", assemblyPath);
        }
    }
}
