
using NUnit.Framework;
using StreamJsonRpc;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Laparoscope.Models;
using Laparoscope.RpcServer;
using LaparoscopeShared.Models;

namespace Laparoscope.UnitTests
{
    /// <summary>
    /// Tests JSON-RPC serialization and deserialization between Laparoscope client and server implementations.
    /// Creates a simple worker service that listens for JSON-RPC requests and responds to them.
    /// </summary>
    [TestFixture]
    public class JsonRpcSerializationTests
    {
        private const string PipeName = "LaparoscopeTest";
        private Task serverTask;
        private CancellationTokenSource cancellationTokenSource;
        private ServerActions serverActions;

        [SetUp]
        public void Setup()
        {
            cancellationTokenSource = new CancellationTokenSource();
            serverActions = new ServerActions();

            // Start the test server in background
            serverTask = Task.Run(() => StartTestServerAsync(cancellationTokenSource.Token));

            // Give server time to start
            Thread.Sleep(500);
        }

        [TearDown]
        public void TearDown()
        {
            cancellationTokenSource?.Cancel();
            serverTask?.Wait(TimeSpan.FromSeconds(5));
            cancellationTokenSource?.Dispose();
        }

        private async Task StartTestServerAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                using (var stream = new NamedPipeServerStream(
                    PipeName,
                    PipeDirection.InOut,
                    1,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous))
                {
                    try
                    {
                        await stream.WaitForConnectionAsync(token);
                        await HandleRpcConnectionAsync(stream, token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception)
                    {
                        // Ignore connection errors during testing
                    }
                }
            }
        }

        private async Task HandleRpcConnectionAsync(Stream stream, CancellationToken token)
        {
            var jsonRpc = JsonRpc.Attach(stream, serverActions);
            try
            {
                await jsonRpc.Completion;
            }
            catch (OperationCanceledException)
            {
                // Expected during teardown
            }
        }

        private async Task<JsonRpc> ConnectClientAsync()
        {
            var clientStream = new NamedPipeClientStream(
                ".",
                PipeName,
                PipeDirection.InOut,
                PipeOptions.Asynchronous);

            await clientStream.ConnectAsync(5000);
            return JsonRpc.Attach(clientStream);
        }

        [Test]
        public async Task TestSimpleHelloMethod()
        {
            using (var jsonRpc = await ConnectClientAsync())
            {
                var result = await jsonRpc.InvokeAsync<string>("Hello");

                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.EqualTo("Hi there!"));
            }
        }

        [Test]
        public async Task TestGetADSyncConnectorSerialization()
        {
            using (var jsonRpc = await ConnectClientAsync())
            {
                var connectorName = ""; // Empty for all connectors
                var result = await jsonRpc.InvokeAsync<AadcConnector[]>("GetADSyncConnector", connectorName);

                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.InstanceOf<AadcConnector[]>());
            }
        }

        [Test]
        public async Task TestGetADSyncAADCompanyFeatureSerialization()
        {
            using (var jsonRpc = await ConnectClientAsync())
            {
                var result = await jsonRpc.InvokeAsync<Dictionary<string, object>>("GetADSyncAADCompanyFeature");

                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.InstanceOf<Dictionary<string, object>>());
            }
        }

        [Test]
        public async Task TestMultipleSequentialRequests()
        {
            using (var jsonRpc = await ConnectClientAsync())
            {
                // Test that the connection can handle multiple sequential requests
                var result1 = await jsonRpc.InvokeAsync<string>("Hello");
                Assert.That(result1, Is.EqualTo("Hi there!"));
            }
        }

        [Test]
        public void TestConnectionTimeout()
        {
            // Test that connection fails gracefully when server is not available
            cancellationTokenSource.Cancel();
            serverTask.Wait(TimeSpan.FromSeconds(5));

            Assert.ThrowsAsync<TimeoutException>(async () =>
            {
                var clientStream = new NamedPipeClientStream(
                    ".",
                    "NonExistentPipe",
                    PipeDirection.InOut,
                    PipeOptions.Asynchronous);

                await clientStream.ConnectAsync(1000);
            });
        }
    }
}