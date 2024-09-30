// See https://aka.ms/new-console-template for more information
using aadcapi.Models;
using StreamJsonRpc;
using System.IO.Pipes;

Console.WriteLine("Connecting to server...");
using (var stream = new NamedPipeClientStream(".", "Laparoscope", PipeDirection.InOut, PipeOptions.Asynchronous)) {
    await stream.ConnectAsync();
    await ActAsRpcClientAsync(stream);
    Console.WriteLine("Terminating stream...");
}
Console.ReadLine();


static void AdSyncFunction()
{

}

static async Task ActAsRpcClientAsync(Stream stream) {
    using var jsonRpc = JsonRpc.Attach(stream);
    {
        Console.WriteLine("\nConnected. Sending request...");
        Console.WriteLine(">> GetAdSyncConnector()");
        var result = await jsonRpc.InvokeAsync<AadcConnector[]>("GetAdSyncConnector", "garage.mcardletech.com");
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
}