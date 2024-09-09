using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text.Json;
//using System.Threading.Tasks;
using System.Diagnostics;

using PeNet;

namespace Safiro.Modules.FileCollectors.PeFiles;

/// <summary>
/// Represents a progress bar that tracks the processing progress and provides updates on completion status.
/// </summary>
public class ProgressBar
{
    private readonly int _total; // Total number of items to process
    private const int BarLength = 50; // Length of the progress bar
    private readonly int _updateThreshold; // Number of processed items needed to update the progress bar
    private int _processed; // Number of items processed
    private int _accessDeniedCount; // Number of items skipped due to access denial
    private int _invalidSizeCount; // Number of items skipped due to invalid size
    private int _invalidPeCount; // Number of items skipped due to invalid PE (Portable Executable) format
    private int _skippedCount; // Number of items skipped due to cache
    private int _lastUpdateCount; // Last count of processed items at the time of the last update

    /// <summary>
    /// Initializes a new instance of the <see cref="ProgressBar"/> class.
    /// </summary>
    /// <param name="total">The total number of items to process.</param>
    /// <param name="updateThreshold">The number of processed items required to update the progress bar. Default is 100.</param>
    public ProgressBar(int total, int updateThreshold = 100)
    {
        _total = total;
        _processed = 0;
        _accessDeniedCount = 0;
        _invalidSizeCount = 0;
        _invalidPeCount = 0;
        _skippedCount = 0; // Initialize skipped files count
        _updateThreshold = updateThreshold;
        _lastUpdateCount = 0;
    }

    /// <summary>
    /// Updates the progress based on the processing status of an item.
    /// </summary>
    /// <param name="isProcessed">Indicates if the item was successfully processed.</param>
    /// <param name="reason">The reason for skipping the item. This is used when the item is not processed.</param>
    /// <param name="isSkipped">Indicates if the item was skipped due to cache. This is mutually exclusive with <paramref name="isProcessed"/>.</param>
    public void UpdateProgress(bool isProcessed, string reason = null, bool isSkipped = false)
    {
        if (isSkipped)
        {
            _skippedCount++; // Increment skipped count
        }
        else if (isProcessed)
        {
            _processed++;
        }
        else if (reason == "access_denied")
        {
            _accessDeniedCount++;
        }
        else if (reason == "invalid_size")
        {
            _invalidSizeCount++;
        }
        else if (reason == "invalid_pe")
        {
            _invalidPeCount++;
        }

        // Only update the console output if the threshold is reached
        if (_processed - _lastUpdateCount >= _updateThreshold)
        {
            _lastUpdateCount = _processed;
            DisplayProgressBar();
        }
    }

    /// <summary>
    /// Displays the final progress state when processing is complete.
    /// </summary>
    public void Complete()
    {
        if (_processed < _total)
        {
            _processed = _total; // Set progress to total to show completion
        }
        DisplayProgressBar();  // Display final state
        Console.WriteLine();   // Move to the next line after completion
    }

    /// <summary>
    /// Displays the progress bar on the console.
    /// </summary>
    private void DisplayProgressBar()
    {
        int filledLength = (int)((double)_processed / _total * BarLength);
        int emptyLength = BarLength - filledLength;

        // Create the progress bar string
        string progressBar = new string('#', filledLength) + new string('-', emptyLength);
        Console.Write($"\r[{progressBar}] {_processed}/{_total} processed");
    }
}


public class PeFileCollector
{
    private readonly string[] _peExtensions = new[]
    {
        ".acm", ".com", ".cpl", ".dll", ".drv", ".efi",
        ".exe", ".msi", ".msu", ".ocx", ".scr", ".sys",
        ".tsp"
    };
    private readonly ConcurrentDictionary<string, string> _processedHashes = new ConcurrentDictionary<string, string>();
    private const int BatchSize = 100; // Define batch size for file processing

