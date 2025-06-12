using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;
using StreamJsonRpc;
using System.IO.Pipes;
using System.Threading;

namespace Laparoscope.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = Global.AuthSchemes, Policy ="HasRoles")]
    public class SidecarStatusController : ControllerBase
    {
        ILogger logger;
        public SidecarStatusController(ILogger<SidecarStatusController> logger)
        {
            this.logger = logger;
        }

        [HttpGet]
        public async Task<SidecarStatus> GetAsync(int timeout=20)
        {
            var watch = new System.Diagnostics.Stopwatch();
            var result = new SidecarStatus();

            watch.Start();
            try
            {
                using (var stream = new NamedPipeClientStream(".", "Laparoscope", PipeDirection.InOut, PipeOptions.Asynchronous))
                {
                    await stream.ConnectAsync().WithTimeout(TimeSpan.FromSeconds(timeout));
                    using (var jsonRpc = JsonRpc.Attach(stream))
                    {
                        await jsonRpc.InvokeAsync<string>("Hello");
                        watch.Stop();
                    }
                }
            }
            catch (TimeoutException te)
            {
                result.PipeStatus = "Timeout";
                result.Error = te.Message;
            }
            catch (Exception ex)
            {
                result.PipeStatus = "Error";
                result.Error = ex.Message;
            }
            finally
            {
                if (watch.IsRunning) watch.Stop();
                else
                {
                    // We got here from failure so won't log the latency
                    result.ConnectionLatency = watch.Elapsed.TotalSeconds.ToString();
                }
            }

            return result;
        }
    }

    public class SidecarStatus
    {
        public string PipeStatus { get; set; } = "Active";
        public string ConnectionLatency { get; set; }
        public string Error { get; set; }
    }
}
