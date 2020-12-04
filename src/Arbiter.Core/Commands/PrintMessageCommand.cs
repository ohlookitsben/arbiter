using System;
using System.Collections.Generic;
using System.Text;

namespace Arbiter.Core.Commands
{
    public class PrintMessageCommand : ICommand
    {
        public string Message { get; set; }

        public int Execute()
        {
            Console.WriteLine();
            Console.WriteLine(Message);

            return 0;
        }
    }
}
