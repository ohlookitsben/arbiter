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

            var runSettings = new RunSettings(new string[] { @"C:\Source\MySolution.sln", "commit", "other_commit", @"C:\Source\all.nunit", "Default" });
            _command = scope.Resolve<BuildOutputCommand>(new TypedParameter(typeof(RunSettings), runSettings));
        }

        [Test]
        public void Execute_DependentTestsFound_OutputContainsTests()
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
            var dependentProjects = new List<AnalysisResult>
            {
                new AnalysisResult { Distance = 0, FilePath = @"C:\Source\SomeProject\SomeProject.csproj", Project = "SomeProject", Assembly = "SomeProject.dll" },
                new AnalysisResult { Distance = 1, FilePath = @"C:\Source\SomeProject.Tests\SomeProject.Tests.csproj", Project = "SomeProject.Tests", Assembly = "SomeProject.Tests.dll" },
            };
            _analyzer.Setup(a => a.FindDependentProjects(It.IsAny<IEnumerable<string>>())).Returns(dependentProjects);
            var dependentTestProjects = new List<AnalysisResult>
            {
                new AnalysisResult { Distance = 1, FilePath = @"C:\Source\SomeProject.Tests\SomeProject.Tests.csproj", Project = "SomeProject.Tests", Assembly = "SomeProject.Tests.dll" }
            };
            _analyzer.Setup(a => a.ExcludeNonTestProjects(It.IsAny<IEnumerable<AnalysisResult>>())).Returns(dependentTestProjects);
            // Write the all tests nunit project so it can be read before the output is written.
            _fileSystem.WriteFile("all.nunit", @"<NUnitProject>
    <Settings activeConfig=""Default"" />
    <Config name=""Default"">
        <assembly path=""SomeOtherProject.Tests\bin\Debug\SomeOtherProject.Tests.dll"" />
        <assembly path=""SomeProject.Tests\bin\Debug\SomeProject.Tests.dll"" />
    </Config>
</NUnitProject>
");

            int result = _command.Execute();

            Assert.AreEqual(0, result, "Command should execute successfully.");

            var document = new XmlDocument();
            document.LoadXml(_fileSystem.File);

            var assemblyNode = document.SelectSingleNode("//assembly");
            var assemblyPath = assemblyNode.Attributes["path"].Value;
            Assert.AreEqual(@"SomeProject.Tests\bin\Debug\SomeProject.Tests.dll", assemblyPath);
        }
    }
}
