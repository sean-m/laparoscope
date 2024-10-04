using Laparoscope.Controllers.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Threading;
using StreamJsonRpc;
using System.IO.Pipes;
using System.Net;
using System.Reflection;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Laparoscope.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : Controller
    {
        public HealthController()
        {
            if (sidecarStatus == null)
            {
                sidecarStatus = new CachedValue<SidecarStatus>()
                {
                    ttl = TimeSpan.FromSeconds(27)
                };
            }

            if (scheduler == null)
            {
                scheduler = new CachedValue<Dictionary<string, object>>()
                {
                    ttl = TimeSpan.FromSeconds(33)
                };
            }
        }

        internal async Task<Dictionary<string, object>> GetSchedulerAsync()
        {
            using (var stream = new NamedPipeClientStream(".", "Laparoscope", PipeDirection.InOut, PipeOptions.Asynchronous))
            {
                await stream.ConnectAsync();
                using (var jsonRpc = JsonRpc.Attach(stream))
                {
                    string function = "GetADSyncScheduler";
                    var result = await jsonRpc.InvokeAsync<Dictionary<string, object>>(function);
                    return result;
                }
            }
        }

        internal async Task<SidecarStatus> GetStatusAsync(int timeout = 20)
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

        /// <summary>
        /// Time the running binary was linked. This is exposed via the /api/health controller 
        /// so operators can monitor the values across multiple servers to ensure the same application
        /// version is deployed across the fleet.
        /// </summary>
        internal static string LinkerTimeUtc { get; private set; } = GetLinkerTime(Assembly.GetExecutingAssembly(), TimeZoneInfo.Utc).ToString("o");

        // Sourced from the comments of a lovely Jeff Atwood article: https://blog.codinghorror.com/determining-build-date-the-hard-way/
        // Jeff's example is in VB.net, C# port is by Joe Spivey. Thank them if you get a chance.
        private static DateTime GetLinkerTime(Assembly assembly, TimeZoneInfo target = null)
        {
            var filePath = assembly.Location;
            const int c_PeHeaderOffset = 60;
            const int c_LinkerTimestampOffset = 8;

            var buffer = new byte[1024];

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                stream.Read(buffer, 0, 1024);

            var offset = BitConverter.ToInt32(buffer, c_PeHeaderOffset);
            var secondsSince1970 = BitConverter.ToInt32(buffer, offset + c_LinkerTimestampOffset);
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var linkTimeUtc = epoch.AddSeconds(secondsSince1970);

            var tz = target ?? TimeZoneInfo.Local;
            var localTime = TimeZoneInfo.ConvertTimeFromUtc(linkTimeUtc, tz);

            return localTime;
        }

        /// <summary>
        /// Assmbly version also exposed via /api/health for the same reasons as LinkerTimeUtc.
        /// </summary>
        public static string AssemblyVersion { get; private set; } = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        internal class CachedValue<T>
        {
            private dynamic value;
            internal DateTime fetchTime;
            internal TimeSpan ttl;
            private bool ShouldRefresh => value == null || (DateTime.Now - fetchTime).Seconds < ttl.Seconds;

            internal async Task<T> GetValueAsync(Func<Task<T>> Refresh)
            {
                if (!ShouldRefresh)
                {
                    return (T)value;
                }
                value = await Refresh();
                fetchTime = DateTime.Now;
                return (T)value;
            }
        }

        private static CachedValue<SidecarStatus> sidecarStatus { get; set; } 

        private static CachedValue<Dictionary<string,object>> scheduler { get; set; }

        /// <summary>
        /// Endpoint for getting basic health information about the application and AADC service
        /// without authentication. Intended to be polled by monitoring services.
        /// </summary>
        /// <returns>Dictionary<string,string>)</returns>
        [HttpGet]
        public async Task<dynamic> GetAsync()
        {
            var result = new Dictionary<string, string>();
            // TODO implement a worker service that updates this on a static class periodically
            result.Add("BuildTimeUTC", LinkerTimeUtc);
            result.Add("Version", AssemblyVersion);
            result.Add("Sidecar", (await sidecarStatus.GetValueAsync(() => GetStatusAsync())).PipeStatus);
            result.Add("StagingMode", (await scheduler.GetValueAsync(() => GetSchedulerAsync()))["StagingModeEnabled"]?.ToString());
            return result;
        }
    }
}
