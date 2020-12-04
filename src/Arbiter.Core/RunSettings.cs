using System;
using System.Collections.Generic;
using System.Text;

namespace Arbiter.Core
{
    public class RunSettings
    {
        public RunSettings(string[] args)
        {
            if (args.Length != 4)
            {
                throw new ArgumentException("4 arguments are required to construct RunSettings");
            }

            Solution = args[0];
            FromCommit = args[1];
            ToCommit = args[2];
            Output = args[3];
        }

        public string Solution { get; set; }
        public string FromCommit { get; set; }
        public string ToCommit { get; set; }
        public string Output { get; set; }
        public string WorkingDirectory { get; set; }
    }
}
