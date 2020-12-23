using Arbiter.Core.Analysis;
using Arbiter.Core.Commands;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Arbiter.Core
{
    public class ArbiterRootCommand : RootCommand
    {
        private readonly IRepositoryReader _repositoryReader;
        private readonly IMSBuildSolutionAnalyzer _analyzer;
        private readonly NUnitProjectWriter _writer;
        private readonly NUnitProjectReader _reader;
        private readonly IFileSystem _fileSystem;
        private readonly IConsole _console;
        private readonly ChildCommands _childCommands;

        public ArbiterRootCommand(IRepositoryReader repositoryReader, IMSBuildSolutionAnalyzer analyzer, NUnitProjectWriter writer, NUnitProjectReader reader, IFileSystem fileSystem, IConsole console, ChildCommands childCommands) : base("Only run the tests you need based on the changes between builds.")
        {
            _repositoryReader = repositoryReader;
            _analyzer = analyzer;
            _writer = writer;
            _reader = reader;
            _fileSystem = fileSystem;
            _console = console;
            _childCommands = childCommands;

            AddOption(CommonOptions.Solution);
            AddOption(CommonOptions.FromCommit);
            AddOption(CommonOptions.ToCommit);
            AddOption(new Option<FileInfo>(new[] { "--nunit-project", "-np" }, "The nunit project file containing all test assemblies to consider.") { IsRequired = true });
            AddOption(new Option<string>(new[] { "--nunit-configuration", "-nc" }, "The configuration in the project to consider.") { IsRequired = true });

            Handler = CommandHandler.Create<FileInfo, string, string, FileInfo, string, CancellationToken>(ExecuteHandler);

            AddCommand(_childCommands.CleanCom);
            AddCommand(_childCommands.SetupCom);
            AddCommand(_childCommands.Diff);
            AddCommand(_childCommands.Sort);
            AddCommand(_childCommands.Graph);
        }

        public async Task<int> ExecuteHandler(FileInfo solution, string fromCommit, string toCommit, FileInfo nunitProject, string nunitConfiguration, CancellationToken token)
        {
            var repositoryValidation = ValidateRepository(fromCommit, toCommit);
            if (!repositoryValidation.IsValid)
            {
                _console.Error.WriteLine(repositoryValidation.FailureReason);
            }

            // Determine which files have changed. If there aren't any changes there's no need to run a potentially expensive solution analysis.
            _console.Out.WriteLine($"Between commits {fromCommit} {toCommit}");
            _childCommands.Diff.ExecuteHandler(fromCommit, toCommit);
            var changedFiles = _childCommands.Diff.Result;
            if (!changedFiles.Any())
            {
                return 0;
            }

            string changedFilesOutput = $"      modified:   {string.Join($"{Environment.NewLine}      modified:   ", changedFiles)}";
            _console.Out.WriteLine($"{changedFiles.Count} modified files found.");
            _console.Out.WriteLine(changedFilesOutput);

            await _analyzer.LoadSolution(solution.FullName, token);
            _console.Out.WriteLine();
            _console.Out.WriteLine($"Loaded solution file {solution.Name}");

            var changedProjects = _analyzer.FindContainingProjects(changedFiles);
            if (!changedProjects.Any())
            {
                _console.Out.WriteLine("O modified projects found. No output will be written.");
                return 0;
            }

            var dependentProjects = _analyzer.FindDependentProjects(changedProjects);
            var dependentTestProjects = _analyzer.ExcludeNonTestProjects(dependentProjects);
            if (!dependentProjects.Any())
            {
                _console.Out.WriteLine("O dependent test projects found. No output will be written.");
                return 0;
            }

            var depdendentTestProjectAssemblyNames = dependentTestProjects.Select(p => p.Assembly).ToList();
            var dependentTestProjectStrings = dependentTestProjects.OrderBy(p => p.Distance).ThenBy(p => p.Project).Select(p => $"Distance: {p.Distance,3} Project: {p.Project}");
            string dependentTestProjectsOutput = $"      {string.Join($"{Environment.NewLine}      ", dependentTestProjectStrings)}";
            _console.Out.WriteLine();
            _console.Out.WriteLine("Found dependent test projects");
            _console.Out.WriteLine(dependentTestProjectsOutput);

            var sourceTestProjectAssemblies = _reader.ReadProject(nunitProject.FullName);

            string output = _fileSystem.Path.Combine(nunitProject.DirectoryName, "arbiter.nunit");
            _writer.WriteProject(output, sourceTestProjectAssemblies, depdendentTestProjectAssemblyNames);
            _console.Out.WriteLine();
            _console.Out.WriteLine($"Wrote {depdendentTestProjectAssemblyNames.Count} test assemblies to {output}");

            return 0;
        }

        private ValidationResult ValidateRepository(string fromCommit, string toCommit)
        {
            if (!_repositoryReader.ToolExists())
            {
                return ValidationResult.Fail("No git executable could be found.");
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
