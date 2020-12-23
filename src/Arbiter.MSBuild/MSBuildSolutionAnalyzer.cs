using Arbiter.Core;
using Arbiter.Core.Analysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Console = System.Console;
using ILogger = Serilog.ILogger;

namespace Arbiter.MSBuild
{
    /// <inheritdoc/>
    public class MSBuildSolutionAnalyzer : IMSBuildSolutionAnalyzer
    {
        private const int _distanceHardLimit = 100;
        private readonly ILogger _log;
        private Solution _solution;
        private int _loadErrorCount = 0;
        private int _loadWarningCount;

        public MSBuildSolutionAnalyzer(ILogger log)
        {
            _log = log;
        }

        public List<string> FindContainingProjects(IEnumerable<string> files)
        {
            if (_solution == null)
            {
                throw new InvalidOperationException("A solution must be loaded before searching for dependent projects.");
            }

            var projects = new Dictionary<ProjectId, Project>();
            foreach (string file in files)
            {
                foreach (var project in _solution.Projects)
                {
                    // TODO: What if it used to be in a project, but now isn't? e.g. deleted
                    if (projects.ContainsKey(project.Id))
                    {
                        continue;
                    }

                    if (project.Documents.Any(d => d.FilePath == file))
                    {
                        projects.Add(project.Id, project);
                    }
                }
            }

            return projects.Values.Select(p => p.FilePath).ToList();
        }

        public List<AnalysisResult> FindDependentProjects(IEnumerable<string> projects)
        {
            if (_solution == null)
            {
                throw new InvalidOperationException("A solution must be loaded before searching for dependent projects.");
            }

            var graph = _solution.GetProjectDependencyGraph();
            var results = new Dictionary<ProjectId, AnalysisResult>();
            int distance = 0;
            foreach (string project in projects)
            {
                var projectObj = _solution.Projects.SingleOrDefault(p => p.FilePath == project);
                results.Add(projectObj.Id, CreateAnalysisResult(projectObj, distance));
            }

            FindDependentProjectsRecursively(graph, results, distance);

            return results.Values.ToList();
        }

        public List<AnalysisResult> GetTopologicallySortedProjects()
        {
            if (_solution == null)
            {
                throw new InvalidOperationException("A solution must be loaded before searching for dependent projects.");
            }

            var graph = _solution.GetProjectDependencyGraph();
            var sortedIds = graph.GetTopologicallySortedProjects().ToList();
            var sortedResults = new List<AnalysisResult>();
            for (int i = 0; i < sortedIds.Count; ++i)
            {
                var project = _solution.GetProject(sortedIds[i]);
                sortedResults.Add(CreateAnalysisResult(project, i));
            }

            return sortedResults;
        }

        public List<Tuple<AnalysisResult, List<AnalysisResult>>> GetGraph()
        {
            if (_solution == null)
            {
                throw new InvalidOperationException("A solution must be loaded before searching for dependent projects.");
            }

            var graph = _solution.GetProjectDependencyGraph();
            var sortedIds = graph.GetTopologicallySortedProjects().ToList();
            var outputGraph = new List<Tuple<AnalysisResult, List<AnalysisResult>>>();
            foreach (var id in sortedIds)
            {
                var project = _solution.GetProject(id);
                var dependentProjects = graph.GetProjectsThatThisProjectDirectlyDependsOn(project.Id).Select(dependentId => _solution.GetProject(dependentId)); ;
                outputGraph.Add(Tuple.Create(CreateAnalysisResult(project, 0), dependentProjects.Select(p => CreateAnalysisResult(p, 0)).ToList()));
            }

            return outputGraph;
        }

