using NUnit.Framework;
using SMM.Helper;
using System;
using System.Linq;

namespace Laparoscope.UnitTests
{
    public partial class PSObjectExtension
    {
        FooTest result;

        [SetUp]
        public void Setup()
        {
            var runner = new SMM.Automation.ScriptRunner("[PSCustomObject]@{'Foo'='Bar';'FooBar'='Baz';Nested=[PSCustomObject]@{'Foo'='Bar';'FooBar'='Baz';}}", null);
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