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
    public class ExportDeletionThresholdController : ApiController
    {
        /// <summary>
        /// Maps to Get-ADSyncExportDeletionThreshold cmdlet. This gives enabled/disabled (1/0)
        /// status and the numeric value of the current threshold.
        /// </summary>
        [ResponseType(typeof(Dictionary<string, object>))]
        public dynamic Get()
        {
            var runner = new SimpleScriptRunner("Get-ADSyncExportDeletionThreshold");
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
