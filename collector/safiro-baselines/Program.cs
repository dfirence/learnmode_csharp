namespace Safiro;

using Safiro.Modules;
using System;
using System.Threading.Tasks;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Create an instance of the PEFileCollector
        var progressBar = new ProgressBar();
        var peCollector = new PeFileCollector();
        string target_dir = @"C:\Windows\System32";
        string outputDir = @"C:\users\archir\Desktop\Testing";   // Path to output directory (for JSON, CSV, etc.)

        // Ensure the output directory exists
        if (!System.IO.Directory.Exists(outputDir))
        {
            System.IO.Directory.CreateDirectory(outputDir);
        }

        // Call the ProcessPeFileAsync method to collect PE file metadata asynchronously
        await peCollector.CollectFilesFromMultipleAreasAsync(outputDir);

        Console.WriteLine("\nPE file processing completed.");
    }
}


public class ProgressBar
{
    // Placeholder class for progress bar. Implement it if you want to show progress updates.
    // For now, it's just an empty class.
}