        private void FindDependentProjectsRecursively(ProjectDependencyGraph graph, Dictionary<ProjectId, AnalysisResult> results, int distance)
        {
            if (distance > _distanceHardLimit)
            {
                throw new InvalidOperationException($"Project recursion depth exceeded hard limit of {_distanceHardLimit}.");
            }

            ++distance;
            int startingProjectCount = results.Values.Count;
            var ancestors = results.Values.Where(r => r.Distance == distance - 1).ToList();
            foreach (var project in ancestors)
            {
                var projectObj = _solution.Projects.SingleOrDefault(p => p.FilePath == project.FilePath);
                var dependentProjectIds = graph.GetProjectsThatDirectlyDependOnThisProject(projectObj.Id);
                var newDependentProjectIds = dependentProjectIds.Where(id => !results.ContainsKey(id));
                var newDependentProjects = _solution.Projects.Where(p => newDependentProjectIds.Contains(p.Id));
                foreach (var newProject in newDependentProjects)
                {
                    results.Add(newProject.Id, CreateAnalysisResult(newProject, distance));
                }
            }

            // As long as we keep finding projects, keep searching.
            if (startingProjectCount < results.Values.Count)
            {
                FindDependentProjectsRecursively(graph, results, distance);
            }
        }


        /// <remarks>
        /// Can't use project.OutputPath because it will not necessarily be the final output path. E.g. On projects without an explicit setting this is currently
        /// evaluating to $(ProjectDir)/bin/Debug/net48/AssemblyName.dll. Might be related to https://github.com/dotnet/roslyn/issues/12562.
        /// </remarks>
        private static AnalysisResult CreateAnalysisResult(Project project, int distance) => new AnalysisResult(project.Name, project.FilePath, distance, $"{project.AssemblyName}.dll");

        public async Task LoadSolution(string solution, CancellationToken token)
        {
            var workspace = MSBuildWorkspace.Create();
            workspace.WorkspaceFailed += LoadSolution_WorkspaceFailed;
            var progress = new Progress<ProjectLoadProgress>();
            progress.ProgressChanged += LoadSolution_ProgressChanged;

            Console.WriteLine();
            _solution = await workspace.OpenSolutionAsync(solution, progress, token);

            Console.WriteLine();
            Console.WriteLine($"Solution opened with {_loadWarningCount} warnings and {_loadErrorCount} errors. See {Constants.LogFile} for more information.");

            _loadErrorCount = 0;
            _loadWarningCount = 0;
        }

        private void LoadSolution_ProgressChanged(object sender, ProjectLoadProgress e)
        {
            switch (e.Operation)
            {
                case ProjectLoadOperation.Evaluate:
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write($"Loading project {Path.GetFileName(e.FilePath)} (Stage 1/3)");
                    break;
                case ProjectLoadOperation.Build:
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write($"Loading project {Path.GetFileName(e.FilePath)} (Stage 2/3)");
                    break;
                case ProjectLoadOperation.Resolve:
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write($"Loading project {Path.GetFileName(e.FilePath)} (Stage 3/3)");
                    Console.WriteLine();
                    break;
            }
        }

        private void LoadSolution_WorkspaceFailed(object sender, WorkspaceDiagnosticEventArgs e)
        {
            switch (e.Diagnostic.Kind)
            {
                case WorkspaceDiagnosticKind.Failure:
                    _log.Warning($"LoadSolution_WorkspaceFailed: {e.Diagnostic.Message}");
                    ++_loadErrorCount;
                    break;
                case WorkspaceDiagnosticKind.Warning:
                    _log.Information($"LoadSolution_WorkspaceFailed: {e.Diagnostic.Message}");
                    ++_loadWarningCount;
                    break;
            }
        }

        public List<AnalysisResult> ExcludeNonTestProjects(IEnumerable<AnalysisResult> dependentProjects)
        {
            var testProjects = new List<AnalysisResult>();
            foreach (var project in dependentProjects)
            {
                var projectObj = _solution.Projects.Single(p => p.FilePath == project.FilePath);
                var referencedAssemblies = projectObj.MetadataReferences.Where(r => r.Properties.Kind == MetadataImageKind.Assembly);
                bool isTestAssembly = referencedAssemblies.Any(r => Path.GetFileName(r.Display) == "nunit.framework.dll");
                if (isTestAssembly)
                {
                    testProjects.Add(project);
                }
            }

            return testProjects;
        }
    }
}
