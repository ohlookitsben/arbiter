using System.CommandLine;
using System.IO;

namespace Arbiter.Core.Commands
{
    public class CommonOptions
    {
        public static Option<bool> Distance => new Option<bool>(new[] { "--distance", "-d" }, "Print the distance from changes as well as the assembly");
        public static Option<string> FromCommit => new Option<string>(new[] { "--from-commit", "-f" }, "The start of the commit range to evaluate") { IsRequired = true };
        public static Option<string> ToCommit => new Option<string>(new[] { "--to-commit", "-t" }, "The end of the commit range to evaluate") { IsRequired = true };
        public static Option<FileInfo> Solution => new Option<FileInfo>(new[] { "--solution", "-s" }, "The path to the solution (.sln) file to evaluate") { IsRequired = true };
    }
}
