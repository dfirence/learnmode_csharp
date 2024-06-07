﻿using Watcher.Modules.Windows.ETW;
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
            "datasamples",
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

        var dynamicObjects = handler.CreateDynamicObject();
        Console.WriteLine($"Loading Config From: {schemaPath}");

        var etw = new SubscriberProcessStart("MySession", schemaPath, testEvent);
        etw.Start();
    }
    /*static void Main(string[] args)
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
    }*/
}

