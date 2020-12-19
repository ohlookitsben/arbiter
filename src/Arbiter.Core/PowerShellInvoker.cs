using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;

namespace Arbiter.Core
{
    public class PowerShellInvoker : IPowerShellInvoker
    {
        public string WorkingDirectory { get; }

        public PowerShellInvoker(string workingDirectory)
        {
            WorkingDirectory = workingDirectory;
        }

        public List<PSObject> Invoke(string script)
        {
            if (!Directory.Exists(WorkingDirectory))
            {
                throw new InvalidOperationException("A working directory must be set before invoking scripts.");
            }

            using var powershell = PowerShell.Create();
            powershell.AddScript($"Set-Location {WorkingDirectory}");
            powershell.Invoke();

            powershell.AddScript(script);
            return powershell.Invoke().ToList();
        }
    }
}
