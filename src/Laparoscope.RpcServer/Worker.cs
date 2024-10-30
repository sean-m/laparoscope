using Laparoscope.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using StreamJsonRpc;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;


namespace Laparoscope.RpcServer {
    public class Worker : BackgroundService {
        private readonly ILogger<Worker> _logger;

        private readonly PipeSecurity pipeSecurity;

        public Worker(ILogger<Worker> logger) {
            _logger = logger;

            RunSafetyChecks();

            var authenticatedUsers = new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null);
            var localService = new SecurityIdentifier(WellKnownSidType.LocalServiceSid, null);

            // Setup a DACL that leaves out anonymous auth. Why the heck is that included by default?
            pipeSecurity = new PipeSecurity();
            pipeSecurity.AddAccessRule(new PipeAccessRule(authenticatedUsers,
                PipeAccessRights.ReadWrite | PipeAccessRights.CreateNewInstance,
                AccessControlType.Allow));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            while (!stoppingToken.IsCancellationRequested) {
                if (_logger.IsEnabled(LogLevel.Information)) {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }
                await NamedPipeServerAsync(stoppingToken);
            }
        }


        private async Task NamedPipeServerAsync(CancellationToken token) {
            int clientId = 0;
            while (true) {
                _logger.LogInformation ("Waiting for client to make a connection...");

                var stream = new NamedPipeServerStream("Laparoscope", PipeDirection.InOut, 
                    NamedPipeServerStream.MaxAllowedServerInstances, 
                    PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 0, 0, pipeSecurity);
                await stream.WaitForConnectionAsync(token);
                
                Task nowait = RespondToRpcRequestsAsync(stream, ++clientId);
            }
        }

        private async Task RespondToRpcRequestsAsync(Stream stream, int clientId) {
            _logger.LogInformation($"Connection request {clientId} received.");

            var jsonRpc = JsonRpc.Attach(stream, new ServerActions());
            _logger.LogInformation($"JSON-RPC listener attached to {clientId}. Waiting for requests...");
            try
            {
                await jsonRpc.Completion;
                _logger.LogInformation($"Connection {clientId} terminated.");
            }
            catch (Exception e){
                _logger.LogError(e, "Encountered exception in worker task.");
            }
        }

        private void RunSafetyChecks()
        {
            // Check the RestrictNullSessAccess key
            string lanmanParameters = "HKEY_LOCAL_MACHINE\\System\\CurrentControlSet\\Services\\LanManServer\\Parameters";
            var desiredValue = 1;
            object regValue = Registry.GetValue(lanmanParameters, "RestrictNullSessAccess", desiredValue);
            if (regValue == null || (int)regValue != desiredValue)
            {
                _logger?.LogWarning($"RestrictNullSessAccess check failed! Expected value: {desiredValue}, discovered value: {regValue ?? "NULL"}.");
                throw new Exception($"RestrictNullSessAccess check failed! Expected value: {desiredValue}, discovered value: {regValue ?? "NULL"}.");
            }

            // Verify the pipe name is not in the NullSessionPipes list
            object pipeList = Registry.GetValue(lanmanParameters, "NullSessionPipes", new string[] { });
            if (pipeList is string[] nullPipes)
            {
                if (nullPipes.Any(x => x.Equals("Laparoscope", StringComparison.CurrentCultureIgnoreCase))) {
                    _logger.LogWarning($"NullSessionPipes includes 'Laparoscope' allowing anonymous access to the pipe. Panic!");
                    throw new Exception("Will not run with anonymous access to named pipe. Remove name from NullSessionPipes.");
                }
            }
        }
    }
}
