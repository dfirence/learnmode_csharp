#if LINUX || MACOS
namespace Watcher.Modules.Linux;

public static class Greet
{
    public static void HelloWorld()
    {
        Console.WriteLine($"Hello From Nix*");
    }
}
#endif