using Laparoscope.Models;
using Laparoscope.Utils.Authorization;
using McAuthorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreamJsonRpc;
using System.IO.Pipes;
using System.Net;

namespace Laparoscope.Controllers.ServerApi
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SchedulerConnectorOverrideController : Controller
    {
        // GET: api/SchedulerConnectorOverride
        /// <summary>
        /// Get the sync scheduler override status for a given connector. This indicates whether the 
        /// next scheduled sync cycle deviates from the standard scheduled delta sync policy. For
        /// standard delta sync policy, a delta import and delta sync operation are performed. Under
        /// certain circumstances, full import and full sync operations are required to clear errors
        /// these can be requested with a POST action to this endpoint.
        /// 
        /// The desired connector is specified with the ConnectorName query parameter. If none is
        /// specified, the result is an empty SchedulerOverride object payload which can be updated
        /// and sent back as the body of a POST method.
        /// 
        /// The requestor must be authorized to access the specified connector. Shares rules with
        /// the /api/Connector context.
        /// </summary>
        /// <param name="ConnectorName"></param>
        /// <returns>SchedulerOverride or SchedulerOverrideResult</returns>
        /// <exception cref="HttpResponseException"></exception>
        public async Task<dynamic> GetAsync(string ConnectorName=null)
        {
            // Construct an anonymous object as the Model for IsAuthorized so we can
            // pass in Connector as the context. This will allow the authorization engine
            // to re-use the rules for /api/Connector. If you have rights to view a given
            // connector, there is no reason you shouldn't see it's statistics.
            var roles = HttpContext.User?.RoleClaims();
            if (!Filter.IsAuthorized<dynamic>(new { Name = ConnectorName, ConnectorName = ConnectorName, Identifier = ConnectorName }, "Connector", roles))
            {
                throw new HttpRequestException($"Allow policy not found.", null, HttpStatusCode.Unauthorized);
            }

            if (string.IsNullOrEmpty(ConnectorName))
            {
                throw new BadHttpRequestException("Must specify ConnectorName.");
            }

            using (var stream = new NamedPipeClientStream(".", "Laparoscope", PipeDirection.InOut, PipeOptions.Asynchronous))
            {
                await stream.ConnectAsync();
                using (var jsonRpc = JsonRpc.Attach(stream))
                {
                    string function = "GetADSyncSchedulerConnectorOverride";
                    var result = await jsonRpc.InvokeAsync<dynamic>(function, ConnectorName.Trim());
                    return result;
                }
            }
        }


        // POST: api/SchedulerConnectorOverride
        /// <summary>
        /// Allows specifying full import and/or full sync to be performed on a given connector
        /// as part of the next scheduled sync operation. Desired configuraiton is specified
        /// by sending a SchedulerOverride object as the request body. Object prototype can be
        /// requested with GET to this endpoint that does not specify a ConnectorName.
        /// 
        /// The requestor must be authorized to access the specified connector. Shares rules with
        /// the /api/Connector context.
        /// </summary>
        /// <param name="OverrideConfig"></param>
        /// <exception cref="HttpResponseException"></exception>
        public async Task<dynamic> PostAsync([FromBody] SchedulerOverride OverrideConfig)
        {
            string ConnectorName = OverrideConfig.ConnectorName;
            if (string.IsNullOrEmpty(ConnectorName)) {
                throw new BadHttpRequestException("Specified OverrideConfig could not be parsed.");
            }

            // Construct an anonymous object as the Model for IsAuthorized so we can
            // pass in Connector as the context. This will allow the authorization engine
            // to re-use the rules for /api/Connector. If you have rights to view a given
            // connector, there is no reason you shouldn't see it's statistics.
            var roles = HttpContext.User?.RoleClaims();
            if (!Filter.IsAuthorized<dynamic>(new { Name = ConnectorName, ConnectorName = ConnectorName, Identifier = ConnectorName }, "Connector", roles))
            {
                throw new HttpRequestException($"Allow policy not found.", null, HttpStatusCode.Unauthorized);
            }


            using (var stream = new NamedPipeClientStream(".", "Laparoscope", PipeDirection.InOut, PipeOptions.Asynchronous))
            {
                await stream.ConnectAsync();
                using (var jsonRpc = JsonRpc.Attach(stream))
                {
                    string function = "SetADSyncSchedulerConnectorOverride";
                    var result = await jsonRpc.InvokeAsync<dynamic>(function, OverrideConfig);
                    return result;
                }
            }
        }
    }
}
