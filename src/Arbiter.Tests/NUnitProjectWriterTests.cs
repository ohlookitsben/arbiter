using Arbiter.Core;
using Arbiter.Tests.Helpers;
using Moq;
using NUnit.Framework;
using System;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Xml;

namespace Arbiter.Tests.Unit
{
    [TestFixture]
    [Category(TestCategory.Small)]
    public class NUnitProjectWriterTests
    {
        private MockFileSystem _fileSystem;
        private NUnitProjectWriter _writer;
        private readonly string _projectPath = @"C:\build\arbiter.nunit";

        [SetUp]
        public void SetUp()
        {
            _fileSystem = new MockFileSystem();
            _writer = new NUnitProjectWriter(_fileSystem);

            _fileSystem.AddDirectory(@"C:\build");
        }

        [Test]
        public void WriteProject_AnyInput_GeneratesNonEmptyProject()
        {
            _writer.WriteProject(_projectPath, Array.Empty<string>(), Array.Empty<string>());

            Assert.IsTrue(_fileSystem.File.Exists(_projectPath));
            string fileContents = _fileSystem.File.ReadAllText(_projectPath);
            Assert.IsNotEmpty(fileContents);

            var document = new XmlDocument();
            document.LoadXml(fileContents);
        }

        [Test]
        public void WriteProject_AnyInput_GeneratesValidProject()
        {
            _writer.WriteProject(_projectPath, Array.Empty<string>(), Array.Empty<string>());

            var document = new XmlDocument();
            string fileContents = _fileSystem.File.ReadAllText(_projectPath);
            document.LoadXml(fileContents);

            var root = document.SelectNodes("/NUnitProject");
            Assert.AreEqual(1, root.Count, "The project should have an NUnitProject node at the root");
            var settings = document.SelectNodes("/NUnitProject/Settings");
            Assert.AreEqual(1, settings.Count, "The project should have a Settings node under the NUnitProject node");
            var config = document.SelectNodes("/NUnitProject/Config");
            Assert.AreEqual(1, config.Count, "The project should have a Config node under the NUnitProject node");
        }

        [Test]
        public void WriteProject_AnyInput_GeneratesValidConfig()
        {
            _writer.WriteProject(_projectPath, Array.Empty<string>(), Array.Empty<string>());

            var document = new XmlDocument();
            string fileContents = _fileSystem.File.ReadAllText(_projectPath);
            document.LoadXml(fileContents);

            var settings = document.SelectSingleNode("//Settings");
            var activeConfig = settings.Attributes["activeConfig"];
            var config = document.SelectSingleNode("//Config");
            var configName = config.Attributes["name"];
            Assert.AreEqual(activeConfig.Value, configName.Value, "The active config and config name should match");
        }

        [Test]
        public void WriteProject_AnyInput_WritesToPath()
        {
            _writer.WriteProject(_projectPath, Array.Empty<string>(), Array.Empty<string>());

            Assert.IsTrue(_fileSystem.File.Exists(_projectPath));
        }

        [Test]
        public void WriteProject_HasAssemblies_WritesEachAssembly([Range(0, 3)] int assemblyCount)
        {
            var assemblies = Enumerable.Range(0, assemblyCount).Select(_ => $"{Guid.NewGuid()}.dll").ToList();

            _writer.WriteProject(_projectPath, assemblies, assemblies);

            var document = new XmlDocument();
            string fileContents = _fileSystem.File.ReadAllText(_projectPath);
            document.LoadXml(fileContents);

            var assemblyNodes = document.SelectNodes("//assembly");
            Assert.AreEqual(assemblies.Count, assemblyNodes.Count, "An assembly node should exist for each assembly");
            var assemblyPaths = assemblyNodes.Cast<XmlNode>().Select(n => n.Attributes["path"].Value);
            CollectionAssert.AreEquivalent(assemblies, assemblyPaths);
        }
    }
}
