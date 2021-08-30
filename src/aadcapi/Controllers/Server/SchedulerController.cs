using SMM.Automation;
using SMM.Helper;
using System.Linq;
using System.Web.Http;

namespace aadcapi.Controllers.Server
{
    /// <summary>
    /// Maps to cmdlets for the AADC Scheduler. Unqualified GET returns the result of Get-ADSyncScheduler.
    /// </summary>
    [Authorize]
    public class SchedulerController : ApiController
    {
        // GET api/<controller>
        /// <summary>
        /// Executes Get-ADSyncScheduler and returns the result as a list of hashtables.
        /// </summary>
        /// <returns>List&lt; Dictionary &lt; string, object &gt; &gt;</returns>
        public dynamic Get()
        {
            var runner = new SimpleScriptRunner("Import-Module ADSync; Get-ADSyncScheduler");
            runner.Run();
            var result = Ok(runner.Results.ToDict().FirstOrDefault());
            return result;
        }
    }
}