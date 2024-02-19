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

        public FileData? GetFile(string filename)
        {
            var filePath = Path.Combine(_distributedFolder, filename);
            if (File.Exists(filePath))
            {
                var stream = File.OpenRead(filePath);
                var filedata = new FileData { ContentBase64 = stream.GetBytes(), FileName = filename };
                return filedata;
            }

            return null;
        }

        public async Task SaveFile(FileData file)
        {
            using var stream = new MemoryStream(file.ContentBase64.Base64Decode());

            if (!Directory.Exists(_distributedFolder))
            {
                Directory.CreateDirectory(_distributedFolder);
            }

            using var fileStream = new FileStream(Path.Combine(_distributedFolder, file.FileName), FileMode.Create);
            await stream.CopyToAsync(fileStream);
        }
    }
}
