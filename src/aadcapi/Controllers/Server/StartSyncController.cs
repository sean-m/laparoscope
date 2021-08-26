using aadcapi.Models;
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
using Newtonsoft.Json;

namespace aadcapi.Controllers.Server
{
    /// <summary>
    /// Enables starting of Delta sync cycles no sooner than every 10 minutes.
    /// </summary>
    public class StartSyncController : ApiController
    {
        // GET: Connectors
        /// <summary>
        /// Executes Start-ADSyncSyncCycle -PolicyType Delta if it is not
        /// currently running and has not ran within the last 10 minutes.
        /// </summary>
        /// <returns>
        /// Results are returned as JSON. If a sync cycle was started the .Started
        /// property is true.
        /// 
        /// {
        ///   "Result": "Last sync was less than 10 minutes ago, 08/26/2021 19:13:47 UTC, not starting sync.",
        ///   "Started": false
        /// }
        /// </returns>
        public dynamic Get()
        {   
            var runner = new SimpleScriptRunner(aadcapi.Properties.Resources.Start_ADSyncDelta);
            runner.Run();
            var result = runner.Results.CapturePSResult<SyncResult>().FirstOrDefault();
            return Ok(result);
        }
    }
}