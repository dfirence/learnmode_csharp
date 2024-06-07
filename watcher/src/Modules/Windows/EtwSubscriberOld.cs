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
    public int? ParentId { get; set; }
    public int? ProcessId { get; set; }

    public EtwEventProcessStart()
    {
        EventName = GetType().Name;
    }

    public EtwEventProcessStart(DateTime timeProcessStart,
                                string parent,
                                string process,
                                string commandLine,
                                int parentId,
                                int processId)
    {
        EventName = GetType().Name;

        Update(timeProcessStart, parent, process,
               commandLine, parentId, processId);
    }

    public void Update(DateTime timeProcessStart,
                       string parent,
                       string process,
                       string commandLine,
                       int parentId,
                       int processId)
    {
        TimeGenerated = DateTime.UtcNow.ToString("o");
        TimeProcessCreated = timeProcessStart.ToUniversalTime().ToString("o");
        Parent = parent;
        Process = process;
        CommandLine = commandLine;
        ParentId = parentId;
        ProcessId = processId;
    }
}

public abstract class EtwSubscriber : IDisposable
{
    public string TraceSessionName { get; set; }
        = $"DefaultTraceSession-{DateTime.UtcNow.ToString("o")}";

    public bool HasAdminRights { get; } = Environment.IsPrivilegedProcess;

    public JsonSerializerOptions JsonOptions { get; }
        = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public EtwEventProcessStart? MyEtwRecord { get; set; }

    private TraceEventSession? _session;

    public void StartSession()
    {
        if (!HasAdminRights)
        {
            Console.WriteLine("Must run with elevated privileges.");
            return;
        }
        var kw = KernelTraceEventParser.Keywords.Process;

        _session = new TraceEventSession(TraceSessionName);
        _session.EnableKernelProvider(kw);
        _session.Source.Kernel.ProcessStart += OnProcessStart;
        _session.Source.Kernel.ProcessStop += OnProcessStop;

        Console.WriteLine("Listening for events... Press CTRL+C to exit.");

        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            StopSession();
        };

        _session.Source.Process();
    }

    public void StopSession()
    {
        _session?.Stop();
        _session?.Dispose();
        _session = null;
        Console.WriteLine("ETW session stopped.");
    }

    public void Dispose()
    {
        StopSession();
    }

    public string ToJson()
    {
        return JsonSerializer.Serialize(MyEtwRecord, JsonOptions);
    }

    public void ToStdoutJson()
    {
        Console.WriteLine(ToJson());
        Console.Out.Flush();
    }

    public string TryGetProcessById(int pid)
    {
        try
        {
            return Process.GetProcessById(pid).ProcessName ?? "null";
        }
        catch (Exception error)
        {
            Console.Error.WriteLine($"{GetType().Name} : {error.Message}");
            return "null";
        }
    }

    protected abstract void OnProcessStart(ProcessTraceData data);

    protected abstract void OnProcessStop(ProcessTraceData data);
}

public partial class MySubscriberTest : EtwSubscriber
{
    public MySubscriberTest(string mySessionName = "")
    {
        if (!string.IsNullOrEmpty(mySessionName))
        {
            TraceSessionName = mySessionName;
        }

        MyEtwRecord = new EtwEventProcessStart();
    }

    protected override void OnProcessStart(ProcessTraceData data)
    {
        MyEtwRecord?.Update(data.TimeStamp,
                            TryGetProcessById(data.ParentID),
                            data.ImageFileName,
                            MyRegex().Replace(data.CommandLine, ""),
                            data.ParentID,
                            data.ProcessID);
        ToStdoutJson();
    }

    protected override void OnProcessStop(ProcessTraceData data)
    {
        Console.WriteLine($@"
                Process Stop:
                    etwTask             : {data.Task}
                    etwTaskName         : {data.TaskName}
                    PID                 : {data.ProcessID}
                    Image Name          : {data.ImageFileName}
                    Process ExitTime    : {data.TimeStamp.ToString("o")}
            ");
    }

    [System.Text.RegularExpressions.GeneratedRegex(@"\s+")]
    private static partial System.Text.RegularExpressions.Regex MyRegex();
}