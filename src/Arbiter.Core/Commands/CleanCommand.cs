using Arbiter.Core.Analysis;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace Arbiter.Core.Commands
{
    public class CleanCommand : Command
    {
        private readonly IMSBuildLocator _locator;

        public CleanCommand(IMSBuildLocator locator) : base("clean", "Return to a clean state without support for additional project types")
        {
            _locator = locator;

            Handler = CommandHandler.Create<bool>(ExecuteHandler);
        }

        public int ExecuteHandler(bool verbose)
        {
            Globals.Verbose = verbose;

            _locator.Clean();

            return 0;
        }
    }
}
