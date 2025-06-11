using Laparoscope.Models;
using Laparoscope.Utils.Authorization;
using McAuthorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Threading;
using StreamJsonRpc;
using System.IO.Pipes;
using System.Net;

using static Laparoscope.Utils.Authorization.ControllerAuthorizationExtensions;

namespace Laparoscope.Controllers.Server
{
    [Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy="HasRoles")]
    public class CSObjectPageController : Controller
    {
        /// <summary>
        /// Controller for the Get-ADSyncCSObject command. This is used to retreive pages of records
        /// from a named connector space. Records are queried by page number. Results don't have a distinct
        /// ordering and are not filtered server side as this is not a function of the Get-ADSyncCSObject cmdlet.
        /// 
        /// StartPage and PageSize parameters are nullable but that's just so they will be presented
        /// as null if elided from the request. Validation is performed server side to ensure that
        /// they are properly set. StartPage must be a positive integer between 0 and 99999, and PageSize must be
        /// between 1 and 250. 
        /// 
        /// Note: OData paging semantics don't cleanly map to the Get-ADSyncCSObject cmdlet so nextLink and 
        /// OData metadata are not returned. The caller is expected to handle paging themselves.
        /// 
        /// Note: records filtered on import may not resolve or may only exist in the connector space and
        /// will not have a ConnectedMVObjectId property.
        /// </summary>
        /// <param name="ConnectorName">
        /// Name of the connector space to query. Can be retreived from /api/Connector.
        /// </param>
        /// <param name="StartPage">
        /// </param>
        /// /// <param name="PageSize">
        /// </param>
        /// <returns>
        /// https://docs.microsoft.com/en-us/azure/active-directory/hybrid/reference-connect-adsync#get-adsynccsobject
        /// </returns>
        [HttpGet]
        public async Task<List<AadcCSObject>?> GetAsync(string ConnectorName, int? StartPage, int? PageSize)
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

            if (string.IsNullOrEmpty(ConnectorName) || StartPage == null || PageSize == null)
            {
                throw new BadHttpRequestException("Must specify ConnectorName and DistinguishedName");
            }

            if ((int)StartPage < 0)
            {
                throw new BadHttpRequestException("StartPage must be a positive integer between 0 and 99999");
            }

            if ((int)PageSize < 1 || (int)PageSize > 250)
            {
                throw new BadHttpRequestException("PageSize must be a positive integer between 1 and 250");
            }

            using (var stream = new NamedPipeClientStream(".", "Laparoscope", PipeDirection.InOut, PipeOptions.Asynchronous))
            {
                await stream.ConnectAsync();
                using (var jsonRpc = JsonRpc.Attach(stream))
                {
                    string function = "GetADSyncCSObjectPage";
                    var result = await jsonRpc.InvokeAsync<List<AadcCSObject>>(function, ConnectorName.Trim(), (int)StartPage, (int)PageSize);
                    return result;
                }
            }

            return null;
        }
    }
}