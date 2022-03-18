using aadcapi.Utils.Authorization;
using McAuthorization;
using SMM.Automation;
using SMM.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Description;

namespace aadcapi.Controllers.Server
{
    [Authorize]
    public class ConnectorStatisticsController : ApiController
    {
        /// <summary>
        /// Maps to Get-ADSyncConnectorStatistics cmdlet. This returns the current
        /// number of connectors and pending import and export operations for a given
        /// connector. Use case: if synchronization is taking much longer than usual, perhaps
        /// there are many more connectors (objects) to synchronize. Monitoring these
        /// results for change over time can give an indication of that.
        /// 
        /// Note: you must be authenticated with valid role claims before calling into
        /// this or you will receive a 403 Forbidden. Returning a 401 can result in an
        /// infinate redirect loop with Azure AD.
        /// </summary>
        /// <param name="Name">Name of a valid AADC connector.</param>
        [ResponseType(typeof(Dictionary<string,object>))]
        public dynamic Get(string Name)
        {
            if (String.IsNullOrEmpty(Name))
            {
                return BadRequest("Must specify a valid connector name.");
            }

            // Construct an anonymous object as the Model for IsAuthorized so we can
            // pass in Connector as the context. This will allow the authorization engine
            // to re-use the rules for /api/Connector. If you have rights to view a given
            // connector, there is no reason you shouldn't see it's statistics.
            var roles = ((ClaimsPrincipal)RequestContext.Principal).RoleClaims();
            if (!Filter.IsAuthorized<dynamic>(new { Name = Name, ConnectorName = Name, Identifier = Name }, "Connector", roles)) {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }

            // Run PowerShell command to get AADC connector configurations
            var runner = new SimpleScriptRunner("param($ConnectorName) Get-ADSyncConnectorStatistics -ConnectorName $ConnectorName");
            runner.Parameters.Add("ConnectorName", Name);
            runner.Run();

            if (runner.HadErrors)
            {
                var err = runner.LastError ?? new Exception("Encountered an error in PowerShell but could not capture the exception.");
                return InternalServerError(err);
            }

            var result = Ok(runner.Results.ToDict().FirstOrDefault());
            return result;
        }
    }
}
