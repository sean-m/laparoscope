using Laparoscope.Controllers.Server;
using LaparoscopeShared.Models;
using McAuthorization;
using McAuthorization.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.Linq;
using Laparoscope.Controllers.Server;
using SMM.Helper;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Laparoscope.Pages.Admin
{
    public class IndexModel : PageModel {
        private readonly IConfiguration _configuration;

        public IndexModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IEnumerable<RoleFilterModel> AuthorizationRules { get; set; } = Array.Empty<RoleFilterModel>();

        public IEnumerable<WindowsTask> Processes { get; set; }

        public IEnumerable<Setting> AppSettings { get; set; } = new List<Setting>();

        public void OnGet()
        {
            AuthorizationRules = RegisteredRoleControllerRules.GetRoleControllerModels();
            Processes = ProcessesController.GetProcesses().GetAwaiter().GetResult();

            // Recursively walk configuration and build path-based dictionary
            AppSettings = FlattenConfiguration(_configuration)
                .Select(x => x.Value).Where(x => !x.Key.Like("Rule.*"));
        }

        private Dictionary<string, Setting> FlattenConfiguration(IConfiguration config)
        {
            var result = new Dictionary<string, Setting>();
            if (config is IConfigurationRoot root)
            {
                foreach (var provider in root.Providers.Reverse())
                {
                    var providerName = provider.GetType().Name;

                    var keys = provider.GetChildKeys(Enumerable.Empty<string>(), null);
                    foreach (var k in keys)
                    {
                        // Fix for CS8601: Ensure v is not null
                        var value = provider.TryGet(k, out var v) && v != null ? v : string.Empty;
                        if (!string.IsNullOrEmpty(value))
                            result[k] = new Setting() { Key = k, Value = value, Source = providerName };
                    }
                }
            }

            return result;
        }

        static internal Func<string,string> maskIt = (string x) => { return "****************"; };
        public string MaskingAction(IEnumerable<string> patterns, string matchTarget, string value, Func<string, string>? transform)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            if (transform == null)
                transform = maskIt;

            string result = value;
            if (patterns.Any(x => matchTarget.Like(x)))
            {
                return transform(value);
            }
            return result;
        }
    }

    public class Setting
    {
        public string Key { get; set; }
        public string Value { get; set; }
        private string _source;
        public string Source { get => GetSourceName(_source); set => _source = value; }

        public string GetSourceName(string source)
        {
            return source switch
            {
                "CommandLine" => "cli",
                "EnvironmentVariablesConfigurationProvider" => "env",
                "UserSecrets" => "userSecret",
                "AzureKeyVault" => "azkv",
                "AzureAppConfigurationProvider" => "azappconfig",
                "JsonConfigurationProvider" => "json",
                _ => source
            };
        }
    }
}
