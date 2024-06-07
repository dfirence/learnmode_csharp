
using System.Diagnostics;
using System.Dynamic;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using Microsoft.Diagnostics.Tracing.Parsers;


namespace Watcher.Modules.Windows.ETW;
// Concrete ETWSubscriber class for process start events
public class SubscriberProcessStart : ETWSubscriber
{
    public SubscriberProcessStart(string sessionName, string schemaFilePath, string schemaEventName) : base(sessionName, schemaFilePath, schemaEventName)
    { }

    public override void Start()
    {
        Session.EnableKernelProvider(KernelTraceEventParser.Keywords.Process);
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
        Session.Source.StopProcessing();
        SessionStatus = ETWSessionStatus.Stopped;
        Console.WriteLine("Process Start subscriber stopped.");
    }

    public override void Dispose()
    {
        Stop();
        SessionStatus = ETWSessionStatus.Disposed;
    }

    private void ProcessEvent(ProcessTraceData data)
    {
        CreateEventObject(data);
        Console.WriteLine($"{SchemaHandler.ToJsonFromDict(SchemaEvent)}");
        Console.Out.Flush();
        // Add your custom event handling logic here
    }

    private void CreateEventObject(ProcessTraceData data)
    {
        // Retrieve details from ETW event and map them to the schema fields
        SchemaEvent["EventCategory"] = "etwProcessStart";
        SchemaEvent["TimeGenerated"] = DateTime.UtcNow.ToString("o");
        SchemaEvent["TimeProcessCreated"] = data.TimeStamp.ToUniversalTime().ToString("o");
        SchemaEvent["pid"] = data.ProcessID;
        SchemaEvent["ppid"] = data.ParentID;
        SchemaEvent["Parent"] = GetProcessName(data.ParentID);
        SchemaEvent["Process"] = data.ImageFileName;
        SchemaEvent["CommandLine"] = data.CommandLine;
        // Add additional fields according to the schema definition
    }

    private string GetProcessName(int processId)
    {
        try
        {
            using var process = Process.GetProcessById(processId);
            // (process.MachineName, process.MainModule.FileName, process.SessionId)
            return process.ProcessName;
        }
        catch (Exception)
        {
            return "Unknown";
        }
    }
}