using aadcapi.Utils.Authorization;
using SMM.Automation;
using SMM.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

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
        /// </summary>
        /// <param name="Name">Name of a valid AADC connector.</param>
        public dynamic Get(string Name)
        {
            if (String.IsNullOrEmpty(Name))
            {
                return BadRequest("Must specify a valid connector name.");
            }

            if (!this.IsAuthorized(new { Context = "Connector", Name = Name, Id = Name })) {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
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
