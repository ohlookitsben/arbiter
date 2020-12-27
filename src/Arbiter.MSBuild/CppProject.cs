using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Arbiter.MSBuild
{
    public class CppProject
    {
        private Guid? _id;

        /// <summary>
        /// The project id in the solution file.
        /// </summary>
        /// <exception cref="InvalidOperationException">The id has not been set yet.</exception>
        public Guid Id => _id ?? throw new InvalidOperationException("Id has not been set. Call SetId before attempting to access the project id.");

        public string FilePath { get; private set; } = string.Empty;

        /// <summary>
        /// The name of the projeect. Matches the name of the project file, but without the extension.
        /// </summary>
        public string Name { get; private set; } = string.Empty;

        /// <summary>
        /// The name of the assembly. Assumed to be identical to <see cref="Name"/>.
        /// </summary>
        public string AssemblyName => Name;

        public List<string> DocumentPaths { get; } = new List<string>();

        public void Load(string filePath)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            var document = new XmlDocument();
            document.Load(filePath);
            var namespaceManager = new XmlNamespaceManager(document.NameTable);
            namespaceManager.AddNamespace("ms", "http://schemas.microsoft.com/developer/msbuild/2003");

            // Set project level properties.
            FilePath = filePath;
            Name = new FileInfo(filePath).Name;

            // Find included documents.
            string projectDirectory = new FileInfo(filePath).DirectoryName;
            var includes = document.SelectNodes("//ms:ItemGroup/ms:ClInclude", namespaceManager);
            foreach (XmlNode include in includes)
            {
                AddAbsolutePath(include, projectDirectory);
            }

            var sources = document.SelectNodes("//ms:ItemGroup/ms:ClCompile", namespaceManager);
            foreach (XmlNode resource in sources)
            {
                AddAbsolutePath(resource, projectDirectory);
            }

            var resources = document.SelectNodes("//ms:ItemGroup/ms:ResourceCompile", namespaceManager);
            foreach (XmlNode resource in resources)
            {
                AddAbsolutePath(resource, projectDirectory);
            }

            var images = document.SelectNodes("//ms:ItemGroup/ms:Image", namespaceManager);
            foreach (XmlNode image in images)
            {
                AddAbsolutePath(image, projectDirectory);
            }

            // TODO: Do we also need to load references from <ItemGroup> <Reference Include="Foo.dll" /> </ItemGroup>?
        }

        public void SetId(Guid id)
        {
            _id = id;
        }

        private void AddAbsolutePath(XmlNode itemNode, string projectDirectory)
        {
            string relativePath = itemNode.Attributes["Include"].Value;
            DocumentPaths.Add(Path.Combine(projectDirectory, relativePath));
        }
    }
}
