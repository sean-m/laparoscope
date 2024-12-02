using Laparoscope.Utils.Authorization;
using LaparoscopeShared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Threading;
using StreamJsonRpc;
using System.IO;
using System.IO.Pipes;
using System.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Laparoscope.Controllers.Server
{
    [Route("api/[controller]")]
    [ApiController]
    /// <summary>
    /// Maps to the Get-ADSyncGlobalSettings command without the schema data.
    /// </summary>
    [Authorize]
    public class ProcessesController : Controller
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
        [HttpGet]
        public async Task<IEnumerable<WindowsTask>> GetAsync()
        {
            // Test for authorization rules for this context with ModelProperty: Authorized,  ModelValue: true
            if (!this.IsAuthorized(new { Authorized = true }))
            {
                throw new HttpRequestException("Not authorized.", null, HttpStatusCode.Unauthorized);
            }

            return await GetProcesses();
        }

        public static async Task<IEnumerable<WindowsTask>> GetProcesses()
        {
            using (var stream = new NamedPipeClientStream(".", "Laparoscope", PipeDirection.InOut, PipeOptions.Asynchronous))
            {
                await stream.ConnectAsync();
                using (var jsonRpc = JsonRpc.Attach(stream))
                {
                    string function = "GetAadcProcesses";
                    var result = await jsonRpc.InvokeAsync<IEnumerable<WindowsTask>>(function);
                    return result;
                }
            }
        }
    }
}
