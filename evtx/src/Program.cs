global using System;
global using Common;


class Program
{
    public static async Task Main(string[] args)
    {
        if (args.Length == 0)
            goto showHelp;

        switch (args[0].Trim())
        {
            case "-p":
            case "--provider-name":
                break;
            case "-h":
            case "--help":
                goto showHelp;
            default:
                goto showHelp;
        }
    showHelp:
        Banner();
        var rh = new RuntimeHost();
        Console.WriteLine(
            "{0}\n{1}\n{2}\n{3}\n{4}",
            DateTime.UtcNow.ToString("o"),
            rh.GetPathProgramDirectory(),
            rh.GetPathProgramDatabase(),
            rh.GetPathProgramLogs(),
            rh.GetPathProgramRuntimeLog());
    }
    public static void Banner()
    {
        string dashes = new string('-', 128);
        Console.WriteLine(
            $@"
            {dashes}
            Version : 0.1
            Program : Windows EventLog Experiment
            By      : carlos_diaz | @dfirence
            {dashes}

            Usage   : program.exe [ switches ]

            Switches
                -p,  --provider-name        Name of Windows EventLog Provider
                -l,  --provider-list        Enumerates Providers
            "
        );
    }
}