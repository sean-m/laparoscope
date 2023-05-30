using aadcapi.Models;
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
    public class DomainReachabilityStatusController : ApiController
    {
        /// <summary>
        /// Maps to Get-ADSyncDomainReachabilityStatus cmdlet. This can be used for troubleshooting
        /// connectivity related no-start-ma sync status messages.
        ///
        /// Note: you must be authenticated with valid role claims before calling into
        /// this or you will receive a 403 Forbidden. Returning a 401 can result in an
        /// infinate redirect loop with Azure AD.
        /// </summary>
        /// <param name="ConnectorName">Name of the AADC connector for the target domain.</param>
        [ResponseType(typeof(DomainReachabilityStatus))]
        public dynamic Get(string ConnectorName)
        {
            // Construct an anonymous object as the Model for IsAuthorized so we can
            // pass in Connector as the context. This will allow the authorization engine
            // to re-use the rules for /api/Connector. If you have rights to view a given
            // connector, there is no reason you shouldn't see its reachability status.
            var roles = ((ClaimsPrincipal)RequestContext.Principal).RoleClaims();
            if (!Filter.IsAuthorized<dynamic>(new { Name = ConnectorName, 
                                                    ConnectorName = ConnectorName, 
                                                    Identifier = ConnectorName},
                                              "Connector", roles))
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }

            // Run PowerShell command to get AADC connector configurations
            using (var runner = new SimpleScriptRunner(aadcapi.Properties.Resources.Get_ADSyncDomainReachabilityStatus))
            {
                runner.Parameters.Add("ConnectorName", ConnectorName);
                runner.Run();

                // Map PowerShell objects to models, C# classes with matching property names.
                // All results should be DomainReachabilityStatus but CapturePSResult can return as
                // type: dynamic if the PowerShell object doesn't successfully map to the
                // desired model type. For those that are the correct model, we pass them
                // to the IsAuthorized method which loads and runs any rules for this connector
                // who match the requestors roles.
                var resultValues = runner.Results.CapturePSResult<DomainReachabilityStatus>()
                    .Where(x => x is DomainReachabilityStatus)     // Filter out results that couldn't be captured as DomainReachabilityStatus.
                    .Select(x => x as DomainReachabilityStatus);   // Take as DomainReachabilityStatus so typed call to WhereAuthorized avoids GetType() call.

                if (resultValues != null)
                {
                    var result = Ok(resultValues);
                    return result;
                }
            }
            return Ok();
        }
    }
}
