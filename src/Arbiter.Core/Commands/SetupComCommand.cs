using Arbiter.Core.Analysis;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace Arbiter.Core.Commands
{
    public class SetupComCommand : Command
    {
        private readonly IMSBuildLocator _locator;

        public SetupComCommand(IMSBuildLocator locator) : base("setup-com", "Setup support for projects with COM references")
        {
            _locator = locator;

            Handler = CommandHandler.Create<bool>(ExecuteHandler);
        }

        public int ExecuteHandler(bool verbose)
        {
            Globals.Verbose = verbose;

            _locator.SetupCom();

            return 0;
        }
    }
}
