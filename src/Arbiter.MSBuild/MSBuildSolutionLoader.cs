using Arbiter.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Serilog;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.IO;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Arbiter.MSBuild
{
    /// <inheritdoc/>
    public class MSBuildSolutionLoader : IMSBuildSolutionLoader
    {
        private static readonly Regex _cppProjectExpression = new Regex(@"Cannot open project '(?<path>.*)' because the file extension '\.(?<extension>.*)' is not associated with a language.");
        private readonly ILogger _log;
        private readonly IConsole _console;
        private Solution _solution;
        public Solution Solution => _solution ?? throw new InvalidOperationException($"Attempted access solution prior to calling {nameof(LoadSolution)}.");

        private List<CppProject> _cppProjects = new List<CppProject>();
        public List<CppProject> CppProjects => _solution != null ? _cppProjects : throw new InvalidOperationException($"Attempted access C++ projects prior to calling {nameof(LoadSolution)}.");

        private int _loadErrorCount;
        private int _loadWarningCount;

        public MSBuildSolutionLoader(ILogger log, IConsole console)
        {
            _log = log;
            _console = console;
        }

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
                    _console.Out.Write($"Loading project {Path.GetFileName(e.FilePath)} .");
                    break;
                case ProjectLoadOperation.Build:
                    _console.Out.Write(".");
                    break;
                case ProjectLoadOperation.Resolve:
                    _console.Out.WriteLine(".");
                    break;
            }
        }

        private void LoadSolution_WorkspaceFailed(object sender, WorkspaceDiagnosticEventArgs e)
        {
            if (e.Diagnostic.Kind == WorkspaceDiagnosticKind.Failure && _cppProjectExpression.IsMatch(e.Diagnostic.Message))
            {
                string path = _cppProjectExpression.Match(e.Diagnostic.Message).Groups["path"].Value;
                // Add via the private property since the solution is not yet loaded.
                var project = new CppProject();
                project.Load(path);
                _cppProjects.Add(project);
                _log.Warning($"C++ project found at {path}. Analysis results may be unreliable due to lack of Roslyn support for vcxproj analysis. Custom analysis will be performed for C++ changes.");
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
