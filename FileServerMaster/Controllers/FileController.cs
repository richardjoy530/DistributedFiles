using Common.Events;
using FileServerMaster.Events;
using Common.Proxy.Controllers;
using FileServerMaster.Storage;
using FileServerMaster.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Web;

namespace FileServerMaster.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FileController : ControllerBase, IMasterFileController, IFileController
    {
        private readonly ILogger<FileController> _logger;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly IFileContainer _fileContainer;
        private readonly IFileDistributorManager _fileDistributorManager;

        public FileController(ILogger<FileController> logger,
            IFileContainer fileQueueContainer,
            IFileDistributorManager fileDistributorManager,
            IEventDispatcher eventDispatcher)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _fileContainer = fileQueueContainer ?? throw new ArgumentNullException(nameof(fileQueueContainer));
            _fileDistributorManager = fileDistributorManager ?? throw new ArgumentNullException(nameof(fileDistributorManager));
            _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
        }

        [HttpPost]
        public async Task UploadAsync(IFormFile file)
        {
            _logger.LogInformation("[FileUpload] received result \"{filename}\"", file.FileName);
            await _fileContainer.AddFileAsync(file.OpenReadStream(), file.FileName, file.ContentType);

            var masterHost = new HostString(Request.Host.Host, HttpContext.Connection.LocalPort);
            _fileDistributorManager.UpdateFileAvailability(masterHost, [file.FileName]);

            _eventDispatcher.FireEvent(new RequestCheckInEvent());
        }

        [HttpGet("{filename}")]
        public ActionResult DownLoadFile(string filename)
        {
            filename = HttpUtility.UrlDecode(filename);
            var result = _fileContainer.GetFile(filename);
            if (result.FileStream == null)
            {
                _logger.LogWarning("\"{}\" was not present in the result container", filename);
                return NoContent();
            }

            var ip = HttpContext.Connection.RemoteIpAddress!.MapToIPv4().ToString();
            var remoteHost = new HostString(ip, HttpContext.Connection.RemotePort);
            _logger.LogInformation("[FileDownload] \"{}\" downloaded \"{}\"", remoteHost, filename);
            return File(result.FileStream, result.ContentType, filename);
        }
    }
}