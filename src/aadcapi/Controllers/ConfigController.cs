﻿using aadcapi.Utils;
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

        /// <summary>
        /// Endpoint for querying runtime application settings. Settings can be specified by
        /// the Setting query parameter as a single setting name or comma separated list of names.
        /// Wildcards are supported. If no Setting is specified, an empty set is returned. You can
        /// return a single setting name by route /api/Config/[Setting]. If both are specified,
        /// query string wins.
        /// </summary>
        /// <param name="Setting">Single or comma separated list of setting names.</param>
        /// <returns>Array of 0 or more values: [ { Setting:name, Value:value } ].</returns>
        public dynamic Get(string Setting = null)
        {
            // Only users of the role specified as allowed to access config via the api may call this (Admin by default).
            if (!this.RequestContext.Principal.IsInRole(Globals.AuthorizedConfigRole)) { return Unauthorized(); }

            // Take either the Setting variable from the query string or 'id' property implicitly from the route. Trim quote marks.
            var nameOrId = String.IsNullOrEmpty(Setting) ? this.RequestContext.RouteData.Values["id"]?.ToString() ?? String.Empty : Setting.Trim('\'', '"');
            // Tokenize the comma separated input value.
            var names = String.IsNullOrEmpty(nameOrId) ? new string[] { nameOrId } : nameOrId?.Split(',')?.Select(x => x.Trim());

            var appSettings = ConfigurationManager.AppSettings.AllKeys;
            var appSettingsResult = appSettings.Where(k => names.Any(n => k.Like(n))).Select(k =>  new PropertyValue { Setting = k, Value = k.Like("*Secret*") ? "*****************" : ConfigurationManager.AppSettings[k]});

            var globalsResult = _global_t.GetProperties().Where(p => names.Any(n => p.Name.Like(n))).Select(p => new PropertyValue { Setting = p.Name, Value = p.GetValue(null, null)});

            return Ok(Enumerable.Concat(appSettingsResult, globalsResult).DistinctBy(x => x.Setting));
        }

        public dynamic Put(string Setting, string Value)
        {
            // Only users of the role specified as allowed to access config via the api may call this (Admin by default).
            if (!this.RequestContext.Principal.IsInRole(Globals.AuthorizedConfigRole)) { return Unauthorized(); }

            // TODO (Sean) Handle incoming Rule.* settings.
            
            // Changes to secrets are not supported via the api. Should be automated with
            // CI/CD or secure credential storage: Azure Key Vault, etc.
            if (Setting.Like("*Secret*")) {
                var response = new HttpResponseMessage(HttpStatusCode.MethodNotAllowed);
                return response;
            }


            return Ok();
        }

        internal class PropertyValue
        {
            public string Setting { get; set; }

            public dynamic Value { get; set; }
        }
    }
}
