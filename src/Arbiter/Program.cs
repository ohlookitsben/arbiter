using Arbiter.Core;
using Arbiter.Core.Commands;
using System;

namespace Arbiter
{
    class Program
    {
        static int Main(string[] args)
        {
            ICommand command;
            try
            {
                var builder = new CommandBuilder(new RepositoryReader(new PowerShellInvoker()), new FileReader());
                command = builder.ProcessArguments(args);
            }
            catch (Exception e)
            {
                Console.WriteLine("Fatal Exception:");
                Console.WriteLine(e);
                return -1;
            }

            return command.Execute();
        }
    }
}
