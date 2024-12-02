using Laparoscope.Models;
using Laparoscope.Utils.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Threading;
using Newtonsoft.Json.Linq;
using StreamJsonRpc;
using System.IO.Pipes;

namespace Laparoscope.Controllers.Server
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
        [HttpGet]
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
                        if (r.Partitions != null) 
                            r.Partitions = JObjectToTypeOrCollectionOfType<Partition>(r.Partitions);
                        
                        if (r.AnchorConstructionSettings != null)
                            r.AnchorConstructionSettings = JObjectToTypeOrCollectionOfType<AnchorConstructionSetting>(r.AnchorConstructionSettings);
                    }
                    return result;
                }
            }
        }

        private dynamic JObjectToTypeOrCollectionOfType<T>(dynamic input)
        {
            if (input == null) return input;
            if (input is JObject jobj)
            {
                return JObjectToTypeOrCollectionOfType<T>(jobj);
            }
            else if (input is JArray jarray)
            {
                return JObjectToTypeOrCollectionOfType<T>(jarray);
            }

            // All else failed
            return input;
        }

        private dynamic JObjectToTypeOrCollectionOfType<T>(JObject jobject)
        {
            if (jobject.Type == JTokenType.Array)
            {
                try
                {
                    var list = jobject.ToObject<T[]>();
                    return list;
                }
                catch { }
            }
            else
            {
                try
                {
                    var record = jobject.ToObject<T>();
                    return record;
                }
                catch { }
            }
            // All else failed
            return jobject;
        }

        private dynamic JObjectToTypeOrCollectionOfType<T>(JArray jobject)
        {
            if (jobject.Type == JTokenType.Array)
            {
                try
                {
                    var list = jobject.ToObject<T[]>();
                    return list;
                }
                catch { }
            }
            else
            {
                try
                {
                    var record = jobject.ToObject<T>();
                    return record;
                }
                catch { }
            }
            // All else failed
            return jobject;
        }
    }
}