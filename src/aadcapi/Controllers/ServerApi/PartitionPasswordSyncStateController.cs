using aadcapi.Models;
using aadcapi.Utils.Authorization;
using SMM.Automation;
using SMM.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

namespace aadcapi.Controllers.Server
{
    [Authorize]
    public class PartitionPasswordSyncStateController : ApiController
    {
        /// <summary>
        /// Maps to the Get-ADSyncPartitionPasswordSyncState cmdlet. The cmdlet takes no
        /// parameters. Results are filtered by authorization rule, if it yields no results
        /// verify that hash sync is enabled and that there is a rule authorizing you to see 
        /// the status for your connector. 
        /// 
        /// This can be used to indicate issues with the password hash sync service on a per
        /// connector basis.
        /// </summary>
        /// <returns>
        /// Yields these properties: ConnectorId, DN, PasswordSyncLastSuccessfulCycleStartTimestamp, PasswordSyncLastSuccessfulCycleEndTimestamp, PasswordSyncLastCycleStartTimestamp, PasswordSyncLastCycleEndTimestamp, PasswordSyncLastCycleStatus.
        /// </returns>
        [ResponseType(typeof(IEnumerable<PasswordSyncState>))]
        public dynamic Get()
        {
            // Filter out the 'default' connector before yielding any results.
            var runner = new SimpleScriptRunner("Import-Module ADSync; Get-ADSyncPartitionPasswordSyncState | ? DN -notlike 'default'");
            runner.Run();

            if (runner.HadErrors)
            {
                var err = runner.LastError ?? new Exception("Encountered an error in PowerShell but could not capture the exception.");
                return InternalServerError(err);
            }

            // See the ConnectorController for more details on how the filtering is suppsoed to work.
            var resultValues = runner.Results.CapturePSResult<PasswordSyncState>()
                .Where(x => x is PasswordSyncState)
                .Select(x => x as PasswordSyncState);

            resultValues = this.WhereAuthorized<PasswordSyncState>(resultValues);

            // Without this, the enum value is stored as an integer. It's a hack
            // but still worth the flexibility of using dynamic types on the model.
            foreach (var r in resultValues)
                r.PasswordSyncLastCycleStatus = r.PasswordSyncLastCycleStatus?.ToString() ?? "Unknown";

            return Ok(resultValues);
        }
    }
}
