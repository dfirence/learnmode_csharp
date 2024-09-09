using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text.Json;
using System.Diagnostics;
using PeNet;

namespace Safiro.Modules.FileCollectors.PeFiles
{
    public class ProgressBar
    {
        private readonly int _total;
        private const int BarLength = 50;
        private readonly int _updateThreshold;
        private int _processed;
        private int _accessDeniedCount;
        private int _invalidSizeCount;
        private int _invalidPeCount;
        private int _skippedCount;
        private int _lastUpdateCount;

        public ProgressBar(int total, int updateThreshold = 100)
        {
            _total = total;
            _updateThreshold = updateThreshold;
        }

        public void UpdateProgress(bool isProcessed, string? reason = null, bool isSkipped = false)
        {
            if (isSkipped) _skippedCount++;
            else if (isProcessed) _processed++;
            else if (reason == "access_denied") _accessDeniedCount++;
            else if (reason == "invalid_size") _invalidSizeCount++;
            else if (reason == "invalid_pe") _invalidPeCount++;

            if (_processed - _lastUpdateCount >= _updateThreshold)
            {
                _lastUpdateCount = _processed;
                DisplayProgressBar();
            }
        }

        public void Complete()
        {
            _processed = _total;
            DisplayProgressBar();
            Console.WriteLine();
        }

        private void DisplayProgressBar()
        {
            int filledLength = (int)((double)_processed / _total * BarLength);
            string progressBar = new string('#', filledLength) + new string('-', BarLength - filledLength);
            Console.Error.Write($"\r[{progressBar}] {_processed}/{_total} processed");
        }
    }

    public class PeFileCollector
    {
        private readonly string[] _peExtensions = { ".dll", ".exe", ".sys", ".ocx", ".scr" };
        private readonly ConcurrentDictionary<string, string> _processedHashes = new();
        private const int BatchSize = 100;

        public async Task CollectFilesFromMultipleAreasAsync(string outputDir)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            string[] directories = { @"C:\Windows" };

            await Parallel.ForEachAsync(directories, async (dir, _) =>
            {
                await CollectPeFilesAsync(dir, outputDir);
            });

            stopwatch.Stop();
            Console.Error.WriteLine($"PE file processing completed in {stopwatch.Elapsed.TotalSeconds:F2} seconds.");
        }

        public async Task CollectSingleFileAsync(string filePath)
        {
            var progressBar = new ProgressBar(1);
            await ProcessPeFileAsync(filePath, null, progressBar);
            progressBar.Complete();
        }

        private async Task CollectPeFilesAsync(string rootPath, string outputDir)
        {
            try
            {
                var peFiles = EnumeratePeFiles(rootPath).ToList();
                var progressBar = new ProgressBar(peFiles.Count);

                await Parallel.ForEachAsync(peFiles, async (filePath, _) =>
                {
                    await ProcessPeFileAsync(filePath, outputDir, progressBar);
                });

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

                if (new DirectoryInfo(currentDir).Attributes.HasFlag(FileAttributes.ReparsePoint))
                {
                    continue;
                }

                IEnumerable<string> files = Enumerable.Empty<string>();
                try
                {
                    files = Directory.EnumerateFiles(currentDir, "*.*", SearchOption.TopDirectoryOnly)
                                     .Where(file => _peExtensions.Contains(Path.GetExtension(file).ToLower()));
                }
                catch (UnauthorizedAccessException) { }
                catch (Exception) { }

                foreach (var file in files) yield return file;

                try
                {
                    foreach (var subDir in Directory.EnumerateDirectories(currentDir))
                    {
                        directories.Push(subDir);
                    }
                }
                catch (UnauthorizedAccessException) { }
                catch (Exception) { }
            }
        }

        private async Task ProcessPeFileAsync(string filePath, string? outputDir, ProgressBar progressBar)
        {
            try
            {
                var (isValid, sha256Hash, fileSize, reason) = await PerformFileChecksAndHashAsync(filePath);
                if (!isValid)
                {
                    progressBar.UpdateProgress(false, reason);
                    return;
                }

                if (!_processedHashes.TryAdd(sha256Hash, filePath))
                {
                    progressBar.UpdateProgress(false, isSkipped: true);
                    return;
                }

                var peFile = new PeFile(filePath);
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
                    .SetHasImports(peFile.ImportedFunctions?.Length > 0)
                    .SetHasExports(peFile.ExportedFunctions?.Length > 0)
                    .SetSubsystem((ushort)peFile.ImageNtHeaders.OptionalHeader.Subsystem)
                    .SetSubsystemCaption(peFile.ImageNtHeaders.OptionalHeader.SubsystemResolved)
                    .SetLibs(GetImportedLibraries(peFile))
                    .SetImports(GetImports(peFile))
                    .SetExports(GetExports(peFile))
                    .SetSha256(sha256Hash);

                var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(peInfo.Build(), jsonOptions);

                if (outputDir == null) Console.WriteLine(json);
                else
                {
                    string outputFilePath = Path.Combine(outputDir, $"{Path.GetFileName(filePath)}__{Guid.NewGuid()}.json");
                    await File.WriteAllTextAsync(outputFilePath, json);
                }

                progressBar.UpdateProgress(true);
            }
            catch (Exception)
            {
                progressBar.UpdateProgress(false, "invalid_pe");
            }
        }

        private List<string> GetImportedLibraries(PeFile peFile) =>
            peFile.ImportedFunctions?.Select(import => import.DLL).Distinct().OrderBy(dll => dll).ToList() ?? new List<string>();

        private List<object> GetImports(PeFile peFile) =>
            peFile.ImportedFunctions?
                .GroupBy(import => import.DLL)
                .Select(group => new
                {
                    lib = group.Key,
                    count = group.Count(),
                    functions = group.Select(import => import.Name).Where(name => name != null).OrderBy(name => name).ToList()
                }).Cast<object>().ToList() ?? new List<object>();

        private List<object> GetExports(PeFile peFile) =>
            peFile.ExportedFunctions?.Select(export => export.Name ?? $"ORDINAL {export.Ordinal}")
                 .OrderBy(name => name).Cast<object>().ToList() ?? new List<object>();

        private async ValueTask<(bool isValid, string sha256Hash, long fileSize, string reason)> PerformFileChecksAndHashAsync(string filePath)
        {
            try
            {
                using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
                long fileSize = stream.Length;

                if (fileSize < 1024) return (false, string.Empty, fileSize, "invalid_size");

                Memory<byte> buffer = new byte[fileSize];
                await stream.ReadAsync(buffer);

                string sha256Hash = ComputeSha256Hash(buffer.Span);
                return (true, sha256Hash, fileSize, string.Empty);
            }
            catch (UnauthorizedAccessException) { return (false, string.Empty, 0, "access_denied"); }
            catch (Exception) { return (false, string.Empty, 0, "error"); }
        }

        private string ComputeSha256Hash(ReadOnlySpan<byte> fileData)
        {
            Span<byte> hashBytes = stackalloc byte[32];
            using var sha256 = SHA256.Create();
            sha256.TryComputeHash(fileData, hashBytes, out _);
            return Convert.ToHexString(hashBytes);
        }
    }
}