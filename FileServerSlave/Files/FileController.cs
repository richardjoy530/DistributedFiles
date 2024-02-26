using Common.Proxy.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
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
        public HttpResponseMessage? DownloadFileStream(string filename)
        {
            filename = HttpUtility.UrlDecode(filename);
            var stream = _fileManager.GetStream(filename);
            if (stream == null)
            {
                _logger.LogWarning("\"{}\" was not present in the file container", filename);
                return null;
            }

            HttpResponseMessage response = new(HttpStatusCode.OK);
            response.Content = new StreamContent(stream);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = filename
            };

            var ip = HttpContext.Connection.RemoteIpAddress!.MapToIPv4().ToString();
            var remotehost = new HostString(ip, HttpContext.Connection.RemotePort);

            _logger.LogInformation("[FileDownload] \"{}\" downloaded \"{}\"", remotehost, filename);
            return response;
        }
    }
}
