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
    public class AADCompanyFeatureController : ApiController
    {
        /// <summary>
        /// Maps to Get-ADSyncAADCompanyFeature cmdlet. This returns enabled status for
        /// features that are global to the AADC install/tenat. These include: 
        /// PasswordHashSync, ForcePasswordChangeOnLogOn, UserWriteback, DeviceWriteback, UnifiedGroupWriteback, GroupWritebackV2.
        /// </summary>
        public dynamic Get()
        { 
            var runner = new SimpleScriptRunner("Get-ADSyncAADCompanyFeature");
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
