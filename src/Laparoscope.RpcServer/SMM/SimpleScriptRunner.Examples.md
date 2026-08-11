# SimpleScriptRunner - Usage Examples

## Overview

The enhanced `SimpleScriptRunner` now provides real-time observable streams and proper async execution for PowerShell scripts running in .NET. This allows you to capture output as it's being produced rather than waiting for the entire script to complete.

## Key Features

1. **Observable Output Streams** - Real-time access to all PowerShell output streams
2. **Event-Based Notifications** - Get notified immediately when data is added to any stream
3. **Async Execution** - Proper async/await support with two execution modes
4. **Stream Access** - Direct access to all PowerShell streams (Output, Error, Warning, Verbose, Debug, Information, Progress)

## Basic Synchronous Usage

```csharp
using SMM.Automation;

var script = @"
    Write-Host 'Starting process...'
    Get-Process | Select-Object -First 5
";

var runner = new SimpleScriptRunner(script);
runner.Run();

// Access results after completion
foreach (var result in runner.Results)
{
    Console.WriteLine(result);
}
```

## Async Execution with Real-Time Output

```csharp
using SMM.Automation;
using System.Management.Automation;

var script = @"
    Write-Host 'Starting process...'
    Start-Sleep -Seconds 2
    Write-Host 'Still working...'
    Get-Process | Select-Object -First 5
";

var runner = new SimpleScriptRunner(script);

// Subscribe to output events for real-time processing
runner.OutputDataAdded += (sender, e) =>
{
    var output = runner.OutputStream[e.Index];
    Console.WriteLine($"[OUTPUT] {output}");
};

// Run asynchronously
var results = await runner.RunAsync();
```

## Capturing All Stream Types

```csharp
using SMM.Automation;

var script = @"
    Write-Verbose 'Verbose message' -Verbose
    Write-Warning 'Warning message'
    Write-Debug 'Debug message' -Debug
    Write-Information 'Information message' -InformationAction Continue
    Write-Error 'Error message'
    Write-Host 'Normal output'
";

var runner = new SimpleScriptRunner(script);

// Subscribe to all stream events
runner.OutputDataAdded += (sender, e) =>
{
    var output = runner.OutputStream[e.Index];
    Console.WriteLine($"[OUTPUT] {output}");
};

runner.ErrorDataAdded += (sender, e) =>
{
    var error = runner.ErrorStream[e.Index];
    Console.WriteLine($"[ERROR] {error.Exception.Message}");
};

runner.WarningDataAdded += (sender, e) =>
{
    var warning = runner.WarningStream[e.Index];
    Console.WriteLine($"[WARNING] {warning.Message}");
};

runner.VerboseDataAdded += (sender, e) =>
{
    var verbose = runner.VerboseStream[e.Index];
    Console.WriteLine($"[VERBOSE] {verbose.Message}");
};

runner.DebugDataAdded += (sender, e) =>
{
    var debug = runner.DebugStream[e.Index];
    Console.WriteLine($"[DEBUG] {debug.Message}");
};

runner.InformationDataAdded += (sender, e) =>
{
    var info = runner.InformationStream[e.Index];
    Console.WriteLine($"[INFO] {info.MessageData}");
};

runner.ProgressDataAdded += (sender, e) =>
{
    var progress = runner.ProgressStream[e.Index];
    Console.WriteLine($"[PROGRESS] {progress.Activity}: {progress.PercentComplete}%");
};

// Use RunAsyncWithStreams to enable all streams
var results = await runner.RunAsyncWithStreams();
```

## Long-Running Script with Progress Reporting

```csharp
using SMM.Automation;

var script = @"
    $items = 1..10
    $total = $items.Count

    foreach ($i in $items) {
        $percent = ($i / $total) * 100
        Write-Progress -Activity 'Processing Items' -Status ""Item $i of $total"" -PercentComplete $percent
        Start-Sleep -Milliseconds 500
        Write-Output ""Processed item $i""
    }

    Write-Progress -Activity 'Processing Items' -Completed
";

var runner = new SimpleScriptRunner(script);

runner.ProgressDataAdded += (sender, e) =>
{
    var progress = runner.ProgressStream[e.Index];
    if (progress.RecordType == System.Management.Automation.ProgressRecordType.Completed)
    {
        Console.WriteLine("Processing complete!");
    }
    else
    {
        Console.WriteLine($"{progress.Activity}: {progress.StatusDescription} - {progress.PercentComplete}%");
    }
};

runner.OutputDataAdded += (sender, e) =>
{
    var output = runner.OutputStream[e.Index];
    Console.WriteLine($"Output: {output}");
};

await runner.RunAsyncWithStreams();
```

