using System;
using System.Collections.Concurrent;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using PeNet;

namespace Safiro.Modules
{
    public class ProgressBar
    {
        private readonly int _total;
        private const int BarLength = 50;
        private int _processed;

        public ProgressBar(int total)
        {
            _total = total;
            _processed = 0;
        }

        public void UpdateProgress()
        {
            _processed++;
            int filledLength = (int)((double)_processed / _total * BarLength);
            int emptyLength = BarLength - filledLength;

            string progressBar = $"[{new string('=', filledLength)}>{new string('.', emptyLength)}]";
            Console.Write($"\rProcessing PE Files: {progressBar} {_processed}/{_total}");
        }
    }

    public class PeFileCollector
    {
        private readonly string[] _peExtensions = new[] { ".exe", ".com", ".sys" };//, ".dll", ".com", ".sys", ".drv", ".cpl", ".msi" };
        private readonly ConcurrentDictionary<string, string> _processedHashes = new ConcurrentDictionary<string, string>();
        private const int BatchSize = 100; // Define batch size for file processing

        public async Task CollectFilesFromMultipleAreasAsync(string outputDir)
        {
            // Define the directories
            string[] directories = { @"C:\Windows", @"C:\Users\Archir" };

            // Run each directory processing task in parallel
            var tasks = directories.Select(dir => Task.Run(() => CollectPeFilesAsync(dir, outputDir)));
            await Task.WhenAll(tasks);
        }

        private async Task CollectPeFilesAsync(string rootPath, string outputDir)
        {
            try
            {
                var peFiles = EnumeratePeFiles(rootPath).ToList();
                int totalFiles = peFiles.Count;
                var progressBar = new ProgressBar(totalFiles);

                for (int i = 0; i < totalFiles; i += BatchSize)
                {
                    var batch = peFiles.Skip(i).Take(BatchSize);
                    var tasks = batch.Select(filePath => Task.Run(() => ProcessPeFileAsync(filePath, outputDir, progressBar)));
                    await Task.WhenAll(tasks);
                }
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
                    Console.WriteLine($"Skipping reparse point: {currentDir}");
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
                    //Console.WriteLine($"Access denied: {currentDir}");
                    continue;
                }
                catch (Exception ex)
                {
                    //Console.WriteLine($"Error enumerating files in {currentDir}: {ex.Message}");
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
                    Console.WriteLine($"Access denied: {currentDir}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error enumerating directories in {currentDir}: {ex.Message}");
                }
            }
        }

        private async Task ProcessPeFileAsync(string filePath, string outputDir, ProgressBar progressBar)
        {
            try
            {
                // Perform all pre-checks and hash calculation in one file open operation
                var (isValid, sha256Hash, fileSize) = await PerformFileChecksAndHashAsync(filePath);
                if (!isValid || _processedHashes.ContainsKey(sha256Hash))
                {
                    return;
                }

                // Parse the PE file (using PeNet or your preferred library)
                var peFile = new PeFile(filePath);

                var peInfo = new
                {
                    name = Path.GetFileName(filePath),
                    path = Path.GetFullPath(filePath),
                    size = fileSize,
                    is_64 = peFile.Is64Bit,
                    is_lib = peFile.ImageNtHeaders.FileHeader.Characteristics.HasFlag(PeNet.Header.Pe.FileCharacteristicsType.Dll),
                    is_dotnet = false,
                    peFile.ImageNtHeaders.FileHeader.MachineResolved,
                    has_imports = peFile.ImportedFunctions != null && peFile.ImportedFunctions.Length > 0,
                    has_exports = peFile.ExportedFunctions != null && peFile.ExportedFunctions.Length > 0,
                    subsystem = peFile.ImageNtHeaders.OptionalHeader.Subsystem,
                    subsystem_caption = peFile.ImageNtHeaders.OptionalHeader.SubsystemResolved,
                    imports = GetImports(peFile),
                    hashes = new
                    {
                        sha2 = sha256Hash,
                        ssdeep = "SSDEEP Not Implemented in PeNet"
                    }
                };

                var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(peInfo, jsonOptions);

                // Generate a unique filename using a random integer
                string randomInteger = new Random().Next(100000, 999999).ToString();
                string outputFilePath = Path.Combine(outputDir, $"{Path.GetFileNameWithoutExtension(filePath)}__{randomInteger}.json");

                await File.WriteAllTextAsync(outputFilePath, json);

                _processedHashes.TryAdd(sha256Hash, filePath);
                progressBar.UpdateProgress();
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"Error processing file {filePath}: {ex.Message}");
            }
        }

        private async Task<(bool isValid, string sha256Hash, long fileSize)> PerformFileChecksAndHashAsync(string filePath)
        {
            try
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true))
                {
                    long fileSize = stream.Length;
                    if (fileSize < 1024)
                    {
                        //Console.WriteLine($"File too small to be a valid PE: {filePath}");
                        return (false, null, fileSize);
                    }

                    Memory<byte> buffer = new byte[fileSize];
                    await stream.ReadAsync(buffer);

                    string sha256Hash = ComputeSha256Hash(buffer.Span);
                    return (true, sha256Hash, fileSize);
                }
            }
            catch (UnauthorizedAccessException)
            {
                //Console.WriteLine($"Access denied: {filePath}");
                return (false, null, 0);
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"Error accessing or processing file {filePath}: {ex.Message}");
                return (false, null, 0);
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

        // Helper functions for extracting imports and libraries from PeFile object...
        private List<object> GetImports(PeFile peFile)
        {
            var imports = new Dictionary<string, List<string>>();

            foreach (var import in peFile.ImportedFunctions)
            {
                if (!imports.ContainsKey(import.DLL))
                {
                    imports[import.DLL] = new List<string>();
                }

                imports[import.DLL].Add(import.Name ?? $"ORDINAL {import.Name}");
            }

            var result = new List<object>();
            foreach (var lib in imports)
            {
                result.Add(new
                {
                    lib = lib.Key,
                    count = lib.Value.Count,
                    functions = lib.Value
                });
            }

            return result;
        }

        private List<string> GetExports(PeFile peFile)
        {
            if (peFile.ExportedFunctions == null || peFile.ExportedFunctions.Length == 0)
            {
                return new List<string>(); // Return an empty list if no exports
            }

            var exports = new List<string>();

            foreach (var export in peFile.ExportedFunctions)
            {
                exports.Add(export.Name ?? $"ORDINAL {export.Ordinal}");
            }

            return exports;
        }
    }
}

