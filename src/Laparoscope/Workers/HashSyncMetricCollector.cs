
using Laparoscope.Models;
using LaparoscopeShared.Models;
using Polly.CircuitBreaker;
using Prometheus;
using StreamJsonRpc;
using System.IO.Pipes;

namespace Laparoscope.Workers
{
    public class HashSyncMetricCollector : BackgroundService
    {
        public HashSyncMetricCollector()
        {
            // TODO make this tunable
            Metrics.SuppressDefaultMetrics();
        }

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
                        double now = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
                        foreach (var metric in result)
                        {
                            Gauge counter;
                            Double value;
                            String key;

                            // last_polled_time
                            var name = $"laparoscope_hash_sync_last_polled_time";
                            key = name + metric.Name;
                            if (counters.ContainsKey(key))
                            {
                                counter = counters[key];
                            }
                            else
                            {
                                counter = Metrics.CreateGauge(name, "Last time hash sync metrics were collected.",
                                    new[] { "connector" }); 
                                counters.Add(key, counter);
                            }
                            counter.WithLabels(metric.Name).Set(now);

                            // last_cycle_time
                            name = $"laparoscope_hash_sync_last_cycle_time";
                            key = name + metric.Name;
                            if (counters.ContainsKey(key))
                            {
                                counter = counters[key];
                            }
                            else
                            {
                                counter = Metrics.CreateGauge(name, "Start time of most reccent sync cycle.",
                                    new[] { "connector" }); 
                                counters.Add(key, counter);
                            }
                            value = Double.Parse(metric.LastCycle);
                            counter.WithLabels(metric.Name).Set(value);

                            // last_success_time
                            name = $"laparoscope_hash_sync_last_success_time";
                            key = name + metric.Name;
                            if (counters.ContainsKey(key))
                            {
                                counter = counters[key];
                            }
                            else
                            {
                                counter = Metrics.CreateGauge(name, "Start time of the last successful sync cycle.",
                                    new[] { "connector" }); 
                                counters.Add(key, counter);
                            }
                            value = Double.Parse(metric.LastSuccess);
                            counter.WithLabels(metric.Name).Set(value);

                            // last_success_duration
                            name = $"laparoscope_hash_sync_last_success_duration";
                            key = name + metric.Name;
                            if (counters.ContainsKey(key))
                            {
                                counter = counters[key];
                            }
                            else
                            {
                                counter = Metrics.CreateGauge(name, "Duration of the last succesful sync cycle.",
                                    new[] { "connector" }); 
                                counters.Add(key, counter);
                            }
                            value = Double.Parse(metric.LastSuccessDuration);
                            counter.WithLabels(metric.Name).Set(value);
                        }
                    }
                }

                await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
            }
        }
    }
}
