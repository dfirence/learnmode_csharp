using Safiro.Modules.FileCollectors.PeFiles;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Safiro
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var peCollector = new PeFileCollector();

            // Ensure at least one argument is provided
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: Safiro.exe <file_or_directory_path> [output_directory]");
                return;
            }

            string inputPath = args[0];
            string? outputDir = args.Length > 1 ? args[1] : null;

            // If input is a file, process the single file
            if (File.Exists(inputPath))
            {
                Console.Error.WriteLine($"Scanning single file: {inputPath}");
                await peCollector.CollectSingleFileAsync(inputPath); // No outputDir for single file mode
            }
            // If input is a directory, process all PE files in the directory
            else if (Directory.Exists(inputPath))
            {
                Console.Error.WriteLine($"Scanning directory: {inputPath}");
                if (outputDir != null && !Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir); // Ensure output directory exists
                }
#pragma warning disable CS8604 // Possible null reference argument.
                await peCollector.CollectFilesFromMultipleAreasAsync(outputDir); // Pass the outputDir
#pragma warning restore CS8604 // Possible null reference argument.
            }
            else
            {
                Console.WriteLine($"Invalid path: {inputPath} is neither a file nor a directory.");
            }

            Console.WriteLine("\nPE file processing completed.");
        }
    }
}
