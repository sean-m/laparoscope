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
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestCapturedTypeCastCorrectly()
        {
            Assert.That(result, Is.InstanceOf<FooTest>(), "Did not capture PSCustomObject as FooTest.");
        }

        [Test]
        public void TestCapturedProperties()
        {
            Assert.That(result.Foo, Is.EqualTo("Bar"));
            Assert.That(result.FooBar, Is.EqualTo("Baz"));
        }

        [Test]
        public void TestNestedCapture()
        {
            Assert.That(result.Nested, Is.Not.Null);
            Assert.That(result.Nested, Is.InstanceOf<FooTest>());
        }
    }
}