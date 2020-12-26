using Microsoft.CodeAnalysis;
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
        /// Open a solution file and all referenced projects.
        /// </summary>
        Task LoadSolution(string solution, CancellationToken token);
    }
}