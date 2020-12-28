using Arbiter.Core.Analysis;
using Arbiter.Core.Commands;
using Arbiter.Tests.Helpers;
using Autofac;
using NUnit.Framework;
using System.IO;
using System.Linq;

namespace Arbiter.Tests
{
    [TestFixture]
    [Category(TestCategory.Medium)]
    public class CleanCommandTests
    {
        private readonly string[] _comResolutionAssemblies = new[]
        {
            "Microsoft.Build.dll",
            "Microsoft.Build.Framework.dll",
            "Microsoft.Build.Tasks.Core.dll",
            "Microsoft.Build.Utilities.Core.dll"
        };

        private CleanCommand _command;
        private IMSBuildLocator _locator;

        [SetUp]
        public void SetUp()
        {
            var container = ContainerHelper.ConfigureContainer().Build();
            _command = container.Resolve<CleanCommand>();
            _locator = container.Resolve<IMSBuildLocator>();

            _locator.RegisterDefaults();
        }

        [TearDown]
        public void TearDown()
        {
            _locator?.Clean();
        }

        [Test]
        public void ExecuteHandler_RemovesComResolutionAssemblies()
        {
            string testDirectory = TestContext.CurrentContext.TestDirectory;
            var expectedAssemblies = _comResolutionAssemblies.Select(a => Path.Combine(testDirectory, a));

            // Ensure the output directory is dirty before the clean operation is tested.
            _locator.SetupCom();

            _command.ExecuteHandler(false);

            foreach (var assembly in expectedAssemblies)
            {
                Assert.That(assembly, Does.Not.Exist);
            }
        }
    }
}
