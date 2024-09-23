namespace Patricia
{
    using System;
    using System.Collections.Generic;
    using System.Net;

    public class PatriciaTrieNode
    {
        public int[] Children { get; set; } = new int[2]; // Child node indices (for '0' and '1')
        public bool IsEndOfPrefix { get; set; } = false;
        public string? Rule { get; set; } = null;
    }

    public class PatriciaSubTrie
    {
        // List to store nodes
        private List<PatriciaTrieNode> nodes = new List<PatriciaTrieNode> { new PatriciaTrieNode() };

        // Cache for inserted CIDRs (tuple of binary IP and prefix length) to avoid duplicates
        private HashSet<(ulong, ulong, int)> insertedCidrs = new HashSet<(ulong, ulong, int)>();

        private int bitLength; // Number of bits for IPv4 (32) or IPv6 (128)

        public PatriciaSubTrie(int bitLength)
        {
            this.bitLength = bitLength;
        }

        // Insert a CIDR block into the Patricia Trie
        public void Insert(string cidr, string rule)
        {
            var parts = cidr.Split('/');
            int prefixLength = int.Parse(parts[1]);

            // Convert the IP to its binary representation (tuple of two ulongs)
            var binaryIp = IpToBinary(parts[0]);

            // Check if this CIDR has already been inserted
            if (!insertedCidrs.Add((binaryIp.Item1, binaryIp.Item2, prefixLength)))
            {
                return; // CIDR already exists, skip insertion
            }

            int nodeIndex = 0;
            for (int i = bitLength - 1; i >= bitLength - prefixLength; i--)
            {
                int bit = GetBitAtPosition(binaryIp, i);
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

        // Get bit at the specific position
        private int GetBitAtPosition((ulong, ulong) binaryIp, int position)
        {
            if (position < 64)
            {
                return (int)((binaryIp.Item1 >> position) & 1);
            }
            else
            {
                return (int)((binaryIp.Item2 >> (position - 64)) & 1);
            }
        }

        // Search for the longest matching rule
        public string? Search(string ip)
        {
            var binaryIp = IpToBinary(ip);

            int nodeIndex = 0;
            string? longestMatch = null;

            for (int i = bitLength - 1; i >= 0; i--)
            {
                int bit = GetBitAtPosition(binaryIp, i);
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

        // Convert IP address (IPv4 or IPv6) to binary representation using stackalloc and Span<byte>
        public (ulong, ulong) IpToBinary(string ip)
        {
            ReadOnlySpan<char> ipSpan = ip.AsSpan();

            // Check if the IP is IPv4 or IPv6
            if (ipSpan.Contains(':')) // IPv6
            {
                Span<byte> addressBytes = stackalloc byte[16]; // IPv6 requires 16 bytes
                if (IPAddress.TryParse(ip, out var ipAddress) &&
                    ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                {
                    // Parse IPv6 address into bytes
                    ipAddress.TryWriteBytes(addressBytes, out _);

                    // Combine into two 64-bit ulong values (128 bits total)
                    ulong high = BitConverter.ToUInt64(addressBytes.Slice(0, 8));
                    ulong low = BitConverter.ToUInt64(addressBytes.Slice(8, 8));
                    return (high, low);
                }
                else
                {
                    throw new ArgumentException("Invalid IPv6 address.");
                }
            }
            else // IPv4
            {
                Span<byte> octets = stackalloc byte[4]; // Stack-allocated memory for 4 bytes
                int octetIndex = 0;

                // Parse the IPv4 address
                for (int i = 0, start = 0; i <= ipSpan.Length; i++)
                {
                    if (i == ipSpan.Length || ipSpan[i] == '.')
                    {
                        octets[octetIndex++] = byte.Parse(ipSpan.Slice(start, i - start));
                        start = i + 1;
                    }
                }

                // Combine octets into a 32-bit uint packed into an ulong
                return ((ulong)(octets[0] << 24 | octets[1] << 16 | octets[2] << 8 | octets[3]), 0);
            }
        }
    }

    public class PatriciaTrie
    {
        private PatriciaSubTrie IPv4Trie;
        private PatriciaSubTrie IPv6Trie;

        public PatriciaTrie()
        {
            IPv4Trie = new PatriciaSubTrie(32);  // IPv4 uses 32 bits
            IPv6Trie = new PatriciaSubTrie(128); // IPv6 uses 128 bits
        }

        // Insert a CIDR block into the correct Trie (IPv4 or IPv6)
        public void Insert(string cidr, string rule)
        {
            if (cidr.Contains(":"))
            {
                IPv6Trie.Insert(cidr, rule); // Insert into the IPv6 Trie
            }
            else
            {
                IPv4Trie.Insert(cidr, rule); // Insert into the IPv4 Trie
            }
        }

        // Search for the longest matching rule in the correct Trie
        public string? Search(string ip)
        {
            if (ip.Contains(":"))
            {
                return IPv6Trie.Search(ip); // Search in IPv6 Trie
            }
            else
            {
                return IPv4Trie.Search(ip); // Search in IPv4 Trie
            }
        }
    }
}