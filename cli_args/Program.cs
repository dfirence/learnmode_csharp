using System.Text;
using static System.Console;

class Program
{
    public static void Main(string[] args)
    {
        string dashes = new string('-', 64);
        string title = "C# LearnMode - Native Args";
        string banner = $"\n{dashes}\n{title}\n{dashes}\n";

        string result = $"\n|{"Argument",-30}|{"Arg Value",30}|\n";

        StringBuilder sb = new StringBuilder(result);
        sb.AppendFormat($"{dashes}\n");
        // Output in Table Format
        for (var i = 0; i < args.Length; i++)
        {
            // result += $"{i,-31}|{args[i],30}\n";
            sb.AppendFormat("{0, -31}|{1, 30}\n", i, args[i]);
        }
        WriteLine("{0}", sb.ToString());
    }
}