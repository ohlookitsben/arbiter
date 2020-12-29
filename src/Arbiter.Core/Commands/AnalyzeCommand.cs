using Arbiter.Core.Analysis;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Arbiter.Core.Commands
{
    public class AnalyzeCommand : Command
    {
        private readonly IRepositoryReader _repositoryReader;
        private readonly IMSBuildSolutionAnalyzer _analyzer;
        private readonly IConsole _console;
        private readonly DiffCommand _diff;

        public List<AnalysisResult> Results { get; } = new List<AnalysisResult>();

        public AnalyzeCommand(IRepositoryReader repositoryReader, IMSBuildSolutionAnalyzer analyzer, IConsole console, DiffCommand diff) : base("analyze", "Analyze changes and return dependent projects")
        {
            _repositoryReader = repositoryReader;
            _analyzer = analyzer;
            _console = console;
            _diff = diff;

            AddOption(CommonOptions.Solution);
            AddOption(CommonOptions.FromCommit);
            AddOption(CommonOptions.Distance);

            Handler = CommandHandler.Create<FileInfo, string, bool, bool, CancellationToken>((solution, fromCommit, verbose, distance, token) => ExecuteHandler(solution, fromCommit, verbose, distance, token));
        }

        public async Task<int> ExecuteHandler(FileInfo solution, string fromCommit, bool verbose, bool distance, CancellationToken token, bool suppressOutput = false)
        {
            Globals.Verbose = verbose;
            Globals.PrintDistance = distance;

            // Always process changes relative to the HEAD. Any other commit would require checking that version out to read the solution.
            string toCommit = "HEAD";

            var repositoryValidation = ValidateRepository(fromCommit, toCommit);
            if (!repositoryValidation.IsValid)
            {
                _console.Error.WriteLine(repositoryValidation.FailureReason);
            }

            // Determine which files have changed. If there aren't any changes there's no need to run a potentially expensive solution analysis.
            _console.Out.VerboseWriteLine($"Between commits {fromCommit} {toCommit}");
            _diff.ExecuteHandler(fromCommit, toCommit, suppressOutput);
            var changedFiles = _diff.Result;
            if (!changedFiles.Any())
            {
                return 0;
            }

            string changedFilesOutput = $"      modified:   {string.Join($"{Environment.NewLine}      modified:   ", changedFiles)}";
            _console.Out.VerboseWriteLine($"{changedFiles.Count} modified files found");
            _console.Out.VerboseWriteLine(changedFilesOutput);

            await _analyzer.LoadSolution(solution.FullName, token);
            _console.Out.VerboseWriteLine();
            _console.Out.VerboseWriteLine($"Loaded solution file {solution.Name}");

            var changedProjects = _analyzer.FindContainingProjects(changedFiles);
            if (!changedProjects.Any())
            {
                if (!suppressOutput || Globals.Verbose)
                {
                    _console.Out.WriteLine("0 modified projects found. No output will be written");
                }

                return 0;
            }

            var dependentProjects = _analyzer.FindDependentProjects(changedProjects);
            if (!dependentProjects.Any())
            {
                if (!suppressOutput || Globals.Verbose)
                {
                    _console.Out.WriteLine("0 dependent projects found. No output will be written");
                }

                return 0;
            }

            _console.Out.VerboseWriteLine($"{dependentProjects.Count} dependent projects found");
            _console.Out.VerboseWriteLine();

            if (!suppressOutput || Globals.Verbose)
            {
                foreach (var dependentProject in dependentProjects)
                {
                    if (Globals.PrintDistance)
                    {
                        _console.Out.Write($"{dependentProject.Distance}\t");
                    }

                    _console.Out.WriteLine(dependentProject.Assembly);
                }
            }

            Results.AddRange(dependentProjects);

            return 0;
        }

        private ValidationResult ValidateRepository(string fromCommit, string toCommit)
        {
            if (!_repositoryReader.ToolExists())
            {
                return ValidationResult.Fail("No git executable could be found");
            }
            if (!_repositoryReader.RepositoryExists())
            {
                return ValidationResult.Fail($"No repository could be found at: {_repositoryReader.WorkingDirectory}");
            }

            if (!_repositoryReader.CommitExists(fromCommit))
            {
                return ValidationResult.Fail($"No commit could be found: {fromCommit}");
            }

            if (!_repositoryReader.CommitExists(toCommit))
            {
                return ValidationResult.Fail($"No commit could be found: {toCommit}");
            }

            if (!_repositoryReader.CommitIsAncestor(toCommit, fromCommit))
            {
                return ValidationResult.Fail($"The commit {fromCommit} is not an ancestor of {toCommit}");
            }

            return ValidationResult.Pass;
        }
    }
}
