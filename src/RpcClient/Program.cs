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


static async Task ActAsRpcClientAsync(Stream stream) {
    Console.WriteLine("Connected. Sending request...");
    using var jsonRpc = JsonRpc.Attach(stream);
    var result = await jsonRpc.InvokeAsync<SyncResult>("StartSync", "garage.mcardletech.com");
    Console.WriteLine("Result:");
    Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(result));
}