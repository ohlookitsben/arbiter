using System.Diagnostics;

namespace Arbiter.Core.Analysis
{
    [DebuggerDisplay("Project: {Project}, Distance: {Distance}")]
    public class AnalysisResult
    {
        public string Project { get; }
        public string FilePath { get; }
        public int Distance { get; }
        public string Assembly { get; }

        public AnalysisResult(string project, string filePath, int distance, string assembly)
        {
            Project = project;
            FilePath = filePath;
            Distance = distance;
            Assembly = assembly;
        }
    }
}
