﻿using System.Collections.Concurrent;

namespace FileServerMaster.Storage
{
    public class FileContainer : IFileContainer
    {
        private readonly ConcurrentDictionary<string, (string Path, string ContentType)> _tempFiles;
        private readonly ILogger<FileContainer> _logger;
        private readonly IFileDistributorManager _fileDistributorManager;

        public FileContainer(ILogger<FileContainer> logger, IFileDistributorManager fileDistributorManager)
        {
            _tempFiles = [];
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _fileDistributorManager = fileDistributorManager ?? throw new ArgumentNullException(nameof(fileDistributorManager));
        }

        public async Task<bool> AddFileAsync(Stream stream, string fileName, string contentType)
        {
            var tempFileName = Path.GetTempFileName();

            var fs = new FileStream(tempFileName, FileMode.Open);
            await stream.CopyToAsync(fs);
            fs.Close();
            await fs.DisposeAsync();

            _tempFiles.TryAdd(fileName, (tempFileName, contentType));
            return true;
        }

        public (FileStream? FileStream, string ContentType) GetFile(string fileName)
        {
            if(_tempFiles.TryGetValue(fileName, out var tempFile))
            {
                var fs = new FileStream(tempFile.Path, FileMode.Open, FileAccess.Read);
                return (fs, tempFile.ContentType);
            }

            _logger.LogWarning("unable to find the requested file");
            return (null, string.Empty);
        }

        public void DiscardFiles(string[] slaveFileNames)
        {
            var filesToRemove = _tempFiles.Where(f => slaveFileNames.Contains(f.Key)).Select(kv => kv.Key).ToArray();
            foreach (var fileName in filesToRemove)
            {
                // master no longer host this file
                _fileDistributorManager.RemoveMaster(fileName);

                _logger.LogInformation("[FileContainer] removing \"{}\"", fileName);
                _tempFiles.Remove(fileName, out _);
            }
        }
    }
}