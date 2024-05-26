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

        string headers = $"\n\t\t{"StarTime",-20}\t{"PID",-15}{"ProcessName",-32}\n";
        Console.WriteLine(headers);

        foreach (var p in processes)
        {
            if (p.Id == 0) {
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
}