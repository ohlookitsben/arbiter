using Arbiter.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Serilog;
using System;
using System.Collections.Generic;
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
        private Solution _solution;
        public Solution Solution => _solution ?? throw new InvalidOperationException($"Attempted access solution prior to calling {nameof(LoadSolution)}.");
        private int _loadErrorCount;
        private int _loadWarningCount;
        private List<CppProject> _cppProjects = new List<CppProject>();

        public MSBuildSolutionLoader(ILogger log)
        {
            _log = log;
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
            if (e.Diagnostic.Kind == WorkspaceDiagnosticKind.Failure && _cppProjectExpression.IsMatch(e.Diagnostic.Message))
            {
                string path = _cppProjectExpression.Match(e.Diagnostic.Message).Groups["path"].Value;
                _cppProjects.Add(new CppProject(path));
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
