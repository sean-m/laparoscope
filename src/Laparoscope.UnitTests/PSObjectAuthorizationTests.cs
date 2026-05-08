using Laparoscope.UnitTests.Properties;
using McAuthorization;
using McAuthorization.Models;
using Newtonsoft.Json;
using NUnit.Framework;
using SMM.Helper;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Laparoscope.UnitTests
{
    class PSObjectAuthorizationTests
    {
        IEnumerable<FooTest> result;
        string[] testRoles = new string[] { "Admin:Garage" };
        
        string testModelText = string.Empty;
        List<Dictionary<string, object>> testModels = new List<Dictionary<string, object>>();

        [SetUp]
        public void Setup()
        {
            const string script = @"[PSCustomObject]@{'Foo'='Bar';'FooBar'='Baz';Nested=[PSCustomObject]@{'Foo'='Bar';'FooBar'='Baz';}}; [PSCustomObject]@{'Foo'='BigBar';'FooBar'='Baz';Nested=[PSCustomObject]@{'Foo'='LittleBar';'FooBar'='Baz';}}; [PSCustomObject]@{'Foo'='Bar';'FooBar'='Baz';Nested=[PSCustomObject]@{'Foo'='Bar';'FooBar'='Baz';}} ";

            var ruleText = Resources.TestRules;
            var rules = JsonConvert.DeserializeObject<List<RoleFilterModel>>(ruleText);
            foreach (var rule in rules) RegisteredRoleControllerRules.RegisterRoleControllerModel(rule);

            // Load some json as an anologue to a .ToDict() result capture
            testModelText = Encoding.ASCII.GetString(Resources.TestRunHistory);
            testModels = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(testModelText);

            var runner = new SMM.Automation.SimpleScriptRunner(script);
            runner.Run();
            result = runner.Results.CapturePSResult<FooTest>().Cast<FooTest>();
        }

        [Test]
        public void TestAnyMatchingRule()
        {
            var match = result.Any(x => Filter.IsAuthorized(x, "Connector", testRoles));
            Assert.That(match, Is.True);
        }

        [Test]
        public void TestFilteringByRule()
        {
            var match = Filter.IsAuthorized(result, "Connector", testRoles);
            Assert.That(match.Count(), Is.Not.EqualTo(result.Count()));
        }

        [Test]
        public void TestFilteringHashtableList()
        {
            Assert.That(testModels.Count, Is.Not.Zero);

            var matches = testModels.Where(x => Filter.IsAuthorized(x, "RunStepResult", testRoles))?.ToList();
            var excluded = testModels.Except(matches).ToList();

            Assert.That(matches, Is.Not.Null);
            Assert.That(matches.Count, Is.Not.Zero);
            Assert.That(testModels.Count, Is.Not.EqualTo(matches.Count));

            // Verify matched records contain the expected property value the rule allows
            Assert.That(matches, Has.All.Matches<Dictionary<string, object>>(
                m => Filter.IsAuthorized(m, "RunStepResult", testRoles)),
                "Every matched record should pass authorization");

            // Verify excluded records genuinely fail authorization
            Assert.That(excluded, Has.All.Matches<Dictionary<string, object>>(
                m => !Filter.IsAuthorized(m, "RunStepResult", testRoles)),
                "Every excluded record should fail authorization");

            // Verify excluded records contain the specific value the rule disallows.
            // Replace "PropertyName" and "excludedValue" with the actual rule property
            // and value from your TestRules.json that should be filtered out.
            Assert.That(excluded, Has.All.Matches<Dictionary<string, object>>(
                m => m.ContainsKey("ConnectorName") && m["ConnectorName"]?.ToString() != "garage.mcardletech.com"),
                "Excluded records should be the ones with the value the rule denies");
        }
    }
}
