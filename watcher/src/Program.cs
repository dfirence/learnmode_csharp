using Watcher.Modules.Windows.ETW;
using Watcher.Modules.Schema;
using Watcher.Common;


namespace Watcher;


/// <summary>
/// Program EntryPoint
/// </summary>


class Program
{
    private static protected string AUTHOR = "carlos_diaz|@dfirence";
    private static protected string PROGRAM = "experiment etw_watcher";

    public static void Main(string[] args)
    {
        if (args.Length == 0)
            goto exitProcessWithHelp;

        switch (args[0].Trim().ToLower())
        {
            case "-p":
            case "--processes":
                RunProcesses();
                break;
            default:
                goto exitProcessWithHelp;
        }
    exitProcessWithHelp:
        Banner();
        return;
    }
    static void Banner()
    {
        string dashes = new('-', 64);
        Console.Error.WriteLine(
            $@"
            {dashes}
            {PROGRAM}
            {AUTHOR}
            {dashes}
            usage:  watcher.exe [switch]
            "
        );
    }

    static void RunProcesses()
    {
        var testEvent = "etwProcessStart";
        var schemaPath = Path.Join(
            Environment.CurrentDirectory,
            "sampleConf.yaml"
        );

        var handler = new SchemaHandler();

        try
        {
            handler.LoadSchema(schemaPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading schema: {ex.Message}");
            return;
        }

        // var dynamicObjects = handler.CreateDynamicObject();
        Console.WriteLine($"Loading Config From: {schemaPath}");

        var etw = new SubscriberProcessStart(
            "MySession", schemaPath, testEvent);
        etw.Start();
    }
    static void RunSchemaCheckFromFile(string confFilePath)
    {
        if (string.IsNullOrEmpty(confFilePath) ||
            string.IsNullOrWhiteSpace(confFilePath))
        {
            Console.Error.WriteLine("Conf File Path cannot be null or empty");
        }
        string f = confFilePath.Trim();
    }
}
