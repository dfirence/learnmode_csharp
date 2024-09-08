namespace Safiro.Core
{
    public interface IRegistryCollector : ICollector
    {
        public Task CollectAsync(string rootKeyPath, string outputPath);
    }
}