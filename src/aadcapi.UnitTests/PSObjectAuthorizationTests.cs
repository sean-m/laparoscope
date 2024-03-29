﻿using aadcapi.UnitTests.Properties;
using McAuthorization;
using McAuthorization.Models;
using Newtonsoft.Json;
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
        string[] testRoles = new string[] { "Admin:Garage" };

        [SetUp]
        public void Setup()
        {
            const string script = @"[PSCustomObject]@{'Foo'='Bar';'FooBar'='Baz';Nested=[PSCustomObject]@{'Foo'='Bar';'FooBar'='Baz';}}; [PSCustomObject]@{'Foo'='BigBar';'FooBar'='Baz';Nested=[PSCustomObject]@{'Foo'='LittleBar';'FooBar'='Baz';}}; [PSCustomObject]@{'Foo'='Bar';'FooBar'='Baz';Nested=[PSCustomObject]@{'Foo'='Bar';'FooBar'='Baz';}} ";

            var ruleText = Resources.TestRules;
            var rules = JsonConvert.DeserializeObject<List<RoleFilterModel>>(ruleText);
            foreach (var rule in rules) RegisteredRoleControllerRules.RegisterRoleControllerModel(rule);

            var runner = new SMM.Automation.SimpleScriptRunner(script);
            runner.Run();
            result = runner.Results.CapturePSResult<FooTest>().Cast<FooTest>();
        }

        [Test]
        public void TestAnyMatchingRule()
        {
            var match = result.Any(x => Filter.IsAuthorized(x, "Connector", testRoles));
            Assert.IsNotNull(match);
        }

        [Test]
        public void TestFilteringByRule()
        {
            var match = Filter.IsAuthorized(result, "Connector", testRoles);
            Assert.AreNotEqual(match.Count(), result.Count());
        }

        [Test]
        public void TestFilteringHashtableList()
        {
            // Load some json as an anologue to a .ToDict() result capture
            var testModelText = Encoding.ASCII.GetString(Resources.TestRunHistory);
            var testModels = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(testModelText);

            Assert.NotZero(testModels.Count);

            // The test rule should filter out two of the records
            var matches = testModels.Where(x => Filter.IsAuthorized(x, "RunProfileResult", testRoles))?.ToList();

            Assert.NotNull(matches);
            Assert.NotZero(matches.Count);
            Assert.AreNotEqual(testModels.Count, matches.Count);
        }
    }
}
