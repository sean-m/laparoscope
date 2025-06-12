using Laparoscope.Utils.Authorization;
using LaparoscopeShared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Threading;
using StreamJsonRpc;
using System.IO.Pipes;
using System.Net;
using System.Text.RegularExpressions;

namespace Laparoscope.Controllers.Server
{
    [Route("api/[controller]")]
    [ApiController]
    /// <summary>
    /// Maps to cmdlets for the AADC Scheduler. Unqualified GET returns the result of Get-ADSyncScheduler.
    /// </summary>
    [Authorize(AuthenticationSchemes = Global.AuthSchemes, Policy ="HasRoles")]
    
    public class SchedulerController : Controller
    {
        // GET api/<controller>
        /// <summary>
        /// Executes Get-ADSyncScheduler and returns the result as a list of hashtables.
        /// </summary>
        /// <returns>List&lt; Dictionary &lt; string, object &gt; &gt;</returns>
        [HttpGet]
        public async Task<Dictionary<string, object>> GetAsync()
        {
            using (var stream = new NamedPipeClientStream(".", "Laparoscope", PipeDirection.InOut, PipeOptions.Asynchronous))
            {
                await stream.ConnectAsync();
                using (var jsonRpc = JsonRpc.Attach(stream))
                {
                    string function = "GetADSyncScheduler";
                    var result = await jsonRpc.InvokeAsync<Dictionary<string, object>>(function);
                    return result;
                }
            }
        }

        static Regex validSyncPolicyTypes = new Regex(@"(?i)(Unspecified|Delta|Initial)");
        /// <summary>
        /// Executes Set-ADSyncScheduler with the specified settings.
        /// The following settings are supported:
        ///     SyncCycleEnabled : boolean
        ///     SchedulerSuspended : boolean
        ///     MaintenanceEnabled : boolean
        ///     NextSyncCyclePolicyType : string, valid options: Unspecified | Delta | Initial
        /// 
        /// Settings must be passed as the body of the request. Permission to interact with individual settings
        /// must be granted by authorization policy. 
        /// 
        /// Example policy:
        /// {
        ///   "Id": "f447d93b-03ee-46af-a4ff-8ca37a310e63",
        ///   "Role": "Garage.Api",
        ///   "Context": "Scheduler",
        ///   "ClaimProperty": null,
        ///   "ClaimValue": null,
        ///   "ModelProperty": "Setting",
        ///   "ModelValue": "SchedulerSuspended",
        ///   "ModelValues": null
        /// }
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        /// <exception cref="HttpResponseException"></exception>
        [HttpPost]
        public async Task<dynamic> PostAsync([FromForm] SyncScheduleSettings settings) {
            // Filter parameters to just include what's allowed
            if (null != settings.SyncCycleEnabled && !this.IsAuthorized(new {Setting = "SyncCycleEnabled"}))
            {
                throw new HttpRequestException($"Allow policy not found.", null, HttpStatusCode.Unauthorized);
            }
            if (null != settings.SchedulerSuspended && !this.IsAuthorized(new { Setting = "SchedulerSuspended" }))
            {
                throw new HttpRequestException($"Allow policy not found.", null, HttpStatusCode.Unauthorized);
            }
            if (null != settings.MaintenanceEnabled && !this.IsAuthorized(new { Setting = "MaintenanceEnabled" }))
            {
                throw new HttpRequestException($"Allow policy not found.", null, HttpStatusCode.Unauthorized);
            }
            if (!string.IsNullOrEmpty(settings.NextSyncCyclePolicyType) && !this.IsAuthorized(new { Setting = "NextSyncCyclePolicyType" }))
            {
                throw new HttpRequestException($"Allow policy not found.", null, HttpStatusCode.Unauthorized);
            }

            // Validate policy type if specified
            if (!string.IsNullOrEmpty(settings.NextSyncCyclePolicyType) && !validSyncPolicyTypes.IsMatch(settings.NextSyncCyclePolicyType.Trim())) {
                throw new BadHttpRequestException("Invalid sync policy type specified.");
            }

            using (var stream = new NamedPipeClientStream(".", "Laparoscope", PipeDirection.InOut, PipeOptions.Asynchronous))
            {
                await stream.ConnectAsync();
                using (var jsonRpc = JsonRpc.Attach(stream))
                {
                    string function = "SetADSyncScheduler";
                    var result = await jsonRpc.InvokeAsync<dynamic>(function, settings);
                    return result;
                }
            }
        }
    }
}