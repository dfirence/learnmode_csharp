using System.Text.Json;
using System.Diagnostics;
using BenchmarkDotNet.Running;
using Patricia.Benchmarks;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IPAddressGenerator;

namespace Patricia
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Running the benchmark
            var summary = BenchmarkRunner.Run<PatriciaTrieBenchmark>();

            //PatriciaTrie trie = new PatriciaTrie();

            // Load CIDR ranges from a local JSON file
            // string currentDirectory = Directory.GetCurrentDirectory();
            // string jsonFilePath = Path.Combine(currentDirectory, "ServiceTags_Public_20240909.json");
            // var jsonCidrs = await LoadCidrFromJson(jsonFilePath);

            // // Measure the time taken to insert CIDR ranges into the trie
            // await BatchInsertCidrRanges(trie, jsonCidrs, "AzureIPs/Services");

            // // Measure IP lookups
            // MeasureLookups(trie, FakeIpData.LoadFakeIPAddresses(100));
        }

        // Batch insert CIDR ranges in parallel
        public static async Task BatchInsertCidrRanges(PatriciaTrie trie, List<string> cidrRanges, string rule)
        {
            var tasks = new List<Task>();
            var lockObject = new object();  // Create a lock object to synchronize access

            foreach (var cidr in cidrRanges)
            {
                tasks.Add(Task.Run(() =>
                {
                    lock (lockObject)  // Lock access to the PatriciaTrie to ensure thread safety
                    {
                        trie.Insert(cidr, rule);
                    }
                }));
            }

            await Task.WhenAll(tasks);
        }

        // Method to measure lookup time for a list of IP addresses
        public static void MeasureLookups(PatriciaTrie trie, List<string> ipAddresses)
        {
            var stopwatch = Stopwatch.StartNew();  // Start measuring time
            int matchCounts = 0;
            int total = ipAddresses.Count();
            foreach (var ip in ipAddresses)
            {
                // Perform the lookup for each IP
                //uint binaryIp = trie.IpToBinaryInt(ip);  // Convert the IP to its binary form
                var rule = trie?.Search(ip);
                if (rule != null) matchCounts++;
                Console.WriteLine($"IP: {ip}, Rule: {rule ?? "-"}");
            }

            stopwatch.Stop();  // Stop measuring time

            // Calculate total time taken
            var totalMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
            var averageMilliseconds = totalMilliseconds / ipAddresses.Count;

            // Output the time measurements
            Console.WriteLine($"\nTotal time for {ipAddresses.Count} lookups: {totalMilliseconds:F2} ms");
            Console.WriteLine($"Average time per lookup: {averageMilliseconds:F5} ms");
            Console.WriteLine($"{matchCounts} Matched Out Of {total}");
        }

        // Loads CIDR ranges from a JSON file and returns them as a list
        public static async Task<List<string>> LoadCidrFromJson(string filePath)
        {
            var jsonData = await File.ReadAllTextAsync(filePath);
            var jsonDoc = JsonDocument.Parse(jsonData);

            // Extract 'addressPrefixes' from the JSON and return them as a list
            return jsonDoc.RootElement
                          .GetProperty("values")
                          .EnumerateArray()
                          .SelectMany(element => element.GetProperty("properties").GetProperty("addressPrefixes").EnumerateArray())
                          .Select(prefix => prefix.GetString() ?? string.Empty)
                          .ToList();
        }
    }
}
