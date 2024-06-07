using System.Diagnostics;
using System.Dynamic;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Analysis;
using Microsoft.Diagnostics.Tracing;


namespace Watcher.Modules.Windows.ETW;


public class SubscriberProcessStart : ETWSubscriber
{
    public SubscriberProcessStart(string sessionName, string schemaFilePath, string schemaEventName) : base(sessionName, schemaFilePath, schemaEventName)
    { }

    public override void Start()
    {
        if (Session == null)
        {
            return;
        }
        //SubscribeToEvent<ProcessTraceData>(ProcessEvent);
        Session.EnableKernelProvider(KernelTraceEventParser.Keywords.Process);
        Session.EnableProvider("Microsoft-Windows-Kernel-Process", TraceEventLevel.Verbose);

        Session.Source.Kernel.ProcessStart += data =>
        {
            // Ignore system processes
            if (data.ProcessID != 0 && data.ProcessID != 4)
                ProcessEvent(data);
        };
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            Stop();
            Dispose();
        };
        Session.Source.Process(); // Start processing events
        Console.WriteLine("Process Start subscriber started.");
    }

    public override void Stop()
    {
        Session?.Dispose();
        SessionStatus = ETWSessionStatus.Stopped;
        Console.WriteLine("Process Start subscriber stopped.");
    }

    public override void Dispose()
    {
        Session?.Dispose();
        SessionStatus = ETWSessionStatus.Disposed;
    }

    private void ProcessEvent(ProcessTraceData data)
    {
        // Create event object based on the defined schema
        CreateEventObject(data);
        Console.WriteLine($"{SchemaHandler.ToJsonFromDict(SchemaEvent)}");
        Console.Out.Flush();
        // Add your custom event handling logic here
        // Reset EventObject
        SchemaEvent.Keys.ToList().ForEach(k => SchemaEvent[k] = null);
    }

    private void CreateEventObject(ProcessTraceData data)
    {
        // Parent
        SchemaEvent["ppid"] = data.ParentID;
        try
        {
            using var parentMetadata = Process.GetProcessById(data.ParentID);
            SchemaEvent["Parent"]
                = parentMetadata?.MainModule?.FileName?.Split("\\").Last()
                    ?? "unknown";
            SchemaEvent["ParentPath"]
                = parentMetadata?.MainModule?.FileName ?? "";
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Parent: {data.ParentID} " + ex.Message);
            Console.Out.Flush();
        }

        // EventDecoration
        SchemaEvent["EventCategory"] = "etwProcessStart";
        SchemaEvent["EventName"] = data.EventName;
        SchemaEvent["Device"] = Environment.MachineName.ToLower();
        // Timestamps
        SchemaEvent["TimeGenerated"] = DateTime.UtcNow.ToString("o");
        SchemaEvent["TimeProcessCreated"]
            = data.TimeStamp.ToUniversalTime().ToString("o");

        // Process
        SchemaEvent["pid"] = data.ProcessID;
        SchemaEvent["Process"] = data.ImageFileName;
        SchemaEvent["CommandLine"] = data.CommandLine;
    }
}