using Laparoscope.Controllers.Server;
using Laparoscope.Controllers.Server;
using LaparoscopeShared.Models;
using McAuthorization;
using McAuthorization.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Configuration;
using SMM.Helper;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace Laparoscope.Pages.Admin
{
    public class IndexModel : PageModel {
        private readonly IConfiguration _configuration;

        private List<MaskingPattern> _patterns = new List<MaskingPattern>()
        {
            new MaskingPattern() { Patterns = new[] { "*Secret*", "*Password*", "*Key*" }, Transform = (string x) => { return "****************"; } },
            new MaskingPattern() { Patterns = new[] { "*AppConfigConnectionString*" }, Transform = (string x) => { return Regex.Replace(x, @"(?i)(?<=Secret\=).+?(?=(\=|$|;))", "********************"); } }
        };


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
                        {
                            foreach (var pattern in _patterns)
                            {
                                value = MaskingAction(pattern.WithTargetValue(k, value));
                            }
                            result[k] = new Setting() { Key = k, Value = value, Source = providerName };
                        }
                    }
                }
            }

            return result;
        }
                
        internal string MaskingAction(MaskingPattern pattern)
        {
            if (string.IsNullOrEmpty(pattern.Value))
                return pattern.Value;

            string result = pattern.Value;
            if (pattern.Patterns.Any(x => pattern.MatchTarget.Like(x)))
            {
                return pattern.Transform(result);
            }
            return result;
        }
    }

    internal class MaskingPattern
    {
        //Model.MaskingAction(new[] { "*AppConfigConnectionString*" }, setting.Key, setting.Value, (string x) => { return Regex.Replace(x, @"(?i)(?<=Secret\=).+(?=(\=|$|;))", "********************"); }) :
        //Model.MaskingAction(new[] { "*Secret*", "*Password*", "*Key*" }, setting.Key, setting.Value, null)

        public IEnumerable<string> Patterns { get; set; } = Array.Empty<string>();
        public string MatchTarget { get; set; }
        public string Value { get; set; }
        public Func<string, string> Transform { get; set; } = (string x) => { return "****************"; };

        public MaskingPattern WithTargetValue(string target, string value)
        {
            return new MaskingPattern()
            {
                MatchTarget = target,
                Patterns = this.Patterns,
                Transform = this.Transform,
                Value = value
            };
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
