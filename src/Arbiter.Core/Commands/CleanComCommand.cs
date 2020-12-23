using Arbiter.Core.Analysis;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace Arbiter.Core.Commands
{
    public class CleanComCommand : Command
    {
        public CleanComCommand(IMSBuildLocator locator) : base("clean-com", "Return to a clean state without support for additional project types.")
        {
            Handler = CommandHandler.Create(locator.SetupCom);
        }
    }
}
