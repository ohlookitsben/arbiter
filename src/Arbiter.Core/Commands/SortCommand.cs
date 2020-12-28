using Arbiter.Core.Analysis;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Arbiter.Core.Commands
{
    public class SortCommand : Command
    {
        private readonly IMSBuildSolutionAnalyzer _analyzer;
        private readonly IConsole _console;

        public SortCommand(IMSBuildSolutionAnalyzer analyzer, IConsole console) : base("sort", "Output a topological sort of the projects in a solution")
        {
            _analyzer = analyzer;
            _console = console;

            AddOption(CommonOptions.Solution);

            Handler = CommandHandler.Create<FileInfo, bool, CancellationToken>(ExecuteHandler);
        }

        public async Task<int> ExecuteHandler(FileInfo solution, bool verbose, CancellationToken token)
        {
            Globals.Verbose = verbose;

            await _analyzer.LoadSolution(solution.FullName, token);
            _console.Out.VerboseWriteLine();

            var projects = _analyzer.GetTopologicallySortedProjects();

            foreach (var project in projects)
            {
                _console.Out.VerboseWrite($"{project.Distance}\t");
                _console.Out.WriteLine(project.Project);
            }

            return 0;
        }
    }
}
