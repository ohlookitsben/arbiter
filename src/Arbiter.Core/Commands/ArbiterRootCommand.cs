using Arbiter.Core.Analysis;
using Arbiter.Core.Commands;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Arbiter.Core
{
    public class ArbiterRootCommand : RootCommand
    {
        private readonly IMSBuildSolutionAnalyzer _analyzer;
        private readonly IConsole _console;
        private readonly ChildCommands _childCommands;

        public ArbiterRootCommand(IMSBuildSolutionAnalyzer analyzer, IConsole console, ChildCommands childCommands) : base("Only run the tests you need based on the changes between builds")
        {
            _analyzer = analyzer;
            _console = console;
            _childCommands = childCommands;

            AddOption(CommonOptions.Solution);
            AddOption(CommonOptions.FromCommit);
            AddOption(CommonOptions.Distance);
            AddGlobalOption(new Option<bool>("--verbose", "Write additional debugging output"));

            Handler = CommandHandler.Create<FileInfo, string, bool, bool, CancellationToken>(ExecuteHandler);

            AddCommand(_childCommands.CleanCom);
            AddCommand(_childCommands.SetupCom);
            AddCommand(_childCommands.Diff);
            AddCommand(_childCommands.Sort);
            AddCommand(_childCommands.Graph);
            AddCommand(_childCommands.Analyze);
        }

        public async Task<int> ExecuteHandler(FileInfo solution, string fromCommit, bool verbose, bool distance, CancellationToken token)
        {
            Globals.Verbose = verbose;
            Globals.PrintDistance = distance;

            await _childCommands.Analyze.ExecuteHandler(solution, fromCommit, verbose, distance, token, true);
            var dependentProjects = _childCommands.Analyze.Results;
            _console.Out.VerboseWriteLine();

            var dependentTestProjects = _analyzer.ExcludeNonTestProjects(dependentProjects);
            if (!dependentTestProjects.Any())
            {
                _console.Out.VerboseWriteLine("O dependent test projects found. No output will be written");
                return 0;
            }

            _console.Out.VerboseWriteLine($"{dependentTestProjects.Count} test projects found");
            _console.Out.VerboseWriteLine();

            foreach (var dependentTestProject in dependentTestProjects)
            {
                if (Globals.PrintDistance)
                {
                    _console.Out.Write($"{dependentTestProject.Distance}\t");
                }

                _console.Out.WriteLine(dependentTestProject.Assembly);
            }

            return 0;
        }
    }
}
