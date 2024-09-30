using aadcapi.Utils.Authorization;
using SMM.Automation;
using SMM.Helper;
using StreamJsonRpc;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

namespace aadcapi.Controllers.Server
{
    /// <summary>
    /// Maps to the Get-ADSyncGlobalSettings command without the schema data.
    /// </summary>
    [Authorize]
    public class GlobalSettingsController : ApiController
    {
        /// <summary>
        /// Provides a subset of the Get-ADSyncGlobalSettings cmdlet output. The Parameters property has been filtered
        /// to promote Name, Value to Key=Value. Other properties on the Parameters values aren't admin tunable
        /// and have not varied for the last several AADC versions.
        /// </summary>
        /// <returns>
        /// Returns the: Version, InstanceId, SqlSchemaVersion, Parameters properties from Get-ADSyncGlobalSettings.
        /// Metavserse schema settings aren't very actionable without direct access to AADC so have been omitted.
        /// </returns>
        [ResponseType(typeof(Dictionary<string, object>))]
        public dynamic Get()
        {
            // Test for authorization rules for this context with ModelProperty: Authorized,  ModelValue: true
            if (!this.IsAuthorized(new { Authorized = true }))
            {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }

            using (var stream = new NamedPipeClientStream(".", "Laparoscope", PipeDirection.InOut, PipeOptions.Asynchronous))
            {
                using (var jsonRpc = JsonRpc.Attach(stream))
                {
                    string function = "GetAdSyncGlobalSettingsStrict";
                    var awaitableResult = jsonRpc.InvokeAsync<Dictionary<string, object>>(function);
                    awaitableResult.Wait();
                    var result = Ok(awaitableResult.Result);
                    return result;
                }                
            }
        }
    }
}
