using aadcapi.UnitTests.Properties;
using NUnit.Framework;
using SMM.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aadcapi.UnitTests
{
    class PSObjectAuthorizationTests
    {
        IEnumerable<FooTest> result;



        [SetUp]
        public void Setup()
        {
            const string script = @"[PSCustomObject]@{'Foo'='Bar';'FooBar'='Baz';Nested=[PSCustomObject]@{'Foo'='Bar';'FooBar'='Baz';}}; [PSCustomObject]@{'Foo'='BigBar';'FooBar'='Baz';Nested=[PSCustomObject]@{'Foo'='LittleBar';'FooBar'='Baz';}}; [PSCustomObject]@{'Foo'='Bar';'FooBar'='Baz';Nested=[PSCustomObject]@{'Foo'='Bar';'FooBar'='Baz';}} ";

            var ruleText = Resources.TestRules;

            var runner = new SMM.Automation.SimpleScriptRunner(script);
            runner.Run();
            result = runner.Results.CapturePSResult<FooTest>().Cast<FooTest>();
        }

        [Test]
        public void TestFilteringByAuthorization()
        {
            Assert.IsNotNull(result);
        }
    }
}
