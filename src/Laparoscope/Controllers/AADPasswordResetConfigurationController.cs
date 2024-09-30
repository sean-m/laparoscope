using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreamJsonRpc;
using System.IO.Pipes;

namespace aadcapi.Controllers.Server
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AADPasswordResetConfigurationController : Controller
    {
        /// <summary>
        /// Maps to Get-ADSyncAADPasswordResetConfiguration cmdlet. Shows the status of 
        /// the tenant wide feature. Only applies to the AAD connector so that is automatically
        /// resolved. If more than one AAD connector is found an exception is thrown as this is
        /// not a supported configuration.
        /// </summary>
        public async Task<dynamic> GetAsync()
        {
            using (var stream = new NamedPipeClientStream(".", "Laparoscope", PipeDirection.InOut, PipeOptions.Asynchronous))
            {
                await stream.ConnectAsync();
                using (var jsonRpc = JsonRpc.Attach(stream)) {                
                    string function = "GetADSyncAADPasswordResetConfiguration";
                    var result = await jsonRpc.InvokeAsync<dynamic>(function);
                    return result;
                }
            }
        }
    }
}
