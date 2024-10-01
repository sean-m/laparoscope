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
                {'Role':'Admin','Context':'*','ClaimProperty':'','ClaimValue':'','ModelProperty':'*id*','ModelValue':'*','ModelValues':[]},
                {'Role':'Admin','Context':'*','ClaimProperty':'','ClaimValue':'','ModelProperty':'ConnectorName','ModelValue':'*','ModelValues':[]},
                {'Role':'Admin','Context':'Scheduler','ClaimProperty':null,'ClaimValue':null,'ModelProperty':'Setting','ModelValue':'SchedulerSuspended','ModelValues':null}
            ]";
            var rules = JsonConvert.DeserializeObject<List<RoleFilterModel>>(adminRoleBuiltinAuthz);

            foreach (var rule in rules) RegisteredRoleControllerRules.RegisterRoleControllerModel(rule);

            // TODO fix all this and get rules from Azure App Config
            foreach (var config in app.ApplicationServices.GetServices<IConfiguration>())
            {
                foreach (var kv in config.AsEnumerable())
                {
                    if (!kv.Key.StartsWith("Rule.", StringComparison.CurrentCultureIgnoreCase)) { continue; }

                    try
                    {
                        var rule = kv.Value;
                        if (rule == null) continue;
                        var rule_model = JsonConvert.DeserializeObject<RoleFilterModel>(rule);
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
