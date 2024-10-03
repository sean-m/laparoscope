using Laparoscope.Utils.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Threading;
using StreamJsonRpc;
using System.IO.Pipes;
using System.Net;

namespace Laparoscope.Controllers.Server
{
    [Route("api/[controller]")]
    [ApiController]
    /// <summary>
    /// Maps to the Get-ADSyncGlobalSettings command without the schema data.
    /// </summary>
    [Authorize]
    public class GlobalSettingsController : Controller
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
        public async Task<dynamic> GetAsync()
        {
            // Test for authorization rules for this context with ModelProperty: Authorized,  ModelValue: true
            if (!this.IsAuthorized(new { Authorized = true }))
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }

            using (var stream = new NamedPipeClientStream(".", "Laparoscope", PipeDirection.InOut, PipeOptions.Asynchronous))
            {
                await stream.ConnectAsync().WithTimeout(TimeSpan.FromSeconds(20));
                using (var jsonRpc = JsonRpc.Attach(stream))
                {
                    string function = "GetAdSyncGlobalSettingsStrict";
                    var result = await jsonRpc.InvokeAsync<dynamic>(function);
                    return result;
                }
            }
        }
    }
}
