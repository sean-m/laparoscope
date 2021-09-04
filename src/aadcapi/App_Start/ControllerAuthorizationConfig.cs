﻿using aadcapi.Utils;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using aadcapi.Models;

namespace aadcapi
{
    public partial class Startup
    {
        private void ConfigureAuthorizationFilters(IAppBuilder app)
        {
            // TODO load from Azure configuration.
            var adminRoleConnectorAuth = @"{""Role"":""Admin"",""Controller"":""Connector"",""ModelProperty"":""Name"",""ModelValue"":""*garage*""}";
            var deserialized = JsonConvert.DeserializeObject<RoleControllerModel>(adminRoleConnectorAuth);
            
            RegisteredRoleControllerRules.RegisterRoleControllerModel(deserialized);
            
        }
    }
}
