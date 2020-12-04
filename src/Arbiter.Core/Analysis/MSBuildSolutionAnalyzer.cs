using System;
using System.Collections.Generic;
using System.Text;

namespace Arbiter.Core.Analysis
{
    public class AnalysisResult
    {
        public string Project { get; set; }
        public int Distance { get; set; }
    }

    public class MSBuildSolutionAnalyzer
    {
        public List<string> FindContainingProjects(IEnumerable<string> files)
        {
            return new List<string>();
        }

        public List<AnalysisResult> FindDependantProjects(IEnumerable<string> projects)
        {
            return new List<AnalysisResult>();
        }
    }
}
