using System.CommandLine;
using System.CommandLine.IO;
using System.IO;

namespace Arbiter.Tests.Fakes
{
    public class FakeConsole : IConsole
    {
        public string OutString => _outWriter.ToString();

        private StringWriter _outWriter = new StringWriter();

        public IStandardStreamWriter Out => StandardStreamWriter.Create(_outWriter);

        public bool IsOutputRedirected => false;

        public string ErrorString => _errorWriter.ToString();

        private StringWriter _errorWriter = new StringWriter();

        public IStandardStreamWriter Error => StandardStreamWriter.Create(_errorWriter);

        public bool IsErrorRedirected => false;

        public bool IsInputRedirected => false;
    }
}
