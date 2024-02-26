using Common;
using Common.Proxy.Controllers;

namespace FileServerSlave.Files
{
    public class FileManager : IFileManager
    {
        private readonly string _distributedFolder;

        public FileManager(IDestinationLocator locator)
        {
            ArgumentNullException.ThrowIfNull(locator);

            _distributedFolder = locator.GetDestinationFolderPath();
        }

        public string[] GetAvailableFilesOnThisServer()
        {
            if (!Directory.Exists(_distributedFolder))
            {
                Directory.CreateDirectory(_distributedFolder);
            }

            return Directory.EnumerateFiles(_distributedFolder).Select(f => f.Split('\\').Last()).ToArray();
        }

        public FileStream? GetStream(string filename)
        {
            var filePath = Path.Combine(_distributedFolder, filename);
            if (File.Exists(filePath))
            {
                using var stream = File.OpenRead(filePath);
                return stream;
            }

            return null;
        }

        public async Task SaveFileAsync(Stream stream, string fileName)
        {
            if (!Directory.Exists(_distributedFolder))
            {
                Directory.CreateDirectory(_distributedFolder);
            }

            using var fileStream = new FileStream(Path.Combine(_distributedFolder, fileName), FileMode.Create);
            await stream.CopyToAsync(fileStream);
        }
    }
}
