#if WINDOWS
using Microsoft.Diagnostics.Tracing.Session;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Watcher.Modules.Windows.ETW;

/// <summary>
/// Partial class for generating JSON serialization context.
/// </summary>
[JsonSerializable(typeof(EtwProviderDatabase))]
[JsonSerializable(typeof(EtwProvider))]
[JsonSerializable(typeof(EtwKeyword))]
public partial class EtwProviderDatabaseJsonContext : JsonSerializerContext
{
}

/// <summary>
/// Static class to manage ETW providers list.
/// </summary>
public static class EtwProvidersList
{
    /// <summary>
    /// Retrieves and processes the list of ETW providers.
    /// </summary>
    public static void GetProviders()
    {
        var providerGuids = TraceEventProviders.GetPublishedProviders().ToList(); // IEnumerable<Guid>
        var sizeGuidList = (ushort)providerGuids.Count;
        var etwDb = new EtwProviderDatabase((sizeGuidList));
        var processedCount = 0;

        // Loop through each provider GUID
        foreach (var guid in providerGuids)
        {
            var providerName = TraceEventProviders.GetProviderName(guid);
            var providerGuid = guid.ToString();

            // Get the keywords for the provider
            List<ProviderDataItem> keyWords = TraceEventProviders.GetProviderKeywords(guid);
            var sizeKeywordList = (ushort)keyWords.Count;
            var provider = new EtwProvider(providerName, providerGuid, sizeKeywordList);

            // Add keywords to the provider
            foreach (var keyWord in keyWords)
            {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                provider.EtwKeyWords.Add(new EtwKeyword { Name = keyWord.Name, Value = keyWord.Value });
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            }

            // Update etwDb and print the progress
            etwDb.EtwProviders.Add(provider);
            processedCount++;
            Console.Write($"\rProcessing >>> {processedCount,7} of {etwDb.EtwProviders.Capacity} ETW Native Providers");
        }

        etwDb.UpdateRecordCount();
        ToJsonFile(etwDb);
    }

    /// <summary>
    /// Converts the ETW provider database to JSON string.
    /// </summary>
    /// <param name="db">The ETW provider database object.</param>
    /// <returns>JSON string representation of the database.</returns>
    private static string? ToJson(EtwProviderDatabase db)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            return JsonSerializer.Serialize(db, EtwProviderDatabaseJsonContext.Default.EtwProviderDatabase);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[error] ToJson|> {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Writes the ETW provider database to a JSON file.
    /// </summary>
    /// <param name="db">The ETW provider database object.</param>
    /// <returns>True if the file was successfully created; otherwise, false.</returns>
    private static bool ToJsonFile(EtwProviderDatabase db)
    {
        var status = false;
        var cwd = Path.Combine(Environment.CurrentDirectory, db.EtwProviderDatabaseName) + ".json";
        try
        {
            File.WriteAllText(cwd, ToJson(db));
            status = true;
            Console.WriteLine($"\n\n[success] FileCreated: {cwd}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[error] ToJsonFile|> {ex.Message}");
        }
        return status;
    }
}

/// <summary>
/// Represents the database of ETW providers.
/// </summary>
public class EtwProviderDatabase
{
    public string EtwProviderDatabaseName { get; set; }
    public int CountEtwProviders { get; set; } = 0;
    public List<EtwProvider> EtwProviders { get; set; }

    /// <summary>
    /// Initializes a new instance of the EtwProviderDatabase class.
    /// </summary>
    /// <param name="initialListSize">Initial size of the providers list.</param>
    public EtwProviderDatabase(ushort initialListSize = 1200)
    {
        EtwProviderDatabaseName = $"{Environment.MachineName.ToLower()}-etw-providers-database";
        EtwProviders = new List<EtwProvider>(initialListSize);
    }

    /// <summary>
    /// Updates the count of ETW providers.
    /// </summary>
    public void UpdateRecordCount()
    {
        CountEtwProviders = EtwProviders.Count;
    }
}

/// <summary>
/// Represents an ETW provider.
/// </summary>
public class EtwProvider
{
    public string? EtwProviderName { get; set; }
    public string? EtwProviderGuid { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<EtwKeyword>? EtwKeyWords { get; set; }

    /// <summary>
    /// Initializes a new instance of the EtwProvider class.
    /// </summary>
    /// <param name="name">Name of the provider.</param>
    /// <param name="guid">GUID of the provider.</param>
    /// <param name="keywordsSize">Initial size of the keywords list.</param>
    public EtwProvider(string name, string guid, ushort keywordsSize = 64)
    {
        EtwProviderName = name;
        EtwProviderGuid = guid;
        EtwKeyWords = new List<EtwKeyword>(keywordsSize);
    }
}

/// <summary>
/// Represents a keyword associated with an ETW provider.
/// </summary>
public class EtwKeyword
{
    public string? Name { get; set; }
    public ulong? Value { get; set; }
}
#endif
