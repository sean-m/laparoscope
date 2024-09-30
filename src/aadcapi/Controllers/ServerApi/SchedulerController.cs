using aadcapi.Utils.Authorization;
using LaparoscopeShared;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using SMM.Automation;
using SMM.Helper;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Http.Description;

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
        [ResponseType(typeof(Dictionary<string, object>))]
        public dynamic Get()
        {
            using (var runner = new SimpleScriptRunner("Import-Module ADSync; Get-ADSyncScheduler"))
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
        public dynamic Post([FromBody] SyncScheduleSettings settings) {
            // Filter parameters to just include what's allowed
            if (null != settings.SyncCycleEnabled && !this.IsAuthorized(new {Setting = "SyncCycleEnabled"}))
            {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }
            if (null != settings.SchedulerSuspended && !this.IsAuthorized(new { Setting = "SchedulerSuspended" }))
            {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }
            if (null != settings.MaintenanceEnabled && !this.IsAuthorized(new { Setting = "MaintenanceEnabled" }))
            {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }
            if (!string.IsNullOrEmpty(settings.NextSyncCyclePolicyType) && !this.IsAuthorized(new { Setting = "NextSyncCyclePolicyType" }))
            {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }

            // Validate policy type if specified
            if (!string.IsNullOrEmpty(settings.NextSyncCyclePolicyType) && !validSyncPolicyTypes.IsMatch(settings.NextSyncCyclePolicyType.Trim())) {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            var parameters = new Dictionary<string, object>();
            if (null != settings.SyncCycleEnabled) { parameters.Add(nameof(settings.SyncCycleEnabled), settings.SyncCycleEnabled); }
            if (null != settings.SchedulerSuspended) { parameters.Add(nameof(settings.SchedulerSuspended), settings.SchedulerSuspended); }
            if (null != settings.MaintenanceEnabled) { parameters.Add(nameof(settings.MaintenanceEnabled), settings.MaintenanceEnabled); }
            if (null != settings.NextSyncCyclePolicyType) { parameters.Add(nameof(settings.NextSyncCyclePolicyType), settings.NextSyncCyclePolicyType); }

            using (var runner = new SimpleScriptRunner(@"
[CmdletBinding()]
param(
    [bool]$SyncCycleEnabled,
    [bool]$SchedulerSuspended,
    [bool]$MaintenanceEnabled,
    [string]$NextSyncCyclePolicyType
)

Set-AdSyncScheduler @PSBoundParameters
"))
            {
                runner.Parameters = parameters;
                runner.Run();

                if (runner.HadErrors)
                {
                    var err = runner.LastError ?? new Exception("Encountered an error in PowerShell but could not capture the exception.");
                    return InternalServerError(err);
                }

                var result = runner.Results.ToDict().FirstOrDefault();
                if (null == result || result.Count == 0) {
                    return Ok();
                }
                var response = Ok(result);
                return response;
            }
        }
    }
}