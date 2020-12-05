using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Arbiter.Core
{
    public class RepositoryReader : IRepositoryReader
    {
        private readonly IPowerShellInvoker _invoker;

        public string WorkingDirectory { get => _invoker.WorkingDirectory; set => _invoker.WorkingDirectory = value; }

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

        public bool CommitIsAncestor(string commit, string ancestor)
        {
            string script = $"git merge-base --is-ancestor {ancestor} {commit}; $LastExitCode";
            var result = _invoker.Invoke(script).SingleOrDefault();
            if (result == null)
            {
                return false;
            }

            return ((int)result.BaseObject) == 0;
        }

        public bool RepositoryExists()
        {
            string script = $"git status";
            var result = _invoker.Invoke(script).FirstOrDefault();
            if (result == null)
            {
                return false;
            }

            return true;
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

        /// <summary>
        /// List the files that have changed between <paramref name="fromCommit"/> and <paramref name="toCommit"/>. If no
        /// files have changed, or the two commit are the same, an empty list will be returned. The list will contain no
        /// duplicate entries in the case that a file has been changed multiple times.
        /// </summary>
        public List<string> ListChangedFiles(string fromCommit, string toCommit)
        {
            string script = $"git diff --name-only {fromCommit} {toCommit}";
            var results = _invoker.Invoke(script);
            var relativePaths = results.Select(r => r.BaseObject.ToString()).Distinct();
            // Ensure canonical paths are returned, rather than a mix of slash types, etc.
            return relativePaths.Select(p => new Uri(Path.Combine(WorkingDirectory, p)).LocalPath).ToList();
        }
    }
}
