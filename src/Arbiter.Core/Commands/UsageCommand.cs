using System;

namespace Arbiter.Core.Commands
{
    public class UsageCommand : ICommand
    {
        public int Execute()
        {
            Console.WriteLine();
            Console.WriteLine("Usage: arbiter [solution] [from_commit] [to_commit] [output]");
            Console.WriteLine();
            WriteOptionHelp("solution", "The path to the solution (.sln) file to evaluate.");
            WriteOptionHelp("from_commit", "The start of the commit range to evaluate.");
            WriteOptionHelp("to_commit", "The end of the commit range to evaluate.");
            WriteOptionHelp("output", "The path to write the output file.");

            return 0;
        }

        private static void WriteOptionHelp(string option, string help)
        {
            Console.WriteLine($"{option}:");
            Console.WriteLine($"  {help}");
            Console.WriteLine();
        }
    }
}