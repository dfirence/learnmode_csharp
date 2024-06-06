using Watcher.Modules.Windows;
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
        var schemaFilePath = Path.Join(Environment.CurrentDirectory,
                                        "datasamples", "sampleConf.yaml");

        var handler = new SchemaHandler();

        try
        {
            handler.LoadSchema(schemaFilePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading schema: {ex.Message}");
            return;
        }

        var dynamicObjects = handler.CreateDynamicObject();
        if (dynamicObjects != null)
        {
            var jsonOutput = handler.ToJson(dynamicObjects);
            Console.WriteLine("Serialized to JSON:");
            Console.WriteLine(jsonOutput);
        }
        else
        {
            Console.WriteLine("No events found in schema.");
        }
    }
}

