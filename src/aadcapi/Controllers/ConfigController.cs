using aadcapi.Utils;
using Microsoft.Ajax.Utilities;
using SMM.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace aadcapi.Controllers
{
    [Authorize]
    public class ConfigController : ApiController
    {
        private readonly Type _global_t = typeof(Globals);

        public dynamic Get(string Name = null)
        {
            // Only users of the role specified as allowed to access config via the api may call this (Admin by default).
            if (!this.RequestContext.Principal.IsInRole(Globals.AuthorizedConfigRole)) { return Unauthorized(); }

            var nameOrId = String.IsNullOrEmpty(Name) ? this.RequestContext.RouteData.Values["id"]?.ToString() ?? String.Empty : Name.TrimStart('\'', '"').TrimEnd('\'', '"'); 
            var names = String.IsNullOrEmpty(nameOrId) ? new string[] { nameOrId } : nameOrId.Split(',');

            var appSettings = ConfigurationManager.AppSettings.AllKeys;
            var appSettingsResult = appSettings.Where(k => names.Any(n => k.Like(n))).Select(k =>  new PropertyValue { Setting = k, Value = k.Like("*Secret*") ? "*****************" : ConfigurationManager.AppSettings[k]});

            var globalsResult = _global_t.GetProperties().Where(p => names.Any(n => p.Name.Like(n))).Select(p => new PropertyValue { Setting = p.Name, Value = p.GetValue(null, null)});

            return Ok(Enumerable.Concat(appSettingsResult, globalsResult).DistinctBy(x => x.Setting));
        }

        public dynamic Put(string Name, string Value)
        {
            // Only users of the role specified as allowed to access config via the api may call this (Admin by default).
            if (!this.RequestContext.Principal.IsInRole(Globals.AuthorizedConfigRole)) { return Unauthorized(); }

            // TODO (Sean) Handle incoming Rule.* settings.

            return Ok();
        }

        internal class PropertyValue
        {
            public string Setting { get; set; }

            public dynamic Value { get; set; }
        }
    }
}
