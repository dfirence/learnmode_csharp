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
    public string OnDataProcessEvent(ProcessTraceData data)
    {
        string error = string.Empty;
        string parent = string.Empty;
        try
        {
            using var parentMeta = System.Diagnostics.Process.GetProcessById(data.ParentID);
            parent = parentMeta?.MainModule?.FileName ?? "";
        }
        catch(Exception ex)
        {
            error = ex.Message;
        }
        //data.FormattedMessage
        string json = $@"
        timeProcessCreated : {data.TimeStamp.ToUniversalTime().ToString("o")}
        etwEvent    : {data.EventName}
        etwEventId  : {data.Task}
        pguid       :
        pcguid      :
        ppid        : {data.ParentID}
        pid         : {data.ProcessID}
        parent      : {parent}
        process     : {data.ImageFileName}
        cmdline     : {data.CommandLine}
        error       : {error}
        ";
        return json;
    }
    public string OnDataProcessStop(ProcessTraceData data)
    {
        //data.FormattedMessage
        string json = $@"
        etwEvent    : {data.EventName}
        etwEventId : {data.Task}
        timeProcessCreated : {data.TimeStamp.ToUniversalTime().ToString("o")}
        ppid        : {data.ParentID}
        pid         : {data.ProcessID}
        process     : {data.ImageFileName}
        cmdline     : {data.CommandLine}
        ";
        return json;
    }
}
#endif