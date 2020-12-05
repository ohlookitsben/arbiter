using System.Collections.Generic;

namespace Arbiter.Core.Analysis
{
    public interface IMSBuildSolutionAnalyzer
    {
        void LoadSolution(string solution);
        List<string> FindContainingProjects(IEnumerable<string> files);
        List<AnalysisResult> FindDependantProjects(IEnumerable<string> projects);
        List<AnalysisResult> ExcludeNonTestProjects(List<AnalysisResult> dependantProjects);
    }
}
