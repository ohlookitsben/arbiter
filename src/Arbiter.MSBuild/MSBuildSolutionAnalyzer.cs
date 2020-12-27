﻿using Arbiter.Core.Analysis;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Arbiter.MSBuild
{
    /// <inheritdoc/>
    public class MSBuildSolutionAnalyzer : IMSBuildSolutionAnalyzer
    {
        private const int _distanceHardLimit = 100;
        private readonly IMSBuildSolutionLoader _loader;

        public MSBuildSolutionAnalyzer(IMSBuildSolutionLoader loader)
        {
            _loader = loader;
        }

        public List<string> FindContainingProjects(IEnumerable<string> files)
        {
            if (files ==  null)
            {
                throw new ArgumentNullException(nameof(files));
            }

            var projects = new Dictionary<ProjectId, Project>();
            var cppProjects = new Dictionary<Guid, CppProject>();
            foreach (string file in files)
            {
                if (file == _loader.Solution.FilePath)
                {
                    // If the solution itself has changed, e.g. a project has been added or deleted, the handling we have for what
                    // dependencies have changed doesn't provide very good guarantees - especially in the case that a project is
                    // deleted from the solution file, but no other changes exist. Consider all projects modified in this case.
                    projects = _loader.Solution.Projects.ToDictionary(p => p.Id, p => p);

                    break;
                }

                foreach (var project in _loader.Solution.Projects)
                {
                    if (projects.ContainsKey(project.Id))
                    {
                        continue;
                    }

                    // Track modfications to the project itself as changes. For projects not using wildcards for files this
                    // is enough to cover the case where a file is deleted. For any project with a wildcard, more sophisticated
                    // handling is needed.
                    if (file == project.FilePath)
                    {
                        projects.Add(project.Id, project);

                        continue;
                    }

                    // TODO: Handle deletion in projects using wildcards.

                    if (project.Documents.Any(d => d.FilePath == file))
                    {
                        projects.Add(project.Id, project);
                    }
                }

                foreach (var project in _loader.CppProjects)
                {
                    if (cppProjects.ContainsKey(project.Id))
                    {
                        continue;
                    }

                    // Track modfications to the project itself as changes. C++ projects don't support wildcards so this is
                    // sufficient to cover source files in the project being deleted.
                    if (file == project.FilePath)
                    {
                        cppProjects.Add(project.Id, project);

                        continue;
                    }

                    if (project.DocumentPaths.Any(d => d == file))
                    {
                        cppProjects.Add(project.Id, project);
                    }
                }
            }

            return projects.Values.Select(p => p.FilePath)
                .Concat(cppProjects.Values.Select(cp => cp.FilePath))
                .ToList();
        }

        public List<AnalysisResult> FindDependentProjects(IEnumerable<string> projects)
        {
            var graph = _loader.Solution.GetProjectDependencyGraph();
            var results = new Dictionary<ProjectId, AnalysisResult>();
            var cppResults = new Dictionary<Guid, AnalysisResult>();
            int distance = 0;
            foreach (string project in projects)
            {
                var projectObj = _loader.Solution.Projects.SingleOrDefault(p => p.FilePath == project);
                if (projectObj != null)
                {
                    results.Add(projectObj.Id, CreateAnalysisResult(projectObj, distance));

                    continue;
                }

                // This may be a C++ project. Search for it there before failing.
                var cppProject = _loader.CppProjects.SingleOrDefault(p => p.FilePath == project);
                if (cppProject == null)
                {
                    throw new ArgumentException($"No project found in the solution at: {project}");
                }

                cppResults.Add(cppProject.Id, CreateAnalysisResult(cppProject, distance));

                // Since Roslyn can't analyze C++ projects, search manually through their referenced assemblies.
                foreach (var csProject in _loader.Solution.Projects)
                {
                    if (ProjectReferencesProject(csProject, cppProject))
                    {
                        results.Add(csProject.Id, CreateAnalysisResult(csProject, distance + 1));
                    }
                }
            }

            FindDependentProjectsRecursively(graph, results, distance);

            // TODO: Handle dependencies on C++ projects.

            return results.Values.ToList();
        }

        public List<AnalysisResult> GetTopologicallySortedProjects()
        {
            var graph = _loader.Solution.GetProjectDependencyGraph();
            var sortedIds = graph.GetTopologicallySortedProjects().ToList();
            var sortedResults = new List<AnalysisResult>();
            for (int i = 0; i < sortedIds.Count; ++i)
            {
                var project = _loader.Solution.GetProject(sortedIds[i]);
                sortedResults.Add(CreateAnalysisResult(project, i));
            }

            return sortedResults;
        }

        public List<Tuple<AnalysisResult, List<AnalysisResult>>> GetGraph()
        {
            var graph = _loader.Solution.GetProjectDependencyGraph();
            var sortedIds = graph.GetTopologicallySortedProjects().ToList();
            var outputGraph = new List<Tuple<AnalysisResult, List<AnalysisResult>>>();
            foreach (var id in sortedIds)
            {
                var project = _loader.Solution.GetProject(id);
                var dependentProjects = graph.GetProjectsThatThisProjectDirectlyDependsOn(project.Id).Select(dependentId => _loader.Solution.GetProject(dependentId)); ;
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
                var projectObj = _loader.Solution.Projects.SingleOrDefault(p => p.FilePath == project.FilePath);
                var dependentProjectIds = graph.GetProjectsThatDirectlyDependOnThisProject(projectObj.Id);
                var newDependentProjectIds = dependentProjectIds.Where(id => !results.ContainsKey(id));
                var newDependentProjects = _loader.Solution.Projects.Where(p => newDependentProjectIds.Contains(p.Id));
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

        private AnalysisResult CreateAnalysisResult(CppProject cppProject, int distance) => new AnalysisResult(cppProject.Name, cppProject.FilePath, distance, $"{cppProject.AssemblyName}.dll");

        public List<AnalysisResult> ExcludeNonTestProjects(IEnumerable<AnalysisResult> dependentProjects)
        {
            var testProjects = new List<AnalysisResult>();
            foreach (var project in dependentProjects)
            {
                var projectObj = _loader.Solution.Projects.Single(p => p.FilePath == project.FilePath);
                bool isTestAssembly = ProjectReferencesAssembly(projectObj, "nunit.framework.dll");
                if (isTestAssembly)
                {
                    testProjects.Add(project);
                }
            }

            return testProjects;
        }

        private static bool ProjectReferencesAssembly(Project project, string assembly)
        {
            var referencedAssemblies = project.MetadataReferences.Where(r => r.Properties.Kind == MetadataImageKind.Assembly);
            return referencedAssemblies.Any(r => Path.GetFileName(r.Display) == assembly);
        }

        private static bool ProjectReferencesProject(Project project, CppProject cppProject)
        {
            var referencedProjects = project.AllProjectReferences.Select(p => p.ProjectId);
            var referencedAssemblies = project.MetadataReferences.Where(r => r.Properties.Kind == MetadataImageKind.Module);
            return referencedAssemblies.Any(r => Path.GetFileName(r.Display) == cppProject.Name);
        }

        public Task LoadSolution(string fullName, CancellationToken token) => _loader.LoadSolution(fullName, token);
    }
}
