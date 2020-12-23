using Arbiter.Core.Analysis;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace Arbiter.Core.Commands
{
    public class SetupComCommand : Command
    {
        public SetupComCommand(IMSBuildLocator locator) : base("setup-com", "Setup support for projects with COM references.")
        {
            Handler = CommandHandler.Create(locator.SetupCom);
        }
    }
}
