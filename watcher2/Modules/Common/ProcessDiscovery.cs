using System.Diagnostics;
using System.Text;

namespace Watcher.Modules.Common;

/// <summary>
/// Provides methods for process discovery using native library methods.
/// </summary>
public static class ProcessDiscovery
    {
    /// <summary>
    /// Gets a list of running processes on the device and prints their details.
    /// </summary>
    public static void GetProcesses()
    {
        try
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\n\n");
            // Iterate through all running processes.
            foreach (var p in Process.GetProcesses())
            {
                // System.Diagnostics.Process is a resource, drop/free after use.
                using (p)
                {
                    // Get process metadata (e.g., process path) if available.
                    var path = GetProcessMetadata(in p) ?? "";

                    // Append process details to the string builder.
                    sb.AppendFormat($"({p.Id, 6}) {p.ProcessName, -32} {path}\n");
                }
            }

            // Print all collected process details.
            Console.WriteLine(sb.ToString());
        }
        catch (Exception e)
        {
            // Print any error messages encountered during the process discovery.
            Console.Error.WriteLine(e.Message);
        }
    }

    /// <summary>
    /// Attempts to get process metadata, such as the full process path.
    /// Checks if the current runtime process has elevated privileges (admin rights).
    /// Failure to retrieve metadata is expected for Protected Process Light (PPL) processes
    /// since this program does not run as a PPL.
    /// </summary>
    /// <param name="process">The process for which to retrieve metadata.</param>
    /// <returns>The full process path if available; otherwise, null.</returns>
    private static string? GetProcessMetadata(in Process process)
    {
        // Check if the current process has elevated privileges.
        if (!Environment.IsPrivilegedProcess) return null;

        try
        {
            // Get the full process path via the main module.
            return process?.MainModule?.FileName;
        }
        catch (Exception e)
        {
            // Return the exception message if an error occurs.
            return e.Message;
        }
    }
}