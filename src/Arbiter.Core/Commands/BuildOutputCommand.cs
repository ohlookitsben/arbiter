using System;
using System.Collections.Generic;
using System.Text;

namespace Arbiter.Core.Commands
{
    public class BuildOutputCommand : ICommand
    {
        private readonly RunSettings _settings;
        private readonly IRepositoryReader _repositoryReader;

        public BuildOutputCommand(RunSettings settings, IRepositoryReader repositoryReader)
        {
            _settings = settings;
            _repositoryReader = repositoryReader;
            _repositoryReader.WorkingDirectory = _settings.WorkingDirectory;
        }

        public int Execute()
        {
            var changedFiles = _repositoryReader.ListChangedFiles(_settings.FromCommit, _settings.ToCommit);

            return 0;
        }
    }
}
