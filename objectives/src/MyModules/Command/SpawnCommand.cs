using System.Diagnostics;
using static System.Console;

namespace MyModules.Command;


/// <summary>
/// SpawnCommand Class. Used to spawn new processes.
/// </summary>
public class SpawnCommand
{
    /// <summary>
    /// Constructor With Command
    /// </summary>
    public static void Run()
    {
        string os = Environment.OSVersion.Platform.ToString();
        string cmd = GetCommandByPlatform(os);
        if (string.IsNullOrEmpty(cmd))
        {
            Display("No Command To Run...");
            return;
        }
        Display(cmd);
        ExecuteProgram(cmd);
    }
    /// <summary>
    /// Template String for stdout
    /// </summary>
    /// <param name="s"></param>
    public static void Display(string s)
    {
        string dashes = new('-', 64);
        WriteLine(
            $@"
            Spawn Command Module
            {dashes}
            Will Run: {s}
            {dashes}
            "
        );
    }
    /// <summary>
    /// Determines which command to run based on
    /// the platform this program is running under.
    /// </summary>
    /// <param name="os"></param>
    /// <returns>string</returns>
    public static string GetCommandByPlatform(string os)
    {
        string cmd = string.Empty;
        if (os.Contains("Win32NT"))
        {
            cmd = @"C:\Windows\System32\ipconfig.exe";
        }
        else if (os.Contains("Unix"))
        {
            cmd = @"whoami";
        }
        return cmd;
    }
    /// <summary>
    /// Executes or spawns a new process based.
    /// </summary>
    /// <param name="os"></param>
    public static void ExecuteProgram(string os)
    {
        try
        {
            using Process p = Process.Start(os);
            p.WaitForExit(1000);
        }
        catch (Exception e)
        {
            WriteLine(e.Message);
        }
    }
}