using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace Arbiter.Core
{
    public class PowerShellInvoker : IPowerShellInvoker
    {
        public string WorkingDirectory { get; set; }

        public List<PSObject> Invoke(string script)
        {
            using (var powershell = PowerShell.Create())
            {
                if (!string.IsNullOrEmpty(WorkingDirectory))
                {
                    powershell.AddScript($"Set-Location {WorkingDirectory}");
                    powershell.Invoke();
                }

                powershell.AddScript(script);
                return powershell.Invoke().ToList();
            }
        }
    }
}
