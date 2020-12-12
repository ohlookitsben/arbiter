using System.Collections.Generic;

namespace Arbiter.Core.Analysis
{
    public interface IMSBuildSolutionAnalyzer
    {
        void LoadSolution(string solution);
        List<string> FindContainingProjects(IEnumerable<string> files);
        List<AnalysisResult> FindDependentProjects(IEnumerable<string> projects);
        List<AnalysisResult> ExcludeNonTestProjects(IEnumerable<AnalysisResult> dependentProjects);
    }
}
