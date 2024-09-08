using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.IO;

public interface IRegistryCollector
{
    Task CollectAsync(string rootKeyPath, string outputPath);
}

public class RegistryCollector : IRegistryCollector
{
    // Enum for Registry Hive
    private enum Hive
    {
        HKCR,
        HKCU,
        HKLM,
        HKU,
        HKCC
    }

    public async Task CollectAsync(string rootKeyPath, string outputPath)
    {
        try
        {
            Console.WriteLine($"Starting registry collection from: {rootKeyPath}");
            
            // Determine the root registry hive
            var hive = ParseHiveFromPath(rootKeyPath);

            // Enumerate keys using a generator with deferred execution
            var registryEntries = EnumerateRegistryKeys(hive, rootKeyPath);

            // Process the registry entries and export results asynchronously
            await ExportRegistryEntriesAsync(registryEntries, outputPath);
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"Access denied to key: {rootKeyPath}, Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing registry: {ex.Message}");
        }
    }

    // Enumerate registry keys using a generator (yield return)
    private IEnumerable<RegistryEntry> EnumerateRegistryKeys(RegistryKey rootHive, string rootKeyPath)
    {
        var keyStack = new Stack<string>();
        keyStack.Push(rootKeyPath);

        while (keyStack.Count > 0)
        {
            var currentKeyPath = keyStack.Pop();

            try
            {
                using (var key = rootHive.OpenSubKey(currentKeyPath))
                {
                    if (key == null) continue;

                    // Get all subkeys and push them onto the stack
                    foreach (var subKey in key.GetSubKeyNames())
                    {
                        keyStack.Push(Path.Combine(currentKeyPath, subKey));
                    }

                    // Yield the current key as an entry
                    foreach (var valueName in key.GetValueNames())
                    {
                        var valueData = key.GetValue(valueName);
                        var valueType = key.GetValueKind(valueName);

                        yield return new RegistryEntry
                        {
                            Hive = rootHive.Name,
                            KeyPath = currentKeyPath,
                            ValueName = valueName,
                            ValueData = valueData?.ToString() ?? string.Empty,
                            ValueType = valueType.ToString()
                        };
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Access denied to key: {currentKeyPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading key {currentKeyPath}: {ex.Message}");
            }
        }
    }

    // Export the collected registry entries asynchronously
    private async Task ExportRegistryEntriesAsync(IEnumerable<RegistryEntry> registryEntries, string outputPath)
    {
        using (var writer = new StreamWriter(outputPath))
        {
            await writer.WriteLineAsync("Hive,KeyPath,ValueName,ValueData,ValueType");

            foreach (var entry in registryEntries)
            {
                await writer.WriteLineAsync($"{entry.Hive},{entry.KeyPath},{entry.ValueName},{entry.ValueData},{entry.ValueType}");
            }

            Console.WriteLine($"Registry collection exported to: {outputPath}");
        }
    }

    // Helper to parse the hive from the root key path
    private RegistryKey ParseHiveFromPath(string path)
    {
        if (path.StartsWith("HKCR")) return Registry.ClassesRoot;
        if (path.StartsWith("HKCU")) return Registry.CurrentUser;
        if (path.StartsWith("HKLM")) return Registry.LocalMachine;
        if (path.StartsWith("HKU")) return Registry.Users;
        if (path.StartsWith("HKCC")) return Registry.CurrentConfig;

        throw new ArgumentException("Invalid registry hive specified.");
    }
}

// Model for storing registry entry information
public class RegistryEntry
{
    public string Hive { get; set; }
    public string KeyPath { get; set; }
    public string ValueName { get; set; }
    public string ValueData { get; set; }
    public string ValueType { get; set; }
}