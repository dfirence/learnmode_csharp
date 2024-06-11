using System.Collections;
using System.Text.Json;

namespace Watcher.Modules.Windows.ETW;


/// <summary>
/// Custom dictionary class for schema events.
/// </summary>
public class SchemaEventDict : IDictionary<string, object>
{
    private readonly int _capacity;
    private readonly Dictionary<string, object> _internalDict;

    private JsonSerializerOptions _jsonOptions { get; }
        = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = new CamelCaseNamingPolicy(),
            DictionaryKeyPolicy = new CamelCaseNamingPolicy()
        };
    /// <summary>
    /// Custom naming policy for JSON serialization to convert property names to camel case.
    /// </summary>
    private class CamelCaseNamingPolicy : JsonNamingPolicy
    {
        /// <summary>
        /// Converts the given name to camel case.
        /// </summary>
        /// <param name="name">The original name.</param>
        /// <returns>The name converted to camel case.</returns>
        public override string ConvertName(string name)
        {
            return char.ToLowerInvariant(name[0]) + name.Substring(1);
        }
    }
    /// <summary>
    /// Initializes a new instance of the <c>SchemaEventDict</c> class with the specified initial capacity.
    /// </summary>
    /// <param name="capacity">The initial capacity of the dictionary.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the capacity is zero.</exception>
    public SchemaEventDict(uint capacity)
    {
        if (capacity == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be greater than zero.");
        }
        _internalDict = new Dictionary<string, object>((int)capacity);
        _capacity = (int)capacity;
    }

    /// <summary>
    /// Gets or sets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to get or set.</param>
    /// <returns>The value associated with the specified key.</returns>
    public object this[string key]
    {
        get => _internalDict[key];
        set => _internalDict[key] = value;
    }

    /// <summary>
    /// Gets a collection containing the keys in the dictionary.
    /// </summary>
    public ICollection<string> Keys => _internalDict.Keys;

    /// <summary>
    /// Gets a collection containing the values in the dictionary.
    /// </summary>
    public ICollection<object> Values => _internalDict.Values;

    /// <summary>
    /// Gets the number of key/value pairs contained in the dictionary.
    /// </summary>
    public int Count => _internalDict.Count;

    /// <summary>
    /// Gets a value indicating whether the dictionary is read-only.
    /// </summary>
    public bool IsReadOnly => ((IDictionary<string, object>)_internalDict).IsReadOnly;

    /// <summary>
    /// Adds the specified key and value to the dictionary.
    /// </summary>
    /// <param name="key">The key of the element to add.</param>
    /// <param name="value">The value of the element to add.</param>
    public void Add(string key, object value)
    {
        if (HasCapacity())
            _internalDict.Add(key, value);
    }

    /// <summary>
    /// Adds the specified key/value pair to the dictionary.
    /// </summary>
    /// <param name="item">The key/value pair to add.</param>
    public void Add(KeyValuePair<string, object> item)
    {
        if (HasCapacity())
        {
            _internalDict.Add(item.Key, item.Value);
        }
    }

    /// <summary>
    /// Removes all keys and values from the dictionary.
    /// </summary>
    public void Clear()
    {
        _internalDict.Clear();
    }

    /// <summary>
    /// Determines whether the dictionary contains the specified key/value pair.
    /// </summary>
    /// <param name="item">The key/value pair to locate in the dictionary.</param>
    /// <returns>true if the key/value pair is found in the dictionary; otherwise, false.</returns>
    public bool Contains(KeyValuePair<string, object> item)
    {
        return _internalDict.ContainsKey(item.Key) && _internalDict[item.Key] == item.Value;
    }

    /// <summary>
    /// Determines whether the dictionary contains the specified key.
    /// </summary>
    /// <param name="key">The key to locate in the dictionary.</param>
    /// <returns>true if the dictionary contains an element with the specified key; otherwise, false.</returns>
    public bool ContainsKey(string key)
    {
        return _internalDict.ContainsKey(key);
    }

    /// <summary>
    /// Copies the elements of the dictionary to an array, starting at a particular array index.
    /// </summary>
    /// <param name="array">The array to copy the elements to.</param>
    /// <param name="arrayIndex">The zero-based index in the array at which copying begins.</param>
    public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
    {
        ((IDictionary<string, object>)_internalDict).CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Returns an enumerator that iterates through the dictionary.
    /// </summary>
    /// <returns>An enumerator for the dictionary.</returns>
    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
        return _internalDict.GetEnumerator();
    }

    /// <summary>
    /// Removes the element with the specified key from the dictionary.
    /// </summary>
    /// <param name="key">The key of the element to remove.</param>
    /// <returns>true if the element is successfully removed; otherwise, false.</returns>
    public bool Remove(string key)
    {
        return _internalDict.Remove(key);
    }

    /// <summary>
    /// Removes the specified key/value pair from the dictionary.
    /// </summary>
    /// <param name="item">The key/value pair to remove.</param>
    /// <returns>true if the key/value pair is successfully removed; otherwise, false.</returns>
    public bool Remove(KeyValuePair<string, object> item)
    {
        return _internalDict.Remove(item.Key);
    }

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key whose value to get.</param>
    /// <param name="value">
    /// When this method returns, the value associated with the specified key, if the key is found; 
    /// otherwise, the default value for the type of the value parameter.
    /// </param>
    /// <returns>true if the dictionary contains an element with the specified key; otherwise, false.</returns>
    public bool TryGetValue(string key, out object value)
    {
#pragma warning disable CS8601 // Possible null reference assignment.
        return _internalDict.TryGetValue(key, out value);
#pragma warning restore CS8601 // Possible null reference assignment.
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _internalDict.GetEnumerator();
    }

    /// <summary>
    /// Checks if the dictionary has reached its capacity.
    /// </summary>
    /// <returns>True if the dictionary can accept more items, otherwise false.</returns>
    private bool HasCapacity()
    {
        if (_internalDict.Count > _capacity)
        {
            Console.Error.WriteLine(
                $@"[WARNING]|{GetType().Name}|> Max Capacity {_capacity} reached, blocking inserts"
            );
            return false;
        }
        return true;
    }

    /// <summary>
    /// Converts the dictionary to a JSON string.
    /// </summary>
    /// <returns>A JSON string representation of the dictionary.</returns>
    public string? ToJson()
    {
        try
        {
            return JsonSerializer.Serialize(_internalDict, _jsonOptions);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(
                $@"{GetType().Name}|> Unable To Serialize: {ex.Message}"
            );
            return null;
        }
    }

    /// <summary>
    /// Writes the JSON representation of the dictionary to the console.
    /// </summary>
    public void ToJsonConsole()
    {
        Console.Error.WriteLine(this.ToJson());
    }
}