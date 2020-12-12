using Arbiter.Core.Commands;

namespace Arbiter.Core
{
    public class CommandBuilder
    {
        private readonly IRepositoryReader _repositoryReader;
        private readonly CommandFactory _factory;
        private readonly IFileSystem _fileSystem;

        public CommandBuilder(IRepositoryReader repositoryReader, CommandFactory factory, IFileSystem fileReader)
        {
            _repositoryReader = repositoryReader;
            _factory = factory;
            _fileSystem = fileReader;
        }

        public ICommand ProcessArguments(string[] args)
        {
            if (args.Length != 5)
            {
                return _factory.CreateCommand<UsageCommand>();
            }

            var settings = new RunSettings(args);
            if (!_fileSystem.Exists(settings.Solution))
            {
                return _factory.CreatePrintMessageCommand($"No solution file could be found at: {settings.Solution}");
            }

            if (!_fileSystem.Exists(settings.Project))
            {
                return _factory.CreatePrintMessageCommand($"No project file could be found at: {settings.Project}");
            }

            settings.Output = _fileSystem.Combine(_fileSystem.GetDirectory(settings.Project), "arbiter.nunit");
            settings.WorkingDirectory = _fileSystem.GetDirectory(settings.Solution);
            _repositoryReader.WorkingDirectory = settings.WorkingDirectory;

            if (!_repositoryReader.ToolExists())
            {
                return _factory.CreatePrintMessageCommand($"No git executable could be found");
            }

            if (!_repositoryReader.RepositoryExists())
            {
                return _factory.CreatePrintMessageCommand($"No repository could be found at: {settings.WorkingDirectory}");
            }

            if (!_repositoryReader.CommitExists(settings.FromCommit))
            {
                return _factory.CreatePrintMessageCommand($"No commit could be found: {settings.FromCommit}");
            }

            if (!_repositoryReader.CommitExists(settings.ToCommit))
            {
                return _factory.CreatePrintMessageCommand($"No commit could be found: {settings.ToCommit}");
            }

            if (!_repositoryReader.CommitIsAncestor(settings.ToCommit, settings.FromCommit))
            {
                return _factory.CreatePrintMessageCommand($"The commit {settings.FromCommit} is not an ancestor of {settings.ToCommit}");
            }

            return _factory.CreateBuildOutputCommand(settings);
        }
    }
}
