using Laparoscope.Models;
using Laparoscope.Utils.Authorization;
using McAuthorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Threading;
using StreamJsonRpc;
using System.IO.Pipes;
using System.Net;

using static Laparoscope.Utils.Authorization.ControllerAuthorizationExtensions;

namespace Laparoscope.Controllers.Server
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CSObjectController : Controller
    {
        /// <summary>
        /// Controller for the Get-ADSyncCSObject command. This is used to retreive a single record
        /// from a named connector space. Records are queried by DN only and must specify a valid connector
        /// name. The returned record contains a ConnectedMVObjectId property that can be used to inspect
        /// the corresponding metaverse record.
        /// 
        /// Note: records filtered on import may not resolve or may only exist in the connector space and
        /// will not have a ConnectedMVObjectId property.
        /// </summary>
        /// <param name="ConnectorName">
        /// Name of the connector space to query. Can be retreived from /api/Connector.
        /// </param>
        /// <param name="DistinguishedName">
        /// The full distinguished name for the desired connector space record. If querying an
        /// Active Directory connector it may look like this: CN=Super Great Person,CN=Cool Stuff,DC=my,DC=garage.
        /// </param>
        /// <returns>
        /// https://docs.microsoft.com/en-us/azure/active-directory/hybrid/reference-connect-adsync#get-adsynccsobject
        /// </returns>
        public async Task<dynamic> GetAsync(string ConnectorName, string DistinguishedName)
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

            if (string.IsNullOrEmpty(ConnectorName) || string.IsNullOrEmpty(DistinguishedName))
            {
                throw new BadHttpRequestException("Must specify ConnectorName and DistinguishedName.");
            }

            using (var stream = new NamedPipeClientStream(".", "Laparoscope", PipeDirection.InOut, PipeOptions.Asynchronous))
            {
                await stream.ConnectAsync().WithTimeout(TimeSpan.FromSeconds(20));
                using (var jsonRpc = JsonRpc.Attach(stream))
                {
                    string function = "GetADSyncCSObjectStrict";
                    var result = await jsonRpc.InvokeAsync<AadcCSObject>(function, ConnectorName.Trim(), DistinguishedName.Trim());
                    if (this.IsAuthorized<AadcCSObject>(result)) return result;
                }
            }

            return new object();
        }
    }
}