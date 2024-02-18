namespace FileServerSlave.Files
{
    public class FileManager : IFileManager
    {
        private readonly string _distributedFolder;

        public FileManager(IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            _distributedFolder = configuration["_distributedFolder"]!;
        }

        public string[] GetAvailableFilesOnThisServer()
        {
            if (!Directory.Exists(_distributedFolder))
            {
                Directory.CreateDirectory(_distributedFolder);
            }

            return Directory.EnumerateFiles(_distributedFolder).ToArray();
        }

        public byte[] GetFile(string filename)
        {
            var filePath = Path.Combine(_distributedFolder, filename);
            if (File.Exists(filePath))
            {
                var stream = System.IO.File.OpenRead(filePath);
                return GetBytes(stream);
            }

            return [];
        }

        public async Task SaveFile(IFormFile formFile)
        {
            using var stream = formFile.OpenReadStream();

            if (!Directory.Exists(_distributedFolder))
            {
                Directory.CreateDirectory(_distributedFolder);
            }

            using var fileStream = new FileStream(Path.Combine(_distributedFolder, formFile.FileName), FileMode.Create);
            await stream.CopyToAsync(fileStream);
        }

        private static byte[] GetBytes(Stream stream)
        {
            var streamLength = (int)stream.Length; // total number of bytes read
            var numBytesReadPosition = 0; // actual number of bytes read
            var fileInBytes = new byte[streamLength];

            while (streamLength > 0)
            {
                // Read may return anything from 0 to numBytesToRead.
                var n = stream.Read(fileInBytes, numBytesReadPosition, streamLength);
                // Break when the end of the file is reached.
                if (n == 0)
                    break;
                numBytesReadPosition += n;
                streamLength -= n;
            }

            return fileInBytes;
        }
    }
}
