using Common;

namespace MyModules.Filesystem.Enumeration;


/// <summary>
/// Class <c>SpecialFolders</c> now uses basic inheritance of
/// the <c>LearnModeObject</c>.
/// </summary>
public class SpecialFolders : LearnModeObject
{
    /// <summary>
    /// Overrides base class `Run()` method.
    /// </summary>
    public override void Run()
    {
        byte count = 0;
        string values = "\n\n";
        var folders = Enum.GetValues(typeof(Environment.SpecialFolder))
                          .Cast<Environment.SpecialFolder>()
                          .OrderBy(x => x.ToString());

        foreach(var folder in folders)
        {
            count  += 1;
            values += $"\t\t{folder, -32} => \"{Environment.GetFolderPath(folder)}\"\n";
        }
        // Leverages through inheritance the `Display` method
        Display($"{count} Special Folders: {values}");
    }
}
