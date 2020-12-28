using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;

namespace Arbiter.Core.Commands
{
    public class DiffCommand : Command
    {
        private readonly IRepositoryReader _repositoryReader;
        private readonly IConsole _console;

        public List<string> Result { get; } = new List<string>();

        public DiffCommand(IRepositoryReader repositoryReader, IConsole console) : base("diff", "Get the changed files between two commits")
        {
            _repositoryReader = repositoryReader;
            _console = console;

            AddOption(CommonOptions.FromCommit);
            AddOption(CommonOptions.ToCommit);

            Handler = CommandHandler.Create<string, string>((fromCommit, toCommit) => ExecuteHandler(fromCommit, toCommit));
        }

        public int ExecuteHandler(string fromCommit, string toCommit, bool suppressOutput = false)
        {
            var changedFiles = _repositoryReader.ListChangedFiles(fromCommit, toCommit);
            Result.AddRange(changedFiles);

            if (!suppressOutput || Globals.Verbose)
            {
                _console.Out.WriteLine(string.Join(Environment.NewLine, changedFiles));
            }

            return 0;
        }
    }
}
