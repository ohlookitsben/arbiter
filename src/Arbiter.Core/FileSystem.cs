using System.IO;

namespace Arbiter.Core
{
    public class FileSystem : IFileSystem
    {
        public bool Exists(string path) => File.Exists(path);

        public string GetDirectory(string solution) => new FileInfo(solution).Directory.FullName;

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