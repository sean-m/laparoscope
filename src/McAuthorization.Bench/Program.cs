using System;
using McAuthorization;
using System.Linq;
using System.Collections.Generic;
using McAuthorization.Models;
using System.Diagnostics;

namespace McAuthorization.Bench
{
    class Program
    {
        static void Main(string[] args)
        {
            // Load Rules
            var rules = new List<RoleFilterModel>();
            using (var ruleFile = System.IO.File.OpenText("./Data/Rules.json"))
            {
                var text = ruleFile.ReadToEnd();
                rules = Newtonsoft.Json.JsonConvert.DeserializeObject<List<RoleFilterModel>>(text);
            }
            foreach (var rule in rules)
                RegisteredRoleControllerRules.RegisterRoleControllerModel(rule);

            // Load Test Models
            var models = new List<TestModel>();
            using (var modelFile = System.IO.File.OpenText("./Data/Models.json"))
            {
                var text = modelFile.ReadToEnd();
                models = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TestModel>>(text);
            }

            // Execute Tests
            var stopwatch = new Stopwatch();
            var roles = new string[] { "Guy" };
            var cycles = 100000;  // 100k
            int count = 0;
            stopwatch.Start();
            for (var i=0; i < cycles; i++)
            {
                var matches = models.Any(x => Filter.IsAuthorized(x, "This", roles));
                if (matches) count++;
            }
            stopwatch.Stop();

            Console.WriteLine($"Rules loaded: {rules.Count}");
            Console.WriteLine($"Models loaded: {models.Count}");
            Console.WriteLine($"Cyles: {(float)cycles / 1000.0} k");
            Console.WriteLine($"Elapsed miliseconds: {stopwatch.ElapsedMilliseconds}");
            Console.WriteLine($"Elapsed seconds: {stopwatch.ElapsedMilliseconds / 1000.0}");
            Console.WriteLine($"Match count: {count}");
        }
    }

    class TestModel
    {
        public string Prop1 { get; set; }
        public string Prop2 { get; set; }
        public dynamic Prop3 { get; set; }
    }
}
