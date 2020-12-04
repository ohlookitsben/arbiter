using Arbiter.Core.Commands;
using System.IO;

namespace Arbiter.Core
{
    public class CommandBuilder
    {
        private readonly RepositoryReader _repositoryReader;
        private readonly IFileReader _fileReader;

        public CommandBuilder(RepositoryReader repositoryReader, IFileReader fileReader)
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

            var settings = new RunSettings(args);
            if (!_fileReader.Exists(settings.Solution))
            {
                return new SolutionNotFoundCommand(settings.Solution);
            }

            if (!_repositoryReader.CommitExists(settings.FromCommit))
            {
                return new CommitNotFoundCommand(settings.FromCommit);
            }

            if (!_repositoryReader.CommitExists(settings.ToCommit))
            {
                return new CommitNotFoundCommand(settings.ToCommit);
            }

            return new BuildOutputCommand(settings);
        }
    }
}