## Using Parameters

```csharp
using SMM.Automation;

var script = @"
    param(
        [string]$Name,
        [int]$Count = 5
    )

    Write-Host ""Hello, $Name!""
    1..$Count | ForEach-Object {
        Write-Output ""Item $_""
    }
";

var runner = new SimpleScriptRunner(script);
runner.Parameters["Name"] = "World";
runner.Parameters["Count"] = 3;

runner.OutputDataAdded += (sender, e) =>
{
    Console.WriteLine(runner.OutputStream[e.Index]);
};

await runner.RunAsync();
```

## Using Proxy Variables

```csharp
using SMM.Automation;

var script = @"
    Write-Host ""User: $UserName""
    Write-Host ""Environment: $Environment""
";

var runner = new SimpleScriptRunner(script);

// Set variables that will be available in the PowerShell runspace
runner.SetProxyVariable("UserName", "admin@example.com");
runner.SetProxyVariable("Environment", "Production");

runner.Run();
```

## Error Handling

```csharp
using SMM.Automation;

var script = @"
    Get-Item C:\NonExistent\File.txt -ErrorAction Stop
";

var runner = new SimpleScriptRunner(script);

runner.ErrorDataAdded += (sender, e) =>
{
    var error = runner.ErrorStream[e.Index];
    Console.WriteLine($"Error: {error.Exception.Message}");
    Console.WriteLine($"Category: {error.CategoryInfo.Category}");
};

try
{
    await runner.RunAsyncWithStreams();

    if (runner.HadErrors)
    {
        Console.WriteLine($"Script completed with errors. Exit code: {runner.ExitCode}");
        var lastError = runner.LastError;
        if (lastError != null)
        {
            Console.WriteLine($"Last error: {lastError.Message}");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Exception during execution: {ex.Message}");
}
finally
{
    runner.Dispose();
}
```

## Best Practices

1. **Always dispose the runner** when you're done to free PowerShell resources:
   ```csharp
   using (var runner = new SimpleScriptRunner(script))
   {
       await runner.RunAsync();
   }
   ```

2. **Choose the right execution method**:
   - `Run()` - Synchronous execution, merged output
   - `RunAsync()` - Async execution with basic output stream
   - `RunAsyncWithStreams()` - Async execution with all streams enabled

3. **Subscribe to events before execution** to ensure you don't miss any output

4. **Use stream properties directly** for post-execution analysis:
   ```csharp
   var results = await runner.RunAsyncWithStreams();

   // Access streams after completion
   foreach (var warning in runner.WarningStream)
   {
       Console.WriteLine($"Warning: {warning.Message}");
   }
   ```

5. **Check for errors** after execution:
   ```csharp
   if (runner.HadErrors)
   {
       Console.WriteLine($"Exit code: {runner.ExitCode}");
   }
   ```

## Differences Between Execution Methods

### `Run()` - Synchronous
- Blocks until completion
- Merges all output streams into one
- Results available in `Results` property
- Best for: Simple scripts where you don't need real-time output

### `RunAsync()` - Basic Async
- Non-blocking async execution
- Output stream enabled with events
- Results available in `OutputStream` property
- Best for: Async execution with output-only streaming

### `RunAsyncWithStreams()` - Full Async with All Streams
- Non-blocking async execution
- All streams enabled (Error, Warning, Verbose, Debug, Information, Progress)
- All stream events will fire as data arrives
- Best for: Complex scripts requiring comprehensive stream monitoring

## Thread Safety

The `SimpleScriptRunner` uses a single PowerShell instance. For concurrent execution, create separate runner instances:

```csharp
var scripts = new[] { "Get-Process", "Get-Service", "Get-EventLog Application -Newest 10" };

var tasks = scripts.Select(async script =>
{
    using (var runner = new SimpleScriptRunner(script))
    {
        return await runner.RunAsync();
    }
});

var allResults = await Task.WhenAll(tasks);
```
