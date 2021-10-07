using NUnit.Framework;
using SMM.Automation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aadcapi.UnitTests
{
    class PowerShellRunnerTests
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void TestCapturingException()
        {
            var runner = new SimpleScriptRunner("throw New-Object System.Exception 'Thrown from PowerShell'");
            runner.Run();

            // runner should show that an error occurred.
            Assert.IsTrue(runner.HadErrors, "PowerShell session did not have errors.");

            var ex = runner.LastError;

            // Exception is extracted from ErrorRecord
            Assert.IsNotNull(ex, "Exception message not retreived from ErrorRecord.");

            // Exception message from inside PowerShell is curried out
            Assert.IsTrue((ex.Message.Equals("Thrown from PowerShell")), "Couldn't get message from PowerShell exception.");
        }

        [Test]
        public void TestCodeInjectionByParameter()
        {
            var runner = new SimpleScriptRunner("param([string]$stringIn) Write-Host Testing $stringIn");
            runner.Parameters.Add("stringIn", "; Write-Output 'Should be on the same line.'");
            runner.Run();

            Assert.IsTrue(runner.Results.Count == 1);
        }
    }
}
