using Laparoscope.Models;
using Laparoscope.Utils.Authorization;
using McAuthorization;
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
    public class DomainReachabilityStatusController : Controller
    {
        /// <summary>
        /// Maps to Get-ADSyncDomainReachabilityStatus cmdlet. This can be used for troubleshooting
        /// connectivity related no-start-ma sync status messages.
        ///
        /// Note: you must be authenticated with valid role claims before calling into
        /// this or you will receive a 403 Forbidden. Returning a 401 can result in an
        /// infinate redirect loop with Azure AD.
        /// </summary>
        /// <param name="ConnectorName">Name of the AADC connector for the target domain.</param>
        public async Task<DomainReachabilityStatus> GetAsync(string ConnectorName)
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

            using (var stream = new NamedPipeClientStream(".", "Laparoscope", PipeDirection.InOut, PipeOptions.Asynchronous))
            {
                await stream.ConnectAsync();
                using (var jsonRpc = JsonRpc.Attach(stream))
                {
                    string function = "GetADSyncDomainReachabilityStatus";
                    var result = await jsonRpc.InvokeAsync<DomainReachabilityStatus>(function, ConnectorName.Trim());
                    return result;
                }
            }
        }
    }
}
