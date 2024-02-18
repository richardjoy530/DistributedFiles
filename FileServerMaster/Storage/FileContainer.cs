namespace FileServerMaster.Storage
{
    public class FileContainer : IFileContainer
    {
        private readonly List<IFormFile> _files;
        private readonly ILogger<FileContainer> _logger;

        public FileContainer(ILogger<FileContainer> logger)
        {
            _files = new List<IFormFile>();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IFormFile? Get(string filename)
        {
            return _files.FirstOrDefault(f => f.FileName == filename);
        }

        public void Add(IFormFile formFile)
        {
            _logger.LogInformation($"Adding: {formFile.FileName}");
            _files.Add(formFile);
        }

        public void DiscardFiles(string[] filesToRemoveFromContainer)
        {
            _files.RemoveAll(f => filesToRemoveFromContainer.Contains(f.FileName));
        }

        public IEnumerable<string> GetTempFileNames()
        {
            return _files.Select(f => f.FileName);
        }
    }
}
