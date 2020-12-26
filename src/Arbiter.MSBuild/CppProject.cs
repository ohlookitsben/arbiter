using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arbiter.MSBuild
{
    public class CppProject
    {
        public string Path { get; }

        public CppProject(string path)
        {
            Path = path;
        }
    }
}
