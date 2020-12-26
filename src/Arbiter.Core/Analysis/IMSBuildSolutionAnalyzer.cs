using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Arbiter.Core.Analysis
{
    public interface IMSBuildSolutionAnalyzer
    {
        List<string> FindContainingProjects(IEnumerable<string> files);
        List<AnalysisResult> FindDependentProjects(IEnumerable<string> projects);
        List<AnalysisResult> ExcludeNonTestProjects(IEnumerable<AnalysisResult> dependentProjects);

        /// <summary>
        /// Returns all the projects for the solution in a topologically sorted order with respect to their dependencies. Projects that 
        /// depend on other projects will always show up later in this sequence than the projects they depend on.
        /// </summary>
        List<AnalysisResult> GetTopologicallySortedProjects();

        /// <summary>
        /// Returns a graph of node -> descendant nodes based on the solution analysis.
        /// </summary>
        List<Tuple<AnalysisResult, List<AnalysisResult>>> GetGraph();

        /// <summary>
        /// Open a solution file and all referenced projects.
        /// </summary>
        Task LoadSolution(string fullName, CancellationToken token);
    }
}
