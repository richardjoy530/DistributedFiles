using Common;
using Common.Proxy.Controllers;
using FileServerSlave.Utils;
using Microsoft.AspNetCore.StaticFiles;
using System.Net.Mime;

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

        public (FileStream? FileStream, string ContentType) GetFile(string filename)
        {
            var filePath = Path.Combine(_distributedFolder, filename);
            if (File.Exists(filePath))
            {
                var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);

                _ = new FileExtensionContentTypeProvider().TryGetContentType(filePath, out var contentType);
                
                return (fs, contentType ?? string.Empty);
            }

            return (null, string.Empty);
        }

        public async Task SaveFile(Stream stream, string fileName)
        {
            if (!Directory.Exists(_distributedFolder))
            {
                Directory.CreateDirectory(_distributedFolder);
            }

            var fileStream = new FileStream(Path.Combine(_distributedFolder, fileName), FileMode.Create);
            await stream.CopyToAsync(fileStream);
            await fileStream.FlushAsync();
            fileStream.Close();
            fileStream.Dispose();
        }
    }
}
