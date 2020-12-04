using System.IO;

namespace Arbiter.Core
{
    public interface IFileSystem
    {
        bool Exists(string path);
        string GetDirectory(string solution);
        void WriteFile(string path, FileMode mode, string contents);
    }
}