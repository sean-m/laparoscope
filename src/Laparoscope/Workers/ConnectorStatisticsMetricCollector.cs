
using LaparoscopeShared.Models;
using Prometheus;
using StreamJsonRpc;
using System.IO.Pipes;

namespace Laparoscope.Workers
{
    public class ConnectorStatisticsMetricCollector : BackgroundService

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
                        string function = "GetAllConnectorStatistics";
                        var result = await jsonRpc.InvokeAsync<IEnumerable<ConnectorStatistics>>(function);

                        // Setup metric here
                        double now = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
                        foreach (var metric in result)
                        {
                            Gauge counter;
                            Double value;
                            String name;
                            String key;

                            // export_adds
                            name = $"laparoscope_connector_statistic_export_adds";
                            key = name + metric.Connector;
                            if (counters.ContainsKey(key))
                            {
                                counter = counters[key];
                            }
                            else
                            {
                                counter = Metrics.CreateGauge(name, "New records provisioned to external directory.",
                                    new[] { "connector" }); 
                                counters.Add(key, counter);
                            }
                            counter.WithLabels(metric.Connector).Set(metric.ExportAdds);

                            // export_updates
                            name = $"laparoscope_connector_statistic_export_updates";
                            key = name + metric.Connector;
                            if (counters.ContainsKey(key))
                            {
                                counter = counters[key];
                            }
                            else
                            {
                                counter = Metrics.CreateGauge(name, "Updates to external records.",
                                    new[] { "connector" }); 
                                counters.Add(key, counter);
                            }
                            counter.WithLabels(metric.Connector).Set(metric.ExportUpdates);

                            // export_deletes
                            name = $"laparoscope_connector_statistic_export_deletes";
                            key = name + metric.Connector;
                            if (counters.ContainsKey(key))
                            {
                                counter = counters[key];
                            }
                            else
                            {
                                counter = Metrics.CreateGauge(name, "External records to delete.",
                                    new[] { "connector" }); 
                                counters.Add(key, counter);
                            }
                            counter.WithLabels(metric.Connector).Set(metric.ExportDeletes);

                            // import_adds
                            name = $"laparoscope_connector_statistic_import_adds";
                            key = name + metric.Connector;
                            if (counters.ContainsKey(key))
                            {
                                counter = counters[key];
                            }
                            else
                            {
                                counter = Metrics.CreateGauge(name, "New records originating from an external directory.",
                                    new[] { "connector" }); 
                                counters.Add(key, counter);
                            }
                            counter.WithLabels(metric.Connector).Set(metric.ImportAdds);

                            // import_updates
                            name = $"laparoscope_connector_statistic_import_updates";
                            key = name + metric.Connector;
                            if (counters.ContainsKey(key))
                            {
                                counter = counters[key];
                            }
                            else
                            {
                                counter = Metrics.CreateGauge(name, "Record updates originating from and external directory.",
                                    new[] { "connector" }); 
                                counters.Add(key, counter);
                            }
                            counter.WithLabels(metric.Connector).Set(metric.ImportUpdates);

                            // import_deletes
                            name = $"laparoscope_connector_statistic_import_deletes";
                            key = name + metric.Connector;
                            if (counters.ContainsKey(key))
                            {
                                counter = counters[key];
                            }
                            else
                            {
                                counter = Metrics.CreateGauge(name, "Record deleted from and external directory.",
                                    new[] { "connector" }); 
                                counters.Add(key, counter);
                            }
                            counter.WithLabels(metric.Connector).Set(metric.ImportDeletes);

                            // import_no_change
                            name = $"laparoscope_connector_statistic_import_no_change";
                            key = name + metric.Connector;
                            if (counters.ContainsKey(key))
                            {
                                counter = counters[key];
                            }
                            else
                            {
                                counter = Metrics.CreateGauge(name, "Records from and external directory with incremented USN but no changes.",
                                    new[] { "connector" }); 
                                counters.Add(key, counter);
                            }
                            counter.WithLabels(metric.Connector).Set(metric.ImportNoChange);

                            // total_connectors
                            name = $"laparoscope_connector_statistic_total_connectors";
                            key = name + metric.Connector;
                            if (counters.ContainsKey(key))
                            {
                                counter = counters[key];
                            }
                            else
                            {
                                counter = Metrics.CreateGauge(name, "Total directory records connected for syncing.",
                                    new[] { "connector" }); 
                                counters.Add(key, counter);
                            }
                            counter.WithLabels(metric.Connector).Set(metric.TotalConnectors);
                        }
                    }
                }

                // These stats only change when syncs occur and that's every 30 minutes so every 5 and change should be sufficient for this.
                await Task.Delay(TimeSpan.FromMinutes(5.11), stoppingToken);
            }
        }
    }
}
