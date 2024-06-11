using System.Dynamic;
using System.Reflection;
using System.Text.Json;
using Watcher.Modules.Windows.ETW;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Watcher.Modules.Schema;


/// <summary>
/// Represents an ETW Event with a category, enabled status, and fields.
/// </summary>
public class EtwEvent
{
    public required int? EventId { get; set; }
    public required string? EventCategory { get; set; }
    public required bool? IsEnabled { get; set; }
    public required List<Field>? Fields { get; set; }
}


/// <summary>
/// Represents a field in an ETW Event with a name and type.
/// </summary>
public class Field
{
    public required string? Name { get; set; }
    public required string? Type { get; set; }
}

public class Metadata
{
    public required string? Id { get; set; }
    public required string? Rv { get; set; }
    public required string? Description { get; set; }
}
/// <summary>
/// Represents a schema definition with a name and a list of ETW events.
/// </summary>
public class SchemaDefinition
{

    public Metadata? Metadata { get; set; }
    public List<EtwEvent>? Events { get; set; }
}

/// <summary>
/// Root schema containing the schema definition.
/// </summary>
public class RootSchema
{
    public List<Field>? CommonFields { get; set; }
    public required SchemaDefinition Schema { get; set; }
}

/// <summary>
/// Custom naming policy for camelCase conversion.
/// </summary>
public class CamelCaseNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name)
    {
        return char.ToLowerInvariant(name[0]) + name.Substring(1);
    }
}

/// <summary>
/// Handles schema operations including loading, dynamic object creation,
/// and serialization.
/// </summary>
public partial class SchemaHandler
{
    private IDictionary<string, SchemaEventDict>? _events { get; set; }
    private JsonSerializerOptions JsonOptions { get; }
        = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = new CamelCaseNamingPolicy(),
            DictionaryKeyPolicy = new CamelCaseNamingPolicy()
        };

    private SchemaDefinition? _schema { get; set; }

    public SchemaHandler(string schemaFilePath)
    {
        LoadSchema(schemaFilePath);
        //Console.WriteLine(ToJsonFromSchema());
        if (CreateDynamicObject())
        {
            Console.WriteLine("Loaded Schema");
        }
    }
    /// <summary>
    /// Loads a schema from a YAML file.
    /// </summary>
    /// <param name="schemaFilePath">The path to the schema YAML file.</param>
    public void LoadSchema(string schemaFilePath)
    {
        try
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            using var reader = new StreamReader(schemaFilePath);
            var rootSchema = deserializer.Deserialize<RootSchema>(reader);
            _schema = rootSchema.Schema;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(
                $"{GetType().Name}|>Error loading schema: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates a dynamic object based on the loaded schema.
    /// </summary>
    /// <returns>A dictionary of dynamic objects representing the schema events.</returns>
    private bool CreateDynamicObject()
    {
        if (_schema?.Events == null || _schema.Events.Count == 0)
            return false;

        _events = new Dictionary<string, SchemaEventDict>(_schema.Events.Count);

        foreach (var etwEvent in _schema.Events)
        {
            if (string.IsNullOrEmpty(etwEvent.EventCategory) || string.IsNullOrWhiteSpace(etwEvent.EventCategory))
                continue;

            if (!etwEvent.IsEnabled ?? false == false)
                continue;

            if (etwEvent.Fields == null || etwEvent?.Fields?.Count == 0)
                continue;

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var dict = new SchemaEventDict((uint)etwEvent.Fields.Count)
            {
                ["EventCategory"] = etwEvent.EventCategory
            };
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            foreach (var field in etwEvent.Fields)
            {
                var fieldName = field.Name;
                var fieldValue = TryGetDefaultValue(field.Type ?? "_");
                if (fieldName != null && fieldValue != null)
                    dict[fieldName] = fieldValue;
            }
            _events.Add(etwEvent.EventCategory, dict);
        }
        return _events.Count > 0;
    }

    /// <summary>
    /// Tries to get the default value for a given type.
    /// </summary>
    /// <param name="type">The type as a string.</param>
    /// <returns>The default value for the type.</returns>
    public static object? TryGetDefaultValue(string type)
    {
        return type.ToLower() switch
        {
            "bool" => false,
            "datetime" => DateTime.UtcNow,
            "string" => string.Empty,
            "int" => -1,
            "uint" => 0,
            "long" => 0,
            _ => null
        };
    }
    public string ToJson()
    {
        return JsonSerializer.Serialize(_events, JsonOptions);
    }
    /// <summary>
    /// Serializes a dynamic object to JSON with error handling.
    /// </summary>
    /// <param name="dynamicObject">The dynamic object to serialize.</param>
    /// <returns>The JSON representation of the object.</returns>
    public string ToJsonFromSchema()
    {
        try
        {
            return JsonSerializer.Serialize(_schema, JsonOptions);
        }
        catch (Exception ex)
        {
            string? method = MethodBase.GetCurrentMethod()?.Name;
            Console.Error.WriteLine(
                $"{method}|> Error serializing to JSON: {ex.Message}");
            return string.Empty;
        }
    }

    [System.Text.RegularExpressions.GeneratedRegex(@"\s+")]
    public static partial System.Text.RegularExpressions.Regex MyRegex();
}