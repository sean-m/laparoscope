using aadcapi.Models;
using aadcapi.Utils.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
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

                    // This is just plain horrible but it's the only way I could thing to get it working
                    // If connector.Partitions only has a single value, PowerShell will box the property
                    // as a single value even though it's a collection. It's trying to do us a favor in
                    // a terrible way.
                    foreach (var r in resultValues)
                    {
                        if (r.Partitions == null) continue;

                        if (r.Partitions is JObject jpartition)
                        {
                            if (jpartition.Type == JTokenType.Array)
                            {
                                try
                                {
                                    var partitionList = jpartition.ToObject<AadcConnector.Partition[]>();
                                    r.Partitions = partitionList;
                                }
                                catch { }
                            } else
                            {
                                try
                                {
                                    var partition = jpartition.ToObject<AadcConnector.Partition>();
                                    r.Partitions = partition;
                                }
                                catch { }
                            }
                        }
                    }
                    return result;
                }
            }
        }
    }
}