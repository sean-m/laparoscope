using aadcapi.Utils.Authorization;
using McAuthorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreamJsonRpc;
using System.IO.Pipes;
using System.Net;

namespace aadcapi.Controllers.Server
{
    [Authorize]
    public class ConnectorStatisticsController : Controller
    {
        /// <summary>
        /// Maps to Get-ADSyncConnectorStatistics cmdlet. This returns the current
        /// number of connectors and pending import and export operations for a given
        /// connector. Use case: if synchronization is taking much longer than usual, perhaps
        /// there are many more connectors (objects) to synchronize. Monitoring these
        /// results for change over time can give an indication of that.
        /// 
        /// Note: you must be authenticated with valid role claims before calling into
        /// this or you will receive a 403 Forbidden. Returning a 401 can result in an
        /// infinate redirect loop with Azure AD.
        /// </summary>
        /// <param name="Name">Name of a valid AADC connector.</param>
        public async Task<dynamic> GetAsync(string Name)
        {
            if (String.IsNullOrEmpty(Name))
            {
                throw new BadHttpRequestException("Must specify a valid connector name.");
            }

            // Construct an anonymous object as the Model for IsAuthorized so we can
            // pass in Connector as the context. This will allow the authorization engine
            // to re-use the rules for /api/Connector. If you have rights to view a given
            // connector, there is no reason you shouldn't see it's statistics.
            var roles = HttpContext.User?.RoleClaims();
            if (!Filter.IsAuthorized<dynamic>(new { Name = Name, ConnectorName = Name, Identifier = Name }, "Connector", roles)) {
                throw new HttpRequestException($"Allow policy not found.", null, HttpStatusCode.Unauthorized);
            }

            using (var stream = new NamedPipeClientStream(".", "Laparoscope", PipeDirection.InOut, PipeOptions.Asynchronous))
            {
                using (var jsonRpc = JsonRpc.Attach(stream))
                {
                    string function = "GetADSyncConnectorStatistics";
                    var result = await jsonRpc.InvokeAsync<dynamic>(function, Name.Trim());
                    return result;
                }
            }
        }
    }
}
