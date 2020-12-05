using Arbiter.Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;

namespace Aribter.Core.Tests.Fakes
{
    public class FakePowerShellInvoker : IPowerShellInvoker
    {
        public List<PSObject> InvokeResult { get; set; } = new List<PSObject>();

        public Exception InvokeThrows { get; set; }

        /// <summary>
        /// PowerShell will return an empty list for a lot of error scenarios.
        /// </summary>
        public bool ResultIsEmpty { get; set; }

        public string WorkingDirectory { get; set; } = TestContext.CurrentContext.WorkDirectory;

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

            return InvokeResult;
        }

        public static List<PSObject> CreateResponse(params object[] baseObjects)
        {
            var responses = new List<PSObject>();
            foreach (object baseObject in baseObjects)
            {
                responses.Add(new PSObject(baseObject));
            }

            return responses;
        }

        public static class Responses
        {
            public static readonly string ExpectedCommit = "fe972a89a56006182f8836fe1dc338b39d889792a";
            public static readonly string ExpectedToCommit = "969e26db332760f755d2429ce65e04061b61e207";
            public static readonly string[] ExpectedChangedFiles =
            {
                Path.Combine(TestContext.CurrentContext.WorkDirectory, @"Aribter.Core.Tests\Fakes\FakePowerShellInvoker.cs"),
                Path.Combine(TestContext.CurrentContext.WorkDirectory, @"Aribter.Core.Tests\RepositoryReaderTests.cs"),
                Path.Combine(TestContext.CurrentContext.WorkDirectory, @"src\Arbiter.Core\Arbiter.Core.csproj"),
                Path.Combine(TestContext.CurrentContext.WorkDirectory, @"src\Arbiter.Core\RepositoryReader.cs")
            };

            public static readonly List<PSObject> Empty = new List<PSObject>();
            public static readonly List<PSObject> GitFound = CreateResponse("git version 2.2.8.0.windows.1");
            public static readonly List<PSObject> GitNotFound = Empty;
            public static readonly List<PSObject> CommitFound = CreateResponse("commit");
            public static readonly List<PSObject> CommitNotFound = Empty;
            public static readonly List<PSObject> ChangedFiles = CreateResponse(ExpectedChangedFiles);
            public static readonly List<PSObject> ChangedFilesWithDuplicates = CreateResponse(ExpectedChangedFiles.Concat(ExpectedChangedFiles).ToArray());
            public static readonly List<PSObject> RepositoryFound = CreateResponse("On branch master", "nothing to commit, working tree clean");
            public static readonly List<PSObject> RepositoryNotFound = Empty;
            public static readonly List<PSObject> CommitIsAncestor = CreateResponse(0);
            public static readonly List<PSObject> CommitNotAncestor = CreateResponse(1);
            public static readonly List<PSObject> NoChangedFiles = Empty;
        }
    }
}
