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
            var runner = new SimpleScriptRunner("[bool]($validationList -notlike $null)");
            runner.SetProxyVariable("validationList", validInputs);
            runner.Run();
            bool result = false;
            var extracted = runner.Results.CapturePSResult<bool>().FirstOrDefault();
            if (extracted is bool value) { result = value; }

            // Will return true if the proxy variable was passed
            Assert.IsTrue(result);
        }

        [Test]
        public void TestParameterValidationFromProxyVariableShouldNotHaveErrors()
        {
            var runner = new SimpleScriptRunner(ParameterValidationScript);
            runner.Parameters.Add("FromOutside", "Hello World!");
            runner.SetProxyVariable("validationList", validInputs);
            runner.Run();

            // Hello World! is in the validation list and should not generate an error
            Assert.IsFalse(runner.HadErrors);
        }

        [Test]
        public void TestParameterValidationFromProxyVariableShouldHaveErrors()
        {
            var runner = new SimpleScriptRunner(ParameterValidationScript);
            runner.Parameters.Add("FromOutside", "Hello Universe!");
            runner.SetProxyVariable("validationList", validInputs);
            runner.Run();
            
            // Should error
            Assert.IsTrue(runner.HadErrors, "Should throw an error, \"Hello Universe!\" is not in the validation list");

            // Should be a parameter binding error
            var ex = runner.LastError;
            Assert.IsTrue(ex is System.Management.Automation.ParameterBindingException, "Parameter validation exception was not thrown but another error was.");
        }

        [Test]
        public void TestCapturingException()
        {
            var runner = new SimpleScriptRunner("throw New-Object System.Exception 'Thrown from PowerShell'");
            runner.Run();

            // runner should show that an error occurred.
            Assert.IsTrue(runner.HadErrors, "PowerShell session did not have errors.");

            // Exception is extracted from ErrorRecord
            var ex = runner.LastError;
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
