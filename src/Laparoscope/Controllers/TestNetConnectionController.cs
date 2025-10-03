using LaparoscopeShared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StreamJsonRpc;
using System.IO.Pipes;

namespace Laparoscope.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = Global.AuthSchemes, Roles = "Admin")]
    public class TestNetConnectionController : ControllerBase {
        [HttpGet]
        public async Task<NetConnectionStatus> GetAsync(string ComputerName, int? Port)
        {
            if (string.IsNullOrEmpty(ComputerName))
            {
                throw new BadHttpRequestException("Target computer must be specified by hostname or IP address.");
            }
            if (Port != null && (Port < 1 || Port > 65534))
            {
                throw new BadHttpRequestException("Must specify a port inside the range: 1-65534");
            }

            using (var stream = new NamedPipeClientStream(".", "Laparoscope", PipeDirection.InOut, PipeOptions.Asynchronous))
            {
                await stream.ConnectAsync();
                using (var jsonRpc = JsonRpc.Attach(stream))
                {
                    string function = "TestNetConnection";
                    List<object> parameters = new List<object>();

                    var result = await jsonRpc.InvokeAsync<NetConnectionStatus>(function, ComputerName.Trim(), Port);
                    return result;
                }
            }
        }
    }
}
