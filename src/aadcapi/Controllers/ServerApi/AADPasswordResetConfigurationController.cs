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
    public class AADPasswordResetConfigurationController : ApiController
    {
        /// <summary>
        /// Maps to Get-ADSyncAADPasswordResetConfiguration cmdlet. Shows the status of 
        /// the tenant wide feature. Only applies to the AAD connector so that is automatically
        /// resolved. If more than one AAD connector is found an exception is thrown as this is
        /// not a supported configuration.
        /// </summary>
        [ResponseType(typeof(Dictionary<string, object>))]
        public dynamic Get()
        {
            using (var runner = new SimpleScriptRunner(Properties.Resources.Get_ADSyncAADPasswordResetConfiguration))
            {
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
}
