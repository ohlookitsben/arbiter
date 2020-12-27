using Arbiter.Core.Analysis;
using Arbiter.MSBuild;
using Arbiter.Tests.Helpers;
using Autofac;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Arbiter.Tests.Integration
{
    [TestFixture]
    [Category(TestCategory.Medium)]
    public class MSBuildSolutionAnalyzer_MixedSolutionTests
    {
        private MSBuildSolutionAnalyzer _analyzer;
        private static readonly string _mixedSolution = Path.Combine(TestContext.CurrentContext.TestDirectory, @"Data\SampleProjects\FormsAppWithCppReference.sln");

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
        public async Task FindDependentProjects_CppProjectChanged_BothProjectsReturned()
        {
            string cppProject = Path.Combine(TestContext.CurrentContext.TestDirectory, @"Data\SampleProjects\CppClrLibrary\CppClrLibrary.vcxproj");
            var changedProjects = new List<string> { cppProject };
            string csProject = Path.Combine(TestContext.CurrentContext.TestDirectory, @"Data\SampleProjects\FormsAppWithCppReference\FormsAppWithCppReference.csproj");
            var expectedProjects = new List<AnalysisResult>
            {
                new AnalysisResult("CppClrLibrary", cppProject, 0, "CppClrLibrary.dll"),
                new AnalysisResult("FormsAppWithCppReference", csProject, 1, "FormsAppWithCppReference.dll")
            };

            await _analyzer.LoadSolution(_mixedSolution, CancellationToken.None);

            var result = _analyzer.FindDependentProjects(changedProjects);

            CollectionAssert.AreEquivalent(expectedProjects, result);
        }
    }
}
