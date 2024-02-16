
namespace Backend.Storage
{
    public class FileQueueContainer : IFileQueueContainer
    {
        private readonly ILogger<FileQueueContainer> _logger;

        private readonly Queue<IFormFile> _files;

        public FileQueueContainer(ILogger<FileQueueContainer> logger)
        {
            _logger = logger;
            _files = new Queue<IFormFile>();
        }

        public IFormFile? DeQueue()
        {
            if (_files.TryPeek(out _))
            {
                _logger.LogInformation($"DeQueueing: {_files.Peek().FileName}");
                return _files.Dequeue();
            }
            return null;
        }

        public void EnQueue(IFormFile formFile)
        {
            _logger.LogInformation($"EnQueue: {formFile.FileName}");
            _files.Enqueue(formFile);
        }
    }
}
