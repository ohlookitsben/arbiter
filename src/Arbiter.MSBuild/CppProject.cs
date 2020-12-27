using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Arbiter.MSBuild
{
    public class CppProject
    {
        /// <summary>
        /// A fake version of <see cref="Project.Id"/>. Good for keying dictionaries but not much else.
        /// </summary>
        public Guid FakeId { get; } = Guid.NewGuid();

        public string FilePath { get; private set; } = string.Empty;

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

            FilePath = filePath;
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

        private void AddAbsolutePath(XmlNode itemNode, string projectDirectory)
        {
            string relativePath = itemNode.Attributes["Include"].Value;
            DocumentPaths.Add(Path.Combine(projectDirectory, relativePath));
        }
    }
}
