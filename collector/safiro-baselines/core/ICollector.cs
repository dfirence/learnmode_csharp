namespace Safiro.Core
{
    public interface ICollector
    {
        public Task CollectAsync(string targetPath, string outputPath);
    }
}