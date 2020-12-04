using Arbiter.Core.Commands;
using System.IO;

namespace Arbiter.Core
{
    public class CommandBuilder
    {
        private readonly IRepositoryReader _repositoryReader;
        private readonly IFileReader _fileReader;

        public CommandBuilder(IRepositoryReader repositoryReader, IFileReader fileReader)
        {
            _repositoryReader = repositoryReader;
            _fileReader = fileReader;
        }

        public ICommand ProcessArguments(string[] args)
        {
            if (args.Length != 4)
            {
                return new UsageCommand();
            }

            if (!_repositoryReader.GitExists())
            {
                return new PrintMessageCommand
                {
                    Message = $"No git executable could be found"
                };
            }

            var settings = new RunSettings(args);
            if (!_fileReader.Exists(settings.Solution))
            {
                return new PrintMessageCommand
                {
                    Message = $"No solution file could be found at: {settings.Solution}"
                };
            }

            settings.WorkingDirectory = _fileReader.GetDirectory(settings.Solution);
            _repositoryReader.WorkingDirectory = settings.WorkingDirectory;
            if (!_repositoryReader.RepositoryExists())
            {
                return new PrintMessageCommand
                {
                    Message = $"No repository could be found at: {settings.WorkingDirectory}"
                };
            }

            if (!_repositoryReader.CommitExists(settings.FromCommit))
            {
                return new PrintMessageCommand
                {
                    Message = $"No commit could be found: {settings.FromCommit}"
                };
            }

            if (!_repositoryReader.CommitExists(settings.ToCommit))
            {
                return new PrintMessageCommand
                {
                    Message = $"No commit could be found: {settings.ToCommit}"
                };
            }

            if (!_repositoryReader.CommitIsAncestor(settings.ToCommit, settings.FromCommit))
            {
                return new PrintMessageCommand
                {
                    Message = $"The commit {settings.FromCommit} is not an ancestor of {settings.ToCommit}"
                };
            }

            return new BuildOutputCommand(settings, new RepositoryReader(new PowerShellInvoker()));
        }
    }
}
