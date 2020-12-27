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
    public class MSBuildSolutionAnalyzer_MixedSolutionTestss
    {
        private MSBuildSolutionAnalyzer _analyzer;
        private static readonly string MixedSolution = Path.Combine(TestContext.CurrentContext.TestDirectory, @"Data\SampleProjects\FormsAppWithCppReference.sln");

        [SetUp]
        public void SetUp()
        {
            // Ensure a clean working directory. Debugging could have left this dirty.
            var locator = new ArbiterMSBuildLocator();
            locator.RegisterDefaults();
            locator.Clean();

            var container = ContainerHelper.ConfigureContainer();
            _analyzer = container.Resolve<MSBuildSolutionAnalyzer>();
        }

        [Test]
        [Explicit("TODO: Implement this feature")]
        public async Task FindDependentProjects_CppProjectChanged_BothProjectsReturned()
        {
            string cppProject = Path.Combine(TestContext.CurrentContext.TestDirectory, @"Data\SampleProjects\CppClrLibrary\CppClrLibrary.vcxproj");
            var changedProjects = new List<string> { cppProject };
            string csProject = Path.Combine(TestContext.CurrentContext.TestDirectory, @"Data\SampleProjects\FormsAppWithCppReference\FormsAppWithCppReference.vcxproj");
            var expectedProjects = new List<string> { cppProject, csProject };

            await _analyzer.LoadSolution(MixedSolution, CancellationToken.None);

            var result = _analyzer.FindDependentProjects(changedProjects);

            CollectionAssert.AreEquivalent(expectedProjects, result);
        }
    }
}
