using System.Collections.Concurrent;
using Common.Proxy.Controllers;

namespace FileServerMaster.Storage
{
    public class FileContainer : IFileContainer
    {
        private readonly ConcurrentDictionary<string, MemoryStream> _filesStreams;
        private readonly ILogger<FileContainer> _logger;
        private readonly IFileDistributorManager _fileDistributorManager;

        public FileContainer(ILogger<FileContainer> logger, IFileDistributorManager fileDistributorManager)
        {
            _filesStreams = [];
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _fileDistributorManager = fileDistributorManager ?? throw new ArgumentNullException(nameof(fileDistributorManager));
        }

        public Stream? GetStream(string filename)
        {
            if (_filesStreams.TryGetValue(filename, out var ms))
            {
                return ms;
            }

            return null;
        }

        public async Task<bool> AddToStream(IFormFile formFile)
        {
            _logger.LogInformation("[FileContainer] adding \"{filename}\"", formFile.FileName);
            var ms = new MemoryStream();
            await formFile.CopyToAsync(ms);

            if (_filesStreams.TryAdd(formFile.FileName, ms))
            {
                _logger.LogDebug("added \"{filename}\" to file container", formFile.FileName);
                return true;
            }

            return false;
        }

        public async Task DiscardFilesAsync(string[] slaveFileNames)
        {
            var filesToRemove = _filesStreams.Where(f => slaveFileNames.Contains(f.Key)).Select(kv => kv.Key).ToArray();
            foreach (var fileName in filesToRemove)
            {
                // master no longer host this file
                _fileDistributorManager.RemoveMaster(fileName);

                _logger.LogInformation("[FileContainer] removing \"{}\"", fileName);
                _filesStreams.Remove(fileName, out var stream);
                await stream!.DisposeAsync();
                _logger.LogInformation("[FileContainer] disposed stream for \"{}\"", fileName);
            }
        }

        public IEnumerable<string> GetTempFileNames()
        {
            return _filesStreams.Keys;
        }
    }
}
