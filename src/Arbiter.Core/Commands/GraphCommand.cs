using Arbiter.Core.Analysis;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Arbiter.Core.Commands
{
    public class GraphCommand : Command
    {
        private readonly IMSBuildSolutionAnalyzer _analyzer;
        private readonly IConsole _console;

        public GraphCommand(IMSBuildSolutionAnalyzer analyzer, IConsole console) : base("graph", "Ouput a graph in dot format")
        {
            _analyzer = analyzer;
            _console = console;

            AddOption(CommonOptions.Solution);

            Handler = CommandHandler.Create<FileInfo, bool, CancellationToken>(ExecuteHandler);
        }

        // Graph Viewer: https://dreampuf.github.io/GraphvizOnline
        public async Task<int> ExecuteHandler(FileInfo solution, bool verbose, CancellationToken token)
        {
            Globals.Verbose = verbose;

            await _analyzer.LoadSolution(solution.FullName, token);
            _console.Out.VerboseWriteLine();

            var graph = _analyzer.GetGraph();

            string graphString = $"digraph G {{{Environment.NewLine}" + string.Join(Environment.NewLine, graph.Where(g => g.Item2.Any()).Select(g => $"  \"{g.Item1.Project}\" -> {{ {string.Join(" ", g.Item2.Select(i => $"\"{i.Project}\""))} }}")) + $"{Environment.NewLine}}}";
            _console.Out.WriteLine(graphString);

            return 0;
        }
    }
}
