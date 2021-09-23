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
            // TODO load from Azure configuration.
            var adminRoleConnectorAuth = "{\"Role\":\"Admin\",\"Context\":\"*\",\"ClaimProperty\":\"\",\"ClaimValue\":\"\",\"ModelProperty\":\"*id*\",\"ModelValue\":\"*\",\"ModelValues\":[]}";
            RoleFilterModel deserialized = JsonConvert.DeserializeObject<RoleFilterModel>(adminRoleConnectorAuth);

            RegisteredRoleControllerRules.RegisterRoleControllerModel(deserialized);

            //Microsoft.Configuration.ConfigurationBuilders.AzureAppConfigurationBuilder()
            foreach (string k in ConfigurationManager.AppSettings.Keys)
            {
                if (! k.StartsWith("Rule.", StringComparison.CurrentCultureIgnoreCase)) { continue; }

                try
                {
                    var rule = ConfigurationManager.AppSettings[k];
                    deserialized = JsonConvert.DeserializeObject<RoleFilterModel>(rule);
                    RegisteredRoleControllerRules.RegisterRoleControllerModel(deserialized);
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
