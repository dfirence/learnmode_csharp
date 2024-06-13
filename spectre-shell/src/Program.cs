global using Spectre.Console;

using System.Threading;


public static class Program
{
    private static string Author { get; } = "carlos_diaz|@dfirence";
    private static string Dashes { get; } = new('-', 64);

    static void Main(string[] args)
    {
        if (args.Length == 0)
            Banner();
        switch (args[0].Trim().ToLower())
        {
            case "--interactive":
                SimpleTui.Render("vertical_layout");
                break;
            default:
                Banner();
                break;
        }
    }
    static void Banner()
    {
        Console.WriteLine($"""

        By : {Author} - A Spectre Console Sketchpad
        {Dashes}

        """);
        // Exit Process
        Environment.Exit(0);
    }
}

//---------------------------------------------------
//
//---------------------------------------------------
public static class SimpleTui
{
    public static void Render(string layout_name)
    {
        switch (layout_name.Trim().ToLower())
        {
            case "vertical_layout":
                VerticalLayout(10);
                break;
            default:
                Environment.Exit(0);
                break;
        }
    }
    public static void VerticalLayout(int width)
    {
        Console.Clear();

        // Create the layout
        var layout = new Layout("Root");
        //---------------------------------------------
        // Left Panel Component
        //---------------------------------------------
        var leftSide = new Layout("Left");
        leftSide.Name = "EventStream";
        //---------------------------------------------
        // Right Panel Component
        //---------------------------------------------
        var rightSide = new Layout("Right");
        rightSide.Name = "Metadata View";

        var rightSearch = new Layout("Search") { Size = 5 };
        rightSearch.Name = "Search";

        var rightTactics = new Layout("Tactics");
        rightTactics.Name = "Tactics";

        var rightTechniques = new Layout("Techniques");
        rightTechniques.Name = "Techniques";

        rightSide.SplitRows(rightSearch, rightTactics, rightTechniques);

        // MainLayout
        layout.SplitColumns(leftSide, rightSide);
        //layout["EventStream"];
        // Render the layout
        AnsiConsole.Write(layout);
    }

}