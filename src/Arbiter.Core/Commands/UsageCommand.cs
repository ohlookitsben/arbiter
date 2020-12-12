using System;

namespace Arbiter.Core.Commands
{
    public class UsageCommand : ICommand
    {
        public int Execute()
        {
            Console.WriteLine();
            Console.WriteLine("Usage: arbiter [solution] [from_commit] [to_commit] [input_nunit_project] [input_nunit_configuration]");
            Console.WriteLine();
            WriteOptionHelp("solution", "The path to the solution (.sln) file to evaluate.");
            WriteOptionHelp("from_commit", "The start of the commit range to evaluate.");
            WriteOptionHelp("to_commit", "The end of the commit range to evaluate.");
            WriteOptionHelp("input_nunit_project", "The nunit project file containing all test assemblies to consider.");
            WriteOptionHelp("input_nunit_configuration", "The configuration in the project to consider.");

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