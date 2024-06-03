using System;
using Common;

namespace MyModules.Async;


public class AsyncCounters
{
    public static readonly int MAX_COUNT = 10_000_000;
    public readonly int START_COUNT = 0;
    public static int CURRENT_COUNTER = 0;
    public async Task Run()
    {
        for (var i = 0; i < MAX_COUNT; ++i)
        {
            await Increment();
        }
    }
    public static async Task Increment()
    {
        var delay = new Random().Next(10, 16);
        await Task.Delay(delay);
        CURRENT_COUNTER += 1;
        int down = MAX_COUNT - CURRENT_COUNTER;
        Console.Write($"\rDelay: {delay,5} >> Up: {CURRENT_COUNTER,7} <> Down {down}");
    }
}