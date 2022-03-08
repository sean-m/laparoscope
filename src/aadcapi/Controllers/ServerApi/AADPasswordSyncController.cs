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
    public class AADPasswordSyncController : ApiController
    {
        /// <summary>
        /// Maps to the Get-ADSyncAADPasswordSync cmdlet. This takes the source connector by name and
        /// returns whether the feature is enabled and the name of the AAD target connector.
        /// </summary>
        /// <param name="SourceConnector">Connector name.</param>
        /// <returns>
        /// Yields: SourceConnector name, TargetConnector name, Enabled password sync status.
        /// </returns>
        public dynamic Get(string SourceConnector)
        {
            if (String.IsNullOrEmpty(SourceConnector))
            {
                return BadRequest("Must indicate SourceConnector by name.");
            }
            
            var _connector = SourceConnector;
            if (!this.IsAuthorized(new { SourceConnector = _connector, Identifier = _connector }))  
            {
                // This will correctly evaluate with a wildcard rule like this: Context: *, ModelProperty: *id*, ModelValue: <connector name>.
                // Can be used to allow a given admin access to almost all information about their AADC connector. Simplifies access in 
                // multi domain setups where the AD administrators don't all belong to the same management structure.
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }

            var runner = new SimpleScriptRunner(aadcapi.Properties.Resources.Get_ADSyncAADPasswordSyncConfiguration);
            runner.Parameters.Add("SourceConnector", SourceConnector.Trim());
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
