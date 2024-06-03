namespace Common;


public class RuntimeHost
{
    //-------------------------------------------------------
    // Member Fields
    //-------------------------------------------------------
    private string? _pathProgram;
    private string? _pathProgramConfig;
    private string? _pathProgramLogs;
    private string? _pathProgramRuntimeLog;
    private string? _pathProgramDatabase;
    //-------------------------------------------------------
    // Constructor
    //-------------------------------------------------------
    public RuntimeHost()
    {
        if (!SetUp())
            return;
    }
    //-------------------------------------------------------
    // Methods: Public
    //-------------------------------------------------------
    public string GetPathProgramDirectory()
    {
        return _pathProgram ?? string.Empty;
    }
    public string GetPathProgramLogs()
    {
        return _pathProgramLogs ?? string.Empty;
    }
    public string GetPathProgramRuntimeLog()
    {
        return _pathProgramRuntimeLog ?? string.Empty;
    }
    public string GetPathProgramDatabase()
    {
        return _pathProgramDatabase ?? string.Empty;
    }
    //-------------------------------------------------------
    // Methods: Private
    //-------------------------------------------------------
    private string GetTimestamp()
    {
        return DateTime.UtcNow.ToString("o");
    }
    private bool CreateFile(string path)
    {
        try
        {
            File.Create(path);
            return DoesFileExist(path);
        }
        catch (Exception error)
        {
            Console.WriteLine($"{GetTimestamp()}:[Runtime]:[CreateFile]:{error.Message}");
            return false;
        }
    }
    private bool CreateDirectory(string path)
    {
        try
        {
            Directory.CreateDirectory(path);
            return DoesDirectoryExist(path);
        }
        catch (Exception error)
        {
            Console.WriteLine($"{GetTimestamp()}:[Runtime]:[CreateDirectory]:{error.Message}");
            return false;
        }
    }
    private bool DoesFileExist(string path)
    {
        try
        {
            return File.Exists(path);
        }
        catch (Exception error)
        {
            Console.WriteLine($"{GetTimestamp()}:[Runtime]:[DoesFileExist]:{error.Message}");
            return false;
        }
    }
    private bool DoesDirectoryExist(string path)
    {
        try
        {
            return Directory.Exists(path);
        }
        catch (Exception error)
        {
            Console.WriteLine($"{GetTimestamp()}:[Runtime]:[DoesDirectoryExist]:{error.Message}");
            return false;
        }
    }
    private bool SetUp()
    {
        SetProgramPaths();

        if (!DoesDirectoryExist(_pathProgram))
            if (!CreateDirectory(_pathProgram))
                return false;

        if (!DoesDirectoryExist(_pathProgramLogs))
            if (!CreateDirectory(_pathProgramLogs))
                return false;

        if (!DoesFileExist(_pathProgramConfig))
            if (!CreateFile(_pathProgramConfig))
                return false;

        if (!DoesFileExist(_pathProgramDatabase))
            if (!CreateFile(_pathProgramDatabase))
                return false;

        if (!DoesFileExist(_pathProgramRuntimeLog))
            if (!CreateFile(_pathProgramRuntimeLog))
                return false;

        return true;
    }
    private void SetProgramPaths()
    {
        string AppData = Environment.GetFolderPath(
        Environment.SpecialFolder.ApplicationData
        ).Replace("AppData\\Roaming", "");

        _pathProgram = Path.Join(AppData, ".evtx");
        _pathProgramConfig = Path.Join(_pathProgram, "evtx_config.yaml");
        _pathProgramDatabase = Path.Join(_pathProgram, "evtx_db.sqlite");

        _pathProgramLogs = Path.Join(_pathProgram, "logs");

        _pathProgramRuntimeLog = Path.Join(
            _pathProgramLogs,
            "evtx_runtime.log");
    }
}