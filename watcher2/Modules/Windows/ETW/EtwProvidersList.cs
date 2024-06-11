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
        string providers = string.Empty;
        var providerGuids = TraceEventProviders.GetPublishedProviders(); // IEnumerable<Guid>
        var etwDB = new EtwProviderDatabase((ushort)providerGuids.Count());
        int processedCount = 0;

        // Loop through each provider GUID
        foreach (var _guid in providerGuids)
        {
            string _providerName = TraceEventProviders.GetProviderName(_guid);
            string _providerGuid = _guid.ToString();

            // Get the keywords for the provider
            List<ProviderDataItem> _keyWords = TraceEventProviders.GetProviderKeywords(_guid);
            var _provider = new EtwProvider(_providerName, _providerGuid, (ushort)_keyWords.Count);

            // Add keywords to the provider
            foreach (var _keyWord in _keyWords)
            {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                _provider.EtwKeyWords.Add(new EtwKeyword { Name = _keyWord.Name, Value = _keyWord.Value });
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            }
            _provider.EtwProviderName = _providerName;
            _provider.EtwProviderGuid = _providerGuid;
            etwDB.EtwProviders.Add(_provider);

            // Update and print the progress
            processedCount++;
            Console.Write($"\rProcessing >>> {processedCount,7} of {etwDB.EtwProviders.Capacity} ETW Native Providers");
        }

        etwDB.UpdateRecordCount();
        ToJsonFile(etwDB);
    }

    /// <summary>
    /// Converts the ETW provider database to JSON string.
    /// </summary>
    /// <param name="db">The ETW provider database object.</param>
    /// <returns>JSON string representation of the database.</returns>
    public static string? ToJson(EtwProviderDatabase db)
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
    public static bool ToJsonFile(EtwProviderDatabase db)
    {
        bool status = false;
        string cwd = Path.Combine(Environment.CurrentDirectory, db.EtwProviderDatabaseName) + ".json";
        try
        {
            var json = ToJson(db);
            if (json != null)
            {
                File.WriteAllText(cwd, json);
                status = true;
                Console.WriteLine($"\n\n[success] FileCreated: {cwd}");
            }
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
    public EtwProviderDatabase(ushort initialListSize = ushort.MaxValue)
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
    /// <param name="_name">Name of the provider.</param>
    /// <param name="_guid">GUID of the provider.</param>
    /// <param name="_keywordsSize">Initial size of the keywords list.</param>
    public EtwProvider(string _name, string _guid, ushort _keywordsSize = 64)
    {
        EtwProviderName = _name;
        EtwProviderGuid = _guid;
        EtwKeyWords = new List<EtwKeyword>(_keywordsSize);
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
