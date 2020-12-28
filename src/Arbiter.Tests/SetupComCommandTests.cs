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
    public class SetupComCommandTests
    {
        private readonly string[] _comResolutionAssemblies = new[]
        {
            "Microsoft.Build.dll",
            "Microsoft.Build.Framework.dll",
            "Microsoft.Build.Tasks.Core.dll",
            "Microsoft.Build.Utilities.Core.dll"
        };

        private SetupComCommand _command;
        private IMSBuildLocator _locator;

        [SetUp]
        public void SetUp()
        {
            var container = ContainerHelper.ConfigureContainer().Build();
            _command = container.Resolve<SetupComCommand>();
            _locator = container.Resolve<IMSBuildLocator>();

            _locator.RegisterDefaults();
        }

        [TearDown]
        public void TearDown()
        {
            _locator?.Clean();
        }

        [Test]
        public void ExecuteHandler_CopiesComResolutionAssemblies()
        {
            string testDirectory = TestContext.CurrentContext.TestDirectory;
            var expectedAssemblies = _comResolutionAssemblies.Select(a => Path.Combine(testDirectory, a));

            _command.ExecuteHandler(false);

            foreach (var assembly in expectedAssemblies)
            {
                Assert.That(assembly, Does.Exist);
            }
        }
    }
}
