using NUnit.Framework;
using System;
using System.Linq;
using System.Management.Automation;
using static SMM.Helper.Extensions;

namespace aadcapi.UnitTests
{
    public partial class PSObjectExtension
    {
        FooTest result;

        [SetUp]
        public void Setup()
        {
            var runner = new SMM.Automation.SimpleScriptRunner("[PSCustomObject]@{'Foo'='Bar';'FooBar'='Baz';Nested=[PSCustomObject]@{'Foo'='Bar';'FooBar'='Baz';}}");
            runner.Run();
            result = runner.Results.CapturePSResult<FooTest>().Cast<FooTest>().FirstOrDefault();
        }

        [Test]
        public void TestCapturedTypeNotNull()
        {
            Assert.IsNotNull(result);
        }

        [Test]
        public void TestCapturedTypeCastCorrectly()
        {
            Assert.IsInstanceOf<FooTest>(result, "Did not capture PSCustomObject as FooTest.");
        }

        [Test]
        public void TestCapturedProperties()
        {
            Assert.AreEqual(result.Foo, "Bar");
            Assert.AreEqual(result.FooBar, "Baz");
        }

        [Test]
        public void TestNestedCapture()
        {
            Assert.IsNotNull(result.Nested);
            Assert.IsInstanceOf<FooTest>(result.Nested);
        }
    }
}