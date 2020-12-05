using System;
using System.Collections.Generic;
using System.Linq;

namespace Arbiter.Core
{
    public class NUnitProjectWriter
    {
        private const string _projectTemplate = @"<NUnitProject>
    <Settings activeConfig=""Arbiter"" />
    <Config name=""Arbiter"">
        {0}
    </Config>
</NUnitProject>
";

        private const string _assemblyTemplate = @"<assembly path=""{0}"" />";

        private readonly IFileSystem _fileSystem;

        public NUnitProjectWriter(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public void WriteProject(string path, IEnumerable<string> assemblies)
        {
            var assemblyTags = assemblies.Select(a => string.Format(_assemblyTemplate, a));
            string assemblyBlock = string.Join($"{Environment.NewLine}        ", assemblyTags);
            string project = string.Format(_projectTemplate, assemblyBlock);

            _fileSystem.WriteFile(path, project);
        }
    }
}
