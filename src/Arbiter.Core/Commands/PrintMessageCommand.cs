using System;

namespace Arbiter.Core.Commands
{
    public class PrintMessageCommand : ICommand
    {
        public string Message { get; }

        public PrintMessageCommand(string message)
        {
            Message = message;
        }

        public int Execute()
        {
            Console.WriteLine();
            Console.WriteLine(Message);

            return 0;
        }
    }
}
