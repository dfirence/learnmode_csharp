using MyModules.Strings;
using MyModules.SysInfo.DeviceInfo;

/// <summary>
/// Main Program Class
/// </summary>
class Program
{
    /// <summary>
    /// Main Entrypoint
    /// </summary>
    /// <param name="args">stdin arguments</param>
    public static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Program.Banner();
            return;
        }

        switch (args[0])
        {
            //--------------------------------------------------
            // Command Module - Learning Process Spawning
            //--------------------------------------------------
            case "-c":
            case "--command":
                break;
            //--------------------------------------------------
            // Strings Module - Learning String Manipulation
            //--------------------------------------------------
            case "-s":
            case "--string":
                MyString s = new ("Lorem Ipsum 42");
                s.Run();
                break;
            //--------------------------------------------------
            // Runtime Module - Learning Static Classes
            //--------------------------------------------------
            case "-r":
            case "--runtime":
                DeviceRuntime.Run();
                break;
        }
    }

    /// <summary>
    /// Main Program Banner & Usage
    /// </summary>
    public static void Banner()
    {
        string dashes = new string('-', 64);
        Console.WriteLine(
            $@"
            {dashes}
            Author              carlos_diaz | @dfirence
            Purpose             LearnMode C# Classes
            Version             0.0.1
            {dashes}
            
            Usage:              classes.exe [switch]

            -r, --runtime       Runs the runtime program - profiles the active process and its hosten environ;
            -s, --strings       Runs the strings program - various methods to learn strings in C#;
            "
        );
    }
}