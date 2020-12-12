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
        private readonly NUnitProjectReader _reader;

        public BuildOutputCommand(RunSettings settings, IRepositoryReader repositoryReader, IMSBuildSolutionAnalyzer analyzer, NUnitProjectWriter writer, NUnitProjectReader reader)
        {
            _settings = settings;
            _repositoryReader = repositoryReader;
            _repositoryReader.WorkingDirectory = _settings.WorkingDirectory;
            _analyzer = analyzer;
            _writer = writer;
            _reader = reader;
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

            var dependentProjects = _analyzer.FindDependentProjects(changedProjects);
            var dependentTestProjects = _analyzer.ExcludeNonTestProjects(dependentProjects);
            if (!dependentProjects.Any())
            {
                Console.WriteLine("O dependent test projects found. No output will be written.");
                return 0;
            }

            var depdendentTestProjectAssemblyNames = dependentTestProjects.Select(p => p.Assembly).ToList();
            var dependentTestProjectStrings = dependentTestProjects.OrderBy(p => p.Distance).ThenBy(p => p.Project).Select(p => $"Distance: {p.Distance,3} Project: {p.Project}");
            string dependentTestProjectsOutput = $"      {string.Join($"{Environment.NewLine}      ", dependentTestProjectStrings)}";
            Console.WriteLine();
            Console.WriteLine("Found dependent test projects");
            Console.WriteLine(dependentTestProjectsOutput);

            var sourceTestProjectAssemblies = _reader.ReadProject(_settings.Project);

            _writer.WriteProject(_settings.Output, sourceTestProjectAssemblies, depdendentTestProjectAssemblyNames);
            Console.WriteLine();
            Console.WriteLine($"Wrote {depdendentTestProjectAssemblyNames.Count} test assemblies to {_settings.Output}");

            return 0;
        }
    }
}
