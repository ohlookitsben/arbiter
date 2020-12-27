using System;
using System.Diagnostics;

namespace Arbiter.Core.Analysis
{
    [DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
    public class AnalysisResult : IEquatable<AnalysisResult>
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

        private string GetDebuggerDisplay() => $"Project: {Project}, Distance: {Distance}";

        public override string ToString() => GetDebuggerDisplay();

        public override bool Equals(object? obj) => Equals(obj as AnalysisResult);

        public bool Equals(AnalysisResult? other)
        {
            return other != null && FilePath.Equals(other.FilePath, StringComparison.OrdinalIgnoreCase) && Distance == other.Distance;
        }

        public override int GetHashCode() => FilePath.GetHashCode() ^ Distance.GetHashCode();
    }
}
