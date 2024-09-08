using System;
using System.Collections.Concurrent;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;

public class PeFileCollector
{
    private readonly string[] _peExtensions = new[] { ".exe", ".dll", ".com", ".sys", ".drv", ".cpl", ".msi" };
    private readonly ConcurrentDictionary<string, string> _processedHashes = new ConcurrentDictionary<string, string>();
    private const int BatchSize = 100; // Define batch size for file processing

    public async Task CollectFilesFromMultipleAreasAsync(string outputDir)
    {
        // Define the directories
        string[] directories = { @"C:\Windows", @"C:\Users", @"C:\Program Files" };

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
        return Directory.EnumerateFiles(rootPath, "*.*", SearchOption.AllDirectories)
            .Where(file => _peExtensions.Contains(Path.GetExtension(file).ToLower()));
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
                is_lib = peFile.ImageNtHeaders.FileHeader.Characteristics.HasFlag(PeNet.Header.Pe.Enums.Characteristics.Dll),
                is_dotnet = peFile.IsManaged,
                has_imports = peFile.ImportedFunctions != null && peFile.ImportedFunctions.Length > 0,
                has_exports = peFile.ExportedFunctions != null && peFile.ExportedFunctions.Length > 0,
                subsystem = peFile.ImageNtHeaders.OptionalHeader.Subsystem,
                subsystem_caption = GetSubsystemCaption(peFile.ImageNtHeaders.OptionalHeader.Subsystem),
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

            string outputFilePath = Path.Combine(outputDir, $"{Path.GetFileNameWithoutExtension(filePath)}.json");
            await File.WriteAllTextAsync(outputFilePath, json);

            _processedHashes.TryAdd(sha256Hash, filePath);
            progressBar.UpdateProgress();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing file {filePath}: {ex.Message}");
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
                    Console.WriteLine($"File too small to be a valid PE: {filePath}");
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
            Console.WriteLine($"Access denied: {filePath}");
            return (false, null, 0);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error accessing or processing file {filePath}: {ex.Message}");
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

    private string GetSubsystemCaption(PeNet.Header.Pe.Enums.SubSystem subsystem)
    {
        return subsystem switch
        {
            PeNet.Header.Pe.Enums.SubSystem.WindowsGui => "The Windows Graphical User Interface (GUI) Subsystem",
            PeNet.Header.Pe.Enums.SubSystem.WindowsCui => "The Windows Character Subsystem",
            _ => "Unknown Subsystem"
        };
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

        imports[import.DLL].Add(import.Name ?? $"ORDINAL {import.Ordinal}");
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