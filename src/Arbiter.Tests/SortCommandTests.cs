using Arbiter.Core.Commands;
using Arbiter.MSBuild;
using Arbiter.Tests.Helpers;
using Autofac;
using NUnit.Framework;
using System.CommandLine;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Arbiter.Tests
{
    [TestFixture]
    [Category(TestCategory.Medium)]
    public class SortCommandTests
    {
        private SortCommand _command;
        private IConsole _console;
        private static readonly string _solution = Path.Combine(TestContext.CurrentContext.TestDirectory, @"Data\SampleProjects\WindowsFormsAppWithReferences.sln");

        [SetUp]
        public void SetUp()
        {
            new ArbiterMSBuildLocator().RegisterDefaults();

            var container = ContainerHelper.ConfigureContainer().WithRealFileSystem().Build();
            _command = container.Resolve<SortCommand>();
            _console = container.Resolve<IConsole>();
        }

        [Test]
        public async Task ExecuteHandler_OutputsSortedProjects()
        {
            await _command.ExecuteHandler(new FileInfo(_solution), false, CancellationToken.None);

            string expected = @"NetStandardLibrary
NetFrameworkLibrary
WindowsFormsAppWithReferences
";
            string actual = _console.Out.ToString();
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public async Task ExecuteHandler_Verbose_OutputsSortedProjectsWithIndexes()
        {
            await _command.ExecuteHandler(new FileInfo(_solution), true, CancellationToken.None);

            string expected = @"
Loaded project NetStandardLibrary.csproj
Loaded project NetFrameworkLibrary.csproj
Loaded project WindowsFormsAppWithReferences.csproj

Solution opened with 2 warnings and 0 errors. See arbiter.log for more information

0	NetStandardLibrary
1	NetFrameworkLibrary
2	WindowsFormsAppWithReferences
";
            string actual = _console.Out.ToString();

            Assert.AreEqual(expected, actual);
        }
    }
}
