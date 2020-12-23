namespace Arbiter.Core.Commands
{
    public class ChildCommands
    {
        public DiffCommand Diff { get; }
        public SetupComCommand SetupCom { get; }
        public CleanComCommand CleanCom { get; }
        public SortCommand Sort { get; }
        public GraphCommand Graph { get; }

        public ChildCommands(DiffCommand diff, SetupComCommand setupCom, CleanComCommand cleanCom, SortCommand sort, GraphCommand graph)
        {
            Diff = diff;
            SetupCom = setupCom;
            CleanCom = cleanCom;
            Sort = sort;
            Graph = graph;
        }
    }
}
