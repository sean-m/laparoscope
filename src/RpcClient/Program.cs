// See https://aka.ms/new-console-template for more information
using Laparoscope.Models;
using LaparoscopeShared.Models;
using StreamJsonRpc;
using System.Diagnostics;
using System.IO.Pipes;

var stopWatch = new Stopwatch();
Thread.Sleep(2000);
Console.WriteLine("Connecting to server...");
stopWatch.Start();
using (var stream = new NamedPipeClientStream(".", "Laparoscope", PipeDirection.InOut, PipeOptions.Asynchronous)) {
    await stream.ConnectAsync();
    stopWatch.Stop();
    Console.WriteLine($"Connected in {stopWatch.ToString()}");
    await ActAsRpcClientAsync(stream);
    Console.WriteLine("Terminating stream...");
}
Console.ReadLine();

static async Task ActAsRpcClientAsync(Stream stream) {
    using var jsonRpc = JsonRpc.Attach(stream);

    
    {
        Console.WriteLine("\nConnected. Sending request...");
        string function = "GetADSyncCSObjectStrict";
        Console.WriteLine($">> {function}()");
        var ConnectorName = "garage.mcardletech.com";
        var DistinguishedName = "CN=astultz,OU=Standard,OU=All Users,DC=garage,DC=mcardletech,DC=com";
        var result = await jsonRpc.InvokeAsync<AadcCSObject>(function, ConnectorName.Trim(), DistinguishedName.Trim());
        Console.WriteLine("Result:");
        Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(result));
    }
    return;
    {
        Console.WriteLine("\nConnected. Sending request...");
        string function = "GetADSyncRunProfileResult";
        Console.WriteLine($">> {function}()");
        var result = await jsonRpc.InvokeAsync<IEnumerable<RunHistory>>(function);
        Console.WriteLine("Result:");
        foreach (var r in result)
        {
            Console.WriteLine($"Connector: {r.ConnectorName}  RunHistoryId: {r.RunHistoryId}  Result: {r.Result}");
            var steps = await jsonRpc.InvokeAsync<IEnumerable<RunStepResult>>("GetADSyncRunStepResult", r.RunHistoryId);
            Console.WriteLine($"Run step count: {steps?.Count().ToString() ?? "NULL"}");
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(steps) + Environment.NewLine);
        }
    }

    {
        Console.WriteLine("\nConnected. Sending request...");
        string function = "GetADSyncConnector";
        Console.WriteLine($">> {function}()");
        var result = await jsonRpc.InvokeAsync<AadcConnector[]>(function, "");
        Console.WriteLine("Result:");
        Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(result));
    }

    {
        Console.WriteLine("\nConnected. Sending request...");
        string function = "GetADSyncAADCompanyFeature";
        Console.WriteLine($">> {function}()");
        var result = await jsonRpc.InvokeAsync<dynamic>(function);
        Console.WriteLine("Result:");
        Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(result));
    }

    {
        Console.WriteLine("\nConnected. Sending request...");
        string function = "GetADSyncAADPasswordResetConfiguration";
        Console.WriteLine($">> {function}()");
        var result = await jsonRpc.InvokeAsync<dynamic>(function);
        Console.WriteLine("Result:");
        Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(result));
    }

    {
        Console.WriteLine("\nConnected. Sending request...");
        string function = "GetADSyncAADPasswordSyncConfiguration";
        Console.WriteLine($">> {function}()");
        var result = await jsonRpc.InvokeAsync<dynamic>(function, "garage.mcardletech.com");
        Console.WriteLine("Result:");
        Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(result));
    }

    {
        Console.WriteLine("\nConnected. Sending request...");
        string function = "GetADSyncAutoUpgrade";
        Console.WriteLine($">> {function}()");
        var result = await jsonRpc.InvokeAsync<dynamic>(function);
        Console.WriteLine("Result:");
        Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(result));
    }

    {
        Console.WriteLine("\nConnected. Sending request...");
        string function = "GetADSyncConnectorStatistics";
        Console.WriteLine($">> {function}()");
        var result = await jsonRpc.InvokeAsync<dynamic>(function, "garage.mcardletech.com");
        Console.WriteLine("Result:");
        Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(result));
    }

    {
        Console.WriteLine("\nConnected. Sending request...");
        string function = "GetADSyncDomainReachabilityStatus";
        Console.WriteLine($">> {function}()");
        var result = await jsonRpc.InvokeAsync<dynamic>(function, "garage.mcardletech.com");
        Console.WriteLine("Result:");
        Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(result));
    }

    {
        Console.WriteLine("\nConnected. Sending request...");
        string function = "GetADSyncExportDeletionThreshold";
        Console.WriteLine($">> {function}()");
        var result = await jsonRpc.InvokeAsync<dynamic>(function);
        Console.WriteLine("Result:");
        Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(result));
    }

    {
        Console.WriteLine("\nConnected. Sending request...");
        string function = "GetAdSyncGlobalSettingsStrict";
        Console.WriteLine($">> {function}()");
        var result = await jsonRpc.InvokeAsync<dynamic>(function);
        Console.WriteLine("Result:");
        Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(result));
    }
        
    {
        Console.WriteLine("\nConnected. Sending request...");
        string function = "GetADSyncPartitionPasswordSyncState";
        Console.WriteLine($">> {function}()");
        var result = await jsonRpc.InvokeAsync<dynamic>(function);
        Console.WriteLine("Result:");
        Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(result));
    }
        
    {
        Console.WriteLine("\nConnected. Sending request...");
        string function = "GetADSyncScheduler";
        Console.WriteLine($">> {function}()");
        var result = await jsonRpc.InvokeAsync<dynamic>(function);
        Console.WriteLine("Result:");
        Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(result));
    }

    {
        Console.WriteLine("\nConnected. Sending request...");
        string function = "GetADSyncSchedulerConnectorOverride";
        Console.WriteLine($">> {function}()");
        var result = await jsonRpc.InvokeAsync<dynamic>(function, "garage.mcardletech.com");
        Console.WriteLine("Result:");
        Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(result));
    }
}