using System.Collections.Generic;
using System.IO.Abstractions;
using System.Xml;

namespace Arbiter.Core
{
    public class NUnitProjectReader
    {
        private readonly IFileSystem _fileSystem;

        public NUnitProjectReader(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public List<string> ReadProject(string path)
        {
            var document = new XmlDocument();
            document.LoadXml(_fileSystem.File.ReadAllText(path));

            var settings = document.SelectSingleNode("//Settings");
            string activeConfig = settings.Attributes["activeConfig"].Value.ToString();
            var config = document.SelectSingleNode($"//Config[@name='{activeConfig}']");
            var assemblies = config.SelectNodes("//assembly");
            var paths = new List<string>();
            foreach (XmlNode assembly in assemblies)
            {
                paths.Add(assembly.Attributes["path"].Value.ToString());
            }

            return paths;
        }
    }
}
