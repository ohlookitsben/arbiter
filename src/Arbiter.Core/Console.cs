using Arbiter.Core.Interfaces;
using SystemConsole = System.Console;

namespace Arbiter.Core
{
    public class Console : IConsole
    {
        public void WriteLine() => SystemConsole.WriteLine();
        public void WriteLine(string value) => SystemConsole.WriteLine(value);
    }
}
