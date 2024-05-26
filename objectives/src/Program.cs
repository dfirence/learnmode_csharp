using MyModules.Command;
using MyModules.Filesystem.Enumeration;
using MyModules.Filesystem.Discovery;
using MyModules.Strings;
using MyModules.SysInfo;
using MyModules.SysInfo.DeviceInfo;

/// <summary>
/// Main <c>Program</c> Class
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
            Banner();
            return;
        }

        switch (args[0])
        {
            //--------------------------------------------------
            // Command Module - Learning Process Spawning When
            //--------------------------------------------------
            case "-c":
            case "--command":
                SpawnCommand.Run();
                break;
            //--------------------------------------------------
            // Filesystem Module - Learning Filesystem Stuff
            //--------------------------------------------------
            case "-f1":
            case "--special-folders":
                new SpecialFolders().Run();
                break;

            case "-f2":
            case "--file-create":
                new FileProfile().Run();
                break;

            case "-f3":
            case "--file-delete":
                new FileProfile().DeleteFile();
                break;

            case "-f4":
            case "--folder-delete":
                new FileProfile().DeleteFolder();
                break;

            case "-f5":
            case "--folder-enumerate":
                new FileProfile().EnumerateFolder();
                break;
            //--------------------------------------------------
            // Strings Module - Learning String Manipulation
            //--------------------------------------------------      
            case "-h":
            case "--help":
                Banner();
                break;
            //--------------------------------------------------
            // Strings Module - Learning String Manipulation
            //--------------------------------------------------
            case "-s":
            case "--string":
                new MyString("Lorem Ipsum 42").Run();
                break;
            //--------------------------------------------------
            // Runtime Module - Learning Static Classes
            //--------------------------------------------------
            case "-p":
            case "--get-processes":
                new ProcessDiscovery().Run();
                break;
            //--------------------------------------------------
            // Runtime Module - Learning Static Classes
            //--------------------------------------------------
            case "-r":
            case "--runtime":
                DeviceRuntime.Run();
                break;
            default:
                Banner();
                break;
        }
    }

    /// <summary>
    /// Main Program Banner & Usage
    /// </summary>
    public static void Banner()
    {
        string dashes = new('-', 64);
        Console.WriteLine(
            $@"
            {dashes}
            Author              carlos_diaz | @dfirence
            Purpose             LearnMode C# Classes
            Version             0.0.1
            {dashes}
            
            Usage:              classes.exe [switch]

            -c, --command               Runs the spawn command program - executes a benign process given the hosted platform type;
            -f1, --special-folders      Runs the filesystem program - enumerates default special folders on the hosted platform;
            -f2, --file-create          Runs the filesystem program - creates a file, gets its metadata;
            -f3, --file-delete          Runs the filesystem program - deletes file previously created in `-f2` option;
            -f4, --folder-delete        Runs the filesystem program - deletes folder previously created in `-f2`;
            -f5, --folder-enumerate     Runs the filesystem program - enumerates folder for its filesystem entries;
            -h, --help                  Runs the banner module;
            -p, --get-processes         Runs the process program - enumerates running processes on the hosted platform;
            -r, --runtime               Runs the runtime program - profiles the active process and its hosted environment;
            -s, --strings               Runs the strings program - various methods to learn strings in C#;
            "
        );
    }
}