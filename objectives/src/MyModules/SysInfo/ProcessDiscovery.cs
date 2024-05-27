using System.Diagnostics;

using Common;


/// <summary>
/// Process Discovery Class uses the stdlib
/// and its `GetProcesses()` to enumerate
/// basic characteristics of processes.
/// </summary>
public class ProcessDiscovery : MyAbstractClass
{
    public override void Run()
    {
        Process[] processes = Process.GetProcesses().OrderBy(x => x.ProcessName).ToArray<Process>();
        Display($"{processes.Length} Processes\n");

        string processListing = $"\n\t\t{"StarTime",-20}\t{"PID",-15}{"ProcessName",-32}\n";

        foreach (var p in processes)
        {
            if (p.Id == 0)
            {
                continue;
            }
            try
            {
                if (!string.IsNullOrEmpty(p.ProcessName))
                {
                    processListing
                    += $"\n\t\t{p.StartTime}\tPID {p.Id,-10}{p.ProcessName,-32} => {p.HasExited,5}{p.HandleCount,5}";
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"\t\tPID {p.Id} => Warning!!! {e.Message}");
            }
        }
        Console.WriteLine(processListing);
    }
    /// <summary>
    /// Gets a Process instance by name (string)
    /// </summary>
    public void GetProcessByName()
    {
        Console.Write("\n\n\t\t[?] What's the process name >>> ");
        string? processName = Console.ReadLine();

        if (string.IsNullOrEmpty(processName) || string.IsNullOrWhiteSpace(processName))
        {
            Display("Error - Input Param Cannot Be Null or Empty");
            return;
        }
        processName = processName.Trim();
        Process[] processes = Process.GetProcessesByName(processName).OrderBy(x => x.ToString()).ToArray();

        if (processes.Length == 0)
        {
            Display($"Info - No Processes By {processName}");
            return;
        }

        Display($"(!) Success: {processes.Length} Processes Actively Running\n\n");
        string results = "\n";
        string dashes = new string('-', 128);

        foreach (Process p in processes)
        {
            results += $@"
            {dashes}
                        {p.StartTime}   PID {p.Id}   {p.ProcessName}
            {dashes}\n\n
            ";

            foreach (ProcessModule mod in p.Modules)
            {
                results += $"\n\t\t\t{mod.EntryPointAddress:X},{mod.BaseAddress:X},{mod.ModuleName}";
            }
        }
        Console.WriteLine(results);
    }
    /// <summary>
    /// Get a Process Object By PID and use the managed methods
    /// to extract its running profile - very basic compared to
    /// unmanaged code.
    /// </summary>
    public void GetProcessByPid()
    {
        Console.Write("\n\n\t\t(?) What is the PID >>> ");
        string? input = Console.ReadLine();

        if (string.IsNullOrEmpty(input) || string.IsNullOrWhiteSpace(input))
        {
            Display("Error - Input cannot be Null or Empty");
            return;
        }

        int pid;
        int handlesCount;
        int modulesCount;
        int threadsCount;
        Process p;

        try
        {
            pid = Convert.ToInt32(input);
            p = Process.GetProcessById(pid);
            handlesCount = p.HandleCount;
            modulesCount = p.Modules.Count;
            threadsCount = p.Threads.Count;
            goto showProcessDetails;
        }
        catch (Exception e)
        {
            Display($"Error - {e.Message}");
            return;
        }
    // Code Label - Pretty Cool!
    showProcessDetails:
        string modules = $"\n\n\t\t{"EntryPoint",-16}{"Base",-16}{"Module (DLL) Name",-32}{"Module (DLL) Path"}\n";

        // Sort Module Entries Alphabetically By Module name
        ProcessModule[]? items = p.Modules.Cast<ProcessModule>()
            .OrderBy(x => x.ModuleName)
            .ToArray();

        foreach (ProcessModule m in items)
        {
            string ep = $"{m.EntryPointAddress:X}";
            string ba = $"{m.BaseAddress:X}";
            modules += $"\n\t\t{ba,-16}{ep,-16}{m.ModuleName,-24}\t{m.FileName}";
        }

        string threads = $"";
        foreach (ProcessThread t in p.Threads)
        {
            string sa = $"{t.StartAddress:X}";
            threads += $"\n\t\t{t?.StartTime,-28}{t.Id,-12}{sa,-16}{t.ThreadState}";
        }
        Display($@"
            Start Time      : {p.StartTime}
            Name            : {p.ProcessName}
            PID             : {pid}
            HasExited       : {p.HasExited}
            Handles (Count) : {handlesCount}
            Modules (Count) : {modulesCount}
            Threads (Count) : {threadsCount}
            --------------------------------------------------------------------
            Modules
            {modules}
            --------------------------------------------------------------------
            Threads
            {threads}
        ");
    }
}