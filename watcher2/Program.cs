using Watcher.Modules.Common;

#if WINDOWS
using Watcher.Modules.Windows.ETW;
#endif

#if LINUX || MACOS
using Watcher.Modules.Linux;
#endif

namespace Watcher;

public static class Program
{
    private static string Author { get; } = "carlos_diaz|@dfirence";
    private static string Version { get; } = "0.0.1";
    private static string ProgramName { get; } = "Watcher";
    private static string Dashes { get; } = new string('-', 64);
    private static string Header { get; }
        = $@"
        {Dashes}
            {Author}
            {ProgramName}
            {Version}
        {Dashes}
        Usage:  watcher.exe [switch]";

    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            HelpBanner();
            return;
        }
        switch (args[0].Trim().ToLower())
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
    private static void RunEtw()
    {
        var esp = new EtwEventsSubscriberProcess("MyKernelSession");
        Thread etwThread = new Thread(() => esp.Start());
        etwThread.Start();
        etwThread.Join();
    }
    /// <summary>
    /// Windows Banner Showing Program Help
    /// </summary>
    private static void HelpBanner()
    {
        Console.WriteLine($@"
        {Header}
 
        --help              Shows This Help Menu

        {Dashes}
        General Purpose Functionality
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
    // Linux Code Path
    //--------------------------------------------------------------------------------

#if LINUX || MACOS
    private static void HelpBanner()
    {
        Greet.HelloWorld();
    }
#endif
}