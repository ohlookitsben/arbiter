using System.Collections.Generic;

namespace Arbiter.Core
{
    public interface IRepositoryReader
    {
        string WorkingDirectory { get; set; }

        bool CommitExists(string commit);
        bool CommitIsAncestor(string commit, string ancestor);
        bool GitExists();
        List<string> ListChangedFiles(string fromCommit, string toCommit);
        bool RepositoryExists();
    }
}