using Arbiter.Core;
using NUnit.Framework;
using System;
using System.IO;

namespace Arbiter.Tests.Integration
{
    [TestFixture]
    [Category("Integration")]
    public class FileSystemTests
    {
        private FileSystem _fileSystem;

        [SetUp]
        public void SetUp()
        {
            _fileSystem = new FileSystem();
        }

        [Test]
        public void GetDirectory_FromRelativePath_ReturnsFullPath()
        {
            string directoryName = Guid.NewGuid().ToString();
            string file = Path.Combine(directoryName, Guid.NewGuid().ToString());
            var directory = Directory.CreateDirectory(directoryName);

            try
            {
                var result = _fileSystem.GetDirectory(file);

                Assert.AreEqual(directory.FullName, result);
            }
            finally
            {
                File.Delete(file);
                Directory.Delete(directoryName);
            }
        }

        [Test]
        public void WriteFile_FileDoesNotExist_WritesNewFile()
        {
            string name = Guid.NewGuid().ToString();
            string expectedContents = "Hello, FileSystem!";

            try
            {
                _fileSystem.WriteFile(name, expectedContents);

                bool exists = File.Exists(name);
                Assert.IsTrue(exists, "The file should be created.");

                string contents = File.ReadAllText(name);
                Assert.AreEqual(expectedContents, contents, "The contents should be written to the file.");
            }
            finally
            {
                if (File.Exists(name))
                {
                    File.Delete(name);
                }
            }
        }

        [Test]
        public void WriteFile_FileExists_TruncatesAndWrites()
        {
            string name = Guid.NewGuid().ToString();
            string originalContents = "Hello, FileSystem!";

            try
            {
                _fileSystem.WriteFile(name, originalContents);
                string expectedContents = "Hello, Dolly!";

                _fileSystem.WriteFile(name, expectedContents);


                string contents = File.ReadAllText(name);
                Assert.AreEqual(expectedContents, contents, "The contents should be written to the file.");
            }
            finally
            {
                if (File.Exists(name))
                {
                    File.Delete(name);
                }
            }
        }
    }
}
