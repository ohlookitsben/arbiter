using NUnit.Framework;
using System.Collections.Generic;
using Serilog;
using Serilog.Sinks.InMemory;
using System.IO;
using Arbiter.MSBuild;
using Serilog.Events;
using System.Linq;

namespace Arbiter.Tests.Integration
{
    [TestFixture, Category("Integration")]
    public class MSBuildSolutionAnalyzerTests
    {
        [SetUp]
        public void SetUp()
        {
            new ArbiterMSBuildLocator().RegisterDefaults();
        }

        // Copying assemblies from C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\amd64 can fix the COM issues to do with assembly loading.
        // Required assemblies:
        // - Microsoft.Build.dll
        // - Microsoft.Build.Framework.dll
        // - Microsoft.Build.Tasks.Core.dll
        // - Microsoft.Build.Utilities.Core.dll
        [Test, Explicit("TODO: Fix this test")]
        public void LoadSolution_WithCppAndCOM_Succeeds()
        {
            var logger = new LoggerConfiguration()
                .WriteTo.InMemory()
                .MinimumLevel.Warning()
                .CreateLogger();
            var analyzer = new MSBuildSolutionAnalyzer(logger);
            string solutionPath = Path.Combine(TestContext.CurrentContext.TestDirectory, @"Data\SampleProjects\SampleProjects.sln");
            analyzer.LoadSolution(solutionPath);

            var renderedEvents = RenderLogEvents(InMemorySink.Instance.LogEvents);
            Assert.IsEmpty(renderedEvents, "No warnings or errors should be logged during load");
        }

        private static string RenderLogEvents(IEnumerable<LogEvent> events)
        {
            var writer = new StringWriter();
            foreach(var @event in events)
            {
                @event.RenderMessage(writer);
                writer.WriteLine();
            }

            return writer.ToString();
        }
    }
}
