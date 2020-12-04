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

        /// <summary>
        /// PowerShell will return an empty list for a lot of error scenarios.
        /// </summary>
        public bool ResultIsEmpty { get; set; }

        public string WorkingDirectory { get; set; }

        public List<PSObject> Invoke(string script)
        {
            if (InvokeThrows != null)
            {
                throw InvokeThrows;
            }

            if (ResultIsEmpty)
            {
                return new List<PSObject>();
            }

            if (!InvokeResults.ContainsKey(script))
            {
                throw new ArgumentException($"Unable to find an invoke result matching the script: {script}");
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
            public static string ExpectedCommit = "fe972a89a56006182f8836fe1dc338b39d889792a";

            public static (string, List<PSObject>) GitFound = ( "git --version", CreateResponse("git version 2.2.8.0.windows.1"));
            public static (string, List<PSObject>) GitNotFound = ("git --version", EmptyResponse());
            public static (string, List<PSObject>) CommitFound = ($"git cat-file -t {ExpectedCommit}", CreateResponse("commit"));
            public static (string, List<PSObject>) CommitNotFound = ($"git cat-file -t {ExpectedCommit}", CreateResponse("fatal: Not a valid object name fe972a89a56006182f8836fe1dc338b39d889792a"));
        }
    }
}
