using aadcapi.Models;
using aadcapi.Utils.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreamJsonRpc;
using System.IO.Pipes;

namespace aadcapi.Controllers.Server
{
    [Route("api/[controller]")]
    [ApiController]
    /// <summary>
    /// Unqualified GET returns a subset of information for connectors. With multiple connectors
    /// the result can be 100MB+ so some properties are left out.
    /// </summary>
    [Authorize]
    public class ConnectorController : Controller
    {
        // GET: Connectors
        /// <summary>
        /// Executes Get-ADSyncConnector and returns a subset of the properties. This will help
        /// to map a connector name to identifier for other cmdlets that only take identifier.
        /// </summary>
        /// <param name="Name">Name of a specific connector to return.</param>
        /// <returns></returns>
        public async Task<IEnumerable<AadcConnector>> GetAsync(string Name=null)
        {

            using (var stream = new NamedPipeClientStream(".", "Laparoscope", PipeDirection.InOut, PipeOptions.Asynchronous))
            {
                await stream.ConnectAsync();
                using (var jsonRpc = JsonRpc.Attach(stream))
                {
                    var result = await jsonRpc.InvokeAsync<AadcConnector[]>("GetADSyncConnector", Name);

                    var resultValues = this.WhereAuthorized<AadcConnector>(result);
                    return result;
                }
            }
        }
    }
}