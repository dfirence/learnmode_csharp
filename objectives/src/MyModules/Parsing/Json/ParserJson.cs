using System;
using System.Diagnostics;
using System.Text.Json;
using Common;


namespace MyModules.Parsing.Json;


public class Fruit
{
    public DateTime? date { get; set; }
    public string? name { get; set; }
    public string? fruitType { get; set; }
}

public class ParserJson : MyAbstractClass
{
    public string item = string.Empty;
    public override void Run()
    {
        ToJson();
        FromJson();
        LargeTest();
    }
    public void ToJson()
    {
        Fruit fruit = GetNewFruit();
        item = JsonSerializer.Serialize(fruit);
        var options = new JsonSerializerOptions { WriteIndented = true };
        string pretty = JsonSerializer.Serialize(fruit, options);

        Display($@"
                JSONL Fruit Record:
                    {item}

                JSON Pretty Fruit Record:
                    {pretty}");
    }
    public void FromJson()
    {
        Fruit? fruit = JsonSerializer.Deserialize<Fruit>(item);

        Console.WriteLine($@"
            Deserialized JSON Fruit String
            Date        : {fruit?.date}
            Name        : {fruit?.name}
            FruitType   : {fruit?.fruitType}
        ");
    }
    public Fruit GetNewFruit()
    {
        return new Fruit
        {
            date = DateTime.UtcNow,
            name = "Kiwi",
            fruitType = "Tropical"
        };
    }
    public void LargeTest()
    {
        const int MAX_CYCLES = 1_000_000;
        string[] items = new string[MAX_CYCLES];
        // Fruit[] items = new Fruit[MAX_CYCLES];

        Stopwatch sw = new Stopwatch();
        sw.Start();
        for (var i = 0; i < MAX_CYCLES; i++)
        {
            //Fruit f = GetNewFruit();
            string f = JsonSerializer.Serialize(GetNewFruit());
            items[i] = f;
        }
        // string json = JsonSerializer.Serialize(items);
        sw.Stop();
        TimeSpan ts = sw.Elapsed;

        string elapsedSerialize = string.Format(
            "{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);

        sw.Reset();
        // New Array, simulate some jsonl file loaded
        string[] items2 = new string[MAX_CYCLES];

        for (var i = 0; i < MAX_CYCLES; i++)
        {
            string f = JsonSerializer.Serialize(GetNewFruit());
            items2[i] = f;
        }
        // Start Deserialization
        sw.Start();
        for (var i = 0; i < MAX_CYCLES; i++)
        {
            Fruit? _ = JsonSerializer.Deserialize<Fruit>(items2[i]);
        }
        sw.Stop();
        ts = sw.Elapsed;
        string elapsedDeSerialize = string.Format(
            "{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);

        Console.WriteLine($@"
            LargeTest: {MAX_CYCLES} Fruit Strings
            ===================================================
            Serialize       : {elapsedSerialize}
            Deserialized    : {elapsedDeSerialize}
        ");
    }
}