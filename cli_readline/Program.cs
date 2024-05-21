using static System.Console;


class Program
{
    public static void Main()
    {
        string banner = "C# LearnMode ReadLine\n\n";
        string[] months = ["December", "January", "February", "March",
                            "April", "May", "June", "July", "August",
                            "September", "October", "November"];

        Write($"\n{banner}\n|> Name a Month In English: ");

        // Note: `!` is shorthand to initialize to Null
        string input = ReadLine()!;
        input = input.Trim().ToLower();
        string search = Array.Find(months, e => e.ToLower() == input)!;
        if (search == null)
        {
            WriteLine($"|\n|\n|----|> '{input}' is not a valid month\n\n");
            return;
        }
        WriteLine($"|\n|\n|-----|> {search} Matched\n\n");
    }
}