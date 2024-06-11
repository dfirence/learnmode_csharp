global using System;


#if WINDOWS
using Watcher.Modules.Windows.ETW;
#endif

#if LINUX
using Watcher.Modules.Linux;
#endif

public class Program
{
    private static string AUTHOR {get;} = "carlos_diaz|@dfirence";
    private static string VERSION {get;} = "0.0.1";
    private static string PROGRAM_NAME {get;} = "Watcher";
    private static string DASHES {get;} = new string('-', 64);
    private static string HEADER { get; }
        = $@"
        {DASHES}
            {AUTHOR}
            {PROGRAM_NAME}
            {VERSION}
        {DASHES}
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
            case "--etw-providers-list":
                EtwProvidersList.GetProviders();
                break;
            case "--etw-process-monitoring":
                RunEtw();
                break;
            case "--evtx":
                break;
            default:
                HelpBanner();
                break;
        }
    }
    //--------------------------------------------------------------------------------
    // Windows Code Path
    //--------------------------------------------------------------------------------
#if WINDOWS
    public static void RunEtw()
    {
        var esp = new EtwEventsSubscriberProcess("MyKernelSession");
        Thread etwThread = new Thread(() => esp.Start());
        etwThread.Start();
        etwThread.Join();
    }
    /// <summary>
    /// Windows Banner Showing Program Help
    /// </summary>
    public static void HelpBanner()
    {
        Console.WriteLine($@"
        {HEADER}
 
        --help              Shows This Help Menu

        {DASHES}
        ETW
            --etw-providers-list
            --etw-process-monitoring

        {DASHES}
        EVTX::
        ");
    }
#endif
    //--------------------------------------------------------------------------------
    // Linux Code Path
    //--------------------------------------------------------------------------------
#if LINUX
    public static void HelpBanner()
    {

    }
#endif
}