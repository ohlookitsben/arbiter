namespace Arbiter.Core.Analysis
{
    public interface IMSBuildLocator
    {
        void RegisterDefaults();
        void SetupCom();
        void Clean();
    }
}