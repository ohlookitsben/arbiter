using NUnit.Framework;
using System.Collections.Generic;
using Serilog;
using Serilog.Sinks.InMemory;
using System.IO;
using Arbiter.MSBuild;
using Serilog.Events;

namespace Arbiter.Tests.Integration
{
    [TestFixture, Category("Integration")]
    public class MSBuildSolutionAnalyzerTests
    {
        private ArbiterMSBuildLocator _locator;
        private ILogger _logger;

        [SetUp]
        public void SetUp()
        {
            _locator = new ArbiterMSBuildLocator();
            _locator.RegisterDefaults();
            _logger = new LoggerConfiguration()
                .WriteTo.InMemory()
                .MinimumLevel.Warning()
                .CreateLogger();

        }

        [TearDown]
        public void TearDown()
        {
            // Always clean in case there is dirty state left over from a failed test or setup.
            _locator.Clean();
            InMemorySink.Instance.Dispose();
        }

        [Test, Explicit("SetupCom doesn't work here because testhost.net48.x86.exe can't find the assemblies we've copied.")]
        public void LoadSolution_ProjectWithComReferences_Succeeds()
        {
            _locator.SetupCom();
            var analyzer = new MSBuildSolutionAnalyzer(_logger);
            string solutionPath = Path.Combine(TestContext.CurrentContext.TestDirectory, @"Data\SampleProjects\ClassLibraryWithComReference\ClassLibraryWithComReference.sln");

            analyzer.LoadSolution(solutionPath);

            var renderedEvents = RenderLogEvents(InMemorySink.Instance.LogEvents);
            Assert.IsEmpty(renderedEvents, "No warnings or errors should be logged during load");
        }

        [Test]
        public void LoadSolution_StandaloneWindowsFormsApp_Succeeds()
        {
            var analyzer = new MSBuildSolutionAnalyzer(_logger);
            string solutionPath = Path.Combine(TestContext.CurrentContext.TestDirectory, @"Data\SampleProjects\StandaloneFormsApp\StandaloneFormsApp.sln");

            analyzer.LoadSolution(solutionPath);

            var renderedEvents = RenderLogEvents(InMemorySink.Instance.LogEvents);
            Assert.IsEmpty(renderedEvents, "No warnings or errors should be logged during load");
        }

        [Test, Ignore("C++ support is not implemented.")]
        public void LoadSolution_CppClrLibrary_Succeeds()
        {
            var analyzer = new MSBuildSolutionAnalyzer(_logger);
            string solutionPath = Path.Combine(TestContext.CurrentContext.TestDirectory, @"Data\SampleProjects\CppClrLibrary\CppClrLibrary.sln");

            analyzer.LoadSolution(solutionPath);

            var renderedEvents = RenderLogEvents(InMemorySink.Instance.LogEvents);
            Assert.IsEmpty(renderedEvents, "No warnings or errors should be logged during load");
        }

        [Test, Ignore("C++ support is not implemented.")]
        public void LoadSolution_CombinedApp_Succeeds()
        {
            var analyzer = new MSBuildSolutionAnalyzer(_logger);
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
