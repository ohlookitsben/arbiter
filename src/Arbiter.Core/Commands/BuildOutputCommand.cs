using Arbiter.Core.Analysis;
using Arbiter.Core.Output;
using System.Linq;

namespace Arbiter.Core.Commands
{
    public class BuildOutputCommand : ICommand
    {
        private readonly RunSettings _settings;
        private readonly IRepositoryReader _repositoryReader;
        private readonly IMSBuildLocator _locator;
        private readonly IMSBuildSolutionAnalyzer _analyzer;
        private readonly NUnitProjectWriter _writer;

        public BuildOutputCommand(RunSettings settings, IRepositoryReader repositoryReader, IMSBuildLocator locator, IMSBuildSolutionAnalyzer analyzer, NUnitProjectWriter writer)
        {
            _settings = settings;
            _repositoryReader = repositoryReader;
            _locator = locator;
            _repositoryReader.WorkingDirectory = _settings.WorkingDirectory;
            _analyzer = analyzer;
            _writer = writer;
        }

        public int Execute()
        {
            var changedFiles = _repositoryReader.ListChangedFiles(_settings.FromCommit, _settings.ToCommit);

            // Locate the correct MSBuild assemblies. This must occur before the solution can be loaded.
            _locator.RegisterDefaults();

            _analyzer.LoadSolution(_settings.Solution);
            var changedProjects = _analyzer.FindContainingProjects(changedFiles);
            var dependantProjects = _analyzer.FindDependantProjects(changedProjects);
            var dependantProjectPaths = dependantProjects.Select(p => p.Project).ToList();
            _writer.WriteProject(_settings.Output, dependantProjectPaths);

            return 0;
        }
    }
}
