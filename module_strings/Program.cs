using MyProgram;
class Program
{
    public static void Main(string[] args)
    {
        MyString m;
        if (args.Length < 1 || args.Length > 1)
        {
            m = new MyString("AaBbCcDd");
            m.Display();
            return;
        }
        if (args[0] == "-i" || args[0] == "--interactive")
        {
            System.Console.Write("\n\n[interactive] Enter String >>> ");
            m = new MyString(System.Console.ReadLine()!);
            m.Display();
        }
    }
}