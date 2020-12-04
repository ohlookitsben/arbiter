using System;

namespace Arbiter.Core.Commands
{
    public class CommitNotFoundCommand : ICommand
    {
        private readonly string _commit;

        public CommitNotFoundCommand(string commit)
        {
            _commit = commit;
        }

        public int Execute()
        {
            Console.WriteLine();
            Console.WriteLine($"Commit not found: {_commit}");

            return 0;
        }
    }
}
