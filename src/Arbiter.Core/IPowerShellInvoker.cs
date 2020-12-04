using System.Collections.Generic;
using System.Management.Automation;

namespace Arbiter.Core
{
    public interface IPowerShellInvoker
    {
        string WorkingDirectory { get; set; }

        List<PSObject> Invoke(string script);
    }
}