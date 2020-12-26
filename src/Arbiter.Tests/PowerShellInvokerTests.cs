using Arbiter.Core;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;

namespace Arbiter.Tests.Integration
{
    [TestFixture]
    [Category(TestCategory.Medium)]
    public class PowerShellInvokerTests
    {
        private PowerShellInvoker _invoker;

        [SetUp]
        public void SetUp() => _invoker = new PowerShellInvoker(TestContext.CurrentContext.WorkDirectory);

        [Test]
        public void Invoke_PowerShellExists()
        {
            var versionTable = _invoker.Invoke("$PSVersionTable");

            Assert.IsNotNull(versionTable);
            Assert.IsNotEmpty(versionTable);
        }

        [Test]
        public void Invoke_WorkingDirectory_InvokesFromWorkingDirectory()
        {
            var workingDirectory = Directory.CreateDirectory(Guid.NewGuid().ToString());
            _invoker = new PowerShellInvoker(workingDirectory.FullName);

            try
            {
                var result = _invoker.Invoke("Get-Location");

                Assert.AreEqual(1, result.Count, "Get-Location should return a single result.");
                Assert.AreEqual(workingDirectory.FullName, result.Single().BaseObject.ToString());
            }
            finally
            {
                Directory.Delete(workingDirectory.FullName);
            }
        }
    }
}
