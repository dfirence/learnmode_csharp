using System;
using System.Runtime.InteropServices;
using static System.Console;

namespace MyModules.SysInfo.DeviceInfo;

/// <summary>
/// DeviceRuntime Class represents details
/// about the current program and its hosted
/// environment - i.e., device.
/// </summary>
public class DeviceRuntime
{
    /// <summary>
    /// Current Process Bits: 32 or 64 bits as boolean value
    /// </summary>
    public static readonly bool Is64BitProcess
        = Environment.Is64BitProcess;

    /// <summary>
    /// Current Operating System Bits: 32 or 64 bits as boolean
    /// </summary>
    public static readonly bool Is64BitOperatingSystem
        = Environment.Is64BitOperatingSystem;

    /// <summary>
    /// Current Process Privileged Profileas boolean
    /// </summary>
    public static readonly bool IsPrivilegedProcess
        = Environment.IsPrivilegedProcess;

    /// <summary>
    /// Operating System Memory Page Size in bytes as int
    /// </summary>
    public static readonly int SystemPageSize
        = Environment.SystemPageSize;

    /// <summary>
    /// Current Process PID, strange C# gives it as int, instead of UInt32
    /// </summary>
    public static readonly int ProcessId
        = Environment.ProcessId;

    /// <summary>
    /// Current .NET Runtime Framework Description as string
    /// </summary>
    public static readonly string Runtime
        = RuntimeInformation.FrameworkDescription.ToString();

    /// <summary>
    /// Current Local Domain Name of the Current Process User
    /// </summary>
    public static readonly string UserDomainName
        = Environment.UserDomainName;

    /// <summary>
    /// Current UserName Identity running the Current Process
    /// </summary>
    public static readonly string UserName
        = Environment.UserName;

    /// <summary>
    /// Current Device Name
    /// </summary>
    public static readonly string HostName
        = Environment.MachineName;

    /// <summary>
    /// Current Verion of .NET, should Match the Runtime string
    /// </summary>
    public static readonly string Version
        = Environment.Version.ToString();

    /// <summary>
    /// Current Operating System Version
    /// </summary>
    public static readonly string OSVersion
        = Environment.OSVersion.ToString();
    public static readonly string CommandLine
        = Environment.CommandLine;
    public static readonly string? ProcessPath
        = Environment.ProcessPath;
    public static readonly string CurrentDirectory
        = Environment.CurrentDirectory;
    public static readonly string SystemDirectory
        = Environment.SystemDirectory;

    /// <summary>
    /// Run Method, when this class is invoked then this method
    /// is used to execute this module.
    /// </summary>
    public static void Run()
    {
        WriteLine(
            $@"
            ------------------------------------------------------------------------
                                    Device Runtime Profile
            ------------------------------------------------------------------------
            Hostname                : {HostName}
            User Domain Name        : {UserDomainName}
            Username                : {UserName}

            OS Is64Bit              : {Is64BitOperatingSystem}
            OS Memory Page Size     : {SystemPageSize} Bytes
            OS Platform Type        : {OSVersion}
            
            .NET CLR Version        : {Version}
            .NET Runtime            : {Runtime}

            Process Is64Bit         : {Is64BitProcess}
            Process IsPrivileged    : {IsPrivilegedProcess}
            Process Id (PID)        : {ProcessId}
            Process Path            : {ProcessPath}
            Process CommandLine     : {CommandLine}

            Directory (Current)     : {CurrentDirectory}
            Directory (System)      : {SystemDirectory}
            ------------------------------------------------------------------------
            "
        );
    }
}