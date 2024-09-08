namespace Safiro.Core
{
    public interface IFileCollector : ICollector
    {
        public const int BatchSize = 100;
        public Task CollectAsync(string targetPath, string outputDir);
    }
}