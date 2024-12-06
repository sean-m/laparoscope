using Laparoscope.Models;
using Laparoscope.Utils.Authorization;
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
    public class PartitionPasswordSyncStateController : Controller
    {
        /// <summary>
        /// Maps to the Get-ADSyncPartitionPasswordSyncState cmdlet. The cmdlet takes no
        /// parameters. Results are filtered by authorization rule, if it yields no results
        /// verify that hash sync is enabled and that there is a rule authorizing you to see 
        /// the status for your connector. 
        /// 
        /// This can be used to indicate issues with the password hash sync service on a per
        /// connector basis.
        /// </summary>
        /// <returns>
        /// Yields these properties: ConnectorId, DN, PasswordSyncLastSuccessfulCycleStartTimestamp, PasswordSyncLastSuccessfulCycleEndTimestamp, PasswordSyncLastCycleStartTimestamp, PasswordSyncLastCycleEndTimestamp, PasswordSyncLastCycleStatus.
        /// </returns>
        [HttpGet]
        public async Task<IEnumerable<PasswordSyncState>> GetAsync()
        {
            using (var stream = new NamedPipeClientStream(".", "Laparoscope", PipeDirection.InOut, PipeOptions.Asynchronous))
            {
                await stream.ConnectAsync();
                using (var jsonRpc = JsonRpc.Attach(stream))
                {
                    string function = "GetADSyncPartitionPasswordSyncState";
                    var result = await jsonRpc.InvokeAsync<IEnumerable<PasswordSyncState>>(function);
                    result = this.WhereAuthorized<PasswordSyncState>(result);
                    return result;
                }
            }
        }
    }
}
