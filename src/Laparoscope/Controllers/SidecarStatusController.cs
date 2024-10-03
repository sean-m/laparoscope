using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Threading;
using System.IO.Pipes;

namespace Laparoscope.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SidecarStatusController : ControllerBase
    {
        ILogger logger;
        public SidecarStatusController(ILogger<SidecarStatusController> logger)
        {
            this.logger = logger;
        }

        [HttpGet]
        public async Task<dynamic> GetAsync(int timeout=20)
        {
            var watch = new System.Diagnostics.Stopwatch();
            var result = new SidecarStatus();

            watch.Start();
            try
            {
                using (var stream = new NamedPipeClientStream(".", "Laparoscope", PipeDirection.InOut, PipeOptions.Asynchronous))
                {
                    await stream.ConnectAsync().WithTimeout(TimeSpan.FromSeconds(timeout));
                    watch.Stop();
                    stream.Close();
                }
            }
            catch (TimeoutException te)
            {
                result.PipeStatus = "Timeout";
                result.Error = te.Message;
                logger?.LogError(te, "Timeout exception.");
            }
            catch (Exception ex)
            {
                result.PipeStatus = "Error";
                result.Error = ex.Message;
                logger?.LogError(ex, ex.Message);
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
