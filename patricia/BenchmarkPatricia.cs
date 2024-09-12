

namespace Patricia.Benchmarks
{
    using BenchmarkDotNet.Attributes;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using Patricia;
    using IPAddressGenerator;
    public class TrieBenchmark
    {
        public PatriciaTrie? trie;

        [GlobalSetup]
        public async Task Setup()
        {
            trie = new PatriciaTrie();
            // Load CIDR ranges from a local JSON file in the current directory
            string currentDirectory = Directory.GetCurrentDirectory();
            string jsonFilePath = Path.Combine(currentDirectory, "ServiceTags_Public_20240909.json");
            var jsonCidrs = await LoadCidrFromJson(jsonFilePath);

            // Insert the loaded CIDR ranges into the Patricia Trie
            BatchInsertCidrRanges(jsonCidrs, "Sample Rule");
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

        // Batch insert CIDR ranges synchronously
        public void BatchInsertCidrRanges(List<string> cidrRanges, string rule)
        {
            var lockObject = new object();  // Create a lock object to synchronize access

            foreach (var cidr in cidrRanges)
            {
                lock (lockObject)  // Lock access to the PatriciaTrie to ensure thread safety
                {
                    trie.Insert(cidr, rule);
                }
            }
        }
        public static List<string> loadTestIps()
        {
            return new List<string> {
            "104.47.51.16",
            "104.47.55.16",
            "13.107.136.10",
            "13.107.137.10",
            "13.107.138.10",
            "13.107.246.40",
            "13.107.6.198",
            "13.89.179.14",
            "150.171.41.10",
            "150.171.43.10",
            "20.189.173.18",
            "20.189.173.4",
            "20.189.173.8",
            "20.190.151.37",
            "20.190.161.25",
            "20.42.73.24",
            "204.79.197.203",
            "23.195.92.41",
            "23.200.3.19",
            "23.34.59.26",
            "23.39.40.48",
            "23.56.210.39",
            "4.152.113.42",
            "4.255.204.159",
            "40.104.46.2",
            "40.104.46.34",
            "40.104.46.66",
            "40.126.23.38",
            "51.104.15.252",
            "51.105.71.136",
            "51.116.246.104",
            "52.108.0.146",
            "52.108.16.47",
            "52.108.216.1",
            "52.109.16.3",
            "52.109.24.24",
            "52.109.24.25",
            "52.109.24.39",
            "52.109.24.68",
            "52.110.2.143",
            "52.110.2.158",
            "52.110.2.162",
            "52.110.2.175",
            "52.111.229.123",
            "52.111.229.3",
            "52.111.230.25",
            "52.111.230.3",
            "52.111.230.9",
            "52.113.194.132",
            "52.170.96.53",
            "52.239.154.132",
            "52.96.165.178",
            "52.96.181.226",
            "52.96.189.2",
            "52.96.35.178",
            "52.96.37.34",
            "52.96.87.210",
            "52.96.87.226",
            "52.96.87.242",
            "52.96.88.18",
            "69.192.21.146"
            };
        }
        // Perform the lookup operation in the benchmark
        [Benchmark]
        public void LookupTest()
        {
            List<string> ipAddresses = FakeIpData.LoadFakeIPAddresses(1000000);
            foreach (var ip in ipAddresses)
            {
                // Perform the lookup for each IP
                var _ = trie?.Search(ip);
            }
        }
    }
}
