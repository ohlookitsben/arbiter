using System.IO;

namespace Arbiter.Core
{
    public class FileReader : IFileReader
    {
        public bool Exists(string path)
        {
            return File.Exists(path);
        }
    }
}