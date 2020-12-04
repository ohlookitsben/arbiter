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
            using (var powershell = PowerShell.Create())
            {
                powershell.AddScript(script);

                var results = powershell.Invoke();
            }

            return true;
        }

        public List<string> ListChangedFiles(string fromCommit, string toCommit)
        {
            return new List<string>();
        }
    }
}
