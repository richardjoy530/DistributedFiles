using Common.Proxy.Controllers;
using Microsoft.AspNetCore.Mvc;
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

        [HttpGet("{filename}")]
        public ActionResult DownLoadFile(string filename)
        {
            filename = HttpUtility.UrlDecode(filename);
            var result = _fileManager.GetFile(filename);
            if (result.FileStream == null)
            {
                _logger.LogWarning("\"{filename}\" was not present in the result container", filename);
                return NoContent();
            }

            var ip = HttpContext.Connection.RemoteIpAddress!.MapToIPv4().ToString();
            var remoteHost = new HostString(ip, HttpContext.Connection.RemotePort);
            _logger.LogInformation("[FileDownload] \"{remoteHost}\" downloaded \"{filename}\"", remoteHost, filename);
            return File(result.FileStream, result.ContentType, filename);
        }
    }
}