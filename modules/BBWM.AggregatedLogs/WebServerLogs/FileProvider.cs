using System.Text;

namespace BBWM.AggregatedLogs
{
    public class FileProvider : IFileProvider
    {
        public IEnumerable<string> ReadLines(string path)
        {
            using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 0x1000, FileOptions.SequentialScan);
            using var stream = new StreamReader(fileStream, Encoding.UTF8);

            string line;
            while ((line = stream.ReadLine()) != null)
            {
                yield return line;
            }
        }

        public IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption, DateTimeOffset lastTimestamp) => Directory.EnumerateFiles(path, searchPattern, searchOption).Where(file => new FileInfo(file).LastWriteTime > lastTimestamp);
    }
}
