using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Arbiter.Core.Output
{
    public class NUnitProjectWriter
    {
        private const string ProjectTemplate = @"<NUnitProject>
    <Settings activeConfig=""Arbiter"" />
    <Config name=""Arbiter"">
    {0}
    </Config>
</NUnitProject>
";

        private const string AssemblyTemplate = @"<assembly path=""{0}"" />";

        private readonly IFileSystem _fileSystem;

        public NUnitProjectWriter(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public void WriteProject(string path, IEnumerable<string> assemblies)
        {
            var assemblyTags = assemblies.Select(a => string.Format(AssemblyTemplate, a));
            string assemblyBlock = string.Join($"    {Environment.NewLine}", assemblyTags);
            string project = string.Format(ProjectTemplate, assemblyBlock);

            _fileSystem.WriteFile(path, FileMode.Truncate, project);
        }
    }
}
