using Common;
using Common.Events;
using Common.Proxy.Controllers;
using FileServerMaster.Events;
using FileServerMaster.Storage;
using FileServerMaster.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Web;

namespace FileServerMaster.Controllers;

[ApiController]
[Route("[controller]")]
public class FileController : ControllerBase, IMasterFileController, IFileController
{
    private readonly ILogger<FileController> _logger;
    private readonly IWebSocketContainer _webSocketContainer;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IFileContainer _fileContainer;
    private readonly IFileDistributorManager _fileDistributorManager;
    private readonly IHostStringRetriver _hostStringRetriver;

    public FileController(ILogger<FileController> logger,
                          IWebSocketContainer webSocketContainer,
                          IFileContainer fileQueueContainer,
                          IFileDistributorManager fileDistributorManager,
                          IHostStringRetriver hostStringRetriver,
                          IEventDispatcher eventDispatcher)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _webSocketContainer = webSocketContainer ?? throw new ArgumentNullException(nameof(webSocketContainer));
        _fileContainer = fileQueueContainer ?? throw new ArgumentNullException(nameof(fileQueueContainer));
        _fileDistributorManager = fileDistributorManager ?? throw new ArgumentNullException(nameof(fileDistributorManager));
        _hostStringRetriver = hostStringRetriver ?? throw new ArgumentNullException(nameof(hostStringRetriver));
        _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
    }

    [HttpPost]
    public async Task UploadAsync(IFormFile file)
    {
        _logger.LogInformation("recived file \"{filename}\"", file.FileName);
        _fileContainer.Add(file);

        var masterHost = _hostStringRetriver.GetLocalFileServerHosts().First();
        _fileDistributorManager.UpdateFileAvailablity(masterHost, [file.FileName]);

        _logger.LogInformation("requesting check-in all slaves");
        await _eventDispatcher.FireEvent(new RequestCheckInEvent());
    }

    [HttpGet("{filename}")]
    public FileData? DownLoadFile(string filename)
    {
        filename = HttpUtility.UrlDecode(filename);
        var file = _fileContainer.Get(filename);
        if (file == null)
        {
            _logger.LogInformation("\"{}\" was not present in the file container", filename);
            return null;
        }

        _logger.LogInformation("\"{}\" was downloaded", filename);
        return file;
    }
}