using Watcher.Modules.Schema;
using Watcher.Common;


namespace Watcher;


/// <summary>
/// Program EntryPoint
/// </summary>
class Program
{
    private static protected string AUTHOR = "carlos_diaz|@dfirence";
    private static protected string PROGRAM = "experiment etw_watcher";

    public static void Main(string[] args)
    {
        string conf = Path.Join(Environment.CurrentDirectory, "sampleConf.yaml");
        SchemaHandler sh = new(conf);
        //Console.WriteLine($"{sh.ToJson()}");
    }
}
