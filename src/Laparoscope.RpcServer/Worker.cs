using Laparoscope.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StreamJsonRpc;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace Laparoscope.RpcServer {
    public class Worker : BackgroundService {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger) {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            while (!stoppingToken.IsCancellationRequested) {
                if (_logger.IsEnabled(LogLevel.Information)) {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }
                await NamedPipeServerAsync();
            }
        }


        private async Task NamedPipeServerAsync() {
            int clientId = 0;
            while (true) {
                _logger.LogInformation ("Waiting for client to make a connection...");
                var stream = new NamedPipeServerStream("Laparoscope", PipeDirection.InOut, 
                    NamedPipeServerStream.MaxAllowedServerInstances, 
                    PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                await stream.WaitForConnectionAsync();
                Task nowait = RespondToRpcRequestsAsync(stream, ++clientId);
            }
        }

        private async Task RespondToRpcRequestsAsync(Stream stream, int clientId) {
            _logger.LogInformation($"Connection request {clientId} received.");
            var jsonRpc = JsonRpc.Attach(stream, new ServerActions());
            _logger.LogInformation($"JSON-RPC listener attached to {clientId}. Waiting for requests...");
            await jsonRpc.Completion;
            _logger.LogInformation($"Connection {clientId} terminated.");
        }
    }
}
