using Watcher.Modules.Windows.ETW;
using Watcher.Modules.Schema;
using Watcher.Common;


namespace Watcher;


/// <summary>
/// Program EntryPoint
/// </summary>


class Program
{
    static void Main(string[] args)
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
}

