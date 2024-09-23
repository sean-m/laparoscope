using aadcapi.Utils;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using aadcapi.Models;
using aadcapi.Utils.Authorization;
using Microsoft.Configuration.ConfigurationBuilders;
using System.Configuration;
using System.Diagnostics;
using McAuthorization.Models;
using McAuthorization;

namespace aadcapi
{
    public partial class Startup
    {
        private void ConfigureAuthorizationFilters(IAppBuilder app)
        {
            var adminRoleBuiltinAuthz = @"[
                {'Role':'Admin','Context':'*','ClaimProperty':'','ClaimValue':'','ModelProperty':'*id*','ModelValue':'*','ModelValues':[]},
                {'Role':'Admin','Context':'*','ClaimProperty':'','ClaimValue':'','ModelProperty':'ConnectorName','ModelValue':'*','ModelValues':[]},
                {'Role':'Admin','Context':'Scheduler','ClaimProperty':null,'ClaimValue':null,'ModelProperty':'Setting','ModelValue':'SchedulerSuspended','ModelValues':null}
            ]";
            var rules = JsonConvert.DeserializeObject<List<RoleFilterModel>>(adminRoleBuiltinAuthz);

            foreach (var rule in rules) RegisteredRoleControllerRules.RegisterRoleControllerModel(rule);

            //Microsoft.Configuration.ConfigurationBuilders.AzureAppConfigurationBuilder()
            foreach (string k in ConfigurationManager.AppSettings.Keys)
            {
                if (! k.StartsWith("Rule.", StringComparison.CurrentCultureIgnoreCase)) { continue; }

                try
                {
                    var rule = ConfigurationManager.AppSettings[k];
                    var rule_model = JsonConvert.DeserializeObject<RoleFilterModel>(rule);
                    RegisteredRoleControllerRules.RegisterRoleControllerModel(rule_model);
                }
                catch (Exception e)
                {
                    // TODO LOGME (Sean) Log failure to deserialize and store rule.
                    Trace.WriteLine($"Failed to deserialize key: {k}. Error:\n{e.Message}");
                }
            }
        }
    }
}
