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

namespace aadcapi.Controllers.ServerApi
{
    [Authorize]
    public class SchedulerConnectorOverrideController : ApiController
    {
        // GET: api/SchedulerConnectorOverride
        /// <summary>
        /// Get the sync scheduler override status for a given connector. This indicates whether the 
        /// next scheduled sync cycle deviates from the standard scheduled delta sync policy. For
        /// standard delta sync policy, a delta import and delta sync operation are performed. Under
        /// certain circumstances, full import and full sync operations are required to clear errors
        /// these can be requested with a POST action to this endpoint.
        /// 
        /// The desired connector is specified with the ConnectorName query parameter. If none is
        /// specified, the result is an empty SchedulerOverride object payload which can be updated
        /// and sent back as the body of a POST method.
        /// 
        /// The requestor must be authorized to access the specified connector. Shares rules with
        /// the /api/Connector context.
        /// </summary>
        /// <param name="ConnectorName"></param>
        /// <returns>SchedulerOverride or SchedulerOverrideResult</returns>
        /// <exception cref="HttpResponseException"></exception>
        public dynamic Get(string ConnectorName=null)
        {
            if (string.IsNullOrEmpty(ConnectorName))
            {
                return new SchedulerOverride();
            }

            // Construct an anonymous object as the Model for IsAuthorized so we can
            // pass in Connector as the context. This will allow the authorization engine
            // to re-use the rules for /api/Connector. If you have rights to view a given
            // connector, there is no reason you shouldn't see its syncoverride status.
            var roles = ((ClaimsPrincipal)RequestContext.Principal).RoleClaims();
            if (!Filter.IsAuthorized<dynamic>(new { Name = ConnectorName, ConnectorName = ConnectorName, Identifier = ConnectorName }, "Connector", roles))
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }

            using (var runner = new SimpleScriptRunner(aadcapi.Properties.Resources.Get_ADSyncSchedulerConnectorOverride))
            {
                runner.Parameters.Add("ConnectorName", ConnectorName);
                runner.Run();
                var resultValues = runner.Results?.CapturePSResult<SchedulerOverrideResult>()?.FirstOrDefault();
                return resultValues;
            }
        }


        // POST: api/SchedulerConnectorOverride
        /// <summary>
        /// Allows specifying full import and/or full sync to be performed on a given connector
        /// as part of the next scheduled sync operation. Desired configuraiton is specified
        /// by sending a SchedulerOverride object as the request body. Object prototype can be
        /// requested with GET to this endpoint that does not specify a ConnectorName.
        /// 
        /// The requestor must be authorized to access the specified connector. Shares rules with
        /// the /api/Connector context.
        /// </summary>
        /// <param name="OverrideConfig"></param>
        /// <exception cref="HttpResponseException"></exception>
        public dynamic Post([FromBody] SchedulerOverride OverrideConfig)
        {
            string ConnectorName = OverrideConfig.ConnectorName;
            if (string.IsNullOrEmpty(ConnectorName)) {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            // Construct an anonymous object as the Model for IsAuthorized so we can
            // pass in Connector as the context. This will allow the authorization engine
            // to re-use the rules for /api/Connector. If you have rights to view a given
            // connector, there is no reason you shouldn't see its syncoverride status.
            var roles = ((ClaimsPrincipal)RequestContext.Principal).RoleClaims();
            if (!Filter.IsAuthorized<dynamic>(new { Name = ConnectorName, ConnectorName = ConnectorName, Identifier = ConnectorName }, "Connector", roles))
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }

            bool hadErrors = true;
            using (var runner = new SimpleScriptRunner(aadcapi.Properties.Resources.Set_ADSyncSchedulerConnectorOverride))
            {
                runner.Parameters.Add("ConnectorName", OverrideConfig.ConnectorName);
                runner.Parameters.Add("FullSyncRequired", OverrideConfig.FullSyncRequired);
                runner.Parameters.Add("FullImportRequired", OverrideConfig.FullImportRequired);
                runner.Run();
                runner.Results.ToDict();
                
                var resultValues = runner.Results.CapturePSResult<SchedulerOverrideResult>();
                if (hadErrors) { 
                    Request.CreateResponse(HttpStatusCode.InternalServerError); 
                }
                return resultValues;
            }
        }
    }
}
