using System.IO;

namespace Arbiter.Core
{
    public class FileSystem : IFileSystem
    {
        public string Combine(string path1, string path2) => Path.Combine(path1, path2);

        public bool Exists(string path) => File.Exists(path);

        public string GetDirectory(string path) => new FileInfo(path).Directory.FullName;
        public string ReadFile(string path) => File.ReadAllText(path);

        public void WriteFile(string path, string contents)
        {
            FileStream file = null;
            try
            {
                if (!File.Exists(path))
                {
                    file = File.Create(path);
                }
                else
                {
                    file = File.Open(path, FileMode.Truncate);
                }

                using (var writer = new StreamWriter(file))
                {
                    writer.Write(contents);
                }
            }
            finally
            {
                file?.Dispose();
            }
        }
    }
}