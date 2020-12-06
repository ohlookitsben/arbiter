using Arbiter.Core.Analysis;
using System;
using System.Linq;

namespace Arbiter.Core.Commands
{
    public class BuildOutputCommand : ICommand
    {
        private readonly RunSettings _settings;
        private readonly IRepositoryReader _repositoryReader;
        private readonly IMSBuildSolutionAnalyzer _analyzer;
        private readonly NUnitProjectWriter _writer;

        public BuildOutputCommand(RunSettings settings, IRepositoryReader repositoryReader, IMSBuildSolutionAnalyzer analyzer, NUnitProjectWriter writer)
        {
            _settings = settings;
            _repositoryReader = repositoryReader;
            _repositoryReader.WorkingDirectory = _settings.WorkingDirectory;
            _analyzer = analyzer;
            _writer = writer;
        }

        public int Execute()
        {
            Console.WriteLine($"Between commits {_settings.FromCommit} {_settings.ToCommit}");
            var changedFiles = _repositoryReader.ListChangedFiles(_settings.FromCommit, _settings.ToCommit);
            if (!changedFiles.Any())
            {
                Console.WriteLine("O modified files found. No analysis will be performed.");
                return 0;
            }

            string changedFilesOutput = $"      modified:   {string.Join($"{Environment.NewLine}      modified:   ", changedFiles)}";
            Console.WriteLine(changedFilesOutput);

            _analyzer.LoadSolution(_settings.Solution);
            Console.WriteLine();
            Console.WriteLine($"Loaded solution file {_settings.Solution}");

            var changedProjects = _analyzer.FindContainingProjects(changedFiles);
            if (!changedProjects.Any())
            {
                Console.WriteLine("O modified projects found. No output will be written.");
                return 0;
            }

            var dependantProjects = _analyzer.FindDependantProjects(changedProjects);
            var dependantTestProjects = _analyzer.ExcludeNonTestProjects(dependantProjects);
            if (!dependantProjects.Any())
            {
                Console.WriteLine("O dependant test projects found. No output will be written.");
                return 0;
            }

            var dependantTestProjectPaths = dependantTestProjects.Select(p => p.FilePath).ToList();
            var dependantTestProjectStrings = dependantTestProjects.OrderBy(p => p.Distance).ThenBy(p => p.Project).Select(p => $"Distance: {p.Distance,3} Project: {p.Project}");
            string dependantTestProjectsOutput = $"      {string.Join($"{Environment.NewLine}      ", dependantTestProjectStrings)}";
            Console.WriteLine();
            Console.WriteLine("Found dependant test projects");
            Console.WriteLine(dependantTestProjectsOutput);

            _writer.WriteProject(_settings.Output, dependantTestProjectPaths);
            Console.WriteLine();
            Console.WriteLine($"Wrote {dependantTestProjectPaths.Count} test assemblies to {_settings.Output}");

            return 0;
        }
    }
}
