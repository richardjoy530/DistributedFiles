
namespace Backend.Storage
{
    public class FileContainer : IFileContainer
    {
        public List<IFormFile> Files => _files;

        private readonly ILogger<FileContainer> _logger;
        private readonly List<IFormFile> _files;

        public FileContainer(ILogger<FileContainer> logger)
        {
            _logger = logger;
            _files = new List<IFormFile>();
        }

        public IFormFile? Get(string filename)
        {
            return _files.FirstOrDefault(f => f.FileName == filename);
        }

        public void Add(IFormFile formFile)
        {
            _logger.LogInformation($"Adding: {formFile.FileName}");
            Files.Add(formFile);
        }
    }
}
