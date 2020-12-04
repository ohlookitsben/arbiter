using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Text.RegularExpressions;

namespace Arbiter.Core
{
    public class RepositoryReader
    {
        private readonly IPowerShellInvoker _invoker;

        public RepositoryReader(IPowerShellInvoker invoker)
        {
            _invoker = invoker;

            // TODO: Set it to the solution directory to ensure we're inside a repository.
            _invoker.WorkingDirectory = @"C:\Users\ben.wiles\source\repos\Arbiter";
        }

        public bool GitExists()
        {
            string script = $"git --version";
            var result = _invoker.Invoke(script).SingleOrDefault();
            if (result == null)
            {
                return false;
            }

            return new Regex("^git.*").IsMatch(result.BaseObject.ToString());
        }

        public bool CommitExists(string commit)
        {
            string script = $"git cat-file -t {commit}";
            var result = _invoker.Invoke(script).SingleOrDefault();
            if (result == null)
            {
                return false;
            }

            return result.BaseObject.ToString() == "commit";
        }

        public List<string> ListChangedFiles(string fromCommit, string toCommit)
        {
            string script = $"git diff --name-only {fromCommit} {toCommit}";
            var results = _invoker.Invoke(script);


            return new List<string>();
        }
    }
}
