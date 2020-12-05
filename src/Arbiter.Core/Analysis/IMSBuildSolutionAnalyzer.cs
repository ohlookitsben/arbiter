using System;
using System.Collections.Generic;
using System.Text;

namespace Arbiter.Core.Analysis
{
    public interface IMSBuildSolutionAnalyzer
    {
        void LoadSolution(string solution);
        List<string> FindContainingProjects(IEnumerable<string> files);
        List<AnalysisResult> FindDependantProjects(IEnumerable<string> projects);
    }
}
