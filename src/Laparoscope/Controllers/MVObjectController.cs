using Laparoscope.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Threading;
using StreamJsonRpc;
using System.IO.Pipes;

namespace Laparoscope.Controllers.Server
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy="HasRoles")]
    public class MVObjectController : Controller
    {
        /// <summary>
        /// Controller for the Get-ADSyncMVObject command. Records are only retrieved by ObjectID
        /// which can only be discovered from an unbounded metaverse search (not allowed by this
        /// api) or by cross walking from the connector space.
        /// 
        /// Note: if validating a merged
        /// identity (one that exists in multiple source connector spaces), inspect the 'Lineage'
        /// property for all connectors joined to the metaverse record.
        /// </summary>
        /// <param name="Id">Guid identifying a single metaverse record.</param>
        /// <returns>
        /// https://docs.microsoft.com/en-us/azure/active-directory/hybrid/reference-connect-adsync#get-adsyncmvobject
        /// </returns>
        // GET api/<controller>
        [HttpGet]
        public async Task<dynamic> GetAsync(string Id)
        {
            // TODO require an MVObject role
            using (var stream = new NamedPipeClientStream(".", "Laparoscope", PipeDirection.InOut, PipeOptions.Asynchronous))
            {
                await stream.ConnectAsync();
                using (var jsonRpc = JsonRpc.Attach(stream))
                {
                    string function = "GetADSyncMVObjectStrict";
                    var result = await jsonRpc.InvokeAsync<AadcCSObject>(function, Id.Trim());
                    return result;
                }
            }
        }
    }
}