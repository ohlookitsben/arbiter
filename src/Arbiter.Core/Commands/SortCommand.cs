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

        public SortCommand(IMSBuildSolutionAnalyzer analyzer, IConsole console) : base("sort", "Output a topological sort of the projects in a solution.")
        {
            AddOption(CommonOptions.Solution);

            Handler = CommandHandler.Create<FileInfo, CancellationToken>(ExecuteHandler);
            _analyzer = analyzer;
            _console = console;
        }

        public async Task<int> ExecuteHandler(FileInfo solution, CancellationToken token)
        {
            await _analyzer.LoadSolution(solution.FullName, token);
            var projects = _analyzer.GetTopologicallySortedProjects();

            foreach (var project in projects)
            {
                _console.Out.WriteLine($"{project.Distance}\t{project.Project}");
            }

            return 0;
        }
    }
}
