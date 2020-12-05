using Arbiter.Core.Analysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Arbiter.MSBuild
{
    public class MSBuildSolutionAnalyzer : IMSBuildSolutionAnalyzer
    {
        private const int _distanceHardLimit = 100;
        private Solution _solution;

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
            foreach (var project in results.Values.Where(r => r.Distance == distance - 1))
            {
                var projectObj = _solution.Projects.SingleOrDefault(p => p.FilePath == project.FilePath);
                var dependantProjectIds = graph.GetProjectsThatDirectlyDependOnThisProject(projectObj.Id);
                var newDependantProjectIds = dependantProjectIds.Where(id => !results.ContainsKey(id));
                var newDependantProjects = _solution.Projects.Where(p => newDependantProjectIds.Contains(p.Id));
                foreach (var newProject in newDependantProjects)
                {
                    results.Add(newProject.Id, CreateAnalysisResult(projectObj, distance));
                }
            }

            // As long as we keep finding projects, keep searching.
            if (startingProjectCount < results.Values.Count)
            {
                FindDependantProjectsRecursively(graph, results, distance);
            }
        }

        private AnalysisResult CreateAnalysisResult(Project project, int distance) => new AnalysisResult
        {
            Distance = distance,
            Project = project.Name,
            FilePath = project.FilePath,
            Assembly = project.AssemblyName
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
            // Log
        }

        private void LoadSolution_WorkspaceFailed(object sender, WorkspaceDiagnosticEventArgs e)
        {
            // Log
        }
    }
}
