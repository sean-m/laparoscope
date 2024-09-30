using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreamJsonRpc;
using System.IO.Pipes;

namespace aadcapi.Controllers.Server
{
    [Authorize]
    public class ExportDeletionThresholdController : Controller
    {
        /// <summary>
        /// Maps to Get-ADSyncExportDeletionThreshold cmdlet. This gives enabled/disabled (1/0)
        /// status and the numeric value of the current threshold.
        /// </summary>
        public async Task<dynamic> GetAsync()
        {
            using (var stream = new NamedPipeClientStream(".", "Laparoscope", PipeDirection.InOut, PipeOptions.Asynchronous))
            {
                using (var jsonRpc = JsonRpc.Attach(stream))
                {
                    string function = "GetADSyncExportDeletionThreshold";
                    var result = await jsonRpc.InvokeAsync<dynamic>(function);
                    return result;
                }
            }
        }
    }
}
