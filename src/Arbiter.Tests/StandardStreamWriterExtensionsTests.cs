using Arbiter.Core;
using Arbiter.Tests.Helpers;
using Moq;
using NUnit.Framework;
using System.CommandLine.IO;

namespace Arbiter.Tests
{
    [TestFixture]
    [Category(TestCategory.Small)]
    public class StandardStreamWriterExtensionsTests
    {
        [Test]
        public void VerboseWrite_NotVerbose_DoesNotWrite()
        {
            var writer = new Mock<IStandardStreamWriter>();

            Globals.Verbose = false;
            writer.Object.VerboseWrite("value");

            writer.Verify(w => w.Write(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void VerboseWriteLine_NotVerbose_DoesNotWrite()
        {
            var writer = new Mock<IStandardStreamWriter>();

            Globals.Verbose = false;
            writer.Object.VerboseWriteLine("value");

            writer.Verify(w => w.Write(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void VerboseWrite_Verbose_Writes()
        {
            var writer = new Mock<IStandardStreamWriter>();

            Globals.Verbose = true;
            writer.Object.VerboseWrite("value");

            writer.Verify(w => w.Write(It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void VerboseWriteLine_Verbose_Writes()
        {
            var writer = new Mock<IStandardStreamWriter>();

            Globals.Verbose = true;
            writer.Object.VerboseWriteLine("value");

            writer.Verify(w => w.Write(It.IsAny<string>()), Times.Exactly(2), "Write should be called once for the value, and once to write a new line.");
        }
    }
}
