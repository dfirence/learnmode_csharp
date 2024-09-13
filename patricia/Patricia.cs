/// 
/// Multi Threaded & Batch Lookup
/// Parallel.ForEach to distribute the IP lookup load across multiple threads.
/// This will allow the CPU to handle multiple lookups simultaneously, taking advantage of the available cores.
/// 
namespace Patricia
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    public class PatriciaTrieNode
    {
        public int[] Children { get; set; } = new int[2]; // Child node indices (for '0' and '1')
        public bool IsEndOfPrefix { get; set; } = false;
        public string? Rule { get; set; } = null;
    }

    public class PatriciaTrie
    {
        // List to store the Trie nodes in a contiguous block of memory
        private List<PatriciaTrieNode> nodes = new List<PatriciaTrieNode> { new PatriciaTrieNode() };

        // Cache for binary representation of IP addresses
        private Dictionary<string, uint> ipCache = new Dictionary<string, uint>();

        // Convert IP address to 32-bit binary integer
        public uint IpToBinaryInt(string ip)
        {
            // Use a Span<char> to avoid creating intermediate string objects
            ReadOnlySpan<char> ipSpan = ip.AsSpan();

            Span<byte> octets = stackalloc byte[4]; // Allocate octets on the stack to avoid heap allocations
            int octetIndex = 0;

            // Parse the IP address octets directly from the Span
            for (int i = 0, start = 0; i <= ipSpan.Length; i++)
            {
                if (i == ipSpan.Length || ipSpan[i] == '.')
                {
                    octets[octetIndex++] = byte.Parse(ipSpan.Slice(start, i - start));
                    start = i + 1; // Move start to the next character after '.'
                }
            }

            // Combine octets into a 32-bit unsigned integer
            return (uint)(octets[0] << 24 | octets[1] << 16 | octets[2] << 8 | octets[3]);
        }

        // public uint IpToBinaryInt(string ip)
        // {
        //     if (ipCache.TryGetValue(ip, out uint binaryIp))
        //     {
        //         return binaryIp;
        //     }

        //     var octets = ip.Split('.').Select(byte.Parse).ToArray();
        //     binaryIp = (uint)(octets[0] << 24 | octets[1] << 16 | octets[2] << 8 | octets[3]);
        //     ipCache[ip] = binaryIp;
        //     return binaryIp;
        // }

        // Insert a CIDR into the Patricia Trie
        public void Insert(string cidr, string rule)
        {
            if (cidr.Contains(":")) return; // Skip IPv6 addresses

            var parts = cidr.Split('/');
            string ip = parts[0];
            int prefixLength = int.Parse(parts[1]);
            uint binaryIp = IpToBinaryInt(ip);
            int nodeIndex = 0;

            for (int i = 31; i >= 32 - prefixLength; i--)
            {
                int bit = (int)((binaryIp >> i) & 1);

                if (nodes[nodeIndex].Children[bit] == 0)
                {
                    nodes[nodeIndex].Children[bit] = nodes.Count;
                    nodes.Add(new PatriciaTrieNode());
                }

                nodeIndex = nodes[nodeIndex].Children[bit];
            }

            nodes[nodeIndex].IsEndOfPrefix = true;
            nodes[nodeIndex].Rule = rule;
        }

        // Search for the longest matching rule in the Patricia Trie
        public string? Search(string ip)
        {
            uint binaryIp = IpToBinaryInt(ip);
            int nodeIndex = 0;
            string? longestMatch = null;

            for (int i = 31; i >= 0; i--)
            {
                int bit = (int)((binaryIp >> i) & 1);

                if (nodes[nodeIndex].Children[bit] != 0)
                {
                    nodeIndex = nodes[nodeIndex].Children[bit];
                    if (nodes[nodeIndex].IsEndOfPrefix)
                    {
                        longestMatch = nodes[nodeIndex].Rule;
                    }
                }
                else
                {
                    break; // No further match
                }
            }

            return longestMatch;
        }

        // Batch processing of IP addresses (without parallelization)
        public List<string?> BatchSearch(List<string> ipAddresses)
        {
            List<string?> results = new List<string?>();
            foreach (var ip in ipAddresses)
            {
                results.Add(Search(ip));
            }
            return results;
        }

        // Multi-threaded batch lookup for IP addresses using parallelism
        public List<string?> ParallelBatchSearch(List<string> ipAddresses)
        {
            List<string?> results = new List<string?>(new string?[ipAddresses.Count]);

            Parallel.ForEach(System.Collections.Concurrent.Partitioner.Create(0, ipAddresses.Count), range =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    results[i] = Search(ipAddresses[i]);
                }
            });

            return results;
        }
    }
}
