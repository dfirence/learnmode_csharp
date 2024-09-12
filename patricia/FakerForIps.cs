

namespace IPAddressGenerator
{
    using Bogus;
    using System;
    using System.Collections.Generic;
    class FakeIpData
    {
        public static List<string> LoadFakeIPAddresses(int ipListSize)
        {
            // Create a faker instance for generating random data
            var faker = new Faker();

            // Generate a large list of random IP addresses (e.g., 100,000 IPs)
            List<string> ipAddresses = new List<string>();
            for (int i = 0; i < ipListSize; i++)
            {
                // Generate a random IPv4 address
                var randomIp = faker.Internet.Ip();
                ipAddresses.Add(randomIp);
            }
            return ipAddresses;
        }
    }
}
