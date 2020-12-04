namespace Arbiter.Core
{
    public interface IFileReader
    {
        bool Exists(string path);
        string GetDirectory(string solution);
    }
}