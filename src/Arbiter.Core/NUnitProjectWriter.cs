using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
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

        public void WriteProject(string path, IEnumerable<string> sourceAssemblyPaths, IEnumerable<string> dependentAssemblies)
        {
            var assemblyPaths = sourceAssemblyPaths.Where(a => dependentAssemblies.Any(d => a.EndsWith(d)));
            var assemblyTags = assemblyPaths.Select(a => string.Format(_assemblyTemplate, a));
            string assemblyBlock = string.Join($"{Environment.NewLine}        ", assemblyTags);
            string project = string.Format(_projectTemplate, assemblyBlock);

            if (_fileSystem.File.Exists(path))
            {
                _fileSystem.File.Delete(path);
            }

            _fileSystem.File.WriteAllText(path, project);
        }
    }
}
