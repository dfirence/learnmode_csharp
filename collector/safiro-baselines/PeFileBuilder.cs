namespace Safiro.Modules.FileCollectors.PeFiles;

public class PeFileInfoBuilder
{
    private string _name;
    private string _path;
    private long _size;
    private bool _is64Bit;
    private bool _isLib;
    private bool _isDotNet;
    private string _machineResolved;
    private bool _hasImports;
    private bool _hasExports;
    private ushort _subsystem;
    private string _subsystemCaption;
    private List<string> _libs;
    private List<object> _imports;
    private List<object> _exports;
    private string _sha256;
    // private string _ssdeep;

    public PeFileInfoBuilder SetName(string name)
    {
        _name = name;
        return this;
    }

    public PeFileInfoBuilder SetPath(string path)
    {
        _path = path;
        return this;
    }

    public PeFileInfoBuilder SetSize(long size)
    {
        _size = size;
        return this;
    }

    public PeFileInfoBuilder SetIs64Bit(bool is64Bit)
    {
        _is64Bit = is64Bit;
        return this;
    }

    public PeFileInfoBuilder SetIsLib(bool isLib)
    {
        _isLib = isLib;
        return this;
    }

    public PeFileInfoBuilder SetIsDotNet(bool isDotNet)
    {
        _isDotNet = isDotNet;
        return this;
    }

    public PeFileInfoBuilder SetMachineResolved(string machineResolved)
    {
        _machineResolved = machineResolved;
        return this;
    }

    public PeFileInfoBuilder SetHasImports(bool hasImports)
    {
        _hasImports = hasImports;
        return this;
    }

    public PeFileInfoBuilder SetHasExports(bool hasExports)
    {
        _hasExports = hasExports;
        return this;
    }

    public PeFileInfoBuilder SetSubsystem(ushort subsystem)
    {
        _subsystem = subsystem;
        return this;
    }

    public PeFileInfoBuilder SetSubsystemCaption(string subsystemCaption)
    {
        _subsystemCaption = subsystemCaption;
        return this;
    }

    public PeFileInfoBuilder SetImports(List<object> imports)
    {
        _imports = imports;
        return this;
    }
    public PeFileInfoBuilder SetExports(List<object> exports)
    {
        _exports = exports;
        return this;
    }

    public PeFileInfoBuilder SetLibs(List<string> libs)
    {
        _libs = libs;
        return this;
    }

    public PeFileInfoBuilder SetSha256(string sha256)
    {
        _sha256 = sha256;
        return this;
    }

    // public PeFileInfoBuilder SetSsdeep(string ssdeep)
    // {
    //     _ssdeep = ssdeep;
    //     return this;
    // }

    // Build the final object
    public object Build()
    {
        return new
        {
            name = _name,
            path = _path,
            size = _size,
            is_64 = _is64Bit,
            is_lib = _isLib,
            is_dotnet = _isDotNet,
            machineResolved = _machineResolved,
            has_imports = _hasImports,
            has_exports = _hasExports,
            subsystem = _subsystem,
            subsystem_caption = _subsystemCaption,
            libs = _libs,
            imports = _imports,
            exports = _exports,
            hashes = new
            {
                sha2 = _sha256,
                // ssdeep = _ssdeep
            }
        };
    }
}

/*
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

        // Build the peInfo object using the PeFileInfoBuilder
        var peInfo = new PeFileInfoBuilder()
            .SetName(Path.GetFileName(filePath))
            .SetPath(Path.GetFullPath(filePath))
            .SetSize(fileSize)
            .SetIs64Bit(peFile.Is64Bit)
            .SetIsLib(peFile.ImageNtHeaders.FileHeader.Characteristics.HasFlag(PeNet.Header.Pe.FileCharacteristicsType.Dll))
            .SetIsDotNet(false) // Assuming this is set to false
            .SetMachineResolved(peFile.ImageNtHeaders.FileHeader.MachineResolved)
            .SetHasImports(peFile.ImportedFunctions != null && peFile.ImportedFunctions.Length > 0)
            .SetHasExports(peFile.ExportedFunctions != null && peFile.ExportedFunctions.Length > 0)
            .SetSubsystem(peFile.ImageNtHeaders.OptionalHeader.Subsystem)
            .SetSubsystemCaption(peFile.ImageNtHeaders.OptionalHeader.SubsystemResolved)
            .SetLibs(GetImportedLibraries(peFile))
            .SetImports(GetImports(peFile))
            .SetSha256(sha256Hash)
            .SetSsdeep("SSDEEP Not Implemented in PeNet")
            .Build(); // Build the object

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
*/