using Arbiter.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Serilog;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.IO;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Arbiter.MSBuild
{
    /// <inheritdoc/>
    public class MSBuildSolutionLoader : IMSBuildSolutionLoader
    {
        private static readonly Regex _cppProjectExpression = new Regex(@"Cannot open project '(?<path>.*)' because the file extension '\.(?<extension>.*)' is not associated with a language");
        private readonly ILogger _log;
        private readonly IConsole _console;
        private readonly IFileSystem _fileSystem;
        private List<SolutionProject> _solutionProjects;
        private Solution _solution;
        public Solution Solution => _solution ?? throw new InvalidOperationException($"Attempted access solution prior to calling {nameof(LoadSolution)}");

        private readonly List<CppProject> _cppProjects = new List<CppProject>();
        public List<CppProject> CppProjects => _solution != null ? _cppProjects : throw new InvalidOperationException($"Attempted access C++ projects prior to calling {nameof(LoadSolution)}");

        private static readonly Regex _projectExpression = new Regex($@"Project\(""{{(?<TypeId>{Constants.GuidExpression})}}""\) = ""(?<Name>.*)"", ""(?<Path>.*)"", ""{{(?<Id>{Constants.GuidExpression})}}""");

        private int _loadErrorCount;
        private int _loadWarningCount;

        public MSBuildSolutionLoader(ILogger log, IConsole console, IFileSystem fileSystem)
        {
            _log = log;
            _console = console;
            _fileSystem = fileSystem;
        }

        public async Task LoadSolution(string solution, CancellationToken token)
        {
            var workspace = MSBuildWorkspace.Create();
            workspace.WorkspaceFailed += LoadSolution_WorkspaceFailed;
            var progress = new Progress<ProjectLoadProgress>();
            progress.ProgressChanged += LoadSolution_ProgressChanged;

            _console.Out.VerboseWriteLine();
            _solutionProjects = ReadSolutionProjects(solution);
            _solution = await workspace.OpenSolutionAsync(solution, progress, token);

            _console.Out.VerboseWriteLine();
            _console.Out.VerboseWriteLine($"Solution opened with {_loadWarningCount} warnings and {_loadErrorCount} errors. See {Constants.LogFile} for more information");

            _loadErrorCount = 0;
            _loadWarningCount = 0;
        }

        private List<SolutionProject> ReadSolutionProjects(string solution)
        {
            string contents = _fileSystem.File.ReadAllText(solution);
            var matches = _projectExpression.Matches(contents);
            var projects = new List<SolutionProject>();
            string root = _fileSystem.FileInfo.FromFileName(solution).DirectoryName;
            foreach (Match match in matches)
            {
                var id = Guid.Parse(match.Groups["Id"].Value);
                var typeId = Guid.Parse(match.Groups["TypeId"].Value);
                // The solution file only holds relative paths to the projects.
                string path = _fileSystem.Path.Combine(root, match.Groups["Path"].Value);

                projects.Add(new SolutionProject(id, typeId, match.Groups["Name"].Value, path));
            }

            return projects;
        }

        private void LoadSolution_ProgressChanged(object sender, ProjectLoadProgress e)
        {
            switch (e.Operation)
            {
                case ProjectLoadOperation.Evaluate:
                case ProjectLoadOperation.Build:
                    // Only log the final progress event
                    break;
                case ProjectLoadOperation.Resolve:
                    _console.Out.VerboseWriteLine($"Loaded project {Path.GetFileName(e.FilePath)}");
                    break;
            }
        }

        private void LoadSolution_WorkspaceFailed(object sender, WorkspaceDiagnosticEventArgs e)
        {
            if (e.Diagnostic.Kind == WorkspaceDiagnosticKind.Failure && _cppProjectExpression.IsMatch(e.Diagnostic.Message))
            {
                string path = _cppProjectExpression.Match(e.Diagnostic.Message).Groups["path"].Value;
                var project = new CppProject();
                project.Load(path);
                var solutionProject = _solutionProjects.SingleOrDefault(p => p.FilePath.Equals(project.FilePath, StringComparison.OrdinalIgnoreCase));
                project.SetId(solutionProject.Id);
                // Add the project via the private property since the solution is not yet loaded.
                _cppProjects.Add(project);
                _log.Warning($"C++ project found at {path}. Analysis results may be unreliable due to lack of Roslyn support for vcxproj analysis. Custom analysis will be performed for C++ changes");
                ++_loadWarningCount;
                return;
            }

            switch (e.Diagnostic.Kind)
            {
                case WorkspaceDiagnosticKind.Failure:
                    _log.Warning(e.Diagnostic.Message);
                    ++_loadErrorCount;
                    break;
                case WorkspaceDiagnosticKind.Warning:
                    _log.Information(e.Diagnostic.Message);
                    ++_loadWarningCount;
                    break;
            }
        }
    }
}
