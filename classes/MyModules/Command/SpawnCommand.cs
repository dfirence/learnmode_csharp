using System.Diagnostics;
using static System.Console;
using static System.Diagnostics.Process;


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

    public static string GetCommandByPlatform(string os)
    {
        string cmd = string.Empty;
        if (os.Contains("Win32NT"))
        {
            cmd = @"C:\Windows\System32\ipconfig.exe";
        }
        else if (cmd.Contains("Unix"))
        {
            cmd = @"/sbin/ifconfig";
        }
        return cmd;
    }
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