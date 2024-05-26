using System.Diagnostics;
using System.Runtime.InteropServices;
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

        string headers = $"\n\t\t{"StarTime",-20}\t{"PID",-15}{"ProcessName",-32}\n";
        Console.WriteLine(headers);

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
                    Console.WriteLine(
                        $"\t\t{p.StartTime}\tPID {p.Id,-10}{p.ProcessName,-32} => {p.HasExited,5}{p.HandleCount,5}"
                    );
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"\t\tPID {p.Id} => Warning!!! {e.Message}");
            }
        }
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
        Process[] processes = Process.GetProcessesByName(processName);

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
    showProcessDetails:
        string modules = $"\n\n\t\t{"EntryPoint",-16}{"Base",-16}{"Module (DLL) Name",-32}{"Module (DLL) Path"}\n";
        foreach (ProcessModule m in p.Modules)
        {
            string ep = $"{m.EntryPointAddress:X}";
            string ba = $"{m.BaseAddress:X}";
            modules += $"\n\t\t{ba,-16}{ep,-16}{m.ModuleName,-24}\t{m.FileName}";
        }
        Display($@"
            Start Time      : {p.StartTime}
            Name            : {p.ProcessName}
            PID             : {pid}
            HasExited       : {p.HasExited}
            Handles (Count) : {handlesCount}
            Modules (Count) : {modulesCount}
            Threads (Count) : {threadsCount}
            -------------------------------------------------------------
            Modules
            {modules}
        ");
    }
}