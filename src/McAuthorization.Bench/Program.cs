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

            // Load Test Models as Dictionary<string, object>
            var models2 = new List<Dictionary<string, object>>();
            using (var modelFile = System.IO.File.OpenText("./Data/Models.json"))
            {
                var text = modelFile.ReadToEnd();
                models2 = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(text);
            }

            var stopwatch2 = new Stopwatch();
            var roles2 = new string[] { "Guy" };
            int count2 = 0;
            stopwatch2.Start();
            for (var i = 0; i < cycles; i++)
            {
                var matches2 = models2.Any(x => Filter.IsAuthorized(x, "This", roles2));
                if (matches2) count2++;
            }
            stopwatch2.Stop();

            Console.WriteLine("Matching POCO data objects.");
            Console.WriteLine($"Rules loaded: {rules.Count}");
            Console.WriteLine($"Models loaded: {models.Count}");
            Console.WriteLine($"Cyles: {(float)cycles / 1000.0} k");
            Console.WriteLine($"Elapsed miliseconds: {stopwatch.ElapsedMilliseconds}");
            Console.WriteLine($"Elapsed seconds: {stopwatch.ElapsedMilliseconds / 1000.0}");
            Console.WriteLine($"Match count: {count}");
            Console.WriteLine();
            Console.WriteLine("Matching Dictionary data objects.");
            Console.WriteLine($"Rules loaded: {rules.Count}");
            Console.WriteLine($"Models loaded: {models2.Count}");
            Console.WriteLine($"Cyles: {(float)cycles / 1000.0} k");
            Console.WriteLine($"Elapsed miliseconds: {stopwatch2.ElapsedMilliseconds}");
            Console.WriteLine($"Elapsed seconds: {stopwatch2.ElapsedMilliseconds / 1000.0}");
            Console.WriteLine($"Match count: {count2}");
        }
    }

    class TestModel
    {
        public string Prop1 { get; set; }
        public string Prop2 { get; set; }
        public dynamic Prop3 { get; set; }
        public string Name { get; set; }
        public string ConnetorName { get; set; }
        public string ConnetorTypeName { get; set; }
    }
}
