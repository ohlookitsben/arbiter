using Arbiter.Core.Analysis;
using Microsoft.Build.Locator;

namespace Arbiter.MSBuild
{
    public class ArbiterMSBuildLocator : IMSBuildLocator
    {
        public void RegisterDefaults()
        {
            MSBuildLocator.RegisterDefaults();
        }
    }
}