    public async Task CollectFilesFromMultipleAreasAsync(string outputDir)
    {
        // Start the stopwatch
        Stopwatch stopwatch = Stopwatch.StartNew();
        // Define the directories
        string[] directories = {
            @"C:\Windows",
            // @"C:\Users",
            // @"C:\ProgramData\",
            // @"C:\Program Files\",
            // @"C:\Program Files (x86)\"
        };

        // Run each directory processing task in parallel
        var tasks = directories.Select(dir => Task.Run(() => CollectPeFilesAsync(dir, outputDir)));
        await Task.WhenAll(tasks);
        stopwatch.Stop();
        Console.WriteLine($"PE file processing completed in {stopwatch.Elapsed.TotalSeconds:F2} seconds.");
    }
    // Method to process a single PE file, outputDir defaults to null
    public async Task CollectSingleFileAsync(string filePath)
    {
        var progressBar = new ProgressBar(1);
        await ProcessPeFileAsync(filePath, null, progressBar); // outputDir is null for single file mode
        progressBar.Complete();
    }
    private async Task CollectPeFilesAsync(string rootPath, string outputDir)
    {
        try
        {
            var peFiles = EnumeratePeFiles(rootPath).ToList();
            int totalFiles = peFiles.Count;
            var progressBar = new ProgressBar(totalFiles, updateThreshold: 100);

            for (int i = 0; i < totalFiles; i += BatchSize)
            {
                var batch = peFiles.Skip(i).Take(BatchSize);
                var tasks = batch.Select(filePath => Task.Run(() => ProcessPeFileAsync(filePath, outputDir, progressBar)));
                await Task.WhenAll(tasks);
            }
            progressBar.Complete();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing directory {rootPath}: {ex.Message}");
        }
    }

