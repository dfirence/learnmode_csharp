#if WINDOWS
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using Microsoft.Diagnostics.Tracing.Parsers;

namespace Watcher.Modules.Windows.ETW;

/// <summary>
/// EtwEventsSubscriberProcess class inherits from ETWSubsriber to handle specific ETW events.
/// </summary>
public class EtwEventsSubscriberProcess : ETWSubsriber
{
    // Constructor that initializes the ETW
    // session and subscribes to ProcessStart events.
    public EtwEventsSubscriberProcess(string sessionName) : base(sessionName)
    {
        // Enable the Kernel provider to capture process events.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        EtwSession.EnableKernelProvider(KernelTraceEventParser.Keywords.Process);
#pragma warning restore CS8602 // Dereference of a possibly null reference.

        // Subscribe to ProcessStart events and handle them with OnDataProcessEvent method.
        EtwSession.Source.Kernel.ProcessStart += data =>
        {
            Console.WriteLine(OnDataProcessEvent(data));
            Console.Out.Flush();
        };
        // Subscribe to ProcessStart events and handle them with OnDataProcessEvent method.
        EtwSession.Source.Kernel.ProcessStop += data =>
        {
            Console.WriteLine(OnDataProcessStop(data));
            Console.Out.Flush();
        };
    }

    // Method to format process event data into a JSON-like string.
    private static string OnDataProcessEvent(ProcessTraceData data)
    {
        var error = string.Empty;
        var parent = string.Empty;
        try
        {
            using var parentMeta = System.Diagnostics.Process.GetProcessById(data.ParentID);
            parent = parentMeta?.MainModule?.FileName ?? "";
        }
        catch(Exception ex)
        {
            error = ex.Message;
        }
        return $"""
                
                timeProcessCreated : {data.TimeStamp.ToUniversalTime():o}
                etwEvent    : {data.EventName}
                etwEventId  : {data.Task}
                ppid        : {data.ParentID}
                pid         : {data.ProcessID}
                parent      : {parent}
                process     : {data.ImageFileName}
                cmdline     : {data.CommandLine}
                error       : {error}
                
                """;
    }
    private static string OnDataProcessStop(ProcessTraceData data)
    {
        return $"""
                
                timeProcessTerminated : {data.TimeStamp.ToUniversalTime():o}
                etwEvent    : {data.EventName}
                etwEventId  : {data.Task}
                ppid        : {data.ParentID}
                pid         : {data.ProcessID}
                process     : {data.ImageFileName}
                cmdline     : {data.CommandLine}
                
                """;
    }
}
#endif