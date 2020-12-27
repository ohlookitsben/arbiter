using Arbiter.MSBuild;
using Arbiter.Tests.Helpers;
using Autofac;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Arbiter.Tests.Integration
{
    [TestFixture]
    [Category(TestCategory.Medium)]
    public class MSBuildSolutionAnalyzer_CsprojTests
    {
        private MSBuildSolutionAnalyzer _analyzer;
        private static readonly string _csprojSolution = Path.Combine(TestContext.CurrentContext.TestDirectory, @"Data\SampleProjects\StandaloneFormsApp\StandaloneFormsApp.sln");

        [SetUp]
        public void SetUp()
        {
            // Ensure a clean working directory. Debugging could have left this dirty.
            var locator = new ArbiterMSBuildLocator();
            locator.RegisterDefaults();
            locator.Clean();

            var container = ContainerHelper.ConfigureContainer().WithRealFileSystem().Build();
            _analyzer = container.Resolve<MSBuildSolutionAnalyzer>();
        }

        [Test]
        public async Task FindContainingProjects_ProjectFileIsChanged_ReturnsProject()
        {
            await _analyzer.LoadSolution(_csprojSolution, CancellationToken.None);

            string project = Path.Combine(TestContext.CurrentContext.TestDirectory, @"Data\SampleProjects\StandaloneFormsApp\StandaloneFormsApp.csproj");
            var files = new List<string> { project };

            var result = _analyzer.FindContainingProjects(files);

            Assert.AreEqual(result.Count, 1, "There should be exactly 1 changed project.");
            Assert.AreEqual(project, result.Single(), "The changed project should match the file passed in.");
        }

        [Test]
        public async Task FindContainingProjects_SourceFileIsChanged_ReturnsProject()
        {
            await _analyzer.LoadSolution(_csprojSolution, CancellationToken.None);

            string project = Path.Combine(TestContext.CurrentContext.TestDirectory, @"Data\SampleProjects\StandaloneFormsApp\StandaloneFormsApp.csproj");
            string file = Path.Combine(TestContext.CurrentContext.TestDirectory, @"Data\SampleProjects\StandaloneFormsApp\Form1.cs");
            var files = new List<string> { file };

            var result = _analyzer.FindContainingProjects(files);

            Assert.AreEqual(result.Count, 1, "There should be exactly 1 changed project.");
            Assert.AreEqual(project, result.Single(), "The changed project should match the file passed in.");
        }

        [Test]
        public async Task FindContainingProjects_MultipleSourceFilesAreChanged_ReturnsProject()
        {
            await _analyzer.LoadSolution(_csprojSolution, CancellationToken.None);

            string project = Path.Combine(TestContext.CurrentContext.TestDirectory, @"Data\SampleProjects\StandaloneFormsApp\StandaloneFormsApp.csproj");
            string file1 = Path.Combine(TestContext.CurrentContext.TestDirectory, @"Data\SampleProjects\StandaloneFormsApp\Form1.cs");
            string file2 = Path.Combine(TestContext.CurrentContext.TestDirectory, @"Data\SampleProjects\StandaloneFormsApp\Program.cs");
            var files = new List<string> { file1, file2 };

            var result = _analyzer.FindContainingProjects(files);

            Assert.AreEqual(result.Count, 1, "There should be exactly 1 changed project.");
            Assert.AreEqual(project, result.Single(), "The changed project should match the file passed in.");
        }

        [Test]
        public async Task FindContainingProjects_FileNotInProjectChanged_DoesNotReturnProject()
        {
            await _analyzer.LoadSolution(_csprojSolution, CancellationToken.None);

            string doesntExist = Path.Combine(TestContext.CurrentContext.TestDirectory, @"Data\SampleProjects\StandaloneFormsApp\FileDoesntExist.cs");
            var files = new List<string> { doesntExist };

            var result = _analyzer.FindContainingProjects(files);

            Assert.IsEmpty(result, "The project should not be changed since the file asw not part of the project.");
        }
    }
}
