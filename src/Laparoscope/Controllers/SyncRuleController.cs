using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreamJsonRpc;
using System.IO.Pipes;

namespace aadcapi.Controllers.Server
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SyncRuleController : Controller
    {
        // GET: Connectors
        /// <summary>
        /// Executes Get-ADSyncRule and returns sync rules the requestor is authorized to see.
        /// Can optionally take the identifier of a specific sync rule. Note: if an identifier 
        /// is specified and no matching authorization rule exists this does not result in a 403
        /// as authorization rules act as filters. The result is an empty set.
        /// </summary>
        /// <param name="Identifier">Guid identifier of a specific sync rule.</param>
        /// <returns></returns>
        public async Task<dynamic> GetAsync(string Identifier=null)
        {
            // TODO parse result and only return rules associated to authorized connectors
            using (var stream = new NamedPipeClientStream(".", "Laparoscope", PipeDirection.InOut, PipeOptions.Asynchronous))
            {
                await stream.ConnectAsync();
                using (var jsonRpc = JsonRpc.Attach(stream))
                {
                    string function = "GetADSyncRule";
                    var result = await jsonRpc.InvokeAsync<dynamic>(function, Identifier.Trim());
                    return result;
                }
            }
        }
    }
}