    private IEnumerable<string> EnumeratePeFiles(string rootPath)
    {
        var directories = new Stack<string>();
        directories.Push(rootPath);

        while (directories.Count > 0)
        {
            var currentDir = directories.Pop();

            // Skip reparse points (like symbolic links)
            var dirInfo = new DirectoryInfo(currentDir);
            if (dirInfo.Attributes.HasFlag(FileAttributes.ReparsePoint))
            {
                Console.WriteLine($"\nSkipping reparse point: {currentDir}");
                continue;
            }

            // Try to enumerate files
            IEnumerable<string> files = Enumerable.Empty<string>();
            try
            {
                files = Directory.EnumerateFiles(currentDir, "*.*", SearchOption.TopDirectoryOnly)
                                 .Where(file => _peExtensions.Contains(Path.GetExtension(file).ToLower()));
            }
            catch (UnauthorizedAccessException)
            {
                continue;
            }
            catch (Exception ex)
            {
                continue;
            }

            // Yield the valid files
            foreach (var file in files)
            {
                yield return file;
            }

            // Try to enumerate directories and add them to the stack
            try
            {
                foreach (var subDir in Directory.EnumerateDirectories(currentDir))
                {
                    directories.Push(subDir);
                }
            }
            catch (UnauthorizedAccessException)
            {
                //Console.WriteLine($"Access denied: {currentDir}");
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"Error enumerating directories in {currentDir}: {ex.Message}");
            }
        }
    }

    private async Task ProcessPeFileAsync(string filePath, string outputDir, ProgressBar progressBar)
    {
        try
        {
            // Perform all pre-checks and hash calculation in one file open operation
            var (isValid, sha256Hash, fileSize, reason) = await PerformFileChecksAndHashAsync(filePath);
            if (!isValid)
            {
                progressBar.UpdateProgress(false, reason);
                return;
            }

            // Check if the file was already processed based on its hash
            if (!_processedHashes.TryAdd(sha256Hash, filePath))
            {
                progressBar.UpdateProgress(false, reason: null, isSkipped: true);  // Skipped file due to cache
                return;
            }

            // Parse the PE file (using PeNet or your preferred library)
            var peFile = new PeFile(filePath);

            // If the file is not a valid PE structure
            if (peFile == null)
            {
                progressBar.UpdateProgress(false, "invalid_pe");
                return;
            }

            var peInfo = new PeFileInfoBuilder()
                .SetName(Path.GetFileName(filePath))
                .SetPath(Path.GetFullPath(filePath))
                .SetSize(fileSize)
                .SetIs64Bit(peFile.Is64Bit)
                .SetIsLib(peFile.ImageNtHeaders.FileHeader.Characteristics.HasFlag(PeNet.Header.Pe.FileCharacteristicsType.Dll))
                .SetIsDotNet(peFile.ImageComDescriptor != null)
                .SetMachineResolved(peFile.ImageNtHeaders.FileHeader.MachineResolved)
                .SetHasImports(peFile.ImportedFunctions != null && peFile.ImportedFunctions.Length > 0)
                .SetHasExports(peFile.ExportedFunctions != null && peFile.ExportedFunctions.Length > 0)
                .SetSubsystem((ushort)peFile.ImageNtHeaders.OptionalHeader.Subsystem)
                .SetSubsystemCaption(peFile.ImageNtHeaders.OptionalHeader.SubsystemResolved)
                .SetLibs(GetImportedLibraries(peFile))
                .SetImports(GetImports(peFile))
                .SetExports(GetExports(peFile))
                .SetSha256(sha256Hash)
                .Build();
            // Check if outputDir is null, if so, output to console (stdout)
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            if (outputDir == null)
            {
                string json = JsonSerializer.Serialize(peInfo, jsonOptions);
                Console.WriteLine(json); // Output to console
            }
            else
            {
                string json = JsonSerializer.Serialize(peInfo, jsonOptions);
                string uniqueFileName = $"{Path.GetFileName(filePath)}__{Guid.NewGuid()}.json";
                string outputFilePath = Path.Combine(outputDir, uniqueFileName);
                await File.WriteAllTextAsync(outputFilePath, json);
            }
            progressBar.UpdateProgress(true);  // Successful processing
        }
        catch (Exception ex)
        {
            progressBar.UpdateProgress(false, "invalid_pe");
            // Handle exceptions if necessary
        }
    }

    private List<string> GetImportedLibraries(PeFile peFile)
    {
        var importedLibs = new List<string>();

        if (peFile.ImportedFunctions != null)
        {
            foreach (var import in peFile.ImportedFunctions)
            {
                if (!importedLibs.Contains(import.DLL))
                {
                    importedLibs.Add(import.DLL);
                }
            }
        }
        importedLibs.Sort();
        return importedLibs;
    }

    private List<object> GetImports(PeFile peFile)
    {
        var imports = new Dictionary<string, List<string>>();
        var result = new List<object>();
        if (peFile.ImportedFunctions == null)
        {
            return result;
        }
        foreach (var import in peFile.ImportedFunctions)
        {
            if (!imports.ContainsKey(import.DLL))
            {
                imports[import.DLL] = new List<string>();
            }
            if (import.Name != null)
            {
                imports[import.DLL].Add(import.Name);
            }
        }

        foreach (var lib in imports)
        {
            lib.Value.Sort();
            result.Add(new
            {
                lib = lib.Key,
                count = lib.Value.Count,
                functions = lib.Value
            });
        }
        return result;
    }

    private List<object> GetExports(PeFile peFile)
    {
        if (peFile.ExportedFunctions == null || peFile.ExportedFunctions.Length == 0)
        {
            return new List<object>(); // Return an empty list if no exports
        }

        var exports = new List<object>();

        foreach (var export in peFile.ExportedFunctions)
        {
            exports.Add(export.Name ?? $"ORDINAL {export.Ordinal}"); // Handle missing names
        }
        exports.Sort();
        return exports;
    }

    private async Task<(bool isValid, string sha256Hash, long fileSize, string reason)> PerformFileChecksAndHashAsync(string filePath)
    {
        try
        {
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true))
            {
                long fileSize = stream.Length;
                if (fileSize < 1024)  // File too small to be a valid PE
                {
                    return (false, null, fileSize, "invalid_size");
                }

                Memory<byte> buffer = new byte[fileSize];
                await stream.ReadAsync(buffer);

                string sha256Hash = ComputeSha256Hash(buffer.Span);
                return (true, sha256Hash, fileSize, null);
            }
        }
        catch (UnauthorizedAccessException)
        {
            return (false, null, 0, "access_denied");
        }
        catch (Exception ex)
        {
            return (false, null, 0, "error");
        }
    }

    private string ComputeSha256Hash(ReadOnlySpan<byte> fileData)
    {
        Span<byte> hashBytes = stackalloc byte[32]; // Allocate on stack for performance
        using (var sha256 = SHA256.Create())
        {
            sha256.TryComputeHash(fileData, hashBytes, out _);
            return Convert.ToHexString(hashBytes);
        }
    }
}