using Arbiter.Core.Analysis;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using IConsole = Arbiter.Core.Interfaces.IConsole;

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
        private readonly IMSBuildLocator _locator;

        public ArbiterRootCommand(IRepositoryReader repositoryReader, IMSBuildSolutionAnalyzer analyzer, NUnitProjectWriter writer, NUnitProjectReader reader, IFileSystem fileSystem, IConsole console, IMSBuildLocator locator) : base("Only run the tests you need based on the changes between builds.")
        {
            _repositoryReader = repositoryReader;
            _analyzer = analyzer;
            _writer = writer;
            _reader = reader;
            _fileSystem = fileSystem;
            _console = console;
            _locator = locator;
            AddOption(new Option<FileInfo>(new[] { "--solution", "-s" }, "The path to the solution (.sln) file to evaluate.") { IsRequired = true });
            AddOption(new Option<string>(new[] { "--from-commit", "-f" }, "The start of the commit range to evaluate.") { IsRequired = true });
            AddOption(new Option<string>(new[] { "--to-commit", "-t" }, "The end of the commit range to evaluate.") { IsRequired = true });
            AddOption(new Option<FileInfo>(new[] { "--nunit-project", "-np" }, "The nunit project file containing all test assemblies to consider.") { IsRequired = true });
            AddOption(new Option<string>(new[] { "--nunit-configuration", "-nc" }, "The configuration in the project to consider.") { IsRequired = true });
            Handler = CommandHandler.Create<FileInfo, string, string, FileInfo, string>(ExecuteHandler);

            var setupCom = new Command("setup-com", "Setup support for projects with COM references.")
            {
                Handler = CommandHandler.Create(_locator.SetupCom)
            };
            AddCommand(setupCom);

            var cleanCom = new Command("clean-com", "Return to a clean state without support for additional project types.")
            {
                Handler = CommandHandler.Create(_locator.Clean)
            };
            AddCommand(cleanCom);
        }

        public int ExecuteHandler(FileInfo solution, string fromCommit, string toCommit, FileInfo nunitProject, string nunitConfiguration)
        {
            var repositoryValidation = ValidateRepository(fromCommit, toCommit);
            if (!repositoryValidation.IsValid)
            {
                _console.WriteLine(repositoryValidation.FailureReason);
            }

            _console.WriteLine($"Between commits {fromCommit} {toCommit}");
            var changedFiles = _repositoryReader.ListChangedFiles(fromCommit, toCommit);
            if (!changedFiles.Any())
            {
                _console.WriteLine("O modified files found. No analysis will be performed.");
                return 0;
            }

            string changedFilesOutput = $"      modified:   {string.Join($"{Environment.NewLine}      modified:   ", changedFiles)}";
            _console.WriteLine(changedFilesOutput);

            _analyzer.LoadSolution(solution.FullName);
            _console.WriteLine();
            _console.WriteLine($"Loaded solution file {solution.Name}");

            var changedProjects = _analyzer.FindContainingProjects(changedFiles);
            if (!changedProjects.Any())
            {
                _console.WriteLine("O modified projects found. No output will be written.");
                return 0;
            }

            var dependentProjects = _analyzer.FindDependentProjects(changedProjects);
            var dependentTestProjects = _analyzer.ExcludeNonTestProjects(dependentProjects);
            if (!dependentProjects.Any())
            {
                _console.WriteLine("O dependent test projects found. No output will be written.");
                return 0;
            }

            var depdendentTestProjectAssemblyNames = dependentTestProjects.Select(p => p.Assembly).ToList();
            var dependentTestProjectStrings = dependentTestProjects.OrderBy(p => p.Distance).ThenBy(p => p.Project).Select(p => $"Distance: {p.Distance,3} Project: {p.Project}");
            string dependentTestProjectsOutput = $"      {string.Join($"{Environment.NewLine}      ", dependentTestProjectStrings)}";
            _console.WriteLine();
            _console.WriteLine("Found dependent test projects");
            _console.WriteLine(dependentTestProjectsOutput);

            var sourceTestProjectAssemblies = _reader.ReadProject(nunitProject.FullName);

            string output = _fileSystem.Path.Combine(nunitProject.DirectoryName, "arbiter.nunit");
            _writer.WriteProject(output, sourceTestProjectAssemblies, depdendentTestProjectAssemblyNames);
            _console.WriteLine();
            _console.WriteLine($"Wrote {depdendentTestProjectAssemblyNames.Count} test assemblies to {output}");

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
