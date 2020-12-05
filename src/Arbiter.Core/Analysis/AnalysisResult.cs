using System.Diagnostics;

namespace Arbiter.Core.Analysis
{
    [DebuggerDisplay("Project: {Project}, Distance: {Distance}")]
    public class AnalysisResult
    {
        public string Project { get; set; }
        public string FilePath { get; set; }
        public string Assembly { get; set; }
        public int Distance { get; set; }
    }
}
