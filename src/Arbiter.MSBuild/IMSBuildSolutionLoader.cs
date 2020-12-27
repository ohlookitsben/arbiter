using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Arbiter.MSBuild
{
    public interface IMSBuildSolutionLoader
    {
        /// <summary>
        /// The opened solution.
        /// </summary>
        /// <exception cref="InvalidOperationException">The solution has not yet been loaded.</exception>"
        Solution Solution { get; }

        /// <summary>
        /// The C++ projects in the opened solution.
        /// </summary>
        List<CppProject> CppProjects { get; }

        /// <summary>
        /// Open a solution file and all referenced projects.
        /// </summary>
        Task LoadSolution(string solution, CancellationToken token);
    }
}