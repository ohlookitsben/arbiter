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
    public class GraphCommandTests
    {
        private GraphCommand _command;
        private IConsole _console;
        private static readonly string _solution = Path.Combine(TestContext.CurrentContext.TestDirectory, @"Data\SampleProjects\WindowsFormsAppWithReferences.sln");

        [SetUp]
        public void SetUp()
        {
            new ArbiterMSBuildLocator().RegisterDefaults();

            var container = ContainerHelper.ConfigureContainer().WithRealFileSystem().Build();
            _command = container.Resolve<GraphCommand>();
            _console = container.Resolve<IConsole>();
        }

        [Test]
        public async Task ExecuteHandler_OutputsSolutionGraph()
        {
            await _command.ExecuteHandler(new FileInfo(_solution), false, CancellationToken.None);

            string expected = @"digraph G {
  ""NetFrameworkLibrary"" -> { ""NetStandardLibrary"" }
  ""WindowsFormsAppWithReferences"" -> { ""NetFrameworkLibrary"" ""NetStandardLibrary"" }
}
";
            string actual = _console.Out.ToString();

            Assert.AreEqual(expected, actual);
        }
    }
}
