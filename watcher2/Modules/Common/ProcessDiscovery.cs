using System.Diagnostics;
using System.Text;

namespace Watcher.Modules.Common;

/// <summary>
/// Process Discovery with native library methods
/// gets a list of running processes on the device.
/// </summary>
public static class ProcessDiscovery
{
    public static void GetProcesses()
    {
        try
        {
            StringBuilder sb = new();
            foreach (var p in Process.GetProcesses())
            {
                using (p)
                {
                    string path = GetProcessMetadata(in p) ?? "";
                    sb.AppendFormat($"({p.Id, 6}) {p.ProcessName, -32} {path}\n");
                }
            }
            Console.WriteLine(sb.ToString());
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
        }
    }
    /// <summary>
    /// Attempts to get Process Metadata (Process Path)
    /// after checking if the current runtime process is
    /// elevated with Admin Rights.
    ///
    /// Failure is expected against PPL processes since
    /// this program does not run as PPL.
    /// 
    /// </summary>
    /// <param name="process"></param>
    /// <returns></returns>
    private static string? GetProcessMetadata(in Process process)
    {
        if (!Environment.IsPrivilegedProcess) return null;
        try
        {
            // Get Full Process Path via LoadedModule
            return process?.MainModule?.FileName;
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }
}