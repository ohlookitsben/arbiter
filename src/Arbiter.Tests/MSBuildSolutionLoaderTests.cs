using Arbiter.MSBuild;
using NUnit.Framework;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.InMemory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Arbiter.Tests.Integration
{
    [TestFixture]
    [Category(TestCategory.Medium)]
    public class MSBuildSolutionLoaderTests
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

        [Test]
        [Explicit("SetupCom doesn't work here because testhost.net48.x86.exe can't find the assemblies we've copied.")]
        public async Task LoadSolution_ProjectWithComReferences_Succeeds()
        {
            _locator.SetupCom();
            var loader = new MSBuildSolutionLoader(_logger);
            string solutionPath = Path.Combine(TestContext.CurrentContext.TestDirectory, @"Data\SampleProjects\ClassLibraryWithComReference\ClassLibraryWithComReference.sln");

            await loader.LoadSolution(solutionPath, CancellationToken.None);

            string errors = RenderLogErrors(InMemorySink.Instance.LogEvents);
            Assert.IsEmpty(errors, "No errors should be logged during load");

            string warnings = RenderLogWarnings(InMemorySink.Instance.LogEvents);
            Assert.IsEmpty(warnings, "No warnings should be logged during load");
        }

        [Test]
        public async Task LoadSolution_StandaloneWindowsFormsApp_Succeeds()
        {
            var loader = new MSBuildSolutionLoader(_logger);
            string solutionPath = Path.Combine(TestContext.CurrentContext.TestDirectory, @"Data\SampleProjects\StandaloneFormsApp\StandaloneFormsApp.sln");

            await loader.LoadSolution(solutionPath, CancellationToken.None);

            string errors = RenderLogErrors(InMemorySink.Instance.LogEvents);
            Assert.IsEmpty(errors, "No errors should be logged during load");

            string warnings = RenderLogWarnings(InMemorySink.Instance.LogEvents);
            Assert.IsEmpty(warnings, "No warnings should be logged during load");
        }

        [Test]
        public async Task LoadSolution_CppClrLibrary_Succeeds()
        {
            var loader = new MSBuildSolutionLoader(_logger);
            string solutionPath = Path.Combine(TestContext.CurrentContext.TestDirectory, @"Data\SampleProjects\CppClrLibrary\CppClrLibrary.sln");

            await loader.LoadSolution(solutionPath, CancellationToken.None);

            string errors = RenderLogErrors(InMemorySink.Instance.LogEvents);
            Assert.IsEmpty(errors, "No errors should be logged during load");

            string warnings = RenderLogWarnings(InMemorySink.Instance.LogEvents);
            Assert.That(warnings,
                Does.StartWith("C++ project found").And
                    .EndsWith($"Custom analysis will be performed for C++ changes.{Environment.NewLine}"),
                "A single C++ warning should be logged during load.");
        }

        [Test]
        [Explicit("SetupCom doesn't work here because testhost.net48.x86.exe can't find the assemblies we've copied.")]
        public async Task LoadSolution_CombinedApp_Succeeds()
        {
            _locator.SetupCom();
            var loader = new MSBuildSolutionLoader(_logger);
            string solutionPath = Path.Combine(TestContext.CurrentContext.TestDirectory, @"Data\SampleProjects\SampleProjects.sln");

            await loader.LoadSolution(solutionPath, CancellationToken.None);

            string errors = RenderLogErrors(InMemorySink.Instance.LogEvents);
            Assert.IsEmpty(errors, "No errors should be logged during load");

            string warnings = RenderLogWarnings(InMemorySink.Instance.LogEvents);
            Assert.IsEmpty(warnings, "No warnings should be logged during load");
        }

        private static string RenderLogErrors(IEnumerable<LogEvent> events)
        {
            return RenderLogEvents(events, LogEventLevel.Error);
        }

        private static string RenderLogWarnings(IEnumerable<LogEvent> events)
        {
            return RenderLogEvents(events, LogEventLevel.Warning);
        }

        private static string RenderLogEvents(IEnumerable<LogEvent> events, LogEventLevel minimumLevel = LogEventLevel.Information)
        {
            var writer = new StringWriter();
            foreach (var @event in events.Where(e => e.Level >= minimumLevel))
            {
                @event.RenderMessage(writer);
                writer.WriteLine();
            }

            return writer.ToString();
        }
    }
}
