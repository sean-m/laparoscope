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
    public class AADCompanyFeatureController : Controller
    {
        /// <summary>
        /// Maps to Get-ADSyncAADCompanyFeature cmdlet. This returns enabled status for
        /// features that are global to the AADC install/tenat. These include: 
        /// PasswordHashSync, ForcePasswordChangeOnLogOn, UserWriteback, DeviceWriteback, UnifiedGroupWriteback, GroupWritebackV2.
        /// </summary>
        [HttpGet]
        public async Task<dynamic> GetAsync()
        {
            using (var stream = new NamedPipeClientStream(".", "Laparoscope", PipeDirection.InOut, PipeOptions.Asynchronous))
            {
                await stream.ConnectAsync();
                using (var jsonRpc = JsonRpc.Attach(stream))
                {
                    string function = "GetADSyncAADCompanyFeature";
                    var result = await jsonRpc.InvokeAsync<Dictionary<string, object>>(function);
                    return result;
                }
            }
        }
    }
}
