using System.Collections.Concurrent;
using Common;
using Common.Proxy.Controllers;

namespace FileServerMaster.Storage
{
    public class FileContainer : IFileContainer
    {
        private readonly ConcurrentDictionary<string, FileData> _files;
        private readonly ILogger<FileContainer> _logger;
        private readonly IFileDistributorManager _fileDistributorManager;

        public FileContainer(ILogger<FileContainer> logger, IFileDistributorManager fileDistributorManager)
        {
            _files = [];
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _fileDistributorManager = fileDistributorManager ?? throw new ArgumentNullException(nameof(fileDistributorManager));
        }

        public FileData? Get(string filename)
        {
            return _files.FirstOrDefault(f => f.Key == filename).Value;
        }

        public bool Add(IFormFile formFile)
        {
            _logger.LogInformation("[FileContainer] adding \"{filename}\"", formFile.FileName);

            var filedata = new FileData
            {
                FileName = formFile.FileName,
                ContentBase64 = formFile.OpenReadStream().GetBytes()
            };

            if (_files.TryAdd(filedata.FileName, filedata))
            {
                _logger.LogDebug("added \"{filename}\" to file container", formFile.FileName);
                return true;
            }

            return false;
        }

        public void DiscardFiles(string[] slaveFileNames)
        {
            var filesToRemove = _files.Where(f => slaveFileNames.Contains(f.Value.FileName)).Select(kv => kv.Key).ToArray();
            foreach (var fileName in filesToRemove)
            {
                // master no longer host this file
                _fileDistributorManager.RemoveMaster(fileName);

                _logger.LogInformation("[FileContainer] removing \"{}\"", fileName);
                _files.Remove(fileName, out _);
            }
        }

        public IEnumerable<string> GetTempFileNames()
        {
            return _files.Select(f => f.Value.FileName);
        }
    }
}
