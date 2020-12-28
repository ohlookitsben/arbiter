namespace Arbiter.Core.Commands
{
    public class ChildCommands
    {
        public DiffCommand Diff { get; }
        public SetupComCommand SetupCom { get; }
        public CleanCommand CleanCom { get; }
        public SortCommand Sort { get; }
        public GraphCommand Graph { get; }
        public AnalyzeCommand Analyze { get; }

        public ChildCommands(DiffCommand diff, SetupComCommand setupCom, CleanCommand cleanCom, SortCommand sort, GraphCommand graph, AnalyzeCommand analyze)
        {
            Diff = diff;
            SetupCom = setupCom;
            CleanCom = cleanCom;
            Sort = sort;
            Graph = graph;
            Analyze = analyze;
        }
    }
}
