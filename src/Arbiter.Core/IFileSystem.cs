using System.IO;

namespace Arbiter.Core
{
    public interface IFileSystem
    {
        bool Exists(string path);
        string GetDirectory(string path);
        string ReadFile(string path);
        void WriteFile(string path, string contents);
        string Combine(string path1, string path2);
    }
}