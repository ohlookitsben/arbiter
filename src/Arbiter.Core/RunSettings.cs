using System;

namespace Arbiter.Core
{
    public class RunSettings
    {
        public RunSettings(string[] args)
        {
            if (args.Length != 5)
            {
                throw new ArgumentException("5 arguments are required to construct RunSettings");
            }

            Solution = args[0];
            FromCommit = args[1];
            ToCommit = args[2];
            Project = args[3];
            ProjectConfiguration = args[4];
        }

        /// <summary>
        /// Relative path of the solution file to analyze.
        /// </summary>
        public string Solution { get; set; }

        /// <summary>
        /// Start of the commit range to analyze.
        /// </summary>
        public string FromCommit { get; set; }

        /// <summary>
        /// End of the commit range to analyze.
        /// </summary>
        public string ToCommit { get; set; }

        /// <summary>
        /// Relative path of the NUnit project file containing all test assemblies that could be run.
        /// </summary>
        public string Project { get; set; }

        /// <summary>
        /// Configuration within the NUnit project file containing all test assemblies that could be run.
        /// </summary>
        public string ProjectConfiguration { get; set; }

        /// <summary>
        /// Relative path of the output NUnit project file. Will be called arbiter.nunit.
        /// </summary>
        public string Output { get; set; }

        /// <summary>
        /// Working directory of Arbiter.
        /// </summary>
        public string WorkingDirectory { get; set; }
    }
}
