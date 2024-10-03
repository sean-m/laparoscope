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
    [Authorize]
    public class AADPasswordSyncController : Controller
    {
        /// <summary>
        /// Maps to the Get-ADSyncAADPasswordSync cmdlet. This takes the source connector by name and
        /// returns whether the feature is enabled and the name of the AAD target connector.
        /// </summary>
        /// <param name="SourceConnector">Connector name.</param>
        /// <returns>
        /// Yields: SourceConnector name, TargetConnector name, Enabled password sync status.
        /// </returns>
        public async Task<dynamic> GetAsync(string SourceConnector)
        {
            if (String.IsNullOrEmpty(SourceConnector))
            {
                return BadRequest("Must indicate SourceConnector by name.");
            }
            
            var _connector = SourceConnector;
            if (!this.IsAuthorized(new { SourceConnector = _connector, Identifier = _connector }))  
            {
                // This will correctly evaluate with a wildcard rule like this: Context: *, ModelProperty: *id*, ModelValue: <connector name>.
                // Can be used to allow a given admin access to almost all information about their AADC connector. Simplifies access in 
                // multi domain setups where the AD administrators don't all belong to the same management structure.
                throw new HttpRequestException($"Allow policy not found.", null, HttpStatusCode.Unauthorized);
            }

            using (var stream = new NamedPipeClientStream(".", "Laparoscope", PipeDirection.InOut, PipeOptions.Asynchronous))
            {
                await stream.ConnectAsync().WithTimeout(TimeSpan.FromSeconds(20));
                using (var jsonRpc = JsonRpc.Attach(stream))
                {
                    string function = "GetADSyncAADPasswordSyncConfiguration";
                    var result = await jsonRpc.InvokeAsync<dynamic>(function);
                    return result;
                }
            }
        }
    }
}
