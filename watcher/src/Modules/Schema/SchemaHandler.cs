using System.Dynamic;
using System.Reflection;
using System.Text.Json;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Watcher.Modules.Schema
{
    /// <summary>
    /// Represents an ETW Event with a category, enabled status, and fields.
    /// </summary>
    public class EtwEvent
    {
        public required string? EventCategory { get; set; }
        public bool? IsEnabled { get; set; }
        public List<Field>? Fields { get; set; }
    }

    /// <summary>
    /// Represents a field in an ETW Event with a name and type.
    /// </summary>
    public class Field
    {
        public required string? Name { get; set; }
        public required string? Type { get; set; }
    }

    /// <summary>
    /// Represents a schema definition with a name and a list of ETW events.
    /// </summary>
    public class SchemaDefinition
    {
        public string? Name { get; set; }
        public List<EtwEvent>? Events { get; set; }
    }

    /// <summary>
    /// Root schema containing the schema definition.
    /// </summary>
    public class RootSchema
    {
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
    public class SchemaHandler
    {
        private JsonSerializerOptions JsonOptions { get; }
            = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = new CamelCaseNamingPolicy(),
                DictionaryKeyPolicy = new CamelCaseNamingPolicy()
            };

        private SchemaDefinition? _schema { get; set; }

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
        public IDictionary<string, IDictionary<string, object>>? CreateDynamicObject()
        {
            if (_schema?.Events == null || _schema.Events.Count == 0)
                return null;

            var objects = new Dictionary<string, IDictionary<string, object>>();

            foreach (var etwEvent in _schema.Events)
            {
                if (string.IsNullOrEmpty(etwEvent.EventCategory) || string.IsNullOrWhiteSpace(etwEvent.EventCategory))
                    continue;

                if (!etwEvent.IsEnabled ?? false)
                    continue;

                if (etwEvent.Fields == null || etwEvent?.Fields?.Count == 0)
                    continue;

                dynamic dynObject = new ExpandoObject();
                var dict = (IDictionary<string, object>)dynObject;

#pragma warning disable CS8602 // Dereference of a possibly null reference.
                dict["EventCategory"] = etwEvent.EventCategory;
#pragma warning restore CS8602

                foreach (var field in etwEvent.Fields)
                {
                    var fieldName = field.Name;
                    var fieldValue = TryGetDefaultValue(field.Type ?? "_");
                    if (fieldName != null && fieldValue != null)
                        dict[fieldName] = fieldValue;
                }

                objects.Add(etwEvent.EventCategory, dict);
            }

            return objects.Count > 0 ? objects : null;
        }

        /// <summary>
        /// Tries to get the default value for a given type.
        /// </summary>
        /// <param name="type">The type as a string.</param>
        /// <returns>The default value for the type.</returns>
        private static object? TryGetDefaultValue(string type)
        {
            return type.ToLower() switch
            {
                "bool" => false,
                "datetime" => DateTime.UtcNow,
                "string" => string.Empty,
                "int" => -1,
                _ => null
            };
        }

        /// <summary>
        /// Serializes a dynamic object to YAML.
        /// </summary>
        /// <param name="dynamicObject">The dynamic object to serialize.</param>
        /// <returns>The YAML representation of the object.</returns>
        public string SerializeDynamicObject(ExpandoObject dynamicObject)
        {
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            return serializer.Serialize(dynamicObject);
        }

        /// <summary>
        /// Serializes a list of dynamic objects to YAML.
        /// </summary>
        /// <param name="dynamicObject">The dictionary of dynamic objects to serialize.</param>
        /// <returns>The YAML representation of the objects.</returns>
        public string SerializeListDynamicObject(IDictionary<string, IDictionary<string, object>> dynamicObject)
        {
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            return serializer.Serialize(dynamicObject);
        }

        /// <summary>
        /// Serializes a dynamic object to JSON with error handling.
        /// </summary>
        /// <param name="dynamicObject">The dynamic object to serialize.</param>
        /// <returns>The JSON representation of the object.</returns>
        public string SerializeDynamicObjectToJson(ExpandoObject dynamicObject)
        {
            try
            {
                return JsonSerializer.Serialize(dynamicObject, JsonOptions);
            }
            catch (Exception ex)
            {
                string? method = MethodBase.GetCurrentMethod()?.Name;
                Console.Error.WriteLine(
                    $"{method}|> Error serializing to JSON: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Serializes a dictionary of dynamic objects to JSON with error handling.
        /// </summary>
        /// <param name="dynamicObject">The dictionary of dynamic objects to serialize.</param>
        /// <returns>The JSON representation of the objects.</returns>
        public string ToJson(IDictionary<string, IDictionary<string, object>> dynamicObject)
        {
            try
            {
                return JsonSerializer.Serialize(dynamicObject, JsonOptions);
            }
            catch (Exception ex)
            {
                string? method = MethodBase.GetCurrentMethod()?.Name;
                Console.Error.WriteLine(
                    $"{method}|>Error serializing to JSON: {ex.Message}");
                return string.Empty;
            }
        }
        public string ToJsonFromDict(IDictionary<string, object> dynamicObject)
        {
            try
            {
                return JsonSerializer.Serialize(dynamicObject, JsonOptions);
            }
            catch (Exception ex)
            {
                string? method = MethodBase.GetCurrentMethod()?.Name;
                Console.Error.WriteLine(
                    $"{method}|>Error serializing to JSON: {ex.Message}");
                return string.Empty;
            }
        }
    }
}