/*

public class ProgressBar
{
    private readonly int _total;
    private const int BarLength = 50;
    private int _processed;
    private int _accessDeniedCount;
    private int _invalidSizeCount;
    private int _invalidPeCount;

    public ProgressBar(int total)
    {
        _total = total;
        _processed = 0;
        _accessDeniedCount = 0;
        _invalidSizeCount = 0;
        _invalidPeCount = 0;
    }

    public void UpdateProgress(bool isProcessed, string reason = null)
    {
        if (isProcessed)
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

        int filledLength = (int)((double)_processed / _total * BarLength);
        int emptyLength = BarLength - filledLength;

        string progressBar = $"[{new string('=', filledLength)}>{new string('.', emptyLength)}]";
        Console.Write($"\rProcessing PE Files: {progressBar} {_processed}/{_total} " +
                      $"| Access Denied: {_accessDeniedCount}, Invalid Size: {_invalidSizeCount}, Invalid PE: {_invalidPeCount}");
    }
}


private async Task ProcessPeFileAsync(string filePath, string outputDir, ProgressBar progressBar)
{
    try
    {
        // Perform all pre-checks and hash calculation in one file open operation
        var (isValid, sha256Hash, fileSize, reason) = await PerformFileChecksAndHashAsync(filePath);
        if (!isValid || _processedHashes.ContainsKey(sha256Hash))
        {
            progressBar.UpdateProgress(false, reason);  // Update progress with failure reason
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

        // Successful processing
        var peInfo = new
        {
            name = Path.GetFileName(filePath),
            path = Path.GetFullPath(filePath),
            size = fileSize,
            is_64 = peFile.Is64Bit,
            is_lib = peFile.ImageNtHeaders.FileHeader.Characteristics.HasFlag(PeNet.Header.Pe.FileCharacteristicsType.Dll),
            is_dotnet = false,
            peFile.ImageNtHeaders.FileHeader.MachineResolved,
            has_imports = peFile.ImportedFunctions != null && peFile.ImportedFunctions.Length > 0,
            has_exports = peFile.ExportedFunctions != null && peFile.ExportedFunctions.Length > 0,
            subsystem = peFile.ImageNtHeaders.OptionalHeader.Subsystem,
            subsystem_caption = peFile.ImageNtHeaders.OptionalHeader.SubsystemResolved,
            libs = GetImportedLibraries(peFile),
            imports = GetImports(peFile),
            hashes = new
            {
                sha2 = sha256Hash,
                ssdeep = "SSDEEP Not Implemented in PeNet"
            }
        };

        var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(peInfo, jsonOptions);

        string randomInteger = new Random().Next(100000, 999999).ToString();
        string outputFilePath = Path.Combine(outputDir, $"{Path.GetFileNameWithoutExtension(filePath)}__{randomInteger}.json");

        await File.WriteAllTextAsync(outputFilePath, json);

        _processedHashes.TryAdd(sha256Hash, filePath);
        progressBar.UpdateProgress(true);  // Successful processing
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error processing file {filePath}: {ex.Message}");
    }
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
                Console.WriteLine($"File too small to be a valid PE: {filePath}");
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
        Console.WriteLine($"Access denied: {filePath}");
        return (false, null, 0, "access_denied");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error accessing or processing file {filePath}: {ex.Message}");
        return (false, null, 0, "error");
    }
}

*/