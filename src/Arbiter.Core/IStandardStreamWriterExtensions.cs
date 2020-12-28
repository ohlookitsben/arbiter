using System.CommandLine.IO;

namespace Arbiter.Core
{
    public static class IStandardStreamWriterExtensions
    {
        public static void VerboseWrite(this IStandardStreamWriter writer, string value = "")
        {
            if (!Globals.Verbose)
            {
                return;
            }

            writer.Write(value);
        }

        public static void VerboseWriteLine(this IStandardStreamWriter writer, string value = "")
        {
            if (!Globals.Verbose)
            {
                return;
            }

            writer.WriteLine(value);
        }
    }
}
