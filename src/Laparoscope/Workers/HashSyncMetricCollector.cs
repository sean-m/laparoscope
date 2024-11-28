
using Laparoscope.Models;
using LaparoscopeShared.Models;
using Prometheus;
using StreamJsonRpc;
using System.IO.Pipes;

namespace Laparoscope.Workers
{
    public class HashSyncMetricCollector : BackgroundService
    {
        Dictionary<string, Gauge> counters = new Dictionary<string, Gauge>();

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var stream = new NamedPipeClientStream(".", "Laparoscope", PipeDirection.InOut, PipeOptions.Asynchronous))
                {
                    await stream.ConnectAsync();
                    using (var jsonRpc = JsonRpc.Attach(stream))
                    {
                        string function = "GetHashSyncMetrics";
                        var result = await jsonRpc.InvokeAsync<IEnumerable<HashSyncMetric>>(function);
                        
                        // Setup metric here
                        var now = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString();
                        foreach (var metric in result)
                        {
                            Gauge counter;
                            if (counters.ContainsKey(metric.Name))
                            {
                                counter = counters[metric.Name];
                            }
                            else
                            {
                                counter = Metrics.CreateGauge("laparoscope_hash_sync", "Hash sync metrics by connector.", new[] { "computedTime", "name", "lastCycleTime", "lastSuccessTime", "lastSuccessDuration", "lastStatus" });
                                counters.Add(metric.Name, counter);
                            }
                            counter.WithLabels(
                               now,
                               metric.Name,
                               metric.LastCycle.ToString(),
                               metric.LastSuccess.ToString(), 
                               metric.LastSuccessDuration.ToString(), 
                               metric.LastStatus
                               );
                        }
                    }
                }

                await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
            }
        }
    }
}
