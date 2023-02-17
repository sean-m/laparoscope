using NUnit.Framework;
using SMM.Automation;
using SMM.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aadcapi.UnitTests
{
    class PowerShellRunnerTests
    {
        string ParameterValidationScript = String.Empty;
        string[] validInputs = new string[] { "Hello World!" };

        [SetUp]
        public void Setup()
        {
            ParameterValidationScript = Properties.Resources.PoshValidateParameterScript;
        }

        [Test]
        public void TestParameterValidationFromProxyVariable()
        {
            using (var runner = new SimpleScriptRunner("[bool]($validationList -notlike $null)"))
            {
                runner.SetProxyVariable("validationList", validInputs);
                runner.Run();
                bool result = false;
                var extracted = runner.Results.CapturePSResult<bool>().FirstOrDefault();
                if (extracted is bool value) { result = value; }

                // Will return true if the proxy variable was passed
                Assert.IsTrue(result);
            }
        }

        [Test]
        public void TestParameterValidationFromProxyVariableShouldNotHaveErrors()
        {
            using (var runner = new SimpleScriptRunner(ParameterValidationScript))
            {
                runner.Parameters.Add("FromOutside", "Hello World!");
                runner.SetProxyVariable("validationList", validInputs);
                runner.Run();

                // Hello World! is in the validation list and should not generate an error
                Assert.IsFalse(runner.HadErrors);
            }
        }

        [Test]
        public void TestParameterValidationFromProxyVariableShouldHaveErrors()
        {
            using (var runner = new SimpleScriptRunner(ParameterValidationScript))
            {
                runner.Parameters.Add("FromOutside", "Hello Universe!");
                runner.SetProxyVariable("validationList", validInputs);
                runner.Run();

                // Should error
                Assert.IsTrue(runner.HadErrors, "Should throw an error, \"Hello Universe!\" is not in the validation list");

                // Should be a parameter binding error
                var ex = runner.LastError;
                Assert.IsTrue(ex is System.Management.Automation.ParameterBindingException, "Parameter validation exception was not thrown but another error was.");
            }
        }

        [Test]
        public void TestCapturingException()
        {
            using (var runner = new SimpleScriptRunner("throw New-Object System.Exception 'Thrown from PowerShell'"))
            {
                runner.Run();

                // runner should show that an error occurred.
                Assert.IsTrue(runner.HadErrors, "PowerShell session did not have errors.");

                // Exception is extracted from ErrorRecord
                var ex = runner.LastError;
                Assert.IsNotNull(ex, "Exception message not retreived from ErrorRecord.");

                // Exception message from inside PowerShell is curried out
                Assert.IsTrue((ex.Message.Equals("Thrown from PowerShell")), "Couldn't get message from PowerShell exception.");
            }
        }

        [Test]
        public void TestCodeInjectionByParameter()
        {
            using (var runner = new SimpleScriptRunner("param([string]$stringIn) Write-Host Testing $stringIn"))
            {
                runner.Parameters.Add("stringIn", "; Write-Output 'Should be on the same line.'");
                runner.Run();

                Assert.IsTrue(runner.Results.Count == 1);
            }
        }


        [Test]
        public void DisposeDoesntMessUpResults()
        {
            // This should all work, if it didn't .net would have much larger problems.
            // That said, I just want to be sure the results of a PowerShell session do
            // properly outlive a script runner with a bounded lifetime.
            var resultList = new List<dynamic>();
            using (var runner = new SimpleScriptRunner("1..5 | foreach { Write-Output \"$_\" }"))
            {
                runner.Run();
                resultList.AddRange(runner.Results.CapturePSResult<string>().Select(x => x["Output"]));
                Assert.IsTrue(runner.Results.Count == 5);
            }
            System.Threading.Thread.Sleep(500);
            Assert.IsTrue(resultList.Count == 5);
            Assert.IsFalse(resultList.Any(x => x == null));
            for (var i = 1; i <= resultList.Count; i++)
            {
                Assert.IsTrue(String.Equals(resultList[i-1].ToString(), i.ToString(), StringComparison.CurrentCultureIgnoreCase));
            }
        }
    }
}
