using Watcher.Modules.Windows;

namespace Watcher;


/// <summary>
/// Program EntryPoint
/// </summary>
class Program
{
    /// <summary>
    /// Main EntrPoint
    /// </summary>
    /// <param name="args">stdin arguments from command line.</param>
    static void Main(string[] args)
    {
        var _ = new MySubscriberTest();
    }
}
