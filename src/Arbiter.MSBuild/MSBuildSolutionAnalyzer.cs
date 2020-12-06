using Arbiter.Core.Analysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Arbiter.MSBuild
{
    public class MSBuildSolutionAnalyzer : IMSBuildSolutionAnalyzer
    {
        private const int _distanceHardLimit = 100;
        private readonly Serilog.ILogger _log;
        private Solution _solution;

        public MSBuildSolutionAnalyzer(Serilog.ILogger log)
        {
            _log = log;
        }

        public List<string> FindContainingProjects(IEnumerable<string> files)
        {
            if (_solution == null)
            {
                throw new InvalidOperationException("A solution must be loaded before searching for dependant projects.");
            }

            var projects = new Dictionary<ProjectId, Project>();
            foreach (string file in files)
            {
                foreach (var project in _solution.Projects)
                {
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

        public List<AnalysisResult> FindDependantProjects(IEnumerable<string> projects)
        {
            if (_solution == null)
            {
                throw new InvalidOperationException("A solution must be loaded before searching for dependant projects.");
            }

            var graph = _solution.GetProjectDependencyGraph();
            var results = new Dictionary<ProjectId, AnalysisResult>();
            int distance = 0;
            foreach (string project in projects)
            {
                var projectObj = _solution.Projects.SingleOrDefault(p => p.FilePath == project);
                results.Add(projectObj.Id, CreateAnalysisResult(projectObj, distance));
            }

            FindDependantProjectsRecursively(graph, results, distance);

            return results.Values.ToList();
        }

        private void FindDependantProjectsRecursively(ProjectDependencyGraph graph, Dictionary<ProjectId, AnalysisResult> results, int distance)
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
                var dependantProjectIds = graph.GetProjectsThatDirectlyDependOnThisProject(projectObj.Id);
                var newDependantProjectIds = dependantProjectIds.Where(id => !results.ContainsKey(id));
                var newDependantProjects = _solution.Projects.Where(p => newDependantProjectIds.Contains(p.Id));
                foreach (var newProject in newDependantProjects)
                {
                    results.Add(newProject.Id, CreateAnalysisResult(newProject, distance));
                }
            }

            // As long as we keep finding projects, keep searching.
            if (startingProjectCount < results.Values.Count)
            {
                FindDependantProjectsRecursively(graph, results, distance);
            }
        }

        private static AnalysisResult CreateAnalysisResult(Project project, int distance) => new AnalysisResult
        {
            Distance = distance,
            Project = project.Name,
            FilePath = project.FilePath
        };

        public void LoadSolution(string solution)
        {
            var workspace = MSBuildWorkspace.Create();
            workspace.WorkspaceFailed += LoadSolution_WorkspaceFailed;
            var progress = new Progress<ProjectLoadProgress>();
            progress.ProgressChanged += LoadSolution_ProgressChanged;

            var openTask = workspace.OpenSolutionAsync(solution, progress);
            openTask.Wait();

            _solution = openTask.Result;
        }

        private void LoadSolution_ProgressChanged(object sender, ProjectLoadProgress e)
        {
            _log.Information($"Loading Solution: {Path.GetFileName(e.FilePath)} - {e.Operation}");
        }

        private void LoadSolution_WorkspaceFailed(object sender, WorkspaceDiagnosticEventArgs e)
        {
            switch (e.Diagnostic.Kind)
            {
                case WorkspaceDiagnosticKind.Failure:
                    _log.Warning($"Workspace Message: {e.Diagnostic.Message}");
                    break;
                case WorkspaceDiagnosticKind.Warning:
                    _log.Information($"Workspace Message: {e.Diagnostic.Message}");
                    break;
            }
        }

        public List<AnalysisResult> ExcludeNonTestProjects(IEnumerable<AnalysisResult> dependantProjects)
        {
            var testProjects = new List<AnalysisResult>();
            foreach (var project in dependantProjects)
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
