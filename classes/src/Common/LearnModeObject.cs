namespace Common;

public abstract class LearnModeObject {
    public abstract void Run();
    public void Display(string s)
    {
        string dashes = new('-', 64);
        string sourceName = new($"LearnMode Class: {GetType().Name}");
        Console.WriteLine(
            $@"
            {sourceName}
            {dashes}
            {s}
            {dashes}
            "
        );
    }
}