﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Threading;
using StreamJsonRpc;
using System.IO.Pipes;

namespace Laparoscope.Controllers.Server
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = Global.AuthSchemes, Policy ="HasRoles")]
    public class ExportDeletionThresholdController : Controller
    {
        /// <summary>
        /// Maps to Get-ADSyncExportDeletionThreshold cmdlet. This gives enabled/disabled (1/0)
        /// status and the numeric value of the current threshold.
        /// </summary>
        [HttpGet]
        public async Task<dynamic> GetAsync()
        {
            using (var stream = new NamedPipeClientStream(".", "Laparoscope", PipeDirection.InOut, PipeOptions.Asynchronous))
            {
                await stream.ConnectAsync();
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
