using System;
using System.Runtime.InteropServices;

#if WINDOWS
using Watcher.Modules.Windows.ETW;
#endif

#if LINUX || MACOS
using Watcher.Modules.Linux;
#endif

using Watcher.Modules.Common;

/// <summary>
/// The main entry point for the Watcher application.
/// </summary>
public static class Program
{
    /// <summary>
    /// Author of the program.
    /// </summary>
    private static string Author { get; } = "carlos_diaz|@dfirence";

    /// <summary>
    /// Version of the program.
    /// </summary>
    private static string Version { get; } = "0.0.1";

    /// <summary>
    /// Name of the program.
    /// </summary>
    private static string ProgramName { get; } = "Watcher";

    /// <summary>
    /// Dashes used for formatting the header.
    /// </summary>
    private static string Dashes { get; } = new string('-', 64);

    /// <summary>
    /// Header displayed in the help banner.
    /// </summary>
    private static string Header { get; } = $@"
        {Dashes}
            {Author}
            {ProgramName}
            {Version}
        {Dashes}
        Usage:  watcher.exe [switch]";

    /// <summary>
    /// Main entry point of the application.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            HelpBanner();
            return;
        }
        switch(args[0].Trim().ToLower())
        {
            case "--get-processes":
                ProcessDiscovery.GetProcesses();
                break;
#if WINDOWS
            case "--etw-providers-list":
                EtwProvidersList.GetProviders();
                break;
            case "--etw-monitor-processes":
                RunEtw();
                break;
            case "--evtx":
                break;
#endif
            default:
                HelpBanner();
                break;
        }
    }

    //--------------------------------------------------------------------------------
    // Windows Code Path
    //--------------------------------------------------------------------------------
#if WINDOWS
    /// <summary>
    /// Runs ETW monitoring process on Windows.
    /// </summary>
    private static void RunEtw()
    {
        var esp = new EtwEventsSubscriberProcess("MyKernelSession");
        Thread etwThread = new Thread(() => esp.Start());
        etwThread.Start();
        etwThread.Join();
    }

    /// <summary>
    /// Displays the help banner for the Windows version of the program.
    /// </summary>
    private static void HelpBanner()
    {
        Console.WriteLine($@"
            {Header}

            --help              Shows This Help Menu

            {Dashes}
            Cross Platform      Available on Linux, MacOS, Windows

                --get-processes
            {Dashes}
            ETW

                --etw-providers-list
                --etw-monitor-processes

            {Dashes}
            EVTX::
            ");
    }
#endif

    //--------------------------------------------------------------------------------
    // Linux and macOS Code Path
    //--------------------------------------------------------------------------------
#if LINUX || MACOS
    private static UseUnixArgs()
    {}
    /// <summary>
    /// Displays the help banner for the Linux or macOS version of the program.
    /// </summary>
    private static void HelpBanner()
    {
        Greet.HelloWorld();
    }
#endif
}