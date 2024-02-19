using Common.Proxy.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Web;

namespace FileServerSlave.Files
{
    [Route("[controller]")]
    [ApiController]
    public class FileController : ControllerBase, IFileController
    {
        private readonly ILogger<FileController> _logger;
        private readonly IFileManager _fileManager;

        public FileController(IFileManager fileManager, ILogger<FileController> logger)
        {
            _fileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // need to optimize this endpoints. use stream.
        [HttpGet("{filename}")]
        public FileData? DownLoadFile(string filename)
        {
            filename = HttpUtility.UrlDecode(filename);
            var file = _fileManager.GetFile(filename);
            if (file == null)
            {
                _logger.LogInformation("\"{}\" was not present in the file container", filename);
                return null;
            }

            _logger.LogInformation("\"{}\" was downloaded", filename);
            return file;
        }
    }
}
