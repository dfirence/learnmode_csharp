using System.Text.Json;

namespace Watcher.Common;


/// <summary>
/// Record representing the device runtime host characteristics.
/// </summary>
public record DeviceRuntime
{
    public string TimeUtcStartup
    { get; } = DateTime.UtcNow.ToString("o");
    /// <summary>
    /// Description of Operating System
    /// </summary>
    public string OperatingSystemDescription
    { get; } = Environment.OSVersion.ToString();

    public string PlatformType
    { get; } = OperatingSystem.IsWindows() ? "Windows"
                : OperatingSystem.IsLinux() ? "Linux"
                    : OperatingSystem.IsMacOS() ? "MacOS"
                        : "Unknown";
    /// <summary>
    /// UserName related to the active session where
    /// this program is executing.
    /// </summary>
    public string UserName
    { get; } = Environment.UserName;

    /// <summary>
    /// Domain Name related to the active session
    /// where this program is executing
    /// </summary>
    public string UserDomainName
    { get; } = Environment.UserDomainName.ToLower();

    public string MachineName
    { get; } = Environment.MachineName.ToLower();

    public string? ProcessName
    { get; } = Environment.ProcessPath?.Split(Path.DirectorySeparatorChar).Last();

    public string ProcessPath
    { get; } = Environment.ProcessPath ?? "";

    public string SystemDirectory
    { get; } = Environment.SystemDirectory;

    public string UserHomeDirectory
    { get; } = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("AppData\\Roaming", string.Empty);
    public string DotnetVersion
    { get; } = Environment.Version.ToString();

    public bool Is64BitOperatingSystem
    { get; } = Environment.Is64BitOperatingSystem;

    public bool Is64BitProcess
    { get; } = Environment.Is64BitProcess;

    public bool IsPrivilegedProcess
    { get; } = Environment.IsPrivilegedProcess;

    public int ProcessorCount
    { get; } = Environment.ProcessorCount;

    public int ProcessId
    { get; } = Environment.ProcessId;
}