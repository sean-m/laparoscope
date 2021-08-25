using SMM.Automation;
using SMM.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Mvc;

namespace aadcapi.Controllers.Server
{   
    /// <summary>
    /// Maps to cmdlets for the AADC Scheduler. Unqualified GET returns the result of Get-ADSyncScheduler.
    /// </summary>
    public class SchedulerController : ApiController
    {
        // GET api/<controller>
        /// <summary>
        /// Get-ADSyncScheduler
        /// </summary>
        /// <returns>Dictionary&lt;string,object&gt;</returns>
        public dynamic Get()
        {
            var runner = new SimpleScriptRunner("Import-Module ADSync; Get-ADSyncScheduler");
            runner.Run();
            var result = Ok(runner.Results.ToDict());
            return result;
        }
    }
}