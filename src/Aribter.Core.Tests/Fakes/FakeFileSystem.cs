using Arbiter.Core;
using System;
using System.IO;

namespace Aribter.Core.Tests.Fakes
{
    public class FakeFileSystem : IFileSystem
    {
        public string File { get; private set; }
        public string FilePath { get; private set; }

        public bool Exists(string path) => throw new NotImplementedException();
        public string GetDirectory(string solution) => throw new NotImplementedException();
        public void WriteFile(string path, FileMode mode, string contents)
        {
            File = contents;
            FilePath = path;
        }
    }
}
