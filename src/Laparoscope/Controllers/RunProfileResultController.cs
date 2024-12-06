using Laparoscope.Utils.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Threading;
using StreamJsonRpc;
using System.IO.Pipes;

namespace Laparoscope.Controllers.ServerApi
{
    [Route("api/[controller]")]
    [ApiController]
    /// <summary>
    /// Unqualified GET returns sync history for the last hour. Results are filtered
    /// to connectors you have rights to view in this context.
    /// </summary>
    [Authorize(Policy="HasRoles")]
    public class RunProfileResultController : Controller
    {
        /// <summary>
        /// Executes Get-ADSyncRunProfileResult and filters out entries older than an hour.
        /// </summary>
        /// <param name="RunHistoryId">For retreiving a specific run profile instance.</param>
        /// <param name="ConnectorId">The guid of a specific container. Can be retreived from: /api/Connector.</param>
        /// <param name="NumberRequested">Positive integer limiting the results. Note this limits results
        /// before authorization filtering. The returned set may be less than you specify if your query
        /// was not scoped to a connector you have rights to. Default: 0, no limit.</param>
        /// <param name="RunStepDetails">Indicates whether you'd like detailed error information included. Default: false.</param>
        /// <returns>
        /// Example:
        ///   RunHistoryId      : 8a654dc1-cf23-40ed-86f4-b745bd553c22
        ///   ConnectorId       : fffb4a69-4ed6-444d-8b89-73bc16f373dd
        ///   ConnectorName     : garage.mcardletech.com
        ///   RunProfileId      : e73df090-c632-4a61-961a-87ae047570c8
        ///   RunProfileName    : Export
        ///   RunNumber         : 26285
        ///   Username          : NT SERVICE\ADSync
        ///   IsRunComplete     : True
        ///   Result            : no-start-connection
        ///   CurrentStepNumber : 1
        ///   TotalSteps        : 1
        ///   StartDate         : 2022-04-08T20:13:31.163
        ///   EndDate           : 2022-04-08T20:13:52.187
        ///   RunStepResults    : 
        /// </returns>
        [HttpGet]
        public async Task<IEnumerable<Dictionary<string, object>>> GetAsync(Guid? RunHistoryId=null, Guid? ConnectorId=null, int NumberRequested=0, bool RunStepDetails=false)
        {
            using (var stream = new NamedPipeClientStream(".", "Laparoscope", PipeDirection.InOut, PipeOptions.Asynchronous))
            {
                await stream.ConnectAsync();
                using (var jsonRpc = JsonRpc.Attach(stream))
                {
                    string function = "GetADSyncRunProfileLastHour";
                    var result = await jsonRpc.InvokeAsync<IEnumerable<Dictionary<string, object>>>(function, RunHistoryId, ConnectorId, NumberRequested, RunStepDetails);
                    result = this.WhereAuthorized(result);
                    return result;
                }
            }
        }
    }
}