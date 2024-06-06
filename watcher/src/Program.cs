using Watcher.Modules.Windows;

namespace Watcher;


/// <summary>
/// Program EntryPoint
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        var subscriber = new MySubscriberTest();
        subscriber.StartSession();
        subscriber.StopSession();
    }
}