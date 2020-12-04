using Arbiter.Core;
using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace Aribter.Core.Tests.Fakes
{
    public class FakePowerShellInvoker : IPowerShellInvoker
    {
        public Dictionary<string, List<PSObject>> InvokeResults { get; set; } = new Dictionary<string, List<PSObject>>();

        public Exception InvokeThrows { get; set; }
        public string WorkingDirectory { get; set; }

        public List<PSObject> Invoke(string script)
        {
            if (InvokeThrows != null)
            {
                throw InvokeThrows;
            }

            if (!InvokeResults.ContainsKey(script))
            {
                throw new ArgumentException("Unable to find an invoke response.");
            }

            return InvokeResults[script];
        }

        public void SetResponse((string, List<PSObject>) invokeResult)
        {
            (var script, var result) = invokeResult;
            InvokeResults.Clear();
            InvokeResults.Add(script, result);
        }

        public void SetResponses(params (string, List<PSObject>)[] invokeResults)
        {
            InvokeResults.Clear();
            foreach (var invokeResult in invokeResults)
            {
                (var script, var result) = invokeResult;
                InvokeResults.Add(script, result);
            }
        }

        public static List<PSObject> CreateResponse(string baseObject)
        {
            return new List<PSObject> { new PSObject(baseObject) };
        }

        public static List<PSObject> EmptyResponse()
        {
            return new List<PSObject>();
        }

        public static class Responses
        {
            public static (string, List<PSObject>) GitFound = ( "git --version", CreateResponse("git version 2.2.8.0.windows.1"));
            public static (string, List<PSObject>) GitNotFound = ("git --version", EmptyResponse());
        }
    }
}
