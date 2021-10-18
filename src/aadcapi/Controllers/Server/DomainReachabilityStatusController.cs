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

namespace aadcapi.Controllers.Server
{
    [Authorize]
    public class DomainReachabilityStatusController : ApiController
    {
        /// <summary>
        /// Maps to Get-ADSyncDomainReachabilityStatus cmdlet. This can be used for troubleshooting
        /// connectivity related no-start-ma sync status messages.
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public dynamic Get(string Name)
        {
            // Run PowerShell command to get AADC connector configurations
            var runner = new SimpleScriptRunner(aadcapi.Properties.Resources.Get_ADSyncDomainReachabilityStatus);
            runner.Parameters.Add("ConnectorName", Name);
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

            resultValues = this.WhereAuthorized<DomainReachabilityStatus>(resultValues);

            if (resultValues != null)
            {
                var result = Ok(resultValues);
                return result;
            }

            return Ok();
        }
    }
}
