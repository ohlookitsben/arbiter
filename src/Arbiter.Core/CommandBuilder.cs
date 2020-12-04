using Arbiter.Core.Commands;

namespace Arbiter.Core
{
    public class CommandBuilder
    {
        private readonly IRepositoryReader _repositoryReader;
        private readonly CommandFactory _factory;
        private readonly IFileSystem _fileReader;

        public CommandBuilder(IRepositoryReader repositoryReader, CommandFactory factory, IFileSystem fileReader)
        {
            _repositoryReader = repositoryReader;
            _factory = factory;
            _fileReader = fileReader;
        }

        public ICommand ProcessArguments(string[] args)
        {
            if (args.Length != 4)
            {
                return _factory.CreateCommand<UsageCommand>();
            }

            if (!_repositoryReader.GitExists())
            {
                return _factory.CreatePrintMessageCommand($"No git executable could be found");
            }

            var settings = new RunSettings(args);
            if (!_fileReader.Exists(settings.Solution))
            {
                return _factory.CreatePrintMessageCommand($"No solution file could be found at: {settings.Solution}");
            }

            settings.WorkingDirectory = _fileReader.GetDirectory(settings.Solution);
            _repositoryReader.WorkingDirectory = settings.WorkingDirectory;
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
