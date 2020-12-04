using System.Collections.Generic;

namespace Arbiter.Core.Analysis
{
    public class AnalysisResult
    {
        public string Project { get; set; }
        public int Distance { get; set; }
    }

    public class MSBuildSolutionAnalyzer
    {
        public List<string> FindContainingProjects(IEnumerable<string> files) => new List<string>();

        public List<AnalysisResult> FindDependantProjects(IEnumerable<string> projects) => new List<AnalysisResult>();
    }
}
