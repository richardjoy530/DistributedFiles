using Common;
using Common.Proxy.Controllers;

namespace FileServerMaster.Storage
{
    public class FileContainer : IFileContainer
    {
        private readonly List<FileData> _files;
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
            return _files.FirstOrDefault(f => f.FileName == filename);
        }

        public void Add(IFormFile formFile)
        {
            _logger.LogInformation("[FileContainer] adding \"{filename}\"", formFile.FileName);

            var filedata = new FileData
            {
                FileName = formFile.FileName,
                ContentBase64 = formFile.OpenReadStream().GetBytes()
            };
            _files.Add(filedata);

            _logger.LogDebug("added \"{filename}\" to file container", formFile.FileName);
        }

        public void DiscardFiles(string[] slaveFileNames)
        {
            var filesToRemove = _files.Select(f => slaveFileNames.Contains(f.FileName)).ToArray();
            foreach (var file in _files)
            {
                // master no longer host this file
                _fileDistributorManager.RemoveMaster(file.FileName);

                _logger.LogInformation("[FileContainer] removing \"{}\"", file.FileName);
                _files.Remove(file);
            }
        }

        public IEnumerable<string> GetTempFileNames()
        {
            return _files.Select(f => f.FileName);
        }
    }
}
