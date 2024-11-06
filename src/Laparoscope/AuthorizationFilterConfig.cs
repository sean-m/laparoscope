using McAuthorization;
using McAuthorization.Models;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Laparoscope
{
    public static class AuthorizationFilters
    {
        public static IApplicationBuilder ConfigureAuthorizationFilters(this IApplicationBuilder app)
        {
            var adminRoleBuiltinAuthz = @"[
                {'LoadedFrom':'Built-in','Role':'Admin','Context':'*','ClaimProperty':'','ClaimValue':'','ModelProperty':'*id*','ModelValue':'*','ModelValues':[]},
                {'LoadedFrom':'Built-in','Role':'Admin','Context':'*','ClaimProperty':'','ClaimValue':'','ModelProperty':'ConnectorName','ModelValue':'*','ModelValues':[]},
                {'LoadedFrom':'Built-in','Role':'Admin','Context':'Scheduler','ClaimProperty':null,'ClaimValue':null,'ModelProperty':'Setting','ModelValue':'SchedulerSuspended','ModelValues':null},
                {'LoadedFrom':'Built-in','Role':'Admin','Context':'Scheduler','ClaimProperty':null,'ClaimValue':null,'ModelProperty':'Setting','ModelValue':'SchedulerSuspended','ModelValues':null},
                {'LoadedFrom':'Built-in','Role':'Admin','Context':'GlobalSettings','ClaimProperty':null,'ClaimValue':null,'ModelProperty':'Authorized','ModelValue':'True','ModelValues':null},
                {'LoadedFrom':'Built-in','Role':'Admin','Context':'Processes','ClaimProperty':null,'ClaimValue':null,'ModelProperty':'Authorized','ModelValue':'True','ModelValues':null},
            ]";
            var rules = JsonConvert.DeserializeObject<List<RoleFilterModel>>(adminRoleBuiltinAuthz);

            foreach (var rule in rules) RegisteredRoleControllerRules.RegisterRoleControllerModel(rule);

            // TODO fix all this and get rules from Azure App Config
            foreach (var config in app.ApplicationServices.GetServices<IConfiguration>())
            {
                string configSource = config.GetType().Name;
                foreach (var kv in config.AsEnumerable())
                {
                    if (!kv.Key.StartsWith("Rule.", StringComparison.CurrentCultureIgnoreCase)) { continue; }

                    try
                    {
                        var rule = kv.Value;
                        if (rule == null) continue;
                        var rule_model = JsonConvert.DeserializeObject<RoleFilterModel>(rule);
                        if (string.IsNullOrEmpty(rule_model.LoadedFrom)) rule_model.LoadedFrom = configSource;
                        RegisteredRoleControllerRules.RegisterRoleControllerModel(rule_model);
                    }
                    catch (Exception e)
                    {
                        // TODO LOGME (Sean) Log failure to deserialize and store rule.
                        Trace.WriteLine($"Failed to deserialize key: {kv.Key}. Error:\n{e.Message}");
                    }
                }
            }

            return app;
        }
    }
}
