using NUnit.Framework;
using SMM.Automation;
using SMM.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laparoscope.UnitTests
{
    class PowerShellRunnerTests
    {
        string ParameterValidationScript = String.Empty;
        string[] validInputs = new string[] { "Hello World!" };

        [SetUp]
        public void Setup()
        {
            ParameterValidationScript = Laparoscope.UnitTests.Properties.Resources.PoshValidateParameterScript;
        }

        [Test]
        public void TestParameterValidationFromProxyVariable()
        {
            using (var runner = new ScriptRunner("[bool]($validationList -notlike $null)", null))
            {
                runner.SetProxyVariable("validationList", validInputs);
                runner.Run();
                bool result = false;
                var extracted = runner.Results.CapturePSResult<bool>().FirstOrDefault();
                if (extracted is bool value) { result = value; }

                // Will return true if the proxy variable was passed
                Assert.That(result, Is.EqualTo(true));
            }
        }

        [Test]
        public void TestParameterValidationFromProxyVariableShouldNotHaveErrors()
        {
            using (var runner = new ScriptRunner(ParameterValidationScript, null))
            {
                runner.Parameters.Add("FromOutside", "Hello World!");
                runner.SetProxyVariable("validationList", validInputs);
                runner.Run();

                // Hello World! is in the validation list and should not generate an error
                Assert.That(runner.HadErrors, Is.False);
            }
        }

        [Test]
        public void TestParameterValidationFromProxyVariableShouldHaveErrors()
        {
            using (var runner = new ScriptRunner(ParameterValidationScript, null))
            {
                runner.Parameters.Add("FromOutside", "Hello Universe!");
                runner.SetProxyVariable("validationList", validInputs);
                runner.Run();

                // Should error
                Assert.That(runner.HadErrors, Is.EqualTo(true), "Should throw an error, \"Hello Universe!\" is not in the validation list");

                // Should be a parameter binding error
                var ex = runner.LastError;
                Assert.That(ex is System.Management.Automation.ParameterBindingException, Is.EqualTo(true), "Parameter validation exception was not thrown but another error was.");
            }
        }

        [Test]
        public void TestCapturingException()
        {
            using (var runner = new ScriptRunner("throw New-Object System.Exception 'Thrown from PowerShell'", null))
            {
                runner.Run();

                // runner should show that an error occurred.
                Assert.That(runner.HadErrors, Is.EqualTo(true), "PowerShell session did not have errors.");

                // Exception is extracted from ErrorRecord
                var ex = runner.LastError;
                Assert.That(ex, Is.Not.Null, "Exception message not retreived from ErrorRecord.");

                // Exception message from inside PowerShell is curried out
                Assert.That((ex.Message.Equals("Thrown from PowerShell")), Is.True, "Couldn't get message from PowerShell exception.");
            }
        }

        [Test]
        public void TestCodeInjectionByParameter()
        {
            using (var runner = new ScriptRunner("param([string]$stringIn) Write-Output \"Testing $stringIn\"", null))
            {
                // Semicolon should place the Write-Output on the following line if interpreted  as code rather than data
                // meaning result count should be higher than 1.
                runner.Parameters.Add("stringIn", "; Write-Output 'Should be on the same line.'");
                var task = runner.RunAsyncWithStreams();
                Task.WaitAll(new Task[] { task }, 5000);
                var result = task.Result;

                Assert.That(result.Count, Is.Not.EqualTo(0), "No results were returned from the script.");
                Assert.That(result.Count, Is.EqualTo(1), $"Expected 1 result but found {result.Count}.");
            }
        }


        [Test]
        public void DisposeDoesntMessUpResults()
        {
            // This should all work, if it didn't .net would have much larger problems.
            // That said, I just want to be sure the results of a PowerShell session do
            // properly outlive a script runner with a bounded lifetime.
            var resultList = new List<dynamic>();
            using (var runner = new ScriptRunner("1..5 | foreach { Write-Output \"$_\" }", null))
            {
                runner.Run();
                resultList.AddRange(runner.Results.CapturePSResult<string>());
                Assert.That(runner.Results.Count, Is.EqualTo(5));
            }
            System.Threading.Thread.Sleep(500);
            Assert.That(resultList.Count, Is.EqualTo(5));
            Assert.That(resultList.Any(x => x == null), Is.False);
            for (var i = 1; i <= resultList.Count; i++)
            {
                Assert.That(String.Equals(resultList[i-1].ToString(), i.ToString(), StringComparison.CurrentCultureIgnoreCase), Is.True);
            }
        }
    }
}
