using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Session;
using System.Diagnostics;
using System.Text.Json;


namespace Watcher.Modules.Windows;


public class EtwEventProcessStart
{
    public string? EventName { get; init; }

    public string? TimeGenerated { get; set; }

    public string? TimeProcessCreated { get; set; }

    public string? Parent { get; set; }

    public string? Process { get; set; }

    public string? CommandLine { get; set; }

    public int? ppid { get; set; }

    public int? pid { get; set; }

    public EtwEventProcessStart() { EventName = this.GetType().Name; }

    public EtwEventProcessStart(DateTime TimeProcessStart,
                                string Parent,
                                string Process,
                                string CommandLine,
                                int ParentId,
                                int ProcessId)
    {
        if (string.IsNullOrEmpty(this.EventName))
            this.EventName = GetType().Name;

        UpdateEtwRecord(TimeProcessStart,
                        Parent,
                        Process,
                        CommandLine,
                        ParentId,
                        ProcessId);
    }

    public void UpdateEtwRecord(DateTime TimeProcessStart,
                                string Parent,
                                string Process,
                                string CommandLine,
                                int ParentId,
                                int ProcessId)
    {
        TimeGenerated = DateTime.UtcNow.ToString("o");

        TimeProcessCreated = TimeProcessStart.ToUniversalTime()
                                             .ToString("o");

        this.Parent = Parent;
        this.Process = Process;
        this.CommandLine = CommandLine;
        this.ppid = ParentId;
        this.pid = ProcessId;
    }
}


/// <summary>
/// Class <c>EtwSubscriber</c> serves an inheritable class
/// where EtwSubscriptionSessions can be created and using
/// specific Etw provider implementations.
/// </summary>
public abstract class EtwSubscriber
{
    //--------------------------------------------------------------------------
    // Public Fields
    //--------------------------------------------------------------------------
    public string TraceSessionName { get; set; }
        = $"DefaultTraceSession-{DateTime.UtcNow.ToString("o")}";

    public bool HasAdminRights { get; } = Environment.IsPrivilegedProcess;

    public JsonSerializerOptions opts = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public EtwEventProcessStart? MyEtwRecord { get; set; }
    //--------------------------------------------------------------------------
    // Public Methods
    //--------------------------------------------------------------------------
    public string ToJson()
    {
        return JsonSerializer.Serialize(this.MyEtwRecord, this.opts);
    }
    public void ToStdoutJson()
    {
        Console.WriteLine(ToJson());
        Console.Out.Flush();
    }
    /// <summary>
    /// Covenient Method to get the ProcessName by PID, this is commonly useful
    /// to get the name of the ParentProcess.
    /// </summary>
    /// <param name="pid"></param>
    /// <returns>string</returns>
    public string TryGetProcessById(int pid)
    {
        try
        {
            return Process.GetProcessById(pid).ProcessName ?? "null";
        }
        catch (Exception error)
        {
            Console.Error.WriteLine(
                $@"{this.GetType().Name} : {error.Message}");
            return "null";
        }
    }
}


/// <summary>
/// An EtwSubscriber WiredUp and inheriting From
/// EtwSubscriber Abstract class.
/// </summary>
public partial class MySubscriberTest : EtwSubscriber
{
    //-----------------------------------------------------------------------
    // Private Fields
    //-----------------------------------------------------------------------

    public MySubscriberTest(string mySessionName = "")
    {
        if (!HasAdminRights)
        {
            Console.WriteLine("Must Run With Elevated Privileges");
            return;
        }

        if (!string.IsNullOrEmpty(mySessionName) &&
            !string.IsNullOrWhiteSpace(mySessionName))
            TraceSessionName = mySessionName;

        // Create a new CustomSchema Event for ProcessStart
        MyEtwRecord = new EtwEventProcessStart();

        // TraceSession Resource - Needs Dispose()
        using (var session = new TraceEventSession(TraceSessionName))
        {
            Console.WriteLine(
                "Listening for process start events... Press CTRL+C to exit.");

            // Registrer Early Exit through CTRL+C
            Console.CancelKeyPress +=
            delegate (object? sender, ConsoleCancelEventArgs _)
            {
                session.Dispose();
                Console.Error.WriteLine(
                    $"CTRL+C Signal - ActiveSession {session.IsActive}");
            };

            // EtwKeywords For KernelTraceEvents
            var kw = KernelTraceEventParser.Keywords.Process;
            // Enable KernelTraceEvents For Provider
            session.EnableKernelProvider(kw);
            // Register Callbacks
            session.Source.Kernel.ProcessStart += CbOnProcessStart;
            // session.Source.Kernel.ProcessStop += cbOnProcessStop;

            session.Source.Process();       // Start Etw Session
            session.Stop();                 // Stop Etw Session
            session.Dispose();              // Release Etw Session Resource
        }
    }
    //-----------------------------------------------------------------------
    // Public Methods
    //-----------------------------------------------------------------------


    /// <summary>
    /// CallBack <c>OnProcessStart</c> handles the process creation event
    /// </summary>
    /// <param name="data">ProcessTraceData Struct</param>
    public void CbOnProcessStart(ProcessTraceData data)
    {
        MyEtwRecord?.UpdateEtwRecord(data.TimeStamp,
                                     TryGetProcessById(data.ParentID),
                                     data.ImageFileName,
                                     MyRegex().Replace(data.CommandLine, " "),
                                     data.ParentID,
                                     data.ProcessID);
        this.ToStdoutJson();
    }


    /// <summary>
    /// Callback <c>OnProcessStop</c> hanldes the process terminate event
    /// </summary>
    /// <param name="data">ProcessTraceDataStruct</param>
    public void cbOnProcessStop(ProcessTraceData data)
    {
        Console.WriteLine(
            $@"
            Process Stop:
                etwTask             : {data.Task}
                etwTaskName         : {data.TaskName}
                PID                 : {data.ProcessID}
                Image Name          : {data.ImageFileName}
                Process ExitTime    : {data.TimeStamp.ToString("o")}
            "
        );
    }


    //-----------------------------------------------------------------------
    // Private Methods
    //-----------------------------------------------------------------------


    [System.Text.RegularExpressions.GeneratedRegex(@"\s+")]
    private static partial System.Text.RegularExpressions.Regex MyRegex();
}