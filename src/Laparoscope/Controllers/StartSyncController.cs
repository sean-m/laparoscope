using aadcapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreamJsonRpc;
using System.IO.Pipes;

namespace aadcapi.Controllers.Server
{
    /// <summary>
    /// Enables starting of Delta sync cycles no sooner than every 10 minutes.
    /// </summary>
    [Authorize]
    public class StartSyncController : Controller
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
        public async Task<SyncResult> PostAsync()
        {
            using (var stream = new NamedPipeClientStream(".", "Laparoscope", PipeDirection.InOut, PipeOptions.Asynchronous))
            {
                using (var jsonRpc = JsonRpc.Attach(stream))
                {
                    string function = "StartADSync";
                    var result = await jsonRpc.InvokeAsync<SyncResult>(function);
                    return result;
                }
            }
        }
    }
}