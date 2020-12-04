using System.IO;

namespace Arbiter.Core
{
    public class FileSystem : IFileSystem
    {
        public bool Exists(string path) => File.Exists(path);

        public string GetDirectory(string solution) => new FileInfo(solution).Directory.FullName;

        public void WriteFile(string path, FileMode mode, string contents)
        {
            using (var file = File.Open(path, mode))
            using (var writer = new StreamWriter(file))
            {
                writer.Write(contents);
            }
        }
    }
}