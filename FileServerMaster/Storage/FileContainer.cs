﻿using Common;
using Common.Proxy.Controllers;

namespace FileServerMaster.Storage
{
    public class FileContainer : IFileContainer
    {
        private readonly List<FileData> _files;
        private readonly ILogger<FileContainer> _logger;

        public FileContainer(ILogger<FileContainer> logger)
        {
            _files = new List<FileData>();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public FileData? Get(string filename)
        {
            return _files.FirstOrDefault(f => f.FileName == filename);
        }

        public void Add(IFormFile formFile)
        {
            _logger.LogDebug("adding \"{filename}\" to file container", formFile.FileName);

            var filedata = new FileData
            {
                FileName = formFile.FileName,
                ContentBase64 = formFile.OpenReadStream().GetBytes()
            };
            _files.Add(filedata);
        }

        public void DiscardFiles(string[] filesToRemoveFromContainer)
        {
            _logger.LogDebug("discarding \"{files}\" from file container", string.Join(',', filesToRemoveFromContainer));
            _files.RemoveAll(f => filesToRemoveFromContainer.Contains(f.FileName));
        }

        public IEnumerable<string> GetTempFileNames()
        {
            return _files.Select(f => f.FileName);
        }
    }
}
