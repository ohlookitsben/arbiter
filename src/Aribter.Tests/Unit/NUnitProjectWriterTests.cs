using Arbiter.Core;
using Arbiter.Tests.Fakes;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Xml;

namespace Arbiter.Tests.Unit
{
    [TestFixture]
    [Category(TestCategory.Unit)]
    public class NUnitProjectWriterTests
    {
        private FakeFileSystem _fileSystem;
        private NUnitProjectWriter _writer;

        [SetUp]
        public void SetUp()
        {
            _fileSystem = new FakeFileSystem();
            _writer = new NUnitProjectWriter(_fileSystem);
        }

        [Test]
        public void WriteProject_AnyInput_GeneratesNonEmptyProject()
        {
            _writer.WriteProject("", Array.Empty<string>());

            Assert.IsNotNull(_fileSystem.File);
            Assert.IsNotEmpty(_fileSystem.File);

            var document = new XmlDocument();
            document.LoadXml(_fileSystem.File);
        }

        [Test]
        public void WriteProject_AnyInput_GeneratesValidProject()
        {
            _writer.WriteProject("", Array.Empty<string>());

            var document = new XmlDocument();
            document.LoadXml(_fileSystem.File);

            var root = document.SelectNodes("/NUnitProject");
            Assert.AreEqual(1, root.Count, "The project should have an NUnitProject node at the root.");
            var settings = document.SelectNodes("/NUnitProject/Settings");
            Assert.AreEqual(1, settings.Count, "The project should have a Settings node under the NUnitProject node.");
            var config = document.SelectNodes("/NUnitProject/Config");
            Assert.AreEqual(1, config.Count, "The project should have a Config node under the NUnitProject node.");
        }

        [Test]
        public void WriteProject_AnyInput_GeneratesValidConfig()
        {
            _writer.WriteProject("", Array.Empty<string>());

            var document = new XmlDocument();
            document.LoadXml(_fileSystem.File);

            var settings = document.SelectSingleNode("//Settings");
            var activeConfig = settings.Attributes["activeConfig"];
            var config = document.SelectSingleNode("//Config");
            var configName = config.Attributes["name"];
            Assert.AreEqual(activeConfig.Value, configName.Value, "The active config and config name should match.");
        }

        [Test]
        public void WriteProject_AnyInput_WritesToPath()
        {
            _writer.WriteProject("MyPath", Array.Empty<string>());

            Assert.AreEqual("MyPath", _fileSystem.FilePath);
        }

        [Test]
        public void WriteProject_HasAssemblies_WritesEachAssembly([Range(0, 3)] int assemblyCount)
        {
            var assemblies = Enumerable.Range(0, assemblyCount).Select(_ => Guid.NewGuid().ToString()).ToList();

            _writer.WriteProject("", assemblies);

            var document = new XmlDocument();
            document.LoadXml(_fileSystem.File);

            var assemblyNodes = document.SelectNodes("//assembly");
            Assert.AreEqual(assemblies.Count, assemblyNodes.Count, "An assembly node should exist for each assembly");
            var assemblyPaths = assemblyNodes.Cast<XmlNode>().Select(n => n.Attributes["path"].Value);
            CollectionAssert.AreEquivalent(assemblies, assemblyPaths);
        }
    }
}
