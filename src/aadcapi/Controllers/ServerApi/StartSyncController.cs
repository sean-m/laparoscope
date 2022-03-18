using aadcapi.Models;
using SMM.Automation;
using SMM.Helper;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;

namespace aadcapi.Controllers.Server
{
    /// <summary>
    /// Enables starting of Delta sync cycles no sooner than every 10 minutes.
    /// </summary>
    [Authorize]
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
        [ResponseType(typeof(SyncResult))]
        public dynamic Post()
        {   
            var runner = new SimpleScriptRunner(aadcapi.Properties.Resources.Start_ADSyncDelta);
            runner.Run();
            var result = runner.Results.CapturePSResult<SyncResult>().FirstOrDefault();
            return Ok(result);
        }
    }
}