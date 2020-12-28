using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Arbiter.Core.Analysis
{
    public interface IMSBuildSolutionAnalyzer
    {
        /// <summary>
        /// Find the projects containing the absolute paths <paramref name="files"/>.
        /// </summary>
        /// <returns>The absolute path to any project containing one of the <paramref name="files"/>.</returns>
        List<string> FindContainingProjects(IEnumerable<string> files);

        /// <summary>
        /// Find the projects that depend on the projects at the absolute paths <paramref name="projects"/>. This will traverse a simple
        /// C# -> C++ dependency, but not C++ -> C++ dependency chains. C# -> C# dependency chains are supported.
        /// </summary>
        List<AnalysisResult> FindDependentProjects(IEnumerable<string> projects);

        /// <summary>
        /// Exclude any projects from the <see cref="AnalysisResult"/>s that don't reference the test framework nunit.framework.dll.
        /// </summary>
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
