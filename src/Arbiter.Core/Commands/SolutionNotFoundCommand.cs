using System;

namespace Arbiter.Core.Commands
{
    public class SolutionNotFoundCommand : ICommand
    {
        private readonly string _solution;

        public SolutionNotFoundCommand(string solution)
        {
            _solution = solution;
        }

        public int Execute()
        {
            Console.WriteLine();
            Console.WriteLine($"No solution file could be found at: {_solution}");

            return 0;
        }
    }
}