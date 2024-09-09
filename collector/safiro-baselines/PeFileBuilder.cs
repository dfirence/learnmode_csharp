namespace Safiro.Modules.FileCollectors.PeFiles;
using System.Text.Json.Serialization;
using System.Text.Json;

public class IgnoreEmptyCollectionsConverter<T> : JsonConverter<T> where T : IEnumerable<object>
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Default deserialization for collections
#pragma warning disable CS8603 // Possible null reference return.
        return JsonSerializer.Deserialize<T>(ref reader, options);
#pragma warning restore CS8603 // Possible null reference return.
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        // If the collection is not empty, serialize it
        if (value != null && value.Any())
        {
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}

public class PeFileInfoBuilder
{
    private string? _name;
    private string? _path;
    private long _size;
    private bool _is64Bit;
    private bool _isLib;
    private bool _isDotNet;
    private string? _machineResolved;
    private bool _hasImports;
    private bool _hasExports;
    private ushort _subsystem;
    private string? _subsystemCaption;

    [JsonConverter(typeof(IgnoreEmptyCollectionsConverter<List<string>>))]
    private List<string>? _libs;

    [JsonConverter(typeof(IgnoreEmptyCollectionsConverter<List<string>>))]
    private List<object>? _imports;

    [JsonConverter(typeof(IgnoreEmptyCollectionsConverter<List<string>>))]
    private List<object>? _exports;

    private string? _sha256;
    private string? _originalFilename;
    private string? _companyName;
    private string? _fileVersion;

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
    public PeFileInfoBuilder SetOriginalFilename(string originalFilename)
    {
        _originalFilename = originalFilename;
        return this;
    }
    public PeFileInfoBuilder SetCompanyName(string companyName)
    {
        _companyName = companyName;
        return this;
    }
    public PeFileInfoBuilder SetFileVersion(string fileVersion)
    {
        _fileVersion = fileVersion;
        return this;
    }
    // public PeFileInfoBuilder SetProductName(string productName)
    // {
    //     _productName = productName;
    //     return this;
    // }
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
            meta = new
            {
                company_name = _companyName,
                file_version = _fileVersion,
                original_filename = _originalFilename
            },
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
