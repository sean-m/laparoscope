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
    public class AutoUpgradeController : ApiController
    {
        /// <summary>
        /// Maps to Get-ADSyncAutoUpgrade. This feature is only available when using LocalDB
        /// in a supported configuration. If status is not "Enabled" detail is given as to why.
        /// </summary>
        /// <returns></returns>
        public dynamic Get()
        {
            var runner = new SimpleScriptRunner(Properties.Resources.Get_ADSyncAutoUpgrade);
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
