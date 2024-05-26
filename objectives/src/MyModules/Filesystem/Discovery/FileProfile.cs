using static System.IO.Path;
using Common;

namespace MyModules.Filesystem.Discovery;

/// <summary>
/// Class <c>FileProfile</c> is used for basic
/// learning of functionality on the filesystem
/// </summary>
public class FileProfile : MyAbstractClass
{
    private readonly string testFolder
        = $@"{Path.Join(Environment.CurrentDirectory, "testMyData")}";
    private readonly string testFile
        = $@"{Path.Join(Environment.CurrentDirectory, "testMyData", "testFile42.txt")}";

    public override void Run()
    {
        if (!DirectoryExists())
        {
            if (!CreateDirectory(testFolder))
            {
                return;
            }
        }
        if (!File.Exists(testFile))
        {
            if (!CreateFile(testFile))
            {
                return;
            }
        }
        Display($"{testFile}");
        ShowFileAttributes(testFile);
    }
    /// <summary>
    /// Creates a directory with basic error handling.
    /// When successful true, false when failure.
    /// </summary>
    /// <param name="full_path"></param>
    /// <returns>boolean</returns>
    public bool CreateDirectory(string full_path)
    {
        bool result = false;
        try
        {
            Directory.CreateDirectory(full_path);
            result = true;
        }
        catch (Exception e)
        {
            Display($"Error: Directory {e.Message}");
        }
        return result;
    }
    /// <summary>
    /// Creates a file with basic error handling.
    /// When successful true, false when failure.
    /// </summary>
    /// <param name="full_path"></param>
    /// <returns>boolean</returns>
    public bool CreateFile(string full_path)
    {
        bool result = false;
        try
        {
            File.Create(full_path);
            result = true;
        }
        catch (Exception e)
        {
            Display($"Error: File {e.Message}"); ;
        }
        return result;
    }
    /// <summary>
    /// Deletes the test file with basic error handling.
    /// When successful true, false when failure.
    /// </summary>
    /// <returns>boolean</returns>
    public bool DeleteFile()
    {
        if (!DirectoryExists() || !File.Exists(testFile))
        {
            Display($"Target File To Delete Does Not Exist: {testFile}");
            return false;
        }
        try
        {
            File.Delete(testFile);
            Display($"Success: Deleted File {testFile}");
            return true;
        }
        catch (Exception e)
        {
            Display($"Error: File {e.Message}");
            return false;
        }
    }
    /// <summary>
    /// Deletes the test folder with basic error handling.
    /// When successful true, false when failure.
    /// </summary>
    /// <returns>boolean</returns>
    public bool DeleteFolder()
    {
        bool doesExist = DirectoryExists();
        if (!doesExist)
        {
            return doesExist;
        }
        int count = Directory.EnumerateFileSystemEntries(testFolder).ToArray<string>().Length;
        bool isEmpty = count > 0 ? false : true;
        bool wantsForced = false;
        if (!isEmpty)
        {
            Console.Write("\n\nDirectory Is Not Empty - Wanna Delete Anyway? (Yes|No) >> ");

            string? answer = Console.ReadLine();

            if (string.IsNullOrEmpty(answer) ||
                answer.Trim().ToLower() != "y" &&
                answer.Trim().ToLower() != "yes")
            {
                return false;
            }
            wantsForced = true;
        }
        try
        {
            Directory.Delete(testFolder, wantsForced);
            if (Directory.Exists(testFolder))
            {
                Console.WriteLine("Very Weird, it should have deleted it...");
                return false;
            }
            else
            {
                Console.WriteLine($"Successfully Deleted: {testFolder}");
            }
            return true;
        }
        catch (Exception e)
        {
            Display($"Error: {e.Message}");
            return false;
        }
    }
    /// <summary>
    /// Enumerates the test folder and its entries.
    /// </summary>
    public void EnumerateFolder()
    {
        if (!DirectoryExists())
        {
            return;
        }

        string target = Environment.OSVersion.ToString().Contains("Windows")
                            ? @"C:\Windows\System32"
                            : testFolder;

        string[] entries = Directory.EnumerateFileSystemEntries(target).ToArray();
        if (entries.Length == 0)
        {
            Display($"Target Folder Is Empty: {testFolder}");
            return;
        }

        string result = $"\n\t{entries.Length} Files In Directory";
        foreach (var entry in entries)
        {
            result += $"\t\t{entry}\n";
        }
        Display(
            $@"
            Non-Recursive Enumeration
            ==========================
            {result}
            "
        );
    }
    /// <summary>
    /// Convenience helper method to validate folder existence.
    /// If exists then true, else false.
    /// </summary>
    /// <returns>boolean</returns>
    public bool DirectoryExists()
    {
        bool doesExist = Directory.Exists(testFolder);
        if (!doesExist)
        {
            Display($"Target Folder To Delete Does Not Exist: {testFolder}");
        }
        return doesExist;
    }
    /// <summary>
    /// Pretty string showing default file attributes available
    /// with standard lib.
    /// </summary>
    /// <param name="full_path"></param>
    public void ShowFileAttributes(string full_path)
    {
        string dashes = new string('-', 64);
        FileInfo fi = new FileInfo(full_path);
        Console.WriteLine(
            $@"
            UserMode Timestamps
                Local Time Accessed     : {fi.LastAccessTime}
                Local Time Created      : {fi.CreationTime}
                Local Time Written      : {fi.LastWriteTime}

                Utc Time Accessed       : {fi.LastAccessTimeUtc}
                Utc Time Created        : {fi.CreationTimeUtc}            
                Utc Time Written        : {fi.LastWriteTimeUtc}

            OnDisk Profile
                Directory               : {fi.Directory}
                File Name               : {fi.Name}
                File Size (Bytes)       : {fi.Length}
                File Extension          : {fi.Extension}
                File IsReadOnly         : {fi.IsReadOnly}
            {dashes}
            Attributes
                {fi.Attributes}
            "
        );
    }
}