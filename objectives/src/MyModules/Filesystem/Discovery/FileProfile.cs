using Common;

namespace MyModules.Filesystem.Discovery;


public class FileProfile : MyAbstractClass
{
    private readonly string tfd = $@"{Environment.CurrentDirectory}/testMyData";
    private readonly string tff = $@"{Environment.CurrentDirectory}/testMyData/testFile42.txt";
    public override void Run()
    {
        if (!DoesDirectoryExist())          // (1) Check if TestFolder Exists
        {
            if (!CreateDirectory(tfd))      // (2) Create Directory
            {
                return;
            }
        }
        if (!File.Exists(tff))              // (3) Check if TestFile Exists
        {
            if (!CreateFile(tff))
            {
                return;
            }
        }
        Display($"{tff}");
        ShowFileAttributes(tff);            // (4) Get File Metadata - Pre
    }
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
    public bool DeleteFile()
    {
        if (!DoesDirectoryExist() || !File.Exists(tff))
        {
            Display($"Target File To Delete Does Not Exist: {tff}");
            return false;
        }
        try
        {
            File.Delete(tff);
            Display($"Success: Deleted File {tff}");
            return true;
        }
        catch (Exception e)
        {
            Display($"Error: File {e.Message}");
            return false;
        }
    }
    public bool DeleteFolder()
    {
        bool doesExist = DoesDirectoryExist();
        if (!doesExist)
        {
            return doesExist;
        }
        int count = Directory.EnumerateFileSystemEntries(tfd).ToArray<string>().Length;
        bool isEmpty = count > 0 ? false : true;
        bool wantsForced = false;
        if (!isEmpty)
        {
            Console.Write("\n\nDirectory Is Not Empty - Wanna Delete Anyway? (Yes|No) >> ");
            string? answer = Console.ReadLine();
            if (string.IsNullOrEmpty(answer) || answer.Trim().ToLower() != "y" && answer.Trim().ToLower() != "yes")
            {
                return false;
            }
            wantsForced = true;
        }
        try
        {
            Directory.Delete(tfd, wantsForced);
            if (Directory.Exists(tfd))
            {
                Console.WriteLine("Very Weird, it should have deleted it...");
                return false;
            }
            else
            {
                Console.WriteLine($"Successfully Deleted: {tfd}");
            }
            return true;
        }
        catch (Exception e)
        {
            Display($"Error: {e.Message}");
            return false;
        }
    }
    public void EnumerateFolder()
    {
        if (!DoesDirectoryExist())
        {
            return;
        }

        string target = Environment.OSVersion.ToString().Contains("Windows")
                            ? @"C:\Windows\System32"
                            : tfd;

        string[] entries = Directory.EnumerateFileSystemEntries(target).ToArray();
        if (entries.Length == 0)
        {
            Display($"Target Folder Is Empty: {tfd}");
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
    public bool DoesDirectoryExist()
    {
        bool doesExist = Directory.Exists(tfd);
        if (!doesExist)
        {
            Display($"Target Folder To Delete Does Not Exist: {tfd}");
        }
        return doesExist;
    }